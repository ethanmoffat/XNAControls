using System;

namespace XNAControls
{
    internal delegate void CharEnteredHandler(object sender, CharEnteredEventArgs e);

    internal class CharEnteredEventArgs : EventArgs
    {
        internal char Character { get; }

        internal CharEnteredEventArgs(char character)
        {
            Character = character;
        }
    }
}
