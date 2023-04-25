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

        public override bool GameIsActive => _isActive;

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
            MouseOverPreviouslyDuringUpdate = MouseOverPreviously;

            base.OnUpdateControl(gameTime);

            Updated = true;

            MouseOverDuringUpdate = MouseOver;
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            Drawn = true;
            base.OnDrawControl(gameTime);
        }
    }
}
