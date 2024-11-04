using System.Collections.Generic;

using Microsoft.Xna.Framework;

using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;

namespace XNAControls.Input
{
    /// <summary>
    /// Component that handles input for the game. Sends messages to controls based on the input event that occurred.
    /// </summary>
    public class InputManager : GameComponent
    {
        private readonly KeyboardListener _keyboardListener;
        private readonly MouseListener _mouseListener;

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

            UpdateOrder = int.MinValue;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            _keyboardListener.KeyTyped += Keyboard_KeyTyped;
            _keyboardListener.KeyPressed += Keyboard_KeyPressed;
            _keyboardListener.KeyReleased += Keyboard_KeyReleased;
            _mouseListener.MouseDown += Mouse_Down;
            _mouseListener.MouseUp += Mouse_Up;
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
            var mouseState = MouseExtended.GetState();
            if (mouseState.PositionChanged)
            {
                var comps = InputTargetFinder.GetMouseOverEventTargetControl(Game.Components);
                foreach (var component in comps)
                {
                    if (component.EventArea.Contains(mouseState.Position))
                    {
                        if (!InputTargetFinder.MouseOverState.TryGetValue(component, out var value) || !value)
                        {
                            InputTargetFinder.MouseOverState[component] = true;
                            Mouse_Enter(component, mouseState);
                        }
                        else
                        {
                            InputTargetFinder.MouseOverState[component] = true;
                            Mouse_Over(component, mouseState);
                        }
                    }
                    else if (InputTargetFinder.MouseOverState.TryGetValue(component, out var value) && value)
                    {
                        InputTargetFinder.MouseOverState[component] = false;
                        Mouse_Leave(component, mouseState);
                    }
                }
            }

            _keyboardListener.Update(gameTime);
            _mouseListener.Update(gameTime);

            base.Update(gameTime);
        }

        private void Keyboard_KeyTyped(object sender, KeyboardEventArgs e)
        {
            // todo: is there a better place to store which textbox is focused?
            XNATextBox.FocusedTextbox?.PostMessage(EventType.KeyTyped, e);
        }

        private void Keyboard_KeyPressed(object sender, KeyboardEventArgs e)
        {
            XNATextBox.FocusedTextbox?.PostMessage(EventType.KeyPressed, e);
        }

        private void Keyboard_KeyReleased(object sender, KeyboardEventArgs e)
        {
            XNATextBox.FocusedTextbox?.PostMessage(EventType.KeyReleased, e);
        }

        private void Mouse_Down(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseButtonEventTargetControl(Game.Components);
            clickTarget?.PostMessage(EventType.MouseDown, e);
        }

        private void Mouse_Up(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseButtonEventTargetControl(Game.Components);
            clickTarget?.PostMessage(EventType.MouseUp, e);
        }

        private void Mouse_Click(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseButtonEventTargetControl(Game.Components);
            clickTarget?.PostMessage(EventType.Click, e);
        }

        private void Mouse_DoubleClick(object sender, MouseEventArgs e)
        {
            var clickTarget = InputTargetFinder.GetMouseButtonEventTargetControl(Game.Components);
            clickTarget?.PostMessage(EventType.DoubleClick, e);
        }

        private void Mouse_DragStart(object sender, MouseEventArgs e)
        {
            if (_dragTarget != null)
                return;

            _dragTarget = InputTargetFinder.GetMouseButtonEventTargetControl(Game.Components);
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
            var clickTarget = InputTargetFinder.GetMouseButtonEventTargetControl(Game.Components);
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
