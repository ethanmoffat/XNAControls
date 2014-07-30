using XNAFramework = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace XNAControls
{
	public class XNALabel : XNAControl, IDisposable
	{
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				GenerateImage();
			}
		}
		public Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;
				GenerateImage();
			}
		}
		public Color ForeColor
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
				GenerateImage();
			}
		}
		public bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				_autoSize = value;
				GenerateImage();
			}
		}
		public ContentAlignment TextAlign
		{
			get
			{
				return _align;
			}
			set
			{
				_align = value;
				GenerateImage();
			}
		}
		public System.Drawing.Text.TextRenderingHint RenderingHint
		{
			get
			{
				return _renderingHint;
			}
			set
			{
				_renderingHint = value;
				GenerateImage();
			}
		}
		
		/// <summary>
		/// The text width in pixels
		/// </summary> 
		public int? TextWidth
		{
			get { return _textWidth; }
			set
			{
				_textWidth = value;
				GenerateImage();
			}
		}
		/// <summary>
		/// The actual text width in pixels
		/// </summary> 
		public int ActualWidth
		{
			get { return _texture.Width; }
		}

		/// <summary>
		/// The text converted as a Texture2D
		/// </summary>
		public Texture2D Texture
		{
			get { return _texture; }
		}

		public int? RowSpacing
		{
			get
			{
				return rowSpacing;
			}
			set
			{
				rowSpacing = value;
				GenerateImage();
			}
		}

		public XNALabel(XNAFramework.Game game, XNAFramework.Rectangle area)
			: base(game, new XNAFramework.Vector2(area.X, area.Y), area)
		{
			_autoSize = true;
			_text = "";
			_font = new Font("Arial", 12);
			_color = Color.Black;
			_align = ContentAlignment.TopLeft;
		}

		int? rowSpacing;
		string _text;
		bool _autoSize;
		Font _font;
		Color _color;
		ContentAlignment _align;
		System.Drawing.Text.TextRenderingHint _renderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
		private int? _textWidth = null;
		Texture2D _texture;
		bool _previousMouseOver = false;

		public override void Initialize()
		{
			GenerateImage();
			base.Initialize();
		}

		public override void Draw(XNAFramework.GameTime gameTime)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();

			int x, y;
			Size size = AutoSize ? new Size(_texture.Width, _texture.Height) : new Size(DrawAreaWithOffset.Width, DrawAreaWithOffset.Height);

			// Figure out alignment
			string align = TextAlign.ToString();
			if (align.Contains("Left"))
				x = 0;
			else if (align.Contains("Center"))
				x = (int)(DrawArea.Width / 2 - size.Width / 2);
			else
				x = (int)(DrawArea.Width - size.Width);
			if (align.Contains("Top"))
				y = 0;
			else if (align.Contains("Middle"))
				y = (int)(DrawArea.Height / 2 - size.Height / 2);
			else
				y = (int)(DrawArea.Height - size.Height);

			SpriteBatch.Draw(_texture, new XNAFramework.Rectangle(DrawAreaWithOffset.X + x, DrawAreaWithOffset.Y + y, size.Width, size.Height), XNAFramework.Color.White);
			SpriteBatch.End();

			if (MouseOver && !_previousMouseOver)
			{
				MouseIsOver();
				_previousMouseOver = true;
			}

			if (!MouseOver && _previousMouseOver)
			{
				_previousMouseOver = false;
				MouseIsOut();
			}

			base.Draw(gameTime);
		}

		protected virtual void MouseIsOver()
		{
		}

		protected virtual void MouseIsOut()
		{
		}

		void GenerateImage()
		{
			_texture = EncapsulatingGame.DrawText(Text, Font, ForeColor, TextWidth, RenderingHint, rowSpacing ?? 0);
		}

		public new void Dispose()
		{
			_font.Dispose();
			base.Dispose();
		}
	}
}
