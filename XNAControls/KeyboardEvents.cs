//// Original Work Copyright (c) Ethan Moffat 2014-2016
//// This file is subject to the GPL v2 License
//// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace XNAControls
{
    internal sealed class KeyboardEvents : IKeyboardEvents
    {
        public event CharEnteredHandler CharEntered;

        private readonly GameWindow _window;

        public KeyboardEvents(GameWindow window)
        {
            _window = window;
            _window.TextInput += GameWindow_TextInput;
        }

        private void GameWindow_TextInput(object sender, TextInputEventArgs e)
        {
            CharEntered?.Invoke(null, new CharEnteredEventArgs(e.Character));
        }

        ~KeyboardEvents()
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
