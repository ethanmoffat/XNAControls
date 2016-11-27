// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
    public enum LabelAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public class XNALabel : XNAControl, IXNALabel
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _whitePixel;

        private string _lastText;
        private int? _lastTextWidth;
        private readonly List<string> _drawStrings;

        /// <summary>
        /// Get or set the text to display in the label.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Get or set the Foreground Color.
        /// </summary>
        public Color ForeColor { get; set; }

        /// <summary>
        /// Get or set the Background Color. Set to 'null' to turn off.
        /// </summary>
        public Color? BackColor { get; set; }

        public bool AutoSize { get; set; }

        /// <summary>
        /// Set the alignment of the text in the label. Only evaluated when <see cref="AutoSize">AutoSize</see> is false.
        /// </summary>
        public LabelAlignment TextAlign { get; set; }

        /// <summary>
        /// Get or set the text width in pixels
        /// </summary> 
        public int? TextWidth { get; set; }

        /// <summary>
        /// Get or set the spacing between rows, in pixels. Includes the height of the text.
        /// </summary>
        public int? RowSpacing
        {
            get { return _font.LineSpacing; }
            set
            {
                if (!value.HasValue)
                    value = (int)_font.MeasureString(Text).Y + 2;
                _font.LineSpacing = value.Value;
            }
        }

        /// <summary>
        /// Get the actual width of the text as measured by the font
        /// </summary>
        public float ActualWidth
        {
            get
            {
                return _drawStrings.Count <= 1
                    ? _font.MeasureString(Text).X
                    : _drawStrings.Select(x => _font.MeasureString(x).X).Max();
            }
        }

        /// <summary>
        /// Get the actual height of the text as measured by the font
        /// </summary>
        public float ActualHeight
        {
            get
            {
                return _drawStrings.Count <= 1
                    ? _font.MeasureString(Text).Y
                    : _drawStrings.Count*_font.LineSpacing;
            }
        }

        /// <summary>
        /// Turn underlining on or off
        /// </summary>
        public bool Underline { get; set; }

        public XNALabel(string spriteFontName)
        {
            _drawStrings = new List<string>();
            Text = "";
            ForeColor = Color.Black;
            TextAlign = LabelAlignment.TopLeft;

            _font = Game.Content.Load<SpriteFont>(spriteFontName);

            _whitePixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new [] {Color.White});
        }

        public void ResizeBasedOnText(uint xPadding = 0, uint yPadding = 0)
        {
            if (_font == null || AutoSize) return;

            var sz = _font.MeasureString(Text);
            SetSize((int)Math.Round(sz.X) + (int)xPadding,
                    (int)Math.Round(sz.Y) + (int)yPadding);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_lastText != Text || _lastTextWidth != TextWidth)
            {
                _lastText = Text;
                _lastTextWidth = TextWidth;

                if (TextWidth != null)
                {
                    var ts = new TextSplitter(Text, _font) {LineLength = TextWidth.Value};
                    _drawStrings.Clear();
                    _drawStrings.AddRange(ts.SplitIntoLines());

                    if (AutoSize && _drawStrings.Any())
                    {
                        var largestWidth = _drawStrings.Select(line => _font.MeasureString(line).X).Max();
                        SetSize((int) largestWidth, _drawStrings.Count*_font.LineSpacing);
                    }
                }
                else
                {
                    _drawStrings.Clear();
                }
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            var totalTextArea = CalculateSizeOfTextArea();
            float adjustedX = 0, adjustedY = 0;
            if (!AutoSize)
            {
                var tmpVec = CalculatePositionFromAlignment(totalTextArea);
                adjustedX = tmpVec.X;
                adjustedY = tmpVec.Y;
            }

            _spriteBatch.Begin();

            DrawBackground(totalTextArea);

            if (TextWidth == null)
                DrawTextLine(Text, adjustedX, adjustedY);
            else
            {
                for (int i = 0; i < _drawStrings.Count; i++)
                {
                    var line = _drawStrings[i];
                    DrawTextLine(line, adjustedX, adjustedY + _font.LineSpacing*i);
                }
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private Vector2 CalculateSizeOfTextArea()
        {
            if (Text == null || _font == null || _drawStrings == null)
                return Vector2.Zero;

            return TextWidth == null
                ? _font.MeasureString(Text)
                : new Vector2(_drawStrings.Count > 0 ? _drawStrings.Select(line => _font.MeasureString(line).X).Max() : 1f,
                              _drawStrings.Count > 0 ? _font.LineSpacing*_drawStrings.Count : _font.LineSpacing);
        }

        private Vector2 CalculatePositionFromAlignment(Vector2 totalTextArea)
        {
            float adjustedX = 0;
            float adjustedY = 0;

            var align = Enum.GetName(typeof(LabelAlignment), TextAlign) ?? "";

            if (align.Contains("Left"))
                adjustedX = 0;
            else if (align.Contains("Center"))
                adjustedX = (int)(DrawArea.Width / 2f - totalTextArea.X / 2);
            else if (align.Contains("Right"))
                adjustedX = DrawArea.Width - totalTextArea.X;

            if (align.Contains("Top"))
                adjustedY = 0;
            else if (align.Contains("Middle"))
                adjustedY = (int)(DrawArea.Height / 2f - totalTextArea.Y / 2);
            else if (align.Contains("Bottom"))
                adjustedY = DrawArea.Height - totalTextArea.Y;

            return new Vector2(adjustedX, adjustedY);
        }

        private void DrawBackground(Vector2 totalArea)
        {
            if (!BackColor.HasValue) return;

            var location = new Vector2(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y);
            var backgroundTargetRectangle = TextWidth == null
                ? DrawAreaWithParentOffset
                : new Rectangle((int) location.X,
                    (int) location.Y,
                    (int) totalArea.X,
                    (int) totalArea.Y);

            _spriteBatch.Draw(_whitePixel, backgroundTargetRectangle, BackColor.Value);
        }

        private void DrawTextLine(string textLine, float adjustedX, float adjustedY)
        {
            if (_font == null || textLine == null)
                return;

            var textLineWidth = _font.MeasureString(textLine).X;
            var extraHeightForUnderline = _font.MeasureString(textLine).Y - 3;

            _spriteBatch.DrawString(_font,
                textLine,
                new Vector2(DrawAreaWithParentOffset.X + adjustedX, DrawAreaWithParentOffset.Y + adjustedY),
                ForeColor);

            if (Underline)
            {
                var underlineDestRect = new Rectangle(
                    (int) (DrawAreaWithParentOffset.X + adjustedX),
                    (int) (DrawAreaWithParentOffset.Y + adjustedY + extraHeightForUnderline),
                    (int) textLineWidth,
                    1);

                _spriteBatch.Draw(_whitePixel, underlineDestRect, null, ForeColor);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _whitePixel.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public interface IXNALabel : IXNAControl
    {
        /// <summary>
        /// Get or set the text to display in the label.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Get or set the Foreground Color.
        /// </summary>
        Color ForeColor { get; set; }

        /// <summary>
        /// Get or set the Background Color. Set to 'null' to turn off.
        /// </summary>
        Color? BackColor { get; set; }

        bool AutoSize { get; set; }

        /// <summary>
        /// Set the alignment of the text in the label. Only evaluated when <see cref="AutoSize">AutoSize</see> is false.
        /// </summary>
        LabelAlignment TextAlign { get; set; }

        /// <summary>
        /// Get or set the text width in pixels
        /// </summary> 
        int? TextWidth { get; set; }

        /// <summary>
        /// Get or set the spacing between rows, in pixels. Includes the height of the text.
        /// </summary>
        int? RowSpacing { get; set; }

        /// <summary>
        /// Get the actual width of the text as measured by the font
        /// </summary>
        float ActualWidth { get; }

        /// <summary>
        /// Get the actual height of the text as measured by the font
        /// </summary>
        float ActualHeight { get; }

        /// <summary>
        /// Turn underlining on or off
        /// </summary>
        bool Underline { get; set; }

        void ResizeBasedOnText(uint xPadding = 0, uint yPadding = 0);
    }
}
