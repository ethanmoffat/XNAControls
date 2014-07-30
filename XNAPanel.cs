using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace XNAControls
{
	public class XNAPanel : DrawableGameComponent
	{
		public List<DrawableGameComponent> Components { get; set; }

		public XNAPanel(Game game)
			: base(game)
		{
			Components = new List<DrawableGameComponent>();
		}

		public override void Initialize()
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Initialize();

			base.Initialize();
		}

		public override void Update(GameTime gameTime)
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Update(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Draw(gameTime);

			base.Draw(gameTime);
		}

		public void ClearTextBoxes()
		{
			List<DrawableGameComponent> tbs = Components.FindAll(x => x is XNATextBox);
			foreach(DrawableGameComponent tb in tbs)
			{
				(tb as XNATextBox).Text = "";
			}
		}

		protected override void Dispose(bool disposing)
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Dispose();

			base.Dispose(disposing);
		}
	}
}
