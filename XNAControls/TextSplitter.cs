using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAControls
{
    public class TextSplitter
    {
        private static readonly char[] _whiteSpace = new[] { ' ', '\t', '\n' };

        private int _lineLength;

        /// <summary>
        /// Gets or sets a string that is used as padding in front of every string (except the first)
        /// </summary>
        public string LineIndent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string that is inserted at the end of every line of text that is processed (except the last)
        /// </summary>
        public string LineEnd { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the string to use when hyphenating a word across lines
        /// </summary>
        public string Hyphen { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text to be processed
        /// </summary>
        public string Text { get; set; } = string.Empty;

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

            var nextLine = new StringBuilder();
            var nextChar = '\0';

            var buffer = new Queue<char>(Text);
            while (buffer.Any())
            {
                nextChar = buffer.Dequeue();

                if (nextChar == '\n')
                {
                    var lineEnd = buffer.Any() ? LineEnd : string.Empty;
                    ResetNextLine(nextLine + lineEnd);
                    continue;
                }

                nextLine.Append(nextChar);

                if (_textIsOverflowFunc(nextLine + LineEnd, () => LineLength))
                {
                    bool isHardBreakOverflow = false;
                    while (buffer.Any() && !_whiteSpace.Contains(nextChar) && !(HardBreak.HasValue && (isHardBreakOverflow = _textIsOverflowFunc(nextLine + LineEnd, () => HardBreak.Value))))
                    {
                        nextChar = buffer.Dequeue();
                        nextLine.Append(nextChar);
                    }

                    var lineEnd = buffer.Any() ? LineEnd : string.Empty;
                    if (isHardBreakOverflow)
                    {
                        ResetNextLine(nextLine + lineEnd + Hyphen);
                    }
                    else if (nextChar == '\n')
                    {
                        ResetNextLine(nextLine + lineEnd, string.Empty);
                    }
                    else
                    {
                        ResetNextLine(nextLine + lineEnd);
                    }
                }
            }

            if (nextChar == '\n' || nextLine.Length > 0)
                retList.Add(nextLine.ToString());

            return retList;

            void ResetNextLine(params string[] toAdd)
            {
                retList.AddRange(toAdd);
                nextLine = new StringBuilder(LineIndent ?? string.Empty);
            }
        }

        private bool _textIsOverflowFunc(string input, Func<int> propGetter)
        {
            return (_spriteFont != null
                ? new Func<string, bool>(input => _spriteFont.MeasureString(input).X > propGetter())
                : input => _bitmapFont.MeasureString(input).Width > propGetter()).Invoke(input);
        }
    }
}
