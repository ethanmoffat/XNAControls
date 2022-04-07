using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls.Test.Controls
{
    public class FakeXNADialog : XNADialog
    {
        public Texture2D BGText
        {
            get => BackgroundTexture;
            set => BackgroundTexture = value;
        }

        public Rectangle? BGTextSource
        {
            get => BackgroundTextureSource;
            set => BackgroundTextureSource = value;
        }

        public void CloseFake() => Close(XNADialogResult.NO_BUTTON_PRESSED);
    }
}
