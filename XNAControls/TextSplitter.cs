using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace XNAControls
{
    public class TextSplitter
    {
        private int _lineLength;

        /// <summary>
        /// Gets or sets a string that is used as padding in front of every string (except the first)
        /// </summary>
        public string LineIndent { get; set; }

        /// <summary>
        /// Gets or sets a string that is inserted at the end of every line of text that is processed (except the last)
        /// </summary>
        public string LineEnd { get; set; }

        /// <summary>
        /// Gets or sets the string to use when hyphenating a word across lines
        /// </summary>
        public string Hyphen { get; set; }

        /// <summary>
        /// Gets or sets the text to be processed
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the number of pixels at which text will be wrapped to a new line
        /// </summary>
        public int LineLength
        {
            get => _lineLength;
            set
            {
                if (HardBreak.HasValue && HardBreak < value)
                    HardBreak = value;
                _lineLength = value;
            }
        }

        /// <summary>
        /// Gets or sets an absolute maximum width for long words without spaces
        /// </summary>
        public int? HardBreak { get; set; }

        /// <summary>
        /// Gets a value determining whether or not the text is long enough to require processing
        /// </summary>
        public bool NeedsProcessing => _textIsOverflowFunc(Text, () => LineLength);

        private SpriteFont _spriteFont;
        private BitmapFont _bitmapFont;

        public TextSplitter(string text, SpriteFont font)
        {
            Text = text;
            SetFont(font);
            ResetToDefaults();
        }

        public TextSplitter(string text, BitmapFont font)
        {
            Text = text;
            SetFont(font);
            ResetToDefaults();
        }

        /// <summary>
        /// Resets optional parameters to their default value
        /// </summary>
        public void ResetToDefaults()
        {
            LineIndent = "";
            LineEnd = "";
            Hyphen = "";
            LineLength = 200;
        }

        public void SetFont(SpriteFont newFont)
        {
            if (_bitmapFont != null)
            {
                _bitmapFont = null;
            }

            _spriteFont = newFont;
        }

        public void SetFont(BitmapFont newFont)
        {
            if (_spriteFont != null)
            {
                _spriteFont = null;
            }

            _bitmapFont = newFont;
        }

        /// <summary>
        /// Splits text into lines based on the class member parameters that have been set
        /// </summary>
        /// <returns>List of strings</returns>
        public List<string> SplitIntoLines()
        {
            var retList = new List<string>();
            var words = Text.Split(new[] { " ", "\t" }, StringSplitOptions.None).ToList();

            while (words.Count > 0)
            {
                var newlineConsumed = false;
                var nextLine = string.Empty;

                while (words.Count > 0 && !_textIsOverflowFunc(LineIndent + nextLine + LineEnd, () => LineLength))
                {
                    if (words[0].Contains("\n"))
                    {
                        newlineConsumed = true;

                        var thisLineWord = words[0][..words[0].IndexOf("\n")];
                        var nextLineWord = words[0][(words[0].IndexOf("\n") + 1)..];
                        nextLine += (nextLine.Any() && thisLineWord.Any() ? " " : string.Empty) + thisLineWord;

                        words.RemoveAt(0);
                        words.Insert(0, nextLineWord);

                        break;
                    }

                    nextLine += (nextLine.Any() ? " " : string.Empty) + words[0];
                    words.RemoveAt(0);
                }

                var extraWord = string.Empty;
                var lineEnd = LineEnd;
                if (HardBreak.HasValue)
                {
                    while (nextLine.Length > 0 && _textIsOverflowFunc(LineIndent + nextLine + LineEnd, () => HardBreak.Value))
                    {
                        extraWord += nextLine.Last();
                        nextLine = nextLine[..^1];
                    }

                    if (extraWord.Any())
                    {
                        words.Insert(0, extraWord);
                        lineEnd += Hyphen;
                    }
                }
                else if (nextLine.Contains(' ') && _textIsOverflowFunc(LineIndent + nextLine + LineEnd, () => LineLength))
                {
                    var lastWord = nextLine[(nextLine.LastIndexOf(' ') + 1)..];
                    nextLine = nextLine.Remove(nextLine.LastIndexOf(' '));

                    if (newlineConsumed)
                        lastWord += "\n";

                    words.Insert(0, lastWord);
                }

                var addString = (retList.Count > 0 ? LineIndent : string.Empty) + nextLine + (words.Count > 0 ? lineEnd : string.Empty);
                retList.Add(string.IsNullOrWhiteSpace(addString) ? string.Empty : addString);
            }

            return retList;
        }

        private bool _textIsOverflowFunc(string input, Func<int> propGetter)
        {
            return (_spriteFont != null
                ? new Func<string, bool>(input => _spriteFont.MeasureString(input).X > propGetter())
                : input => _bitmapFont.MeasureString(input).Width > propGetter()).Invoke(input);
        }
    }
}
