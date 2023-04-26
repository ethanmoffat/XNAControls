using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System.Collections.Generic;
using System.Linq;

namespace XNAControls.Input
{
    /// <summary>
    /// Component that handles input for the game. Sends messages to controls based on the input event that occurred.
    /// </summary>
    public class InputManager : GameComponent
    {
        private readonly KeyboardListener _keyboardListener;
        private readonly MouseListener _mouseListener;

        private readonly Dictionary<object, bool> _mouseOverState;

        private IEventReceiver _dragTarget;

        /// <summary>
        /// Create a new InputManager using the default game previously set in the GameRepository
        /// </summary>
        public InputManager()
            : this (GameRepository.GetGame()) { }

        /// <summary>
        /// Create a new InputManager using the specified game and default MouseListenerSettings (60ms double-click, 1px drag threshold)
        /// </summary>
        public InputManager(Game game)
            : this(game, new MouseListenerSettings { DoubleClickMilliseconds = 60, DragThreshold = 1 })
        {
        }

        /// <summary>
        /// Create a new InputManager using the specified game and MouseListenerSettings
        /// </summary>
        public InputManager(Game game, MouseListenerSettings mouseListenerSettings)
            : base(game)
        {
            _keyboardListener = new KeyboardListener();
            _mouseListener = new MouseListener(mouseListenerSettings);

            _mouseOverState = new Dictionary<object, bool>();

            UpdateOrder = int.MinValue;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            _keyboardListener.KeyTyped += Keyboard_KeyTyped;
            _mouseListener.MouseClicked += Mouse_Click;
            _mouseListener.MouseDoubleClicked += Mouse_DoubleClick;
            _mouseListener.MouseDragStart += Mouse_DragStart;
            _mouseListener.MouseDragEnd += Mouse_DragEnd;
            _mouseListener.MouseDrag += Mouse_Drag;
            _mouseListener.MouseWheelMoved += Mouse_WheelMoved;

            base.Initialize();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            _keyboardListener.Update(gameTime);
            _mouseListener.Update(gameTime);

            var mouseState = MouseExtended.GetState();
            if (mouseState.PositionChanged)
            {
                var xnaControls = Game.Components.OfType<IXNAControl>();

                var comps = InputTargetFinder.GetMouseOverEventTargetControl(Game.Components, mouseState.Position);
                foreach (var component in comps)
                {
                    if (component.EventArea.Contains(mouseState.Position))
                    {
                        if (!_mouseOverState.TryGetValue(component, out var value) || !value)
                        {
                            _mouseOverState[component] = true;
                            Mouse_Enter(component, mouseState);
                        }
                        else
                        {
                            _mouseOverState[component] = true;
                            Mouse_Over(component, mouseState);
                        }
                    }
                    else if (_mouseOverState.TryGetValue(component, out var value) && value)
                    {
                        _mouseOverState[component] = false;
                        Mouse_Leave(component, mouseState);
                    }
                }
            }

            base.Update(gameTime);
        }

        private void Keyboard_KeyTyped(object sender, KeyboardEventArgs e)
        {
            // todo: is there a better place to store which textbox is focused?
            XNATextBox.FocusedTextbox?.PostMessage(EventType.KeyTyped, e);
        }

        private void Mouse_Click(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseDownEventTargetControl(Game.Components, e.Position);
            clickTarget?.PostMessage(EventType.Click, e);
        }

        private void Mouse_DoubleClick(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseDownEventTargetControl(Game.Components, e.Position);
            clickTarget?.PostMessage(EventType.DoubleClick, e);
        }

        private void Mouse_DragStart(object sender, MouseEventArgs e)
        {
            if (_dragTarget != null)
                return;

            _dragTarget = InputTargetFinder.GetMouseDownEventTargetControl(Game.Components, e.Position);
            _dragTarget?.PostMessage(EventType.DragStart, e);
        }

        private void Mouse_DragEnd(object sender, MouseEventArgs e)
        {
            if (_dragTarget == null)
                return;

            _dragTarget.PostMessage(EventType.DragEnd, e);
            _dragTarget = null;
        }

        private void Mouse_Drag(object sender, MouseEventArgs e)
        {
            if (_dragTarget == null)
                return;

            _dragTarget.PostMessage(EventType.Drag, e);
        }

        private void Mouse_WheelMoved(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseDownEventTargetControl(Game.Components, e.Position);
            clickTarget?.PostMessage(EventType.MouseWheelMoved, e);
        }

        private static void Mouse_Enter(IEventReceiver component, MouseStateExtended mouseState)
        {
            component.PostMessage(EventType.MouseEnter, mouseState);
        }

        private static void Mouse_Over(IEventReceiver component, MouseStateExtended mouseState)
        {
            component.PostMessage(EventType.MouseOver, mouseState);
        }

        private static void Mouse_Leave(IEventReceiver component, MouseStateExtended mouseState)
        {
            component.PostMessage(EventType.MouseLeave, mouseState);
        }
    }
}
