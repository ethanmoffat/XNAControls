using Microsoft.Xna.Framework.Input;

namespace XNAControls.Adapters
{
    internal class MouseAdapter : IMouseAdapter
    {
        public MouseState State => Mouse.GetState();
    }

    internal interface IMouseAdapter
    {
        MouseState State { get; }
    }
}
