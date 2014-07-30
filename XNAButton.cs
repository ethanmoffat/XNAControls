using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	public class XNAButton : XNAControl
	{
		public delegate void ButtonClickEvent(object sender, EventArgs e = null);

		private string _text;
		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				_TEXTture = EncapsulatingGame.DrawText(_text, new System.Drawing.Font("Arial", 12), System.Drawing.Color.Black);
			}
		}

		private Texture2D _out; //texture for mouse out (primary display texture)
		private Texture2D _over; //texture for mouse over
		private Texture2D _TEXTture; //get it?
		private Texture2D _drawTexture; //texture to be drawn, selected based on MouseOver in Update method
		
		private Rectangle? area; //nullable rectangle for optional area that will respond to mouse click/hover
		/// <summary>
		/// Get/set the area that should respond to a click event.
		/// If null, the entire button area responds to a click event.
		/// </summary>
		public Rectangle? ClickArea
		{
			get { return this.area; }
			set
			{
				area = value;
				if(area != null)
				{
					//factor in the draw coordinates and the offset from the parent to the new click area
					area = new Rectangle(area.Value.X + DrawAreaWithOffset.X, area.Value.Y + DrawAreaWithOffset.Y,
						area.Value.Width, area.Value.Height);
				}
			}
		}
		
		/// <summary>
		/// Construct a button where the textures for over/out are a part of a sprite sheet.
		/// </summary>
		/// <param name="encapsulatingGame">The game that owns the control</param>
		/// <param name="sheet">The sprite sheet texture</param>
		/// <param name="location">Location to draw the button</param>
		/// <param name="outSource">Source within the sprite sheet that contains the texture to draw on MouseOut</param>
		/// <param name="overSource">Source within the sprite sheet that contains the texture to draw on MouseOver</param>
		public XNAButton(Game encapsulatingGame, Texture2D sheet, Vector2 location, Rectangle outSource, Rectangle overSource) 
			: base(encapsulatingGame, location, new Rectangle((int)location.X, (int)location.Y, outSource.Width, outSource.Height))
		{
			ClickArea = null;
			
			_out = new Texture2D(sheet.GraphicsDevice, outSource.Width, outSource.Height);
			Color[] outData = new Color[outSource.Width * outSource.Height];
			sheet.GetData<Color>(0, outSource, outData, 0, outData.Length);
			_out.SetData<Color>(outData);

			_over = new Texture2D(sheet.GraphicsDevice, overSource.Width, overSource.Height);
			Color[] overData = new Color[overSource.Width * overSource.Height];
			sheet.GetData<Color>(0, overSource, overData, 0, overData.Length);
			_over.SetData<Color>(overData);
			
			_drawTexture = _out;
		}

		/// <summary>
		/// Construct a button where the textures for over/out are individual textures.
		/// </summary>
		/// <param name="encapsulatingGame">The game that owns the button</param>
		/// <param name="textures">An array of length 2 containing an over texture and an out texture</param>
		/// <param name="location">Location to draw the button</param>
		public XNAButton(Game encapsulatingGame, Texture2D[] textures, Vector2 location)
			: base(encapsulatingGame, location, new Rectangle((int)location.X, (int)location.Y, textures[0].Width, textures[0].Height))
		{
			if (textures.Length != 2)
				throw new ArgumentException("You must specify an array of two textures for this constructor.");

			ClickArea = null;

			_out = textures[0];
			_over = textures[1];
			_drawTexture = _out;
		}

		public XNAButton(Game encapsulatingGame, Vector2 location, string text = "default")
			: base(encapsulatingGame, location, null)
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			using (System.IO.Stream s = assembly.GetManifestResourceStream(@"XNAControls.img.button.png"))
			{
				_out = Texture2D.FromStream(encapsulatingGame.GraphicsDevice, s);
			}

			using(System.IO.Stream s = assembly.GetManifestResourceStream(@"XNAControls.img.button_hover.png"))
			{
				_over = Texture2D.FromStream(encapsulatingGame.GraphicsDevice, s);
			}

			ClickArea = null;
			//_setSize(_out.Width, _out.Height);
			_setSize(80, 30);

			Text = text;
			_drawTexture = _out;
		}

		public override void Update(GameTime gt)
		{
			if (!Visible || (XNAControl.ModalDialogs.Count > 0 && (XNAControl.ModalDialogs.Peek() != TopParent as XNADialog || TopParent == null)))
				return;

			if (MouseOver && OnClick != null && PreviousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Released)
				OnClick(this);

			if (DrawAreaWithOffset.Contains(PreviousMouseState.X, PreviousMouseState.Y) && OnClickDrag != null 
				&& PreviousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Pressed
				&& shouldClickDrag)
			{
				SuppressParentClickDrag(true);
				OnClickDrag(this);
			}
			else
			{
				SuppressParentClickDrag(false);
			}

			_drawTexture = MouseOver ? _over : _out;

			PreviousMouseState = Mouse.GetState();
		}
		
		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;
			SpriteBatch.Begin();
			
			//draw the background texture
			//if(_drawTexture != null)
			try
			{
				SpriteBatch.Draw(_drawTexture, new Rectangle((int)DrawLocation.X + xOff, (int)DrawLocation.Y + yOff, DrawArea.Width, DrawArea.Height), Color.White);
			}
			catch(ArgumentException)
			{
				SpriteBatch.End();
				return;
			}
			catch(NullReferenceException)
			{
				SpriteBatch.End();
				return;
			}

			//draw the text string
			if (_TEXTture != null)
			{
				float xCoord, yCoord;
				xCoord = (DrawAreaWithOffset.Width / 2) - (_TEXTture.Width / 2) + DrawAreaWithOffset.X;
				yCoord = (DrawAreaWithOffset.Height / 2) - (_TEXTture.Height / 2) + DrawAreaWithOffset.Y;
				SpriteBatch.Draw(_TEXTture, new Vector2(xCoord, yCoord), Color.White);
			}
			SpriteBatch.End();

			base.Draw(gameTime);
		}

		public event ButtonClickEvent OnClick;
		public event ButtonClickEvent OnClickDrag;
	}
}
