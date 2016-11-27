// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace XNAControls
{
    public abstract class XNAControl : DrawableGameComponent, IXNAControl
    {
        private readonly List<IXNAControl> _children;

        /// <summary>
        /// The X,Y coordinates of this control, based on DrawArea
        /// </summary>
        public Vector2 DrawPosition
        {
            get { return new Vector2(DrawArea.X, DrawArea.Y); }
        }

        /// <summary>
        /// The X,Y coordinates of this control, based on the parent control's X,Y coordinates (if any)
        /// </summary>
        public Vector2 DrawPositionWithParentOffset
        {
            get { return new Vector2(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y); }
        }

        /// <summary>
        /// The draw area of this control. Use to set position and size of this control relative to the parent.
        /// </summary>
        public Rectangle DrawArea { get; set; }

        /// <summary>
        /// The draw area of this control, based on the parent control's X,Y coordinates (if any)
        /// </summary>
        public Rectangle DrawAreaWithParentOffset
        {
            get
            {
                var parentLocationX = ImmediateParent == null ? 0 : ImmediateParent.DrawAreaWithParentOffset.X;
                var parentLocationY = ImmediateParent == null ? 0 : ImmediateParent.DrawAreaWithParentOffset.Y;

                return new Rectangle(parentLocationX + DrawArea.X,
                    parentLocationY + DrawArea.Y,
                    DrawArea.Width,
                    DrawArea.Height);
            }
        }

        /// <summary>
        /// The immediate parent control of this control. null if no parent exists
        /// </summary>
        public IXNAControl ImmediateParent { get; private set; }

        /// <summary>
        /// The top-most parent control of this control in the hierarchy. null if no parent exists
        /// </summary>
        public IXNAControl TopParent
        {
            get
            {
                if (ImmediateParent == null) return ImmediateParent;

                var parent = ImmediateParent;
                while (parent.ImmediateParent != null)
                    parent = parent.ImmediateParent;

                return parent;
            }
        }

        /// <summary>
        /// A list of all child controls of this control
        /// </summary>
        public IReadOnlyList<IXNAControl> ChildControls { get { return _children; } }

        protected XNAControl()
            : base(GameRepository.GetGame())
        {
            _children = new List<IXNAControl>();
        }

        /// <summary>
        /// Add this control to the components of the default game. 
        /// Prerequisite: game must be registered using GameRepository.SetGame()
        /// </summary>
        public void AddControlToDefaultGame()
        {
            AddToGameComponents(this);

            foreach (var child in ChildControls)
                RemoveFromGameComponents(child);
        }

        /// <summary>
        /// Set the immediate parent control of this control. XNAControls take ownership of Draw()ing, and Update()ing their children.
        /// </summary>
        /// <param name="parent">The parent control to set. Must not be null.</param>
        public void SetParentControl(IXNAControl parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent", "Parent should not be null. To unset a parent, use the method SetControlUnparented()");

            if (ImmediateParent != null && ImmediateParent.ChildControls.Contains(this))
            {
                ImmediateParent.SetControlUnparented();
            }

            ImmediateParent = parent;
            ((List<IXNAControl>) ImmediateParent.ChildControls).Add(this);
            RemoveFromGameComponents(this);

            UpdateDrawOrderBasedOnParent(ImmediateParent, this);
        }

        /// <summary>
        /// Unset the immediate parent control of this control. Sets ImmediateParent to null. 
        /// Note: this method will not automatically add the control back to the game's components. 
        ///       The user is responsible for re-adding this component to the Game's controls if they desire to have it automatically drawn/updated by the framework.
        /// </summary>
        public void SetControlUnparented()
        {
            if (ImmediateParent == null) return;

            ((List<IXNAControl>) ImmediateParent.ChildControls).Remove(this);
            ImmediateParent = null;
        }

        public void SetDrawOrder(int drawOrder)
        {
            DrawOrder = drawOrder;
        }

        private void AddToGameComponents(IXNAControl control)
        {
            if (!Game.Components.Contains(control))
                Game.Components.Add(control);
        }

        private void RemoveFromGameComponents(IXNAControl control)
        {
            if (Game.Components.Contains(control))
                Game.Components.Remove(control);
        }

        private static void UpdateDrawOrderBasedOnParent(IXNAControl parent, IXNAControl child)
        {
            child.SetDrawOrder(parent.DrawOrder + 1);

            foreach (var childControl in child.ChildControls)
                UpdateDrawOrderBasedOnParent(child, childControl);
        }
    }
}
