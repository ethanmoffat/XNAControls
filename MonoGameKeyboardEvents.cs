//// Original Work Copyright (c) Ethan Moffat 2014-2016
//// This file is subject to the GPL v2 License
//// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace XNAControls
{
	internal sealed class MonoGameKeyboardEvents : IKeyboardEvents
	{
		public event CharEnteredHandler CharEntered;
		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;

		private readonly GameWindow _window;

		public MonoGameKeyboardEvents(GameWindow window)
		{
			_window = window;
#if MONO
			_window.TextInput += GameWindow_TextInput;
#endif
		}

#if MONO
		private void GameWindow_TextInput(object sender, TextInputEventArgs e)
		{
			if (CharEntered != null)
			{
				CharEntered(null, new CharEnteredEventArgs(e.Character));
			}
		}

		~MonoGameKeyboardEvents()
		{
			Dispose(false);
		}
#endif

		public void Dispose()
		{
#if !MONO
		}
#else
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (!disposing) return;

			_window.TextInput -= GameWindow_TextInput;
		}
#endif
	}
}
