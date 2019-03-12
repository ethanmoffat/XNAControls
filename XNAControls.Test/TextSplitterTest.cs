// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using XNAControls.Test.Helpers;

namespace XNAControls.Test
{
    [TestFixture]
    public class TextSplitterTest
    {
        private TextSplitter _ts;
        private static TestGameManager _gameManager;
        private static SpriteFont _spriteFont;

        [OneTimeSetUp]
        public static void ClassInitialize()
        {
            _gameManager = new TestGameManager();
            _spriteFont = LoadSpriteFontFromWorkingDirectory();
        }

        [OneTimeTearDown]
        public static void ClassCleanup()
        {
            _gameManager?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _ts = new TextSplitter("", _spriteFont);
        }

        [Test]
        [Timeout(2000)]
        public void GivenShortMessage_WhenSplittingAt200px_NeedProcessingShouldBeFalse()
        {
            _ts.Text = "Test Message";
            _ts.LineLength = 200;

            Assert.IsFalse(_ts.NeedsProcessing);
        }

        [Test]
        [Timeout(2000)]
        public void GivenShortMessage_WhenSplittingAt200px_ShouldBeOneLineOfText()
        {
            _ts.Text = "Test Message";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageWithLongWords1_WhenSplittingAt200px_ShouldBe3LinesOfText()
        {
            _ts.Text = "Testttttttttttttttttttttttttttttttttttttttttttttttttttt messageeeeeeeeeeeeeeeeeeeeeeeeee oneee";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageWithLongWords2_WhenSplittingAt200px_ShouldBe5LinesOfText()
        {
            _ts.Text = "Test messageeeeeeeeeeeeeeeeeeeeeeeeee oneeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(4, result.Count);
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageWithShortWords_WhenSplittingAt200px_ShouldBe3LinesOfText()
        {
            _ts.Text = "This is a test message in which the words should be able to easily be split into multiple lines";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageAndLongWordsAndLineEnd_WhenSplittingAt200px_ShouldEndEachLineExceptLastWithLineEnd()
        {
            _ts.Text = "Test messageeeeeeeeeeeeeeeeeeeeeeeeee oneeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            _ts.LineLength = 200;
            _ts.LineEnd = "---";

            var result = _ts.SplitIntoLines();

            Assert.IsTrue(result.Except(new[] {result.Last()}).All(x => x.EndsWith(_ts.LineEnd)));
            Assert.IsFalse(result.Last().EndsWith(_ts.LineEnd));
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageAndShortWordsAndLineEnd_WhenSplittingAt200px_ShouldEndEachLineExceptLastWithLineEnd()
        {
            _ts.Text = "This is a test message in which the words should be able to easily be split into multiple lines";
            _ts.LineLength = 200;
            _ts.LineEnd = "---";

            var result = _ts.SplitIntoLines();

            Assert.IsTrue(result.Except(new[] { result.Last() }).All(x => x.EndsWith(_ts.LineEnd)));
            Assert.IsFalse(result.Last().EndsWith(_ts.LineEnd));
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageWithNewLinesAndShortWordsAndLineEnd_WhenSplittingAt200px_ShouldEndEachLineExceptLastWithLineEnd()
        {
            _ts.Text = "This is a test message \nin which the words should \n\n\nbe able to easily be \nsplit into multiple\n\n lines";
            _ts.LineLength = 200;
            _ts.LineEnd = "---";

            var result = _ts.SplitIntoLines();

            Assert.IsTrue(result.Except(new[] { result.Last() }).All(x => x.EndsWith(_ts.LineEnd)));
            Assert.IsFalse(result.Last().EndsWith(_ts.LineEnd));
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageAndLongWordsAndLineIndent_WhenSplittingAt200px_ShouldStartEachLineExceptFirstWithLineIndent()
        {
            _ts.Text = "Test messageeeeeeeeeeeeeeeeeeeeeeeeee oneeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            _ts.LineLength = 200;
            _ts.LineIndent = "        ";

            var result = _ts.SplitIntoLines();

            Assert.IsFalse(result.First().StartsWith(_ts.LineIndent));
            Assert.IsTrue(result.Except(new[] { result.First() }).All(x => x.StartsWith(_ts.LineIndent)));
        }

        [Test]
        [Timeout(2000)]
        public void GivenLongMessageAndShortWordsAndLineIndent_WhenSplittingAt200px_ShouldStartEachLineExceptFirstWithLineIndent()
        {
            _ts.Text = "This is a test message in which the words should be able to easily be split into multiple lines";
            _ts.LineLength = 200;
            _ts.LineIndent = "        ";

            var result = _ts.SplitIntoLines();

            Assert.IsFalse(result.First().StartsWith(_ts.LineIndent));
            Assert.IsTrue(result.Except(new[] { result.First() }).All(x => x.StartsWith(_ts.LineIndent)));
        }

        private static SpriteFont LoadSpriteFontFromWorkingDirectory()
        {
            using (var content = new ContentManager(_gameManager.Game.Services, TestContext.CurrentContext.TestDirectory))
            {
                return content.Load<SpriteFont>("font_for_testing");
            }
        }
    }
}
