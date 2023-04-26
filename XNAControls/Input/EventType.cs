using System;

namespace XNAControls.Input
{
    /// <summary>
    /// Type of the event being fired (flags)
    /// </summary>
    [Flags]
    public enum EventType
    {
        /// <summary>
        /// No event
        /// </summary>
        None = 0,
        /// <summary>
        /// MouseOver event
        /// </summary>
        MouseOver = 1,
        /// <summary>
        /// MouseEnter event
        /// </summary>
        MouseEnter = 2,
        /// <summary>
        /// MouseLeave event
        /// </summary>
        MouseLeave = 4,
        /// <summary>
        /// DragStart event
        /// </summary>
        DragStart = 8,
        /// <summary>
        /// DragEnd event
        /// </summary>
        DragEnd = 16,
        /// <summary>
        /// Drag event
        /// </summary>
        Drag = 32,
        /// <summary>
        /// Click event
        /// </summary>
        Click = 64,
        /// <summary>
        /// Double click event
        /// </summary>
        DoubleClick = 128,
        /// <summary>
        /// Key typed event
        /// </summary>
        KeyTyped = 256,
        /// <summary>
        /// Got focus event
        /// </summary>
        GotFocus = 512,
        /// <summary>
        /// Lost focus event
        /// </summary>
        LostFocus = 1024,
        /// <summary>
        /// MouseWheelMoved event
        /// </summary>
        MouseWheelMoved = 2048,
        /// <summary>
        /// All events
        /// </summary>
        All = 0xfff
    }
}
