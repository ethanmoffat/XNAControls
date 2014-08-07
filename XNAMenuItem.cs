using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using SD = System.Drawing;

namespace XNAControls
{
	public class XNAMenuItem : XNAHyperLink
	{
		public bool Selected { get; set; }
		public SD.Color RegularColor { get; set; }

		[Obsolete("Passing a font as a parameter is deprecated. Specify font family and font size instead, and set additional parameters using the .Font property.")]
		public XNAMenuItem(Game game, Rectangle area, SD.Font font)
			: base(game, area, font)
		{
			SelectionChanged(false);
			enableImmediateColorRevert = false;
		}

		public XNAMenuItem(Game game, Rectangle area, string fontFamily, float fontSize = 12.0f)
			: base(game, area, fontFamily, fontSize)
		{
			SelectionChanged(false);
			enableImmediateColorRevert = false;
		}

		public void SelectionChanged(bool selected)
		{
			ForeColor = (Selected = selected) ? HighlightColor : RegularColor;
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || (XNAControl.Dialogs.Count != 0 && XNAControl.Dialogs.Peek() != TopParent as XNADialog))
				return;

			MouseState state = Mouse.GetState();

			if (!Selected && MouseOver && (state.X != PreviousMouseState.X || state.Y != PreviousMouseState.Y))
				MouseIsOver();

			base.Update(gameTime);
		}
	}
}
