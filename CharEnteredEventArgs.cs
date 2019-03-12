// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
