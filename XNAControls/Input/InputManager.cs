using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System.Collections.Generic;
using System.Linq;

namespace XNAControls.Input
{
    public class InputManager : GameComponent
    {
        private readonly KeyboardListener _keyboardListener;
        private readonly MouseListener _mouseListener;
        private readonly InputTargetFinder _inputTargetFinder;

        private readonly Dictionary<object, bool> _mouseOverState;

        private IEventReceiver _dragTarget;

        public InputManager()
            : this (GameRepository.GetGame()) { }

        public InputManager(Game game)
            : base(game)
        {
            _inputTargetFinder = new InputTargetFinder();
            _keyboardListener = new KeyboardListener();
            _mouseListener = new MouseListener();

            _mouseOverState = new Dictionary<object, bool>();

            UpdateOrder = int.MinValue;
        }

        public override void Initialize()
        {
            _keyboardListener.KeyTyped += Keyboard_KeyTyped;
            _mouseListener.MouseClicked += Mouse_Click;
            _mouseListener.MouseDoubleClicked += Mouse_DoubleClick;
            _mouseListener.MouseDragStart += Mouse_DragStart;
            _mouseListener.MouseDragEnd += Mouse_DragEnd;
            _mouseListener.MouseDrag += Mouse_Drag;

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            _keyboardListener.Update(gameTime);
            _mouseListener.Update(gameTime);

            var mouseState = MouseExtended.GetState();
            if (mouseState.PositionChanged)
            {
                var comps = Game.Components.OfType<IEventReceiver>().ToList();
                foreach (var component in comps)
                {
                    if (component.EventArea.Contains(mouseState.Position))
                    {
                        if (!_mouseOverState.TryGetValue(component, out var value))
                        {
                            _mouseOverState[component] = true;
                            Mouse_Enter(component, mouseState);
                        }
                        else
                        {
                            if (value)
                                Mouse_Over(component, mouseState);
                            _mouseOverState[component] = true;
                        }
                    }
                    else
                    {
                        if (_mouseOverState.TryGetValue(component, out var value) && value)
                        {
                            _mouseOverState[component] = false;
                            Mouse_Leave(component, mouseState);
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        private void Keyboard_KeyTyped(object sender, KeyboardEventArgs e)
        {
            // todo: is there a better place to store which textbox is focused?
            XNATextBox.FocusedTextbox?.SendMessage(EventType.KeyTyped, e);
        }

        private void Mouse_Click(object sender, MouseEventArgs e)
        {
            var clickTarget = _inputTargetFinder.GetMouseEventTargetControl(Game.Components, e.Position);
            clickTarget?.SendMessage(EventType.Click, e);
        }

        private void Mouse_DoubleClick(object sender, MouseEventArgs e)
        {
            var clickTarget = _inputTargetFinder.GetMouseEventTargetControl(Game.Components, e.Position);
            clickTarget?.SendMessage(EventType.DoubleClick, e);
        }

        private void Mouse_DragStart(object sender, MouseEventArgs e)
        {
            if (_dragTarget != null)
                return;

            _dragTarget = _inputTargetFinder.GetMouseEventTargetControl(Game.Components, e.Position);
            _dragTarget?.SendMessage(EventType.DragStart, e);
        }

        private void Mouse_DragEnd(object sender, MouseEventArgs e)
        {
            if (_dragTarget == null)
                return;

            _dragTarget.SendMessage(EventType.DragEnd, e);
            _dragTarget = null;
        }

        private void Mouse_Drag(object sender, MouseEventArgs e)
        {
            if (_dragTarget == null)
                return;

            _dragTarget.SendMessage(EventType.Drag, e);
        }

        private static void Mouse_Enter(IEventReceiver component, MouseStateExtended mouseState)
        {
            component.SendMessage(EventType.MouseEnter, mouseState);
        }

        private static void Mouse_Over(IEventReceiver component, MouseStateExtended mouseState)
        {
            component.SendMessage(EventType.MouseOver, mouseState);
        }

        private static void Mouse_Leave(IEventReceiver component, MouseStateExtended mouseState)
        {
            component.SendMessage(EventType.MouseLeave, mouseState);
        }
    }
}
