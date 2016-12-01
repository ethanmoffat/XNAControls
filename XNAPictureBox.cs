// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
    public enum StretchMode
    {
        CenterInFrame,
        Stretch
    }

    public class XNAPictureBox : XNAControl, IXNAPictureBox
    {
        public StretchMode StretchMode { get; set; }

        public Texture2D Texture { get; set; }

        protected override void OnDrawControl(GameTime gameTime)
        {
            if (Texture != null)
            {
                _spriteBatch.Begin();

                switch (StretchMode)
                {
                    case StretchMode.CenterInFrame:
                        _spriteBatch.Draw(Texture,
                            new Rectangle(DrawAreaWithParentOffset.X + DrawArea.Width / 2 - Texture.Width / 2,
                                          DrawAreaWithParentOffset.Y + DrawArea.Height / 2 - Texture.Width / 2,
                                          Texture.Width,
                                          Texture.Height),
                            Color.White);
                        break;
                    case StretchMode.Stretch:
                        _spriteBatch.Draw(Texture, DrawAreaWithParentOffset, Color.White);
                        break;
                }
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }
    }

    public interface IXNAPictureBox
    {
        StretchMode StretchMode { get; set; }
        Texture2D Texture { get; set; }
    }
}
