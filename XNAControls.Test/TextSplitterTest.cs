using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XNAControls.Test
{
    [TestClass]
    public class TextSplitterTest
    {
        private const string FONT_FAMILY = "Microsoft Sans Serif";
        private const float FONT_SIZE = 8.5f;

        private TextSplitter _ts;
        private Font _font;

        [TestInitialize]
        public void TestInitialize()
        {
            _font = new Font(FONT_FAMILY, FONT_SIZE);
            _ts = new TextSplitter("", _font);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _font.Dispose();
        }

        [TestMethod]
        [Timeout(2000)]
        public void GivenShortMessage_WhenSplittingAt200px_NeedProcessingShouldBeFalse()
        {
            _ts.Text = "Test Message";
            _ts.LineLength = 200;

            Assert.AreEqual(false, _ts.NeedsProcessing);
        }

        [TestMethod]
        [Timeout(2000)]
        public void GivenShortMessage_WhenSplittingAt200px_ShouldBeOneLineOfText()
        {
            _ts.Text = "Test Message";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [Timeout(2000)]
        public void GivenLongMessageWithLongWords1_WhenSplittingAt200px_ShouldBe3LinesOfText()
        {
            _ts.Text = "Testttttttttttttttttttttttttttttttttttttttttttttttttttt messageeeeeeeeeeeeeeeeeeeeeeeeee oneee";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        [Timeout(2000)]
        public void GivenLongMessageWithLongWords2_WhenSplittingAt200px_ShouldBe5LinesOfText()
        {
            _ts.Text = "Test messageeeeeeeeeeeeeeeeeeeeeeeeee oneeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(5, result.Count);
        }

        [TestMethod]
        [Timeout(2000)]
        public void GivenLongMessageWithShortWords_WhenSplittingAt200px_ShouldBe3LinesOfText()
        {
            _ts.Text = "This is a test message in which the words should be able to easily be split into multiple lines";
            _ts.LineLength = 200;

            var result = _ts.SplitIntoLines();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        //[Timeout(2000)]
        public void GivenLongMessageAndLongWordsAndLineIndent_WhenSplittingAt200px_ShouldStartEachLineExceptFirstWithLineIndent()
        {
            _ts.Text = "Test messageeeeeeeeeeeeeeeeeeeeeeeeee oneeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
            _ts.LineLength = 200;
            _ts.LineIndent = "        ";

            var result = _ts.SplitIntoLines();

            Assert.IsFalse(result.First().StartsWith(_ts.LineIndent));
            Assert.IsTrue(result.Except(new[] { result.First() }).All(x => x.StartsWith(_ts.LineIndent)));
        }

        [TestMethod]
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
    }
}
