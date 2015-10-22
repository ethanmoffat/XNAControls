// Original Work Copyright (c) Ethan Moffat 2014-2015
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
		BottomRight,
	}

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
		public LabelAlignment TextAlign { get; set; }

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
			TextAlign = LabelAlignment.TopLeft;

			_font = Game.Content.Load<SpriteFont>(spriteFontName);

			_underlineTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
			_underlineTexture.SetData(new [] {Color.White});
		}

		public void ResizeBasedOnText(uint x_padding = 0, uint y_padding = 0)
		{
			if (_font == null || AutoSize) return;

			var sz = _font.MeasureString(Text);
			drawArea = new Rectangle(DrawArea.X,
									 DrawArea.Y,
									 (int)Math.Round(sz.X) + (int)x_padding,
									 (int)Math.Round(sz.Y) + (int)y_padding);
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || !ShouldUpdate())
				return;

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			var totalTextArea = CalculateSizeOfTextArea();
			float adjustedX = 0, adjustedY = 0;
			if (!AutoSize)
			{
				var tmpVec = CalculatePositionFromAlignment(totalTextArea);
				adjustedX = tmpVec.X;
				adjustedY = tmpVec.Y;
			}


			SpriteBatch.Begin();

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

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		private Vector2 CalculateSizeOfTextArea()
		{
			return TextWidth == null
				? _font.MeasureString(Text)
				: new Vector2(_drawStrings.Count > 0 ? _drawStrings.Select(line => _font.MeasureString(line).X).Max() : 1f,
							  _drawStrings.Count > 0 ? _font.LineSpacing*_drawStrings.Count : _font.LineSpacing);
		}

		private Vector2 CalculatePositionFromAlignment(Vector2 totalArea)
		{
			float adjustedX = 0;
			float adjustedY = 0;

			var align = Enum.GetName(typeof(LabelAlignment), TextAlign) ?? "";

			if (align.Contains("Left"))
				adjustedX = 0;
			else if (align.Contains("Center"))
				adjustedX = DrawArea.Width / 2f - totalArea.X / 2;
			else if(align.Contains("Right"))
				adjustedX = DrawArea.Width - totalArea.X;

			if (align.Contains("Top"))
				adjustedY = 0;
			else if (align.Contains("Middle"))
				adjustedY = DrawArea.Height / 2f - totalArea.Y / 2;
			else if(align.Contains("Bottom"))
				adjustedY = DrawArea.Height - totalArea.Y;

			return new Vector2(adjustedX, adjustedY);
		}

		private void DrawBackground(Vector2 totalArea)
		{
			var location = new Vector2(DrawAreaWithOffset.X, DrawAreaWithOffset.Y);

			if (BackColor.HasValue && _backgroundTexture != null)
				SpriteBatch.Draw(_backgroundTexture,
								 TextWidth == null ? DrawAreaWithOffset
								 			       : new Rectangle((int) location.X,
																   (int) location.Y,
																   (int) totalArea.X,
																   (int) totalArea.Y),
								 Color.White);
		}

		private void DrawTextLine(string textLine, float adjustedX, float adjustedY)
		{
			var textLineWidth = _font.MeasureString(textLine).X;
			var extraHeightForUnderline = _font.MeasureString(textLine).Y - 3;

			SpriteBatch.DrawString(_font,
					textLine,
					new Vector2(DrawAreaWithOffset.X + adjustedX, DrawAreaWithOffset.Y + adjustedY),
					ForeColor);

			if (Underline)
			{
				SpriteBatch.Draw(_underlineTexture,
								 new Rectangle((int)(DrawAreaWithOffset.X + adjustedX),
											   (int)(DrawAreaWithOffset.Y + adjustedY + extraHeightForUnderline),
											   (int)textLineWidth,
											   1),
								 null,
								 ForeColor);
			}
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
