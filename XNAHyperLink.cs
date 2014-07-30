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

		public EventHandler OnClick = null;
		public EventHandler OnMouseOver = null;
		public EventHandler OnMouseOut = null;

		protected bool enableImmediateColorRevert = true;

		public XNAHyperLink(XNAFramework.Game game, XNAFramework.Rectangle area, Font font, FontStyle style, System.Drawing.Text.TextRenderingHint renderHint)
			: base(game, area)
		{
			RenderingHint = renderHint;
			Font = new Font(font, style);
			ForeColor = Color.FromArgb(13, 158, 17);
		}

		public XNAHyperLink(XNAFramework.Game game, XNAFramework.Rectangle area, Font font)
			: base(game, area)
		{
			Font = new Font(font, FontStyle.Underline);
			ForeColor = Color.FromArgb(13, 158, 17);
		}

		public override void Update(XNAFramework.GameTime gameTime)
		{
			if (!Visible || (XNAControl.ModalDialogs.Count != 0 && XNAControl.ModalDialogs.Peek() != TopParent as XNADialog))
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
