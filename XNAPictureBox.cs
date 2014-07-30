using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
	public enum StretchMode
	{
		None,
		Stretch
	}

	public class XNAPictureBox : XNAControl
	{
		public StretchMode StretchMode { get; set; }

		public Texture2D Texture { get; set; }

		public XNAPictureBox(Game game, Rectangle area)
			: base(game, new Vector2(area.X, area.Y), area)
		{
		}

		public override void Draw(GameTime gameTime)
		{
			if (Texture != null)
			{
				SpriteBatch.Begin();
				switch (StretchMode)
				{
					case XNAControls.StretchMode.None:
						SpriteBatch.Draw(Texture,
							new Rectangle(DrawAreaWithOffset.X + DrawArea.Width / 2 - Texture.Width / 2,
								DrawAreaWithOffset.Y + DrawArea.Height / 2 - Texture.Width / 2,
								Texture.Width,
								Texture.Height),
							Color.White);
						break;

					case XNAControls.StretchMode.Stretch:
						SpriteBatch.Draw(Texture, new Rectangle(DrawAreaWithOffset.X, DrawAreaWithOffset.Y, DrawArea.Width, DrawArea.Height), Color.White);
						break;
				}
				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}
	}
}
