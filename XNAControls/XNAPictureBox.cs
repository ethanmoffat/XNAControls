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

        public Rectangle? SourceRectangle { get; set; }

        protected override void OnDrawControl(GameTime gameTime)
        {
            if (Texture != null)
            {
                _spriteBatch.Begin();

                switch (StretchMode)
                {
                    case StretchMode.CenterInFrame:
                        _spriteBatch.Draw(Texture,
                            new Rectangle(DrawAreaWithParentOffset.X + DrawArea.Width / 2 - (SourceRectangle ?? Texture.Bounds).Width / 2,
                                          DrawAreaWithParentOffset.Y + DrawArea.Height / 2 - (SourceRectangle ?? Texture.Bounds).Height / 2,
                                          (SourceRectangle ?? Texture.Bounds).Width,
                                          (SourceRectangle ?? Texture.Bounds).Height),
                            SourceRectangle,
                            Color.White);
                        break;
                    case StretchMode.Stretch:
                        _spriteBatch.Draw(Texture, DrawAreaWithParentOffset, SourceRectangle, Color.White);
                        break;
                }
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }
    }

    public interface IXNAPictureBox : IXNAControl
    {
        StretchMode StretchMode { get; set; }
        Texture2D Texture { get; set; }
        Rectangle? SourceRectangle { get; set; }
    }
}
