using System;

namespace XNAControls.Input
{
    [Flags]
    public enum EventType
    {
        None = 0,
        MouseOver = 1,
        MouseEnter = 2,
        MouseLeave = 4,
        DragStart = 8,
        DragEnd = 16,
        Drag = 32,
        Click = 64,
        DoubleClick = 128,
        KeyTyped = 256,
        GotFocus = 512,
        LostFocus = 1024,
        All = 0x7ff
    }
}
