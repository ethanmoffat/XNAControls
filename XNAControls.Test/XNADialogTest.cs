using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using XNAControls.Test.Controls;
using XNAControls.Test.Helpers;

namespace XNAControls.Test
{
    [TestFixture]
    public class XNADialogTest
    {
        private static TestGameManager _gameManager;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            _gameManager = new TestGameManager();
            GameRepository.SetGame(_gameManager.Game);
        }

        [TearDown]
        public void TearDown()
        {
            Singleton<DialogRepository>.Instance.OpenDialogs.Clear();
        }

        [Test]
        public void XNADialog_SetBackgroundTexture_SetsControlSizeToTextureDimensions()
        {
            using var dlg = new FakeXNADialog();
            using var tex = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 420, 69);

            dlg.BGText = tex;

            Assert.That(dlg.DrawArea.Width, Is.EqualTo(420));
            Assert.That(dlg.DrawArea.Height, Is.EqualTo(69));
        }

        [Test]
        public void XNADialog_SetBackgroundTexture_SetsControlSizeToSourceAreaDimensionsIfSet()
        {
            var area = new Rectangle(0, 0, 69, 420);
            using var tex = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 420, 69);

            using var dlg = new FakeXNADialog();
            dlg.BGTextSource = area;
            dlg.BGText = tex;

            Assert.That(dlg.DrawArea.Width, Is.EqualTo(69));
            Assert.That(dlg.DrawArea.Height, Is.EqualTo(420));
        }

        [Test]
        public void XNADialog_SetBackgroundTexture_DoesNotSetControlSizeIfTextureAndAreaAreNull()
        {
            using var dlg = new FakeXNADialog();
            dlg.BGTextSource = null;
            dlg.BGText = null;

            Assert.That(dlg.DrawArea.Width, Is.Zero);
            Assert.That(dlg.DrawArea.Height, Is.Zero);
        }

        [Test]
        public void XNADialog_SetBackgroundTextureSource_SetsControlSizeToSourceAreaDimensionsIfSet()
        {
            using var dlg = new FakeXNADialog();
            var area = new Rectangle(0, 0, 69, 420);

            dlg.BGTextSource = area;

            Assert.That(dlg.DrawArea.Width, Is.EqualTo(69));
            Assert.That(dlg.DrawArea.Height, Is.EqualTo(420));
        }

        [Test]
        public void XNADialog_SetBackgroundTextureSource_SetsControlSizeToTextureDimensionsIfAreaUnset()
        {
            var area = new Rectangle(0, 0, 69, 420);
            using var tex = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 420, 69);

            using var dlg = new FakeXNADialog();
            dlg.BGText = tex;
            dlg.BGTextSource = area;

            Assert.That(dlg.DrawArea.Width, Is.EqualTo(69));
            Assert.That(dlg.DrawArea.Height, Is.EqualTo(420));

            dlg.BGTextSource = null;

            Assert.That(dlg.DrawArea.Width, Is.EqualTo(420));
            Assert.That(dlg.DrawArea.Height, Is.EqualTo(69));
        }

        [Test]
        public void XNADialog_SetBackgroundTextureSource_DoesNotSetControlSizeIfTextureAndAreaAreNull()
        {
            using var dlg = new FakeXNADialog();
            dlg.BGText = null;
            dlg.BGTextSource = null;

            Assert.That(dlg.DrawArea.Width, Is.Zero);
            Assert.That(dlg.DrawArea.Height, Is.Zero);
        }

        [Test]
        public void XNADialog_Initialize_PushesDialogOntoStack()
        {
            using var dlg = new FakeXNADialog();

            dlg.Initialize();

            var actual = Singleton<DialogRepository>.Instance.OpenDialogs.Peek();
            Assert.That(actual, Is.EqualTo(dlg));
        }

        [Test]
        public void XNADialog_Close_PopsDialogFromStack()
        {
            using var dlg = new FakeXNADialog();
            dlg.Initialize();

            dlg.CloseFake();

            Assert.That(Singleton<DialogRepository>.Instance.OpenDialogs, Is.Empty);
        }

        [Test]
        public void XNADialog_CenterInGameView_UsesTextureSourceForDimensions()
        {
            _gameManager.GraphicsDeviceManager.PreferredBackBufferWidth = 100;
            _gameManager.GraphicsDeviceManager.PreferredBackBufferHeight = 100;
            _gameManager.GraphicsDeviceManager.ApplyChanges();

            var texSource = new Rectangle(0, 0, 20, 20);
            using var tex = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 50, 50);

            using var dlg = new FakeXNADialog();
            dlg.BGText = tex;
            dlg.BGTextSource = texSource;

            dlg.CenterInGameView();

            // (100 - 20) / 2
            Assert.That(dlg.DrawArea.X, Is.EqualTo(40));
            Assert.That(dlg.DrawArea.Y, Is.EqualTo(40));
        }

        [Test]
        public void XNADialog_CenterInGameView_UsesTextureForDimensions_IfNoSourceExists()
        {
            _gameManager.GraphicsDeviceManager.PreferredBackBufferWidth = 100;
            _gameManager.GraphicsDeviceManager.PreferredBackBufferHeight = 100;
            _gameManager.GraphicsDeviceManager.ApplyChanges();

            using var tex = new Texture2D(_gameManager.GraphicsDeviceManager.GraphicsDevice, 50, 50);

            using var dlg = new FakeXNADialog();
            dlg.BGText = tex;
            dlg.BGTextSource = null;

            dlg.CenterInGameView();

            // (100 - 50) / 2
            Assert.That(dlg.DrawArea.X, Is.EqualTo(25));
            Assert.That(dlg.DrawArea.Y, Is.EqualTo(25));
        }

        [Test]
        public void XNADialog_CenterInGameView_UsesEmptyRectangle_IfNoSourceOrTextureExists()
        {
            _gameManager.GraphicsDeviceManager.PreferredBackBufferWidth = 100;
            _gameManager.GraphicsDeviceManager.PreferredBackBufferHeight = 100;
            _gameManager.GraphicsDeviceManager.ApplyChanges();

            using var dlg = new FakeXNADialog();
            dlg.BGText = null;
            dlg.BGTextSource = null;

            dlg.CenterInGameView();

            // (100 - 0) / 2
            Assert.That(dlg.DrawArea.X, Is.EqualTo(50));
            Assert.That(dlg.DrawArea.Y, Is.EqualTo(50));
        }
    }
}
