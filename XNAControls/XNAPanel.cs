using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
    /// <summary>
    /// Represents a panel control
    /// </summary>
    public class XNAPanel : XNAControl, IXNAPanel
    {
        /// <inheritdoc />
        public Texture2D BackgroundImage { get; set; }

        /// <inheritdoc />
        public void ClearTextBoxes()
        {
            foreach (var childTextBox in ChildControls.OfType<IXNATextBox>())
                childTextBox.Text = "";
        }

        /// <inheritdoc />
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

    /// <summary>
    /// Interface for a panel control
    /// </summary>
    public interface IXNAPanel : IXNAControl
    {
        /// <summary>
        /// Get or set the background image of the panel
        /// </summary>
        Texture2D BackgroundImage { get; set; }

        /// <summary>
        /// Clear any text box controls that are part of this panel
        /// </summary>
        void ClearTextBoxes();
    }
}
