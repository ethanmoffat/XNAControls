//// Original Work Copyright (c) Ethan Moffat 2014-2016
//// This file is subject to the GPL v2 License
//// For additional details, see the LICENSE file

using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace XNAControls
{
	internal sealed class CrossPlatformKeyboardEvents : IKeyboardEvents
	{
		private readonly Control _gameWindowControl;

		public event CharEnteredHandler CharEntered;
		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;

		public CrossPlatformKeyboardEvents(GameWindow window)
		{
			_gameWindowControl = Control.FromHandle(window.Handle);
			_gameWindowControl.KeyDown += GameWindow_KeyDown;
			_gameWindowControl.KeyUp += GameWindow_KeyUp;
			_gameWindowControl.KeyPress += GameWindow_KeyPress;
		}

		private void GameWindow_KeyDown(object sender, KeyEventArgs e)
		{
			if (KeyDown != null)
			{
				KeyDown(null, new XNAKeyEventArgs((Keys)e.KeyValue));
				e.Handled = true;
			}
		}

		private void GameWindow_KeyUp(object sender, KeyEventArgs e)
		{
			if (KeyUp != null)
			{
				KeyUp(null, new XNAKeyEventArgs((Keys) e.KeyValue));
				e.Handled = true;
			}
		}

		private void GameWindow_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (CharEntered != null)
			{
				CharEntered(null, new CharEnteredEventArgs(e.KeyChar));
				e.Handled = true;
			}
		}

		~CrossPlatformKeyboardEvents()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing) return;

			_gameWindowControl.KeyDown -= GameWindow_KeyDown;
			_gameWindowControl.KeyUp -= GameWindow_KeyUp;
			_gameWindowControl.KeyPress -= GameWindow_KeyPress;
		}
	}
}
