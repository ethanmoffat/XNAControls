using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls.Input;

namespace XNAControls
{
    public abstract class XNAControl : DrawableGameComponent, IXNAControl
    {
        static XNAControl()
        {
            Singleton<GameRepository>.MapIfMissing(new GameRepository());
        }

        private readonly Queue<(EventType, object)> _eventQueue;

        private readonly List<IXNAControl> _children;

        private readonly InputTargetFinder _inputTargetFinder;

        protected readonly SpriteBatch _spriteBatch;

        private bool _disposed;

        public int ZOrder => DrawOrder;

        public virtual Rectangle EventArea => DrawAreaWithParentOffset;

        /// <summary>
        /// Returns true if the default game is active (i.e. has focus), false otherwise
        /// </summary>
        public virtual bool GameIsActive => Game.IsActive;

        /// <summary>
        /// Returns true if the mouse is currently over this control
        /// </summary>
        public bool MouseOver { get; private set; }

        /// <summary>
        /// Returns true if the mouse was over the control during the last Update()
        /// </summary>
        public bool MouseOverPreviously { get; private set; }

        /// <inheritdoc />
        public EventType HandlesEvents { get; set; } = EventType.All;

        /// <summary>
        /// The X,Y coordinates of this control, based on DrawArea
        /// </summary>
        public virtual Vector2 DrawPosition
        {
            get => new Vector2(DrawArea.X, DrawArea.Y);
            set => DrawArea = new Rectangle((int) value.X, (int) value.Y, DrawArea.Width, DrawArea.Height);
        }

        /// <summary>
        /// The X,Y coordinates of this control, based on the parent control's X,Y coordinates (if any)
        /// </summary>
        public Vector2 DrawPositionWithParentOffset => new Vector2(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y);

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
                var parentLocationX = ImmediateParent?.DrawAreaWithParentOffset.X ?? 0;
                var parentLocationY = ImmediateParent?.DrawAreaWithParentOffset.Y ?? 0;

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
        public IReadOnlyList<IXNAControl> ChildControls => _children;

        /// <summary>
        /// True to force the control to stay within the client's window bounds, false otherwise.
        /// Default: true
        /// </summary>
        public bool KeepInClientWindowBounds { get; set; } = true;

        /// <summary>
        /// Event that is invoked when the mouse is over the control
        /// </summary>
        public event EventHandler<MouseStateExtended> OnMouseOver = delegate { };

        /// <summary>
        /// Event that is invoked when the mouse first enters the control
        /// </summary>
        public event EventHandler<MouseStateExtended> OnMouseEnter = delegate { };

        /// <summary>
        /// Event that is invoked when the mouse first leaves the control
        /// </summary>
        public event EventHandler<MouseStateExtended> OnMouseLeave = delegate { };

        protected XNAControl()
            : base(GameRepository.GetGame())
        {
            _eventQueue = new Queue<(EventType, object)>();
            _children = new List<IXNAControl>();
            _inputTargetFinder = new InputTargetFinder();

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        #region Public Interface

        /// <summary>
        /// Add this control to the components of the default game. 
        /// Prerequisite: game must be registered using GameRepository.SetGame(). 
        /// NOTE: Adding to the game's components automatically calls Initialize()
        /// </summary>
        public void AddControlToDefaultGame()
        {
            SetControlUnparented();
            AddToGameComponents(this);
        }

        /// <summary>
        /// Set the immediate parent control of this control. XNAControl parents take ownership of Draw()ing, Update()ing, and Dispose()ing their children.
        /// </summary>
        /// <param name="parent">The parent control to set. Must not be null.</param>
        public virtual void SetParentControl(IXNAControl parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent), "Parent should not be null. To unset a parent, use the method SetControlUnparented()");
            if (parent == this)
                throw new ArgumentException("Error: cannot set a control as its own parent!", nameof(parent));

            if (ImmediateParent != null && ImmediateParent.ChildControls.Contains(this))
                SetControlUnparented();

            ImmediateParent = parent;
            ((XNAControl)ImmediateParent)._children.Add(this);
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

            ((XNAControl)ImmediateParent)._children.Remove(this);
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

        #endregion

        #region GameComponent overrides

        /// <summary>
        /// Default game component Update() method. To override, use OnUpdateControl in derived classes. 
        /// Default logic of this method captures begin/end keyboard and mouse state and assigns the protected members
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            OnUnconditionalUpdateControl(gameTime);

            if (!ShouldUpdate()) return;

            OnUpdateControl(gameTime);

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

        public virtual void SendMessage(EventType eventType, object eventArgs)
        {
            // queue handling of the event so it happens as part of this control's update loop
            _eventQueue.Enqueue((eventType, eventArgs));
        }

        /// <summary>
        /// Does unconditional update logic for the control (this logic ALWAYS runs, regardless of the implementation of ShouldUpdate())
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void OnUnconditionalUpdateControl(GameTime gameTime)
        {
            foreach (var child in _children.OfType<XNAControl>().OrderBy(x => x.UpdateOrder))
                child.OnUnconditionalUpdateControl(gameTime);
        }

        /// <summary>
        /// Does the update logic for the control if the Update should occur (see ShouldUpdate() method) 
        /// Base class update logic checks and fires events MouseEnter, MouseLeave, and MouseOver 
        /// It also updates all child controls and ensures parent controls stay within bounds of the window
        /// </summary>
        protected virtual void OnUpdateControl(GameTime gameTime)
        {
            foreach (var child in _children.OrderBy(x => x.UpdateOrder))
                child.Update(gameTime);

            while (_eventQueue.Any())
            {
                var (messageType, messageArgs) = _eventQueue.Dequeue();
                HandleEvent(messageType, messageArgs);
            }

            if (KeepInClientWindowBounds && TopParent == null && Game.Window != null)
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

            MouseOverPreviously = MouseOver;
        }

        /// <summary>
        /// Does the draw logic for the control if the Draw should occur (see ShouldDraw method)
        /// Base class draw logic calls draw for all immediate child controls
        /// </summary>
        protected virtual void OnDrawControl(GameTime gameTime)
        {
            foreach (var child in _children.OrderBy(x => x.DrawOrder))
                child.Draw(gameTime);
        }

        #endregion

        #region Events

        protected virtual bool HandleEvent(EventType eventType, object eventArgs)
        {
            if (ChildControls.Any())
            {
                Point? position = eventArgs switch
                {
                    MouseStateExtended mse => mse.Position,
                    MouseEventArgs args => args.Position,
                    _ => null
                };

                if (position != null)
                {
                    var target = _inputTargetFinder.GetMouseEventTargetControl(ChildControls, position.Value);
                    if (target != null && target.HandlesEvents.HasFlag(eventType))
                    {
                        target.SendMessage(eventType, eventArgs);
                        return true;
                    }
                }
            }

            if (!HandlesEvents.HasFlag(eventType))
                return false;

            var handled = true;

            switch (eventType)
            {
                case EventType.MouseOver:
                    {
                        MouseOver = true;
                        OnMouseOver?.Invoke(this, (MouseStateExtended)eventArgs);
                    }
                    break;
                case EventType.MouseEnter:
                    {
                        MouseOver = true;
                        OnMouseEnter?.Invoke(this, (MouseStateExtended)eventArgs);
                    }
                    break;
                case EventType.MouseLeave:
                    {
                        MouseOver = false;
                        OnMouseLeave?.Invoke(this, (MouseStateExtended)eventArgs);
                    }
                    break;
                case EventType.DragStart: HandleDragStart(this, (MouseEventArgs)eventArgs); break;
                case EventType.DragEnd: HandleDragEnd(this, (MouseEventArgs)eventArgs); break;
                case EventType.Drag: HandleDrag(this, (MouseEventArgs)eventArgs); break;
                case EventType.Click: HandleClick(this, (MouseEventArgs)eventArgs); break;
                case EventType.DoubleClick: HandleDoubleClick(this, (MouseEventArgs)eventArgs);  break;
                case EventType.KeyTyped: HandleKeyTyped(this, (KeyboardEventArgs)eventArgs); break;
                case EventType.GotFocus: HandleGotFocus(this, EventArgs.Empty); break;
                case EventType.LostFocus: HandleLostFocus(this, EventArgs.Empty); break;
                case EventType.MouseWheelMoved: HandleMouseWheelMoved(this, (MouseEventArgs)eventArgs); break;
                default: handled = false; break;
            }

            return handled;
        }

        protected virtual void HandleDragStart(IXNAControl control, MouseEventArgs eventArgs)
        {
        }

        protected virtual void HandleDragEnd(IXNAControl control, MouseEventArgs eventArgs)
        {
        }

        protected virtual void HandleDrag(IXNAControl control, MouseEventArgs eventArgs)
        {
        }

        protected virtual void HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
        }

        protected virtual void HandleDoubleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
        }

        protected virtual void HandleKeyTyped(IXNAControl control, KeyboardEventArgs eventArgs)
        {
        }

        protected virtual void HandleGotFocus(IXNAControl control, EventArgs eventArgs)
        {
        }

        protected virtual void HandleLostFocus(IXNAControl control, EventArgs eventArgs)
        {
        }

        protected virtual void HandleMouseWheelMoved(IXNAControl control, MouseEventArgs eventArgs)
        {
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Returns true if the control should be updated 
        /// Returns false if: Game is inactive, or any dialogs are open and this control is not part of the dialog.
        /// </summary>
        protected virtual bool ShouldUpdate()
        {
            return GameIsActive && Visible && !_disposed;
            //if (!GameIsActive || !Visible || _disposed) return false;

            //var dialogStack = Singleton<DialogRepository>.Instance.OpenDialogs;

            //if (dialogStack.Count <= 0 || this == dialogStack.Peek()) return true;

            //// replacement for IgnoreDialogs: if the dialog is not modal, update
            //if (!dialogStack.Peek().Modal) return true;

            ////return false if:
            ////dialog is open and this control is a top parent OR
            ////dialog is open and this control does not belong to it
            //return TopParent != null && TopParent == dialogStack.Peek();
        }

        /// <summary>
        /// Returns true if the control should be drawn 
        /// Returns false if: control is not visible
        /// </summary>
        protected virtual bool ShouldDraw()
        {
            return Visible && !_disposed;
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
            if (Game.Components == null)
                return;

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
            PrepareForDisposal();

            if (disposing)
            {
                RemoveFromGameComponents(this);

                _spriteBatch.Dispose();

                foreach (var child in _children)
                    child.Dispose();
            }

            base.Dispose(disposing);
        }

        protected void PrepareForDisposal()
        {
            _disposed = true;
        }
    }
}
