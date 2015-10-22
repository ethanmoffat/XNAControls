// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
	public class TextSplitter
	{
		/// <summary>
		/// Gets or sets a string that is used as padding in front of every string (except the first)
		/// </summary>
		public string LineIndent { get; set; }
		/// <summary>
		/// Gets or sets a string that is inserted at the end of every line of text that is processed (except the last)
		/// </summary>
		public string LineEnd { get; set; }
		/// <summary>
		/// Gets or sets the text to be processed
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Gets or sets the number of pixels at which text will be wrapped to a new line
		/// </summary>
		public int LineLength { get; set; }
		/// <summary>
		/// Gets a value determining whether or not the text is long enough to require processing
		/// </summary>
		public bool NeedsProcessing { get { return _textIsOverflowFunc(Text); } }

		private SpriteFont _spriteFont;

		public TextSplitter(string text, SpriteFont font)
		{
			Text = text;
			SetFont(font);
			ResetToDefaults();
		}

		/// <summary>
		/// Resets optional parameters to their default value
		/// </summary>
// ReSharper disable once MemberCanBePrivate.Global
		public void ResetToDefaults()
		{
			LineIndent = "";
			LineEnd = "";
			LineLength = 200;
		}

// ReSharper disable once MemberCanBePrivate.Global
		public void SetFont(SpriteFont newFont)
		{
			_spriteFont = newFont;
		}

		/// <summary>
		/// Splits text into lines based on the class member parameters that have been set
		/// </summary>
		/// <returns>List of strings</returns>
		public List<string> SplitIntoLines()
		{
			string buffer = Text;
			buffer = buffer.TrimEnd(' ');
			List<string> retList = new List<string>();
			char[] whiteSpace = {' ', '\t', '\n'};
			string nextWord = "", newLine = "";
			while (buffer.Length > 0) //keep going until the buffer is empty
			{
				//get the next word
				bool endOfWord = true, lineOverFlow = true; //these are negative logic booleans: will be set to false when flagged
				while (buffer.Length > 0 && !(endOfWord = whiteSpace.Contains(buffer[0])) &&
					   !(lineOverFlow = _textIsOverflowFunc(newLine + nextWord + LineEnd)))
				{
					nextWord += buffer[0];
					buffer = buffer.Remove(0, 1);
				}

				if (endOfWord)
				{
					newLine += nextWord + buffer[0];
					buffer = buffer.Remove(0, 1);
					nextWord = "";
				}
				else if (lineOverFlow)
				{
					if (nextWord.Length > 0)
					{
						if (newLine == LineIndent)
						{
							newLine += nextWord;
							nextWord = "";
						}
					}

					if (newLine.Contains('\n'))
					{
						retList.AddRange(newLine.Split('\n').Select(x => x + (x.Length > 0 ? LineEnd : "")));
					}
					else
					{
						newLine += LineEnd;
						retList.Add(newLine);
					}
					newLine = LineIndent;
				}
				else
				{
					newLine += nextWord;
					retList.Add(newLine);
				}
			}

			return retList;
		}

		private bool _textIsOverflowFunc(string toMeasure)
		{
			return _spriteFont.MeasureString(toMeasure).X > LineLength;
		}
	}
}
