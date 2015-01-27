using XNAFramework = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;

namespace XNAControls
{
	public class XNAHyperLink : XNALabel
	{
		Color _highlightColor = Color.FromArgb(102, 158, 0);
		Color _backupColor;
		public Color HighlightColor
		{
			get { return _highlightColor; }
			set { _highlightColor = value; }
		}

		public EventHandler OnClick;
		public EventHandler OnMouseOver;
		public EventHandler OnMouseOut;

		protected bool enableImmediateColorRevert = true;

		[Obsolete("Passing a font as a parameter is deprecated. Specify font family and font size instead, and set additional parameters using the .Font property.")]
		public XNAHyperLink(XNAFramework.Rectangle area, Font font, FontStyle style, System.Drawing.Text.TextRenderingHint renderHint)
			: base(area)
		{
			RenderingHint = renderHint;
			Font = new Font(font, style);
			ForeColor = Color.FromArgb(13, 158, 17);
		}

		[Obsolete("Passing a font as a parameter is deprecated. Specify font family and font size instead, and set additional parameters using the .Font property.")]
		public XNAHyperLink(XNAFramework.Rectangle area, Font font)
			: base(area)
		{
			Font = new Font(font, FontStyle.Underline);
			ForeColor = Color.FromArgb(13, 158, 17);
		}

		public XNAHyperLink(XNAFramework.Rectangle area, string fontFamily, float fontSize, FontStyle style, System.Drawing.Text.TextRenderingHint renderHint)
			: base(area)
		{
			RenderingHint = renderHint;
			Font = new Font(fontFamily, fontSize, style);
			ForeColor = Color.FromArgb(13, 158, 17);
		}

		public XNAHyperLink(XNAFramework.Rectangle area, string fontFamily, float fontSize = 12.0f)
			: base(area)
		{
			Font = new Font(fontFamily, fontSize);
			ForeColor = Color.FromArgb(13, 158, 17);
		}

		public override void Update(XNAFramework.GameTime gameTime)
		{
			if (!Visible || (Dialogs.Count != 0 && Dialogs.Peek() != TopParent as XNADialog))
				return;

			if (MouseOver && OnClick != null && PreviousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released)
				OnClick(this, null);

			base.Update(gameTime);
		}

		protected override void MouseIsOver()
		{
			_backupColor = ForeColor;
			ForeColor = HighlightColor;

			if (OnMouseOver != null)
				OnMouseOver(this, null);

			base.MouseIsOver();
		}

		protected override void MouseIsOut()
		{
			if (enableImmediateColorRevert)
				ForeColor = _backupColor;

			if (OnMouseOut != null)
				OnMouseOut(this, null);

			base.MouseIsOut();
		}
	}
}
