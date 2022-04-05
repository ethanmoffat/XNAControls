using System;

namespace XNAControls
{
    internal interface IKeyboardEvents : IDisposable
    {
        event CharEnteredHandler CharEntered;
    }
}
