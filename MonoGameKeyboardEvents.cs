﻿//// Original Work Copyright (c) Ethan Moffat 2014-2016
//// This file is subject to the GPL v2 License
//// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace XNAControls
{
	internal sealed class MonoGameKeyboardEvents : IKeyboardEvents
	{
		public event CharEnteredHandler CharEntered;

		private readonly GameWindow _window;

		public MonoGameKeyboardEvents(GameWindow window)
		{
			_window = window;
#if !MONO
			if (CharEntered != null) //hide warning for "member is not used"
				CharEntered(null, null);
#else
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
