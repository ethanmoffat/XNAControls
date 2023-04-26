using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
    /// <summary>
    /// PictureBox stretch mode
    /// </summary>
    public enum StretchMode
    {
        /// <summary>
        /// Center the picture in the draw area
        /// </summary>
        CenterInFrame,

        /// <summary>
        /// Stretch the picture to fit the draw area
        /// </summary>
        Stretch
    }

    /// <summary>
    /// Represents a picture box control
    /// </summary>
    public class XNAPictureBox : XNAControl, IXNAPictureBox
    {
        /// <inheritdoc />
        public StretchMode StretchMode { get; set; }

        /// <inheritdoc />
        public Texture2D Texture { get; set; }

        /// <inheritdoc />
        public Rectangle? SourceRectangle { get; set; }

        /// <inheritdoc />
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


    /// <summary>
    /// Interface for a picture box control
    /// </summary>
    public interface IXNAPictureBox : IXNAControl
    {
        /// <summary>
        /// Get or set the stretch mode
        /// </summary>
        StretchMode StretchMode { get; set; }

        /// <summary>
        /// Get or set the picture
        /// </summary>
        Texture2D Texture { get; set; }

        /// <summary>
        /// Get or set the source rectangle of the picture
        /// </summary>
        Rectangle? SourceRectangle { get; set; }
    }
}
