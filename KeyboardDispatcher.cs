﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using Microsoft.Xna.Framework;

namespace XNAControls
{
	public class KeyboardDispatcher : IDisposable
	{
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

		public KeyboardDispatcher(GameWindow window)
		{
#if !MONO
			_events = new Win32KeyboardEvents(window);
			_events.CharEntered += EventInput_CharEntered;
			_events.KeyDown += EventInput_KeyDown;
#endif
		}

		private void EventInput_KeyDown(object sender, XNAKeyEventArgs e)
		{
			if (_subscriber == null)
				return;

			_subscriber.ReceiveSpecialInput(e.KeyCode);
		}

		private void EventInput_CharEntered(object sender, CharEnteredEventArgs e)
		{
			if (_subscriber == null)
				return;

			if (char.IsControl(e.Character))
			{
				//ctrl-v
				if (e.Character == 0x16)
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
				_events.KeyDown -= EventInput_KeyDown;
			}
		}

		#endregion
	}
}