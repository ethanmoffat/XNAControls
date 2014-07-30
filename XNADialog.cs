using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	public class XNADialog : XNAControl
	{
		public enum XNADialogButtons
		{
			Ok,
			OkCancel
		}

		/// <summary>
		/// XNADialog.XNADialogResult
		/// Returns the value of the clicked button (based on the button text)
		/// </summary>
		public enum XNADialogResult
		{
			OK,
			Cancel,
			Yes,
			No,
			Back,
			Next
		}
		
		public delegate void CustomClose(bool success);

		public CustomClose CloseAction { get; set; }

		protected XNALabel caption;
		public string CaptionText
		{
			get { return caption.Text; }
			set { caption.Text = value; }
		}

		protected XNALabel message;
		public string MessageText
		{
			get { return message.Text; }
			set { message.Text = value; }
		}
		
		protected Texture2D bgTexture;
		
		protected List<XNAButton> dlgButtons;

		protected TimeSpan? openTime;
		
		public XNADialog(Game encapsulatingGame, string msgText, string captionText = "", XNADialogButtons whichButtons = XNADialogButtons.Ok)
			: base(encapsulatingGame)
		{
			//specify location of any buttons relative to where control is being drawn
			dlgButtons = new List<XNAButton>();
			Visible = true;

			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			using (System.IO.Stream s = assembly.GetManifestResourceStream(@"XNAControls.img.dlg.png"))
			{
				bgTexture = Texture2D.FromStream(encapsulatingGame.GraphicsDevice, s);
			}

			_setSize(bgTexture.Width, bgTexture.Height);

			XNAButton Ok, Cancel;
			Ok = new XNAButton(encapsulatingGame, new Vector2(196, 116));
			Ok.Text = "Ok";
			Ok.OnClick += (object x, EventArgs e) => { Close(); };
			Ok.SetParent(this);
			Cancel = new XNAButton(encapsulatingGame, new Vector2(196, 116));
			Cancel.Text = "Cancel";
			Cancel.OnClick += (object x, EventArgs e) => { Close(); };
			Cancel.SetParent(this);

			switch (whichButtons)
			{
				case XNADialogButtons.Ok:
					dlgButtons.Add(Ok);
					Cancel.Close();
					break;
				case XNADialogButtons.OkCancel:
					Ok.DrawLocation = new Vector2(106, 116);
					dlgButtons.Add(Ok);
					dlgButtons.Add(Cancel);
					break;
			}

			//top left of text: 15, 40
			message = new XNALabel(encapsulatingGame, new Rectangle(15, 40, this.DrawArea.Width - 30, this.DrawArea.Height - 80));
			message.Text = msgText;
			message.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			message.Font = new System.Drawing.Font("Arial", 12);
			message.ForeColor = System.Drawing.Color.Black;
			message.SetParent(this);
			message.TextWidth = 250;

			//top left of cap : 9, 11
			caption = new XNALabel(encapsulatingGame, new Rectangle(9, 11, this.DrawArea.Width - 18, this.DrawArea.Height - 22));
			caption.Text = captionText;
			caption.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			caption.Font = new System.Drawing.Font("Arial", 12);
			caption.ForeColor = System.Drawing.Color.Black;
			caption.SetParent(this);

			//center dialog based on txtSize of background texture
			Center(encapsulatingGame.GraphicsDevice);

			//draw dialog on top of everything - always!
			//child controls DrawOrder is set accordingly

			XNAControl.ModalDialogs.Push(this);

			_fixDrawOrder();

			EncapsulatingGame.Components.Add(this);
		}

		protected XNADialog(Game encapsulatingGame)
			: base(encapsulatingGame)
		{
			//specify location of any buttons relative to where control is being drawn
			dlgButtons = new List<XNAButton>();
			Visible = true;
		}

		protected void _fixDrawOrder()
		{			
			this.DrawOrder = 0;
			foreach(XNADialog dlg in ModalDialogs)
			{
				if (dlg == this)
					continue;
				dlg.DrawOrder -= 5;
				//_updateChildrenDrawOrder(dlg.DrawOrder); //handled in OnDrawOrderChanged
			}

			foreach(DrawableGameComponent component in EncapsulatingGame.Components)
			{
				if (!(component is XNAControl))
					continue;

				XNAControl ctrl = component as XNAControl;
				if (ctrl.TopParent == null && !(ctrl is XNADialog))
				{
					ctrl.DrawOrder -= 5;
				}
			}
		}
		
		public void Center(GraphicsDevice device)
		{
			int viewWidth = device.Viewport.Width;
			int viewHeight = device.Viewport.Height;

			DrawLocation = new Vector2( (viewWidth / 2) - (bgTexture.Width / 2), (viewHeight / 2) - (bgTexture.Height / 2));
		}
		
		public override void Update(GameTime gt)
		{
			if (!Visible || (XNAControl.ModalDialogs.Count > 0 && XNAControl.ModalDialogs.Peek() != this))
				return;
			
			KeyboardState keyState = Keyboard.GetState();
			//give a time buffer of 50ms so that an enter keypress from a textbox that produces a dialog isn't picked up by the update method here
			if(keyState.IsKeyUp(Keys.Enter) && PreviousKeyState.IsKeyDown(Keys.Enter) && (gt.TotalGameTime - (openTime ?? (openTime = gt.TotalGameTime))).Value.Duration().Milliseconds > 50)
			{
				//tie enter key press to close the dialog
				//if we ever implement dialogresults this should constitute an "OK" response
				Close();
			}

			MouseState curState = Mouse.GetState();
			if(PreviousMouseState.LeftButton == ButtonState.Pressed && curState.LeftButton == ButtonState.Pressed 
				&& DrawAreaWithOffset.Contains(curState.X, curState.Y) && shouldClickDrag)
			{
				Rectangle gdm = EncapsulatingGame.Window.ClientBounds;

				Vector2 newDrawLoc = new Vector2(DrawAreaWithOffset.X + (curState.X - PreviousMouseState.X), DrawAreaWithOffset.Y + (curState.Y - PreviousMouseState.Y));
				if (newDrawLoc.X < 0) newDrawLoc.X = 0;
				else if (newDrawLoc.Y < 0) newDrawLoc.Y = 0;
				else if (newDrawLoc.X > gdm.Width - DrawAreaWithOffset.Width) newDrawLoc.X = gdm.Width - DrawAreaWithOffset.Width;
				else if (newDrawLoc.Y > gdm.Height - DrawAreaWithOffset.Height) newDrawLoc.Y = gdm.Height - DrawAreaWithOffset.Height;
				DrawLocation = newDrawLoc;
			}
			
			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if (!Visible)
				return;

			SpriteBatch.Begin();
			SpriteBatch.Draw(bgTexture, DrawAreaWithOffset, Color.White);
			SpriteBatch.End();

			base.Draw(gt);
		}

		public override void Close()
		{
			XNAControl.ModalDialogs.Pop();

			foreach(XNAControl ctrl in XNAControl.ModalDialogs)
			{
				ctrl.DrawOrder += 5;
				//(ctrl as XNADialog)._updateChildrenDrawOrder(ctrl.DrawOrder); //handled in OnDrawOrderChanged
			}

			foreach (XNAControl ctrl in EncapsulatingGame.Components)
			{
				if (ctrl.TopParent == null && !(ctrl is XNADialog))
				{
					ctrl.DrawOrder += 5;
				}
			}

			if (CloseAction != null)
				CloseAction(false); //should be called with 'true' explicitly when the action succeeds

			//set the dialog results accordingly based on the button press here
			base.Close();
		}
	}
}
