using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace XNAControls
{
    public interface IXNAControl : IGameComponent, IDrawable, IUpdateable, IDisposable
    {
        /// <summary>
        /// Returns true if the default game is active (i.e. has focus), false otherwise
        /// </summary>
        bool GameIsActive { get; }

        /// <summary>
        /// Returns true if the mouse is currently over this control
        /// </summary>
        bool MouseOver { get; }

        /// <summary>
        /// Returns true if the mouse was over the control during the last Update()
        /// </summary>
        bool MouseOverPreviously { get; }

        /// <summary>
        /// The X,Y coordinates of this control, based on DrawArea
        /// </summary>
        Vector2 DrawPosition { get; set; }

        /// <summary>
        /// The X,Y coordinates of this control, based on the parent control's X,Y coordinates (if any)
        /// </summary>
        Vector2 DrawPositionWithParentOffset { get; }

        /// <summary>
        /// The draw area of this control. Use to set position and size of this control relative to the parent.
        /// </summary>
        Rectangle DrawArea { get; set; }

        /// <summary>
        /// The draw area of this control, based on the parent control's X,Y coordinates (if any)
        /// </summary>
        Rectangle DrawAreaWithParentOffset { get; }

        /// <summary>
        /// The immediate parent control of this control. null if no parent exists
        /// </summary>
        IXNAControl ImmediateParent { get; }

        /// <summary>
        /// The top-most parent control of this control in the hierarchy. null if no parent exists
        /// </summary>
        IXNAControl TopParent { get; }

        /// <summary>
        /// A list of all child controls of this control
        /// </summary>
        IReadOnlyList<IXNAControl> ChildControls { get; }

        /// <summary>
        /// Event that is invoked when the mouse is over the control
        /// </summary>
        event EventHandler OnMouseOver;

        /// <summary>
        /// Event that is invoked when the mouse first enters the control
        /// </summary>
        event EventHandler OnMouseEnter;

        /// <summary>
        /// Event that is invoked when the mouse first leaves the control
        /// </summary>
        event EventHandler OnMouseLeave;

        /// <summary>
        /// True to force the control to stay within the client's window bounds, false otherwise.
        /// Default: true
        /// </summary>
        bool KeepInClientWindowBounds { get; set; }

        /// <summary>
        /// Add this control to the components of the default game. 
        /// Prerequisite: game must be registered using GameRepository.SetGame(). 
        /// NOTE: Adding to the game's components automatically calls Initialize()
        /// </summary>
        void AddControlToDefaultGame();

        /// <summary>
        /// Set the immediate parent control of this control. XNAControl parents take ownership of Draw()ing, Update()ing, and Dispose()ing their children.
        /// </summary>
        /// <param name="parent">The parent control to set. Must not be null.</param>
        void SetParentControl(IXNAControl parent);

        /// <summary>
        /// Unset the immediate parent control of this control. Sets ImmediateParent to null. 
        /// Note: this method will not automatically add the control back to the game's components. 
        ///       The user is responsible for re-adding this component to the Game's controls if they desire to have it automatically drawn/updated by the framework.
        /// </summary>
        void SetControlUnparented();

        /// <summary>
        /// Set the DrawOrder property for this control. Updates all child controls of this control with the new draw order.
        /// </summary>
        /// <param name="drawOrder">The new draw order.</param>
        void SetDrawOrder(int drawOrder);

        /// <summary>
        /// Called when a control should not be click dragged because another control is being dragged on top of it. Example: object being drag/droped to dialog
        /// </summary>
        /// <param name="suppress">True if dragging should be disabled (suppressed), false to enable dragging</param>
        void SuppressClickDragEvent(bool suppress);

        /// <summary>
        /// Called when a child control is being dragged and the parent should not respond to click drag.  Example: scroll bar being dragged within a dialog
        /// </summary>
        /// <param name="suppress">True if parent dragging should be disabled (suppressed), false to enable dragging</param>
        void SuppressParentClickDragEvent(bool suppress);
    }
}
