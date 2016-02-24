// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using Microsoft.Xna.Framework;

namespace XNAControls
{
	public enum PlatformTarget
	{
		Windows,
		Linux
	}

	public class KeyboardDispatcher : IDisposable
	{
		private const char CHAR_PASTE_CODE = (char)0x16;
		internal const char CHAR_RETURNKEY_CODE = '\r';
		internal const char CHAR_BACKSPACE_CODE = '\b';
		internal const char CHAR_TAB_CODE = '\t';

		private readonly IKeyboardEvents _events;

		IKeyboardSubscriber _subscriber;
		public IKeyboardSubscriber Subscriber
		{
			get { return _subscriber; }
			set
			{
				SetSubscriberSelected(false);
				_subscriber = value;
				SetSubscriberSelected(true);
			}
		}

		private void SetSubscriberSelected(bool selected)
		{
			if (_subscriber != null)
				_subscriber.Selected = selected;
		}

		public KeyboardDispatcher(GameWindow window, PlatformTarget target)
		{
			switch (target)
			{
				case PlatformTarget.Windows: _events = new Win32KeyboardEvents(window); break;
				case PlatformTarget.Linux: _events = new KeyboardEvents(window); break;
				default: throw new ArgumentOutOfRangeException("target", target, "Invalid platform target. Specify either Windows or Linux.");
			}

			_events.CharEntered += EventInput_CharEntered;
		}

		private void EventInput_CharEntered(object sender, CharEnteredEventArgs e)
		{
			if (_subscriber == null)
				return;

			if (char.IsControl(e.Character))
			{
				//ctrl-v
				if (e.Character == CHAR_PASTE_CODE)
				{
					GetClipboardInfoFromThread();
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

		#region Clipboard Handling

		//XNA runs in Multiple Thread Apartment state, which cannot recieve clipboard
		private void GetClipboardInfoFromThread()
		{
			Thread thread = new Thread(SetPasteResult);
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}

		//Thread has to be in Single Thread Apartment state in order to receive clipboard
		string _pasteResult = "";

		[STAThread]
		void SetPasteResult()
		{
			_pasteResult = System.Windows.Forms.Clipboard.ContainsText() ? System.Windows.Forms.Clipboard.GetText() : "";
		}

		#endregion

		#region IDisposable

		~KeyboardDispatcher()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_events.CharEntered -= EventInput_CharEntered;
				_events.Dispose();
			}
		}

		#endregion
	}
}
