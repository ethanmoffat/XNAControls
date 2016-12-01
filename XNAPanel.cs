// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
    public class XNAPanel : XNAControl
    {
        public Texture2D BackgroundImage { get; set; }

        public void ClearTextBoxes()
        {
            foreach (var childTextBox in ChildControls.OfType<IXNATextBox>())
                childTextBox.Text = "";
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            if (BackgroundImage != null)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(BackgroundImage, DrawAreaWithParentOffset, Color.White);
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }
    }
}
