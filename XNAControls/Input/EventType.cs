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
        None = 0x0000,
        /// <summary>
        /// MouseOver event
        /// </summary>
        MouseOver = 0x0001,
        /// <summary>
        /// MouseEnter event
        /// </summary>
        MouseEnter = 0x0002,
        /// <summary>
        /// MouseLeave event
        /// </summary>
        MouseLeave = 0x0004,
        /// <summary>
        /// DragStart event
        /// </summary>
        DragStart = 0x0008,
        /// <summary>
        /// DragEnd event
        /// </summary>
        DragEnd = 0x0010,
        /// <summary>
        /// Drag event
        /// </summary>
        Drag = 0x0020,
        /// <summary>
        /// Click event
        /// </summary>
        Click = 0x0040,
        /// <summary>
        /// Double click event
        /// </summary>
        DoubleClick = 0x0080,
        /// <summary>
        /// Key typed event
        /// </summary>
        KeyTyped = 0x0100,
        /// <summary>
        /// Got focus event
        /// </summary>
        GotFocus = 0x0200,
        /// <summary>
        /// Lost focus event
        /// </summary>
        LostFocus = 0x0400,
        /// <summary>
        /// MouseWheelMoved event
        /// </summary>
        MouseWheelMoved = 0x0800,
        /// <summary>
        /// KeyPressed event
        /// </summary>
        KeyPressed = 0x1000,
        /// <summary>
        /// KeyReleased event
        /// </summary>
        KeyReleased = 0x2000,
        /// <summary>
        /// MouseDown event
        /// </summary>
        MouseDown = 0x4000,
        /// <summary>
        /// MouseUp event
        /// </summary>
        MouseUp = 0x8000,
        /// <summary>
        /// All events
        /// </summary>
        All = 0xFFFF
    }
}
