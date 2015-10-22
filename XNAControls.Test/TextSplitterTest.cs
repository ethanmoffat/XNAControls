﻿// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls.Test
{
	[TestClass]
	public class TextSplitterTest
	{
		private TextSplitter _ts;

		[TestInitialize]
		public void TestInitialize()
		{
			_ts = new TextSplitter("", LoadSpriteFontFromWorkingDirectory());
		}

		[TestMethod]
		[Timeout(2000)]
		public void GivenShortMessage_WhenSplittingAt200px_NeedProcessingShouldBeFalse()
		{
			_ts.Text = "Test Message";
			_ts.LineLength = 200;

			Assert.IsFalse(_ts.NeedsProcessing);
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

			Assert.AreEqual(4, result.Count);
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

		private SpriteFont LoadSpriteFontFromWorkingDirectory()
		{
			SpriteFont font;

			using (var form = new Form())
			{
				var gds = GraphicsDeviceService.AddRef(form.Handle, form.ClientSize.Width, form.ClientSize.Height);

				using (var services = new ServiceContainer())
				{
					services.AddService(typeof(IGraphicsDeviceService), gds);
					var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					using (var content = new ContentManager(services, path))
					{
						font = content.Load<SpriteFont>("font_for_testing");
					}
				}
			}

			return font;
		}
	}
}
