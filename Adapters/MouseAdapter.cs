// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Input;

namespace XNAControls.Adapters
{
    internal class MouseAdapter : IMouseAdapter
    {
        static MouseAdapter()
        {
            Singleton<IMouseAdapter, MouseAdapter>.Map();
        }

        public MouseState State => Mouse.GetState();
    }

    internal interface IMouseAdapter
    {
        MouseState State { get; }
    }
}
