using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace XNAControls
{
	public class XNALabel : XNAControl
	{
	    /// <summary>
	    /// Get or set the text to display in the label.
	    /// </summary>
	    public string Text
	    {
	        get { return _text; }
	        set
	        {
	            _text = value;
	            if (TextWidth != null)
	            {
	                TextSplitter ts = new TextSplitter(_text, _font) { LineLength = TextWidth.Value };
	                _drawStrings.Clear();
                    _drawStrings.AddRange(ts.SplitIntoLines());

	                if (AutoSize && _drawStrings.Count > 0)
	                {
	                    float largestWidth = _drawStrings.Select(line => _font.MeasureString(line).X).Max();
	                    drawArea = new Rectangle(drawArea.X, drawArea.Y, (int) largestWidth, _drawStrings.Count*_font.LineSpacing);
	                }
	            }
	            else
	            {
	                _drawStrings.Clear();
	            }
            }
	    }

	    /// <summary>
        /// Get or set the Foreground Color.
        /// </summary>
	    public Color ForeColor { get; set; }

        /// <summary>
        /// Get or set the Background Color. Set to 'null' to turn off.
        /// </summary>
	    public Color? BackColor
	    {
	        get { return _backColor; }
	        set
	        {
	            _backColor = value;
	            if (_backColor != null)
	            {
	                if (_backgroundTexture == null)
	                    _backgroundTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
	                _backgroundTexture.SetData(new[] {_backColor.Value}, 0, 1);
	            }
	        }
	    }

        public bool AutoSize { get; set; }

        /// <summary>
        /// Set the alignment of the text in the label. Only evaluated when <see cref="AutoSize">AutoSize</see> is false.
        /// </summary>
	    public ContentAlignment TextAlign { get; set; }

	    /// <summary>
	    /// Get or set the text width in pixels
	    /// </summary> 
	    public int? TextWidth
	    {
	        get { return _textWidth; }
	        set
	        {
	            if (_textWidth.HasValue != value.HasValue)
                {
                    _textWidth = value;
                    Text = Text; //regenerate
	            }
	            else
	            {
                    _textWidth = value;
                }
	        }
	    }

	    /// <summary>
	    /// Get or set the spacing between rows, in pixels. Includes the height of the text.
	    /// </summary>
	    public int? RowSpacing { get; set; }

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

        private readonly SpriteFont _font;
        private Color? _backColor;
        private Texture2D _backgroundTexture;
	    private Texture2D _underlineTexture;

	    private string _text;
	    private readonly List<string> _drawStrings;
	    private int? _textWidth;

	    public XNALabel(Rectangle area, string spriteFontName)
			: base(new Vector2(area.X, area.Y), area)
        {
            _drawStrings = new List<string>();
            Text = "";
            ForeColor = Color.Black;
            TextAlign = ContentAlignment.TopLeft;

            _font = Game.Content.Load<SpriteFont>(spriteFontName);

            _underlineTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
	        _underlineTexture.SetData(new [] {Color.White});
        }

	    public override void Update(GameTime gameTime)
		{
			if (!Visible || !ShouldUpdate())
				return;

	        if (RowSpacing.HasValue)
	            _font.LineSpacing = RowSpacing.Value;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();

            Vector2 pt = new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y);
		    var size = TextWidth == null
		                          ? _font.MeasureString(Text)
		                          : new Vector2(_drawStrings.Count > 0 ? _drawStrings.Select(line => _font.MeasureString(line).X).Max() : 1f,
		                                        _drawStrings.Count > 0 ? _font.LineSpacing*_drawStrings.Count : _font.LineSpacing);
            float x = 0, y = 0;
            if (!AutoSize)
            {
                // Figure out alignment
                string align = Enum.GetName(typeof(ContentAlignment), TextAlign) ?? "";

                if (align.Contains("Left"))
                    x = 0;
                else if (align.Contains("Center"))
                    x = DrawArea.Width / 2f - size.X / 2;
                else
                    x = DrawArea.Width - size.X;

                if (align.Contains("Top"))
                    y = 0;
                else if (align.Contains("Middle"))
                    y = DrawArea.Height / 2f - size.Y / 2;
                else
                    y = DrawArea.Height - size.Y;
            }

		    if (BackColor.HasValue && _backgroundTexture != null)
		        SpriteBatch.Draw(_backgroundTexture, 
                                 TextWidth == null
                                            ? DrawAreaWithOffset
		                                    : new Rectangle((int) pt.X, (int) pt.Y, (int) size.X, (int) size.Y),
                                 Color.White);

            if (TextWidth == null)
		    {
		        SpriteBatch.DrawString(_font,
		            Text,
		            new Vector2(DrawAreaWithOffset.X + x, DrawAreaWithOffset.Y + y),
		            ForeColor);

		        if (Underline)
		        {
		            SpriteBatch.Draw(_underlineTexture,
		                             new Rectangle((int) (DrawAreaWithOffset.X + x), (int) (DrawAreaWithOffset.Y + y), (int) size.X, 2),
		                             null,
		                             ForeColor);
		        }
		    }
		    else
		    {
		        for (int i = 0; i < _drawStrings.Count; i++)
		        {
		            var line = _drawStrings[i];

		            SpriteBatch.DrawString(_font,
		                line,
		                new Vector2(DrawAreaWithOffset.X + x, DrawAreaWithOffset.Y + y + _font.LineSpacing * i),
		                ForeColor);

		            if (Underline)
		            {
		                SpriteBatch.Draw(_underlineTexture,
		                                 new Rectangle((int) (DrawAreaWithOffset.X + x),
		                                     (int) (DrawAreaWithOffset.Y + y + _font.LineSpacing*i),
		                                     (int) _font.MeasureString(line).X, 2),
		                                 null,
		                                 ForeColor);
		            }
		        }
		    }

		    SpriteBatch.End();

			base.Draw(gameTime);
		}

	    /// <summary>
	    /// Resizes the label using the current font to fit the current text
	    /// <para>Not valid for when <see cref="AutoSize">AutoSize</see> is true</para>
	    /// </summary>
	    /// <param name="x_padding">Total extra space to add to the new width, in pixels</param>
	    /// <param name="y_padding">Total extra space to add to the new height, in pixels</param>
	    public void ResizeBasedOnText(uint x_padding = 0, uint y_padding = 0)
	    {
	        if (_font == null || AutoSize) return;

	        var sz = _font.MeasureString(Text);
	        drawArea = new Rectangle(DrawArea.X,
	            DrawArea.Y,
	            (int) Math.Round(sz.X) + (int) x_padding,
	            (int) Math.Round(sz.Y) + (int) y_padding);
	    }

	    protected override void Dispose(bool disposing)
	    {
	        if (disposing)
	        {
	            if (_backgroundTexture != null)
	            {
	                _backgroundTexture.Dispose();
	                _backgroundTexture = null;
	            }
	            if (_underlineTexture != null)
	            {
	                _underlineTexture.Dispose();
	                _underlineTexture = null;
	            }
	        }
	        base.Dispose(disposing);
	    }
	}
}
