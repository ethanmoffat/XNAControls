// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNAControls.Test.Controls
{
    internal class FakeXNAControl : XNAControl
    {
        private bool _isActive;

        internal bool Updated { get; private set; }
        internal bool Drawn { get; private set; }

        internal new bool ShouldClickDrag => base.ShouldClickDrag;

        public override bool GameIsActive => _isActive;

        public MouseState CurrentMouseStateDuringUpdate { get; private set; }
        public KeyboardState CurrentKeyStateDuringUpdate { get; private set; }
        public MouseState PreviousMouseStateDuringUpdate { get; private set; }
        public KeyboardState PreviousKeyStateDuringUpdate { get; private set; }
        public bool MouseOverDuringUpdate { get; private set; }
        public bool MouseOverPreviouslyDuringUpdate { get; private set; }

        internal bool IsDisposed
        {
            get
            {
                var disposedProperty = typeof(XNAControl).GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic);
                return (bool) disposedProperty.GetValue(this);
            }
        }

        internal void SetIsActive(bool value)
        {
            _isActive = value;
        }

        internal void ResetUpdated()
        {
            Updated = false;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            Updated = true;

            CurrentMouseStateDuringUpdate = CurrentMouseState;
            CurrentKeyStateDuringUpdate = CurrentKeyState;
            PreviousMouseStateDuringUpdate = PreviousMouseState;
            PreviousKeyStateDuringUpdate = PreviousKeyState;
            MouseOverDuringUpdate = MouseOver;
            MouseOverPreviouslyDuringUpdate = MouseOverPreviously;

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            Drawn = true;
            base.OnDrawControl(gameTime);
        }
    }
}
