//// Original Work Copyright (c) Ethan Moffat 2014-2016
//// This file is subject to the GPL v2 License
//// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace XNAControls
{
	internal sealed class CrossPlatformKeyboardEvents : IKeyboardEvents
	{
		public event CharEnteredHandler CharEntered;
		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;

		private readonly GameWindow _window;

		public CrossPlatformKeyboardEvents(GameWindow window)
		{
			_window = window;
			_window.TextInput += GameWindow_TextInput;
		}

		private void GameWindow_TextInput(object sender, TextInputEventArgs e)
		{
			if (CharEntered != null)
			{
				CharEntered(null, new CharEnteredEventArgs(e.Character));
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

			_window.TextInput -= GameWindow_TextInput;
		}
	}
}
