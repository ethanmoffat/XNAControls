using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	public class XNAHyperLink : XNALabel
	{
	    Color _backupColor;
		public Color HighlightColor { get; set; }

	    public event EventHandler OnClick;
        
		public XNAHyperLink(Rectangle area, string spriteFontContentName)
			: base(area, spriteFontContentName) { }

		public override void Initialize()
		{
			ForeColor = Color.Blue;
			HighlightColor = Color.Blue;

			OnMouseEnter += (o, e) =>
			{
				_backupColor = ForeColor;
				ForeColor = HighlightColor;
			};
			OnMouseLeave += (o, e) => ForeColor = _backupColor;
			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			if (!Visible || !ShouldUpdate())
				return;

		    if (MouseOver &&
		        MouseOverPreviously &&
		        OnClick != null &&
		        PreviousMouseState.LeftButton == ButtonState.Pressed &&
		        Mouse.GetState().LeftButton == ButtonState.Released)
		        OnClick(this, null);

			base.Update(gameTime);
		}

		public void Click()
		{
			if (OnClick != null)
				OnClick(this, null);
		}
	}
}
