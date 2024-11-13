using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace XNAControls
{
    /// <summary>
    /// Controls text alignment of a label control
    /// </summary>
    [Flags]
    public enum LabelAlignment
    {
        /// <inheritdoc />
        Top = 1,
        /// <inheritdoc />
        Middle = 2,
        /// <inheritdoc />
        Bottom = 4,
        /// <inheritdoc />
        Left = 8,
        /// <inheritdoc />
        Right = 16,
        /// <inheritdoc />
        Center = 32,

        /// <inheritdoc />
        TopLeft = Top | Left,
        /// <inheritdoc />
        MiddleLeft = Middle | Left,
        /// <inheritdoc />
        BottomLeft = Bottom | Left,
        /// <inheritdoc />
        TopCenter = Top | Center,
        /// <inheritdoc />
        MiddleCenter = Middle | Center,
        /// <inheritdoc />
        BottomCenter = Bottom | Center,
        /// <inheritdoc />
        TopRight = Top | Right,
        /// <inheritdoc />
        MiddleRight = Middle | Right,
        /// <inheritdoc />
        BottomRight = Bottom | Right
    }

    /// <summary>
    /// Controls the text wrapping behavior of a label control
    /// </summary>
    public enum WrapBehavior
    {
        /// <summary>
        /// Default behavior. Wrap long text to the next line.
        /// </summary>
        WrapToNewLine,
        /// <summary>
        /// Scroll the text horizontally when width is exceeded.
        /// </summary>
        ScrollText,
    }

    /// <summary>
    /// Represents a text label control
    /// </summary>
    public class XNALabel : XNAControl, IXNALabel
    {
        private readonly string _spriteFontName;
        private readonly List<string> _drawStrings;

        private bool _isBitmapFont;
        private SpriteFont _sFont;
        private BitmapFont _bFont;
        private Texture2D _whitePixel;

        private string _lastText;
        private int? _lastTextWidth;

        private string _actualText;
        private string _displayText;

        private int? _rowHeight;

        private Vector2 _alignmentOffset, _totalTextArea;

        /// <inheritdoc />
        public string Text
        {
            get => _actualText;
            set
            {
                _actualText = value;
                switch (WrapBehavior)
                {
                    case WrapBehavior.WrapToNewLine: _displayText = _actualText; break;
                    case WrapBehavior.ScrollText:
                        {
                            _displayText = _actualText;
                            if (TextWidth.HasValue)
                            {
                                while (MeasureString(_displayText).X > TextWidth)
                                    _displayText = _displayText[1..];
                            }
                        }
                        break;
                }
            }
        }

        /// <inheritdoc />
        public Color ForeColor { get; set; }

        /// <inheritdoc />
        public Color BackColor { get; set; }

        /// <inheritdoc />
        public bool AutoSize { get; set; }

        /// <inheritdoc />
        public LabelAlignment TextAlign { get; set; }

        /// <inheritdoc />
        public int? TextWidth { get; set; }

        /// <inheritdoc />
        public int? HardBreak { get; set; }

        /// <inheritdoc />
        public string Hyphen { get; set; } = string.Empty;

        /// <inheritdoc />
        public WrapBehavior WrapBehavior { get; set; }

        /// <inheritdoc />
        public int? RowSpacing
        {
            get => _rowHeight;
            set
            {
                if (!value.HasValue)
                    value = (int)MeasureString(Text).Y + 2;
                _rowHeight = value.Value;
            }
        }

        /// <inheritdoc />
        public float ActualWidth
        {
            get
            {
                return _drawStrings.Count <= 1
                    ? MeasureString(Text).X
                    : _drawStrings.Max(x => MeasureString(x).X);
            }
        }

        /// <inheritdoc />
        public float ActualHeight =>
            _drawStrings.Count <= 1
                ? MeasureString(Text).Y
                : _drawStrings.Count * LineHeight;

        internal Vector2 AdjustedDrawPosition => DrawPositionWithParentOffset + (AutoSize ? Vector2.Zero : _alignmentOffset);

        /// <inheritdoc />
        public bool Underline { get; set; }

        /// <summary>
        /// The height of each text line, as measured by the loaded font
        /// </summary>
        protected int LineHeight => _isBitmapFont ? _bFont.LineHeight : _sFont.LineSpacing;

        /// <summary>
        /// The list of strings as split by the label's properties and measured by the font in use
        /// </summary>
        protected IReadOnlyList<string> DrawStrings => _drawStrings;

        /// <summary>
        /// Create a new label control with the given sprite font name (content name)
        /// </summary>
        public XNALabel(string spriteFontName)
        {
            _spriteFontName = spriteFontName;
            _drawStrings = new List<string>();

            Text = "";
            _lastText = null;

            ForeColor = Color.Black;

            TextAlign = LabelAlignment.TopLeft;
            _alignmentOffset = CalculatePositionFromAlignment();
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            _whitePixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            var contentFile = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Path.Combine("Contents", "Resources", Game.Content.RootDirectory, $"{_spriteFontName}.xnb")
                : Path.Combine(Game.Content.RootDirectory, $"{_spriteFontName}.xnb");

            using var st = File.OpenRead(contentFile);
            using var br = new BinaryReader(st, Encoding.UTF8);

            var importerLen = br.Read7BitEncodedInt();
            var importer = Encoding.UTF8.GetString(br.ReadBytes(importerLen));

            _isBitmapFont = importer.Contains("MonoGame.Extended.BitmapFonts.BitmapFontReader");

            if (_isBitmapFont)
            {
                _bFont = Game.Content.Load<BitmapFont>(_spriteFontName);
            }
            else
            {
                _sFont = Game.Content.Load<SpriteFont>(_spriteFontName);
            }

            _rowHeight ??= LineHeight;

            base.LoadContent();
        }

        /// <inheritdoc />
        public void ResizeBasedOnText(uint xPadding = 0, uint yPadding = 0)
        {
            if ((_sFont == null && _bFont == null) || AutoSize) return;

            var sz = MeasureString(Text);
            SetSize((int)Math.Round(sz.X) + (int)xPadding,
                    (int)Math.Round(sz.Y) + (int)yPadding);
        }

        /// <inheritdoc />
        protected override void OnUnconditionalUpdateControl(GameTime gameTime)
        {
            if (_lastText != Text || _lastTextWidth != TextWidth)
            {
                _lastText = Text;
                _lastTextWidth = TextWidth;

                if (TextWidth != null && WrapBehavior == WrapBehavior.WrapToNewLine)
                {
                    var ts = _isBitmapFont
                        ? new TextSplitter(Text, _bFont) { LineLength = TextWidth.Value, HardBreak = HardBreak, Hyphen = Hyphen }
                        : new TextSplitter(Text, _sFont) { LineLength = TextWidth.Value, HardBreak = HardBreak, Hyphen = Hyphen };
                    _drawStrings.Clear();
                    _drawStrings.AddRange(ts.SplitIntoLines());

                    if (AutoSize && _drawStrings.Any())
                    {
                        var largestWidth = _drawStrings.Max(line => MeasureString(line).X);
                        SetSize((int)largestWidth, _drawStrings.Count * LineHeight);
                    }

                    OnWrappedTextUpdated();
                }
                else
                {
                    _drawStrings.Clear();
                }
            }

            _totalTextArea = CalculateSizeOfTextArea();
            _alignmentOffset = CalculatePositionFromAlignment();

            base.OnUnconditionalUpdateControl(gameTime);
        }

        private Vector2 CalculatePositionFromAlignment()
        {
            float adjustedX = 0;
            float adjustedY = 0;

            if ((TextAlign & LabelAlignment.Left) == LabelAlignment.Left)
                adjustedX = 0;
            else if ((TextAlign & LabelAlignment.Center) == LabelAlignment.Center)
                adjustedX = (int)(DrawArea.Width / 2f - _totalTextArea.X / 2);
            else if ((TextAlign & LabelAlignment.Right) == LabelAlignment.Right)
                adjustedX = DrawArea.Width - _totalTextArea.X;

            if ((TextAlign & LabelAlignment.Top) == LabelAlignment.Top)
                adjustedY = 0;
            else if ((TextAlign & LabelAlignment.Middle) == LabelAlignment.Middle)
                adjustedY = (int)(DrawArea.Height / 2f - _totalTextArea.Y / 2);
            else if ((TextAlign & LabelAlignment.Bottom) == LabelAlignment.Bottom)
                adjustedY = DrawArea.Height - _totalTextArea.Y;

            return new Vector2(adjustedX, adjustedY);
        }

        private Vector2 CalculateSizeOfTextArea()
        {
            if (_sFont == null && _bFont == null)
                return Vector2.Zero;

            if (string.IsNullOrEmpty(Text) || _drawStrings == null || !_drawStrings.Any())
                return MeasureString(Text);

            return TextWidth == null
                ? MeasureString(Text)
                : WrapBehavior == WrapBehavior.WrapToNewLine
                    ? new Vector2(_drawStrings.Count > 0 ? _drawStrings.Select(line => MeasureString(line).X).Max() : 1f,
                                  _drawStrings.Count > 0 ? LineHeight * _drawStrings.Count : LineHeight)
                    : new Vector2(TextWidth.Value, MeasureString(Text).Y);
        }

        /// <inheritdoc />
        protected override void OnDrawControl(GameTime gameTime)
        {
            float adjustedX = 0, adjustedY = 0;
            if (!AutoSize)
            {
                adjustedX = _alignmentOffset.X;
                adjustedY = _alignmentOffset.Y;
            }

            _spriteBatch.Begin();

            DrawBackground();

            if (TextWidth == null)
            {
                DrawTextLine(Text, adjustedX, adjustedY);
            }
            else if (WrapBehavior == WrapBehavior.WrapToNewLine)
            {
                DrawMultiLine(adjustedX, adjustedY);
            }
            else if (WrapBehavior == WrapBehavior.ScrollText)
            {
                DrawTextLine(_displayText, adjustedX, adjustedY);
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private void DrawBackground()
        {
            var location = new Vector2(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y);
            var backgroundTargetRectangle = TextWidth == null
                ? DrawAreaWithParentOffset
                : new Rectangle((int) location.X,
                    (int) location.Y,
                    (int) _totalTextArea.X,
                    (int) _totalTextArea.Y);

            _spriteBatch.Draw(_whitePixel, backgroundTargetRectangle, BackColor);
        }

        /// <summary>
        /// Handler to draw multiline text strings. Default implementation internally calls DrawTextLine. Used when wrap behavior is WrapToNewLine.
        /// </summary>
        /// <param name="adjustedX">Adjusted X coordinate based on <see cref="TextAlign"/> property</param>
        /// <param name="adjustedY">Adjusted Y coordinate based on <see cref="TextAlign"/> property</param>
        protected virtual void DrawMultiLine(float adjustedX, float adjustedY)
        {
            for (int i = 0; i < _drawStrings.Count; i++)
            {
                var line = _drawStrings[i];
                DrawTextLine(line, adjustedX, adjustedY + (RowSpacing ?? LineHeight) * i);
            }
        }

        /// <summary>
        /// Handler for drawing a single line of text
        /// </summary>
        /// <param name="textLine">The text to draw</param>
        /// <param name="adjustedX">Adjusted X coordinate based on <see cref="TextAlign"/> property</param>
        /// <param name="adjustedY">Adjusted Y coordinate based on <see cref="TextAlign"/> property</param>
        protected virtual void DrawTextLine(string textLine, float adjustedX, float adjustedY)
        {
            if ((_sFont == null && _bFont == null) || textLine == null)
                return;

            var textLineWidth = MeasureString(textLine).X;
            var extraHeightForUnderline = MeasureString(textLine).Y;

            if (_isBitmapFont)
            {
                _spriteBatch.DrawString(_bFont,
                    textLine,
                    new Vector2(DrawAreaWithParentOffset.X + adjustedX, DrawAreaWithParentOffset.Y + adjustedY),
                    ForeColor);
            }
            else
            {
                _spriteBatch.DrawString(_sFont,
                    textLine,
                    new Vector2(DrawAreaWithParentOffset.X + adjustedX, DrawAreaWithParentOffset.Y + adjustedY),
                    ForeColor);
            }

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

        /// <summary>
        /// Measures the input string using the current font
        /// </summary>
        protected Vector2 MeasureString(string input)
        {
            var measureFunc = _isBitmapFont
                ? new Func<string, Vector2>(s => (Vector2)_bFont.MeasureString(s))
                : _sFont.MeasureString;

            if (string.IsNullOrEmpty(input))
                return new Vector2(0, measureFunc("L").Y);

            return measureFunc(input);
        }

        /// <summary>
        /// Called when <see cref="WrapBehavior"/> is <see cref="WrapBehavior.WrapToNewLine"/> and the text is changed in an Update() call
        /// </summary>
        protected virtual void OnWrappedTextUpdated()
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            PrepareForDisposal();

            if (disposing)
            {
                if (_whitePixel != null)
                {
                    _whitePixel.Dispose();
                    _whitePixel = null;
                }
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Interface for a text label control
    /// </summary>
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
        /// Get or set the Background Color. Use Color.Transparent for no color
        /// </summary>
        Color BackColor { get; set; }

        /// <summary>
        /// Get or set the control's size state. When true, label is automatically sized to text content
        /// </summary>
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
        /// Get or set the maximum text width for hard breaks (long words that should be force-wrapped with a hyphen)
        /// </summary>
        public int? HardBreak { get; set; }

        /// <summary>
        /// Get or set the hyphen character used when a hard break is applied
        /// </summary>
        public string Hyphen { get; set; }

        /// <summary>
        /// Get or set the behavior of wrapping when text width is exceeded
        /// </summary>
        WrapBehavior WrapBehavior { get; set; }

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

        /// <summary>
        /// Resize the label based on the contained text, with optional padding
        /// </summary>
        /// <param name="xPadding">Number of pixels to pad in the X direction (horizontal)</param>
        /// <param name="yPadding">Number of pixels to pad in the Y direction (vertical)</param>
        void ResizeBasedOnText(uint xPadding = 0, uint yPadding = 0);
    }
}
