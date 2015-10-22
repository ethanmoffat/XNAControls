// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace XNAControls
{
	//From top answer on: http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game
	//Some modifications made by Ethan Moffat and Brian Gradin
	internal static class NativeMethods
	{
		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr ImmGetContext(IntPtr hWnd);

		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
	}

	public class CharacterEventArgs : EventArgs
	{
		private readonly char character;
		private readonly int lParam;

		public CharacterEventArgs(char character, int lParam)
		{
			this.character = character;
			this.lParam = lParam;
		}

		public char Character
		{
			get { return character; }
		}

		public int Param
		{
			get { return lParam; }
		}

		public int RepeatCount
		{
			get { return lParam & 0xffff; }
		}

		public bool ExtendedKey
		{
			get { return (lParam & (1 << 24)) > 0; }
		}

		public bool AltPressed
		{
			get { return (lParam & (1 << 29)) > 0; }
		}

		public bool PreviousState
		{
			get { return (lParam & (1 << 30)) > 0; }
		}

		public bool TransitionState
		{
			get { return (lParam & (1 << 31)) > 0; }
		}
	}

	public class XNAKeyEventArgs : EventArgs
	{
		private readonly Keys keyCode;

		public XNAKeyEventArgs(Keys keyCode)
		{
			this.keyCode = keyCode;
		}

		public Keys KeyCode
		{
			get { return keyCode; }
		}
	}

	public delegate void CharEnteredHandler(object sender, CharacterEventArgs e);
	public delegate void KeyEventHandler(object sender, XNAKeyEventArgs e);

	public static class EventInput
	{
		/// <summary>
		/// Event raised when a character has been entered.
		/// </summary>
		public static event CharEnteredHandler CharEntered;

		/// <summary>
		/// Event raised when a key has been pressed down. May fire multiple times due to keyboard repeat.
		/// </summary>
		public static event KeyEventHandler KeyDown;

		/// <summary>
		/// Event raised when a key has been released.
		/// </summary>
		public static event KeyEventHandler KeyUp;

		delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		static bool initialized;
		static IntPtr prevWndProc;
		static WndProc hookProcDelegate;
		static IntPtr hIMC;

		//various Win32 constants that we need
		const int GWL_WNDPROC = -4;
		const int WM_KEYDOWN = 0x100;
		const int WM_KEYUP = 0x101;
		const int WM_CHAR = 0x102;
		const int WM_IME_SETCONTEXT = 0x0281;
		const int WM_INPUTLANGCHANGE = 0x51;
		const int WM_GETDLGCODE = 0x87;
		const int WM_IME_COMPOSITION = 0x10f;
		const int DLGC_WANTALLKEYS = 4;
		
		/// <summary>
		/// Initialize the TextInput with the given GameWindow.
		/// </summary>
		/// <param name="window">The XNA window to which text input should be linked.</param>
		public static void Initialize(GameWindow window)
		{
			if (initialized)
				throw new InvalidOperationException("TextInput.Initialize can only be called once!");

			hookProcDelegate = HookProc;
			prevWndProc = (IntPtr)NativeMethods.SetWindowLong(window.Handle, GWL_WNDPROC,
				(int)Marshal.GetFunctionPointerForDelegate(hookProcDelegate));

			hIMC = NativeMethods.ImmGetContext(window.Handle);
			initialized = true;
		}

		static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr returnCode = NativeMethods.CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);

			switch (msg)
			{
				case WM_GETDLGCODE:
					returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
					break;

				case WM_KEYDOWN:
					if (KeyDown != null)
						KeyDown(null, new XNAKeyEventArgs((Keys)wParam));
					break;

				case WM_KEYUP:
					if (KeyUp != null)
						KeyUp(null, new XNAKeyEventArgs((Keys)wParam));
					break;

				case WM_CHAR:
					if (CharEntered != null)
						CharEntered(null, new CharacterEventArgs((char)wParam, lParam.ToInt32()));
					break;

				case WM_IME_SETCONTEXT:
					if (wParam.ToInt32() == 1)
						NativeMethods.ImmAssociateContext(hWnd, hIMC);
					break;

				case WM_INPUTLANGCHANGE:
					NativeMethods.ImmAssociateContext(hWnd, hIMC);
					returnCode = (IntPtr)1;
					break;
			}

			return returnCode;
		}
	}
	public interface IKeyboardSubscriber
	{
		void ReceiveTextInput(char inputChar);
		void ReceiveTextInput(string text);
		void ReceiveCommandInput(char command);
		void ReceiveSpecialInput(Keys key);

		bool Selected { get; set; } //or Focused
	}

	public class KeyboardDispatcher
	{
		public KeyboardDispatcher(GameWindow window)
		{
			EventInput.Initialize(window);
			EventInput.CharEntered += EventInput_CharEntered;
			EventInput.KeyDown += EventInput_KeyDown;
		}

		void EventInput_KeyDown(object sender, XNAKeyEventArgs e)
		{
			if (_subscriber == null)
				return;

			_subscriber.ReceiveSpecialInput(e.KeyCode);
		}

		void EventInput_CharEntered(object sender, CharacterEventArgs e)
		{
			if (_subscriber == null)
				return;
			if (char.IsControl(e.Character))
			{
				//ctrl-v
				if (e.Character == 0x16)
				{
					//XNA runs in Multiple Thread Apartment state, which cannot recieve clipboard
					Thread thread = new Thread(PasteThread);
					thread.SetApartmentState(ApartmentState.STA);
					thread.Start();
					thread.Join();
					_subscriber.ReceiveTextInput(_pasteResult);
				}
				else
				{
					_subscriber.ReceiveCommandInput(e.Character);
				}
			}
			else
			{
				_subscriber.ReceiveTextInput(e.Character);
			}
		}

		IKeyboardSubscriber _subscriber;
		public IKeyboardSubscriber Subscriber
		{
			get { return _subscriber; }
			set
			{
				if (_subscriber != null)
					_subscriber.Selected = false;
				_subscriber = value;
				if (value != null)
					value.Selected = true;
			}
		}

		//Thread has to be in Single Thread Apartment state in order to receive clipboard
		string _pasteResult = "";
		[STAThread]
		void PasteThread()
		{
			if (System.Windows.Forms.Clipboard.ContainsText())
			{
				_pasteResult = System.Windows.Forms.Clipboard.GetText();
			}
			else
			{
				_pasteResult = "";
			}
		}
	}

	public class XNATextBox : XNAControl, IKeyboardSubscriber
	{
		private readonly Texture2D _textBoxBG;
		private readonly Texture2D _textBoxLeft;
		private readonly Texture2D _textBoxRight;
		private readonly Texture2D _caretTexture;

		private XNALabel _textLabel, _defaultTextLabel;
		private string _actualText;
		
		private bool _selected;
		private int _leftPadding;

		public int MaxChars { get; set; }

		public bool Highlighted { get; set; }

		public bool PasswordBox { get; set; }

		public int LeftPadding
		{
			get { return _leftPadding; }
			set
			{
				_leftPadding = value;
				_textLabel.DrawLocation = new Vector2(_leftPadding, 0);
				_defaultTextLabel.DrawLocation = new Vector2(_leftPadding, 0);
			}
		}

		public string Text
		{
			get
			{
				return _actualText;
			}
			set
			{
				if (MaxChars > 0 && value.Length > MaxChars)
					return;

				_actualText = value;
				_textLabel.Text = PasswordBox ? new string(value.Select(x => '*').ToArray()) : value;
				if (OnTextChanged != null)
					OnTextChanged(this, new EventArgs());

				if (_actualText.Length == 0)
				{
					_textLabel.Visible = false;
					_defaultTextLabel.Visible = true;
				}
				else
				{
					_textLabel.Visible = true;
					_defaultTextLabel.Visible = false;
				}
			}
		}

		public string DefaultText
		{
			get { return _defaultTextLabel.Text; }
			set
			{
				if (MaxChars > 0 && value.Length > MaxChars)
					return;

				_defaultTextLabel.Text = value;
			}
		}

		public Color TextColor
		{
			get { return _textLabel.ForeColor; }
			set { _textLabel.ForeColor = value; }
		}

		public event EventHandler OnFocused;

		public event EventHandler OnEnterPressed;
		public event EventHandler OnTabPressed;
		public event EventHandler OnTextChanged;
		public event EventHandler OnClicked;

		public bool Selected
		{
			get { return _selected; }
			set
			{
				bool oldSel = _selected;
				_selected = value;
				if (!oldSel && _selected && OnFocused != null)
					OnFocused(this, new EventArgs());
			}
		}

		//accepts array with following:
		//	length 4: background texture, leftEnd, rightEnd, caret
		/// <summary>
		/// Construct an XNATextBox UI control.
		/// </summary>
		/// <param name="area">The area of the screen in which the TextBox should be rendered (x, y)</param>
		/// <param name="textures">Array of four textures. 0=background, 1=leftEdge, 2=rightEdge, 3=caret</param>
		/// <param name="spriteFontContentName">Name of the SpriteFont content resource</param>
		public XNATextBox(Rectangle area, Texture2D[] textures, string spriteFontContentName)
			: base(new Vector2(area.X, area.Y), area)
		{
			if (textures.Length != 4)
				throw new ArgumentException("The textures array is invalid. Please pass in an array that contains 4 textures.");
			_textBoxBG = textures[0];
			_textBoxLeft = textures[1];
			_textBoxRight = textures[2];
			_caretTexture = textures[3];

			_setupLabels(area, spriteFontContentName);

			LeftPadding = 0;
			drawArea.Height = _textBoxBG.Height;
		}

		public XNATextBox(Rectangle area, Texture2D cursor, string spriteFontContentName)
			:base(new Vector2(area.X, area.Y), area)
		{
			_textBoxBG = null;
			_textBoxLeft = null;
			_textBoxRight = null;
			_caretTexture = cursor;

			_setupLabels(area, spriteFontContentName);

			LeftPadding = 0;
		}

		private void _setupLabels(Rectangle area, string spriteFontContentName)
		{
			_textLabel = new XNALabel(new Rectangle(0, 0, area.Width, area.Height), spriteFontContentName)
			{
				AutoSize = false,
				BackColor = Color.Transparent,
				ForeColor = Color.Black,
				TextAlign = LabelAlignment.MiddleLeft,
				Visible = true,
				Enabled = true
			};
			_textLabel.SetParent(this);

			_defaultTextLabel = new XNALabel(new Rectangle(0, 0, area.Width, area.Height), spriteFontContentName)
			{
				AutoSize = false,
				BackColor = Color.Transparent,
				ForeColor = Color.FromNonPremultiplied(80, 80, 80, 0xff),
				TextAlign = LabelAlignment.MiddleLeft,
				Visible = true,
				Enabled = true
			};
			_defaultTextLabel.SetParent(this);
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;
			MouseState mouse = Mouse.GetState();
			Point mousePoint = new Point(mouse.X, mouse.Y);

			if (DrawAreaWithOffset.Contains(mousePoint))
			{
				Highlighted = true;
				if (PreviousMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed)
				{
					if (OnClicked != null)
					{
						bool prevSel = Selected;
						OnClicked(this, new EventArgs());

						//if clicking selected the TB
						if (Selected && !prevSel && OnFocused != null)
						{
							OnFocused(this, new EventArgs());
						}
					}
				}
			}
			else
			{
				Highlighted = false;
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			bool caretVisible = !((gameTime.TotalGameTime.TotalMilliseconds % 1000) < 500);

			SpriteBatch.Begin();
						
			//draw bg tiled
			if(_textBoxBG != null)
				SpriteBatch.Draw(_textBoxBG, DrawAreaWithOffset, Color.White);
			
			//draw left side
			if (_textBoxLeft != null)
			{
				Rectangle leftDrawArea = new Rectangle(DrawArea.X, DrawArea.Y, _textBoxLeft.Width, DrawArea.Height);
				SpriteBatch.Draw(_textBoxLeft, leftDrawArea, Color.White);
			}

			//draw right side
			if (_textBoxRight != null)
			{
				Rectangle rightDrawArea = new Rectangle(DrawArea.X + DrawAreaWithOffset.Width - _textBoxRight.Width, DrawAreaWithOffset.Y, _textBoxRight.Width, DrawAreaWithOffset.Height);
				SpriteBatch.Draw(_textBoxRight, rightDrawArea, Color.White);
			}

			if (caretVisible && Selected)
			{
				SpriteBatch.Draw(_caretTexture,
					new Vector2(DrawAreaWithOffset.X + LeftPadding + _textLabel.ActualWidth + 2, DrawAreaWithOffset.Y + 4),
					Color.White);
			}

			SpriteBatch.End();

			base.Draw(gameTime);
		}

		public virtual void ReceiveTextInput(char inputChar)
		{
			Text = Text + inputChar;
		}
		public virtual void ReceiveTextInput(string text)
		{
			Text = Text + text;
		}
		public virtual void ReceiveCommandInput(char command)
		{
			if (!XNAControls.IgnoreEnterForDialogs && Dialogs.Count != 0 && Dialogs.Peek() != TopParent as XNADialog)
				return;

			switch (command)
			{
				case '\b': //backspace
					if (Text.Length > 0)
						Text = Text.Substring(0, Text.Length - 1);
					break;
				case '\r': //return
					if (OnEnterPressed != null)
						OnEnterPressed(this, new EventArgs());
					break;
				case '\t': //tab
					if (OnTabPressed != null)
						OnTabPressed(this, new EventArgs());
					break;
			}
		}
		public virtual void ReceiveSpecialInput(Keys key)
		{

		}
	}
}