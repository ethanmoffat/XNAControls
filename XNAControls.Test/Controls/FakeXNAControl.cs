﻿// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace XNAControls.Test.Controls
{
    internal class FakeXNAControl : XNAControl
    {
        private bool _isActive;

        internal bool Updated { get; private set; }
        internal bool Drawn { get; private set; }

        internal new bool ShouldClickDrag => base.ShouldClickDrag;

        public override bool GameIsActive => _isActive;

        public void SetIsActive(bool value)
        {
            _isActive = value;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            Updated = true;
            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            Drawn = true;
            base.OnDrawControl(gameTime);
        }
    }
}
