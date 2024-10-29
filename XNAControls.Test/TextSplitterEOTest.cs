using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.BitmapFonts;
using NUnit.Framework;
using XNAControls.Test.Helpers;

namespace XNAControls.Test
{
    [TestFixture]
    public class TextSplitterEOTest
    {
        private TextSplitter _ts;
        private static TestGameManager _gameManager;
        private static BitmapFont _font;

        [OneTimeSetUp]
        public static void ClassInitialize()
        {
            _gameManager = new TestGameManager();
            _font = LoadBitmapFontFromWorkingDirectory();
        }

        [OneTimeTearDown]
        public static void ClassCleanup()
        {
            _gameManager?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _ts = new TextSplitter("", _font)
            {
                LineLength = 102,
                HardBreak = 150,
                Hyphen = "-"
            };
        }

        [Test]
        [CancelAfter(1000)]
        public void SordieMacroDisplaysCorrectly()
        {
            _ts.Text = @"  ___                  _ _    /  __|  __ _ _ __|  (_)__  \__ \/ _ \ '_/  _`  | / -_)  |___/\__/_| \__,_|,\__|";

            var expected = new[]
            {
                @"  ___                  _ _   ",
                @" /  __|  __ _ _ __|  (_)__ ",
                @" \__ \/ _ \ '_/  _`  | / -_) ",
                @" |___/\__/_| \__,_|,\__|"
            };

            var actual = _ts.SplitIntoLines();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [CancelAfter(1000)]
        public void ByeMacroDisplaysCorrectly()
        {
            _ts.Text = @" ___               Byes     |  _  )_   _ ___  Byes   |  _  \ |_| /  - _)   Byes  |___/\_, \___|     Byes         |__/";

            var expected = new[]
            {
                @" ___               Byes     ",
                @"|  _  )_   _ ___  Byes   ",
                @"|  _  \ |_| /  - _)   Byes ",
                @" |___/\_, \___|     Byes ",
                @"        |__/"
            };

            var actual = _ts.SplitIntoLines();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [CancelAfter(1000)]
        public void STFUMacroDisplaysCorrectly()
        {
            _ts.Text = @"    __  ___  ___             /__`    |    |__    |      |    .__/   |    |        \__/    .";

            var expected = new[]
            {
                @"    __  ___  ___          ",
                @"   /__`    |    |__    |      | ",
                @"   .__/   |    |        \__/ ",
                @"   .",
            };

            var actual = _ts.SplitIntoLines();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [CancelAfter(1000)]
        public void LoveMacroDisplaysCorrectly()
        {
            _ts.Text = @" ,d88b.d88b,               8888888888    Sordie  Y8888888Y    Loves     Y8888Y        You            ''Y''\";

            var expected = new[]
            {
                @" ,d88b.d88b,              ",
                @" 8888888888    Sordie ",
                @" Y8888888Y    Loves ",
                @"    Y8888Y        You   ",
                @"         ''Y''\"
            };

            var actual = _ts.SplitIntoLines();

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static BitmapFont LoadBitmapFontFromWorkingDirectory()
        {
            using (var content = new ContentManager(_gameManager.Game.Services, TestContext.CurrentContext.TestDirectory))
            {
                return content.Load<BitmapFont>("bmp_font_for_testing");
            }
        }
    }
}
