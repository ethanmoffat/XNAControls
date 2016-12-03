// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
    public abstract class XNAControl : DrawableGameComponent, IXNAControl
    {
        private readonly List<IXNAControl> _children;
        protected readonly SpriteBatch _spriteBatch;

        protected bool ShouldClickDrag { get; private set; }

        private MouseState _currentMouseState, _previousMouseState;
        private KeyboardState _currentKeyState, _previousKeyState;

        protected MouseState CurrentMouseState { get { return _currentMouseState; } }
        protected MouseState PreviousMouseState { get { return _previousMouseState; } }

        protected KeyboardState CurrentKeyState { get { return _currentKeyState; } }
        protected KeyboardState PreviousKeyState { get { return _previousKeyState; } }

        /// <summary>
        /// Returns true if the mouse is currently over this control
        /// </summary>
        public bool MouseOver
        {
            get
            {
                return DrawAreaWithParentOffset.ContainsPoint(CurrentMouseState.X, CurrentMouseState.Y);
            }
        }

        /// <summary>
        /// Returns true if the mouse was over the control during the last Update()
        /// </summary>
        public bool MouseOverPreviously
        {
            get
            {
                return DrawAreaWithParentOffset.ContainsPoint(PreviousMouseState.X, PreviousMouseState.Y);
            }
        }

        /// <summary>
        /// The X,Y coordinates of this control, based on DrawArea
        /// </summary>
        public virtual Vector2 DrawPosition
        {
            get { return new Vector2(DrawArea.X, DrawArea.Y); }
            set { DrawArea = new Rectangle((int) value.X, (int) value.Y, DrawArea.Width, DrawArea.Height); }
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
        public virtual Rectangle DrawArea { get; set; }

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

        /// <summary>
        /// Event that is invoked when the mouse is over the control
        /// </summary>
        public event EventHandler OnMouseOver = delegate { };

        /// <summary>
        /// Event that is invoked when the mouse first enters the control
        /// </summary>
        public event EventHandler OnMouseEnter = delegate { };

        /// <summary>
        /// Event that is invoked when the mouse first leaves the control
        /// </summary>
        public event EventHandler OnMouseLeave = delegate { };

        protected XNAControl()
            : base(GameRepository.GetGame())
        {
            _children = new List<IXNAControl>();

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            _currentKeyState = _previousKeyState = Keyboard.GetState();
            _currentMouseState = _previousMouseState = Mouse.GetState();
        }

        #region Public Interface

        /// <summary>
        /// Add this control to the components of the default game. 
        /// Prerequisite: game must be registered using GameRepository.SetGame(). 
        /// NOTE: Adding to the game's components automatically calls Initialize()
        /// </summary>
        public void AddControlToDefaultGame()
        {
            AddToGameComponents(this);

            foreach (var child in ChildControls)
                RemoveFromGameComponents(child);
        }

        /// <summary>
        /// Set the immediate parent control of this control. XNAControl parents take ownership of Draw()ing, Update()ing, and Dispose()ing their children.
        /// </summary>
        /// <param name="parent">The parent control to set. Must not be null.</param>
        public virtual void SetParentControl(IXNAControl parent)
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
        public virtual void SetControlUnparented()
        {
            if (ImmediateParent == null) return;

            ((List<IXNAControl>) ImmediateParent.ChildControls).Remove(this);
            ImmediateParent = null;
        }

        /// <summary>
        /// Set the DrawOrder property for this control. Updates all child controls of this control with the new draw order.
        /// </summary>
        /// <param name="drawOrder">The new draw order.</param>
        public void SetDrawOrder(int drawOrder)
        {
            DrawOrder = drawOrder;

            foreach (var childControl in ChildControls)
                UpdateDrawOrderBasedOnParent(this, childControl);
        }

        /// <summary>
        /// Called when a child control is being dragged and the parent should not respond to click drag.  Example: scroll bar being dragged within a dialog
        /// </summary>
        /// <param name="suppress">True if parent dragging should be disabled (suppressed), false to enable dragging</param>
        public void SuppressParentClickDragEvent(bool suppress)
        {
            if (ImmediateParent == null)
                return;

            ((XNAControl)ImmediateParent).ShouldClickDrag = !suppress;
            ImmediateParent.SuppressParentClickDragEvent(suppress);
        }

        #endregion

        #region GameComponent overrides

        /// <summary>
        /// Default game component Update() method. To override, use OnUpdateControl in derived classes. 
        /// Default logic of this method captures begin/end keyboard and mouse state and assigns the protected members
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (!ShouldUpdate()) return;

            _currentKeyState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();

            OnUpdateControl(gameTime);

            _previousKeyState = _currentKeyState;
            _previousMouseState = _currentMouseState;

            base.Update(gameTime);
        }

        /// <summary>
        /// Default game component Draw() method. To override, use OnDrawControl in derived classes.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (!ShouldDraw()) return;

            OnDrawControl(gameTime);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Does the update logic for the control if the Update should occur (see ShouldUpdate() method) 
        /// Base class update logic checks and fires events MouseEnter, MouseLeave, and MouseOver 
        /// It also updates all child controls and ensures parent controls stay within bounds of the window
        /// </summary>
        protected virtual void OnUpdateControl(GameTime gameTime)
        {
            if (MouseOver)
                OnMouseOver(this, EventArgs.Empty);

            if (MouseOver && !MouseOverPreviously)
                OnMouseEnter(this, EventArgs.Empty);
            else if (!MouseOver && MouseOverPreviously)
                OnMouseLeave(this, EventArgs.Empty);

            foreach (var child in _children)
                child.Update(gameTime);

            if (TopParent == null && Game.Window != null)
            {
                var clientBounds = Game.Window.ClientBounds;
                if (clientBounds.Width > 0 && clientBounds.Height > 0)
                {
                    var rightBound = clientBounds.Width - DrawArea.Width;
                    var newX = DrawArea.X < 0
                        ? 0
                        : DrawArea.X > rightBound
                            ? rightBound
                            : DrawArea.X;

                    var bottomBound = clientBounds.Height - DrawArea.Height;
                    var newY = DrawArea.Y < 0
                        ? 0
                        : DrawArea.Y > bottomBound
                            ? bottomBound
                            : DrawArea.Y;

                    DrawArea = new Rectangle(newX, newY, DrawArea.Width, DrawArea.Height);
                }
            }
        }

        /// <summary>
        /// Does the draw logic for the control if the Draw should occur (see ShouldDraw method)
        /// Base class draw logic calls draw for all immediate child controls
        /// </summary>
        protected virtual void OnDrawControl(GameTime gameTime)
        {
            foreach (var child in _children)
                child.Draw(gameTime);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Returns true if the control should be updated 
        /// Returns false if: Game is inactive, or any dialogs are open and this control is not part of the dialog.
        /// </summary>
        protected virtual bool ShouldUpdate()
        {
            if (!Game.IsActive || !Visible) return false;

            var dialogStack = Singleton<DialogRepository>.Instance.OpenDialogs;

            if (dialogStack.Count <= 0) return true;

            //todo: ignore dialogs? old logic:
            //if (Visible && Dialogs.Count > 0 && IgnoreDialogs.Contains(Dialogs.Peek().GetType()))
            //    return true;

            //return false if:
            //dialog is open and this control is a top parent OR
            //dialog is open and this control does not belong to it
            return TopParent != null && TopParent == dialogStack.Peek();
        }

        /// <summary>
        /// Returns true if the control should be drawn 
        /// Returns false if: control is not visible
        /// </summary>
        protected virtual bool ShouldDraw()
        {
            return Visible;
        }

        protected void SetSize(int newWidth, int newHeight)
        {
            DrawArea = new Rectangle(DrawArea.X, DrawArea.Y, newWidth, newHeight);
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
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveFromGameComponents(this);

                _spriteBatch.Dispose();

                foreach (var child in _children)
                    child.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
