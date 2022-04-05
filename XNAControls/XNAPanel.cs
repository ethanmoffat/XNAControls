using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
    public class XNAPanel : XNAControl, IXNAPanel
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

    public interface IXNAPanel : IXNAControl
    {
        Texture2D BackgroundImage { get; set; }

        void ClearTextBoxes();
    }
}
