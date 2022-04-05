using Microsoft.Xna.Framework.Input;

namespace XNAControls.Adapters
{
    internal class KeyboardAdapter : IKeyboardAdapter
    {
        public KeyboardState State => Keyboard.GetState();
    }

    internal interface IKeyboardAdapter
    {
        KeyboardState State { get; }
    }
}
