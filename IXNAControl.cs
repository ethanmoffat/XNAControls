// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace XNAControls
{
    public interface IXNAControl : IGameComponent, IDrawable, IUpdateable, IDisposable
    {
        Vector2 DrawPosition { get; }
        
        Vector2 DrawPositionWithParentOffset { get; }

        Rectangle DrawArea { get; set; }

        Rectangle DrawAreaWithParentOffset { get; }

        IXNAControl ImmediateParent { get; }

        IXNAControl TopParent { get; }

        IReadOnlyList<IXNAControl> ChildControls { get; }

        void AddControlToDefaultGame();

        void SetParentControl(IXNAControl parent);

        void SetControlUnparented();

        void SetDrawOrder(int drawOrder);
    }
}
