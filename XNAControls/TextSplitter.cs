using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAControls
{
    /// <summary>
    /// Class for splitting text based on line length
    /// </summary>
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

        /// <summary>
        /// Construct a SpriteFont-based TextSplitter with the specified text
        /// </summary>
        public TextSplitter(string text, SpriteFont font)
        {
            Text = text;
            SetFont(font);
            ResetToDefaults();
        }

        /// <summary>
        /// Construct a BitmapFont-based TextSplitter with the specified text
        /// </summary>
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

        /// <summary>
        /// Set the font to the specified SpriteFont
        /// </summary>
        public void SetFont(SpriteFont newFont)
        {
            if (_bitmapFont != null)
            {
                _bitmapFont = null;
            }

            _spriteFont = newFont;
        }

        /// <summary>
        /// Set the font to the specified BitmapFont
        /// </summary>
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

            var buffer = new Deque<char>(Text);
            while (buffer.Any())
            {
                buffer.RemoveFromFront(out nextChar);

                if (nextChar == '\n')
                {
                    var lineEnd = buffer.Any() ? LineEnd : string.Empty;
                    ResetNextLine(nextLine + lineEnd);
                    continue;
                }

                nextLine.Append(nextChar);

                // the text in the line measures longer than LineLength
                if (_textIsOverflowFunc(nextLine + LineEnd, () => LineLength))
                {
                    bool isHardBreakOverflow = false;
                    if (HardBreak.HasValue)
                    {
                        // hard breaks are enabled: find either the next whitespace or the point where the overflow happens
                        while (buffer.Any() && !_whiteSpace.Contains(nextChar) && !(isHardBreakOverflow = _textIsOverflowFunc(nextLine + LineEnd, () => HardBreak.Value)))
                        {
                            buffer.RemoveFromFront(out nextChar);
                            nextLine.Append(nextChar);
                        }
                    }
                    else if (!_whiteSpace.Contains(nextLine[^1]))
                    {
                        // hard breaks are not enabled: take characters back from the split word until we find whitespace
                        // if there is no whitespace, leave string as-is
                        var parts = nextLine.ToString().Split(_whiteSpace, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                        {
                            foreach (var c in parts.Last().Reverse())
                                buffer.AddToFront(c);

                            nextLine = nextLine.Remove(nextLine.Length - parts.Last().Length, parts.Last().Length);
                        }
                    }

                    var lineEnd = buffer.Any() ? LineEnd : string.Empty;
                    if (isHardBreakOverflow)
                    {
                        // for hard breaks: append hyphen string to the end (not considered for length measurements)
                        ResetNextLine(nextLine + lineEnd + Hyphen);
                    }
                    else if (nextChar == '\n')
                    {
                        // for newlines: append the next line as well as an empty line
                        ResetNextLine(nextLine + lineEnd, string.Empty);
                    }
                    else
                    {
                        // for other whitespace: use the line as-is
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
