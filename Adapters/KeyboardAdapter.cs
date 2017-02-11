// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Input;

namespace XNAControls.Adapters
{
    internal class KeyboardAdapter : IKeyboardAdapter
    {
        static KeyboardAdapter()
        {
            Singleton<IKeyboardAdapter, KeyboardAdapter>.Map();
        }

        public KeyboardState State => Keyboard.GetState();
    }

    internal interface IKeyboardAdapter
    {
        KeyboardState State { get; }
    }
}
