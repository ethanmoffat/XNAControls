﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
	/// <summary>
	/// XNADialogButtons
	/// Specifies the buttons that should be shown on a dialog
	/// </summary>
	public enum XNADialogButtons
	{
		Ok,
		Cancel,
		OkCancel
	}

	/// <summary>
	/// XNADialogResult
	/// Returns the value of the clicked button (based on the button text)
	/// </summary>
	public enum XNADialogResult
	{
		OK,
		Cancel,
		Yes,
		No,
		Back,
		Next,
		NO_BUTTON_PRESSED
	}

	public class CloseDialogEventArgs : EventArgs
	{
		public XNADialogResult Result { get; protected set; }
		public bool CancelClose { get; set; }

		public CloseDialogEventArgs(XNADialogResult result)
		{
			Result = result;
			CancelClose = false;
		}
	}

	public class XNADialog : XNAControl
	{
		public delegate void OnDialogClose(object sender, CloseDialogEventArgs e);

		public event OnDialogClose DialogClosing;

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

		protected XNADialogButtons whichButtons;
		
		public XNADialog(string msgText, string captionText = "", XNADialogButtons whichButtons = XNADialogButtons.Ok)
		{
			//specify location of any buttons relative to where control is being drawn
			dlgButtons = new List<XNAButton>();
			Visible = true;

			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			using (System.IO.Stream s = assembly.GetManifestResourceStream(@"XNAControls.img.dlg.png"))
			{
				bgTexture = Texture2D.FromStream(Game.GraphicsDevice, s);
			}

			_setSize(bgTexture.Width, bgTexture.Height);

			XNAButton Ok = new XNAButton(new Vector2(196, 116)) {Text = "Ok"};
			Ok.OnClick += (x, e) => Close(Ok, XNADialogResult.OK);
			Ok.SetParent(this);
			XNAButton Cancel = new XNAButton(new Vector2(196, 116)) {Text = "Cancel"};
			Cancel.OnClick += (x, e) => Close(Cancel, XNADialogResult.Cancel);
			Cancel.SetParent(this);

			switch (this.whichButtons = whichButtons)
			{
				case XNADialogButtons.Ok:
					dlgButtons.Add(Ok);
					Cancel.Close();
					break;
				case XNADialogButtons.Cancel:
					dlgButtons.Add(Cancel);
					Ok.Close();
					break;
				case XNADialogButtons.OkCancel:
					Ok.DrawLocation = new Vector2(106, 116);
					dlgButtons.Add(Ok);
					dlgButtons.Add(Cancel);
					break;
			}

			//top left of text: 15, 40
			message = new XNALabel(new Rectangle(15, 40, DrawArea.Width - 30, DrawArea.Height - 80))
			{
				Text = msgText,
				TextAlign = System.Drawing.ContentAlignment.TopLeft,
				Font = new System.Drawing.Font("Arial", 12),
				ForeColor = System.Drawing.Color.Black,
				TextWidth = 250
			};
			message.SetParent(this);

			//top left of cap : 9, 11
			caption = new XNALabel(new Rectangle(9, 11, DrawArea.Width - 18, DrawArea.Height - 22))
			{
				Text = captionText,
				TextAlign = System.Drawing.ContentAlignment.TopLeft,
				Font = new System.Drawing.Font("Arial", 12),
				ForeColor = System.Drawing.Color.Black
			};
			caption.SetParent(this);

			//center dialog based on txtSize of background texture
			Center(Game.GraphicsDevice);

			//draw dialog on top of everything - always!
			//child controls DrawOrder is set accordingly

			Dialogs.Push(this);

			_fixDrawOrder();

			Game.Components.Add(this);
		}

		protected XNADialog()
		{
			//specify location of any buttons relative to where control is being drawn
			dlgButtons = new List<XNAButton>();
			Visible = true;
		}

		protected void _fixDrawOrder()
		{
			DrawOrder = (int)ControlDrawLayer.DialogLayer + (5 * Dialogs.Count);
		}
		
		public void Center(GraphicsDevice device)
		{
			int viewWidth = device.Viewport.Width;
			int viewHeight = device.Viewport.Height;

			DrawLocation = new Vector2( (viewWidth / 2) - (bgTexture.Width / 2), (viewHeight / 2) - (bgTexture.Height / 2));
		}
		
		public override void Update(GameTime gt)
		{
			if (!Visible || (Dialogs.Count > 0 && Dialogs.Peek() != this))
				return;
			
			KeyboardState keyState = Keyboard.GetState();
			bool enterPressed = keyState.IsKeyUp(Keys.Enter) && PreviousKeyState.IsKeyDown(Keys.Enter);
			bool escPressed = keyState.IsKeyUp(Keys.Escape) && PreviousKeyState.IsKeyDown(Keys.Escape);
			//give a time buffer of 50ms so that an enter keypress from a textbox that produces a dialog isn't picked up by the update method here
			if((gt.TotalGameTime - (openTime ?? (openTime = gt.TotalGameTime))).Value.Duration().Milliseconds > 50)
			{
				if (enterPressed)
				{
					Close(dlgButtons[0], whichButtons == XNADialogButtons.Cancel ? XNADialogResult.Cancel : XNADialogResult.OK);
				}
				else if (escPressed)
				{
					if (whichButtons == XNADialogButtons.Ok)
						Close(dlgButtons[0], XNADialogResult.OK);
					else
						Close(whichButtons == XNADialogButtons.OkCancel ? dlgButtons[1] : dlgButtons[0], XNADialogResult.Cancel);
				}
			}

			MouseState curState = Mouse.GetState();
			if(PreviousMouseState.LeftButton == ButtonState.Pressed && curState.LeftButton == ButtonState.Pressed 
				&& DrawAreaWithOffset.Contains(curState.X, curState.Y) && shouldClickDrag)
			{
				DrawLocation = new Vector2(DrawAreaWithOffset.X + (curState.X - PreviousMouseState.X), DrawAreaWithOffset.Y + (curState.Y - PreviousMouseState.Y));
			}
			
			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if (!Visible)
				return;

			if (bgTexture != null)
			{
				SpriteBatch.Begin();
				SpriteBatch.Draw(bgTexture, DrawAreaWithOffset, Color.White);
				SpriteBatch.End();
			}

			base.Draw(gt);
		}

		/// <summary>
		/// This should be called whenever a button is clicked. Any action to be taken by the dialog when it closes
		/// should be specified in the DialogClosing event, with a switch on the CloseDialogEventArgs.Result property
		/// </summary>
		/// <param name="whichButton">The button closing the dialog</param>
		/// <param name="result">The result that the DialogClosing event should receive</param>
		protected virtual void Close(XNAButton whichButton, XNADialogResult result)
		{
			Dialogs.Pop(); //remove this dialog from XNADialogs initially
			int cntBeforeDlgClosingEvent = Dialogs.Count;

			CloseDialogEventArgs args = new CloseDialogEventArgs(result);
			if (DialogClosing != null)
				DialogClosing(whichButton, args);

			if (args.CancelClose) //user code cancelled the closing operation. this dialog needs to be pushed back onto the stack properly
			{
				if (cntBeforeDlgClosingEvent == Dialogs.Count) //no other dialogs were created: push this back on the stack normally
					Dialogs.Push(this); 
				else if(cntBeforeDlgClosingEvent < Dialogs.Count) //other dialogs were created: remove them, push this dialog, push them back
				{
					//this is super hacky, but i like having a stack for the dialogs that are open and don't want to change it to a list w/random access
					Stack<XNADialog> newDialogs = new Stack<XNADialog>();
					while (Dialogs.Count > cntBeforeDlgClosingEvent)
						newDialogs.Push(Dialogs.Pop());
					
					Dialogs.Push(this);
					while (newDialogs.Count > 0)
						Dialogs.Push(newDialogs.Pop());
				}

				return;
			}
		
			base.Close();
		}
	}
}
