using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;

namespace XNAControls
{
    /// <summary>
    /// Represents a hyperlink control
    /// </summary>
    public class XNAHyperLink : XNALabel, IXNAHyperLink
    {
        private Color _temporaryForeColor;

        /// <inheritdoc />
        public Color MouseOverColor { get; set; }

        /// <inheritdoc />
        public event EventHandler<MouseEventArgs> OnMouseDown = delegate { };

        /// <inheritdoc />
        public event EventHandler<MouseEventArgs> OnMouseUp = delegate { };

        /// <inheritdoc />
        public event EventHandler<MouseEventArgs> OnClick = delegate { };

        /// <summary>
        /// Create a new hyperlink control with the given sprite font name (content name)
        /// </summary>
        public XNAHyperLink(string spriteFontContentName)
            : base(spriteFontContentName) { }

        /// <inheritdoc />
        public override void Initialize()
        {
            OnMouseEnter += MouseEnterControl;
            OnMouseLeave += MouseLeaveControl;

            base.Initialize();
        }

        /// <inheritdoc />
        protected override bool HandleMouseDown(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (OnMouseDown == null)
                return false;

            OnMouseDown(control, eventArgs);

            return true;
        }

        /// <inheritdoc />
        protected override bool HandleMouseUp(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (OnMouseUp == null)
                return false;

            OnMouseUp(control, eventArgs);

            return true;
        }

        /// <inheritdoc />
        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (OnClick == null)
                return false;

            OnClick?.Invoke(control, eventArgs);

            return true;
        }

        private void MouseEnterControl(object sender, MouseStateExtended e)
        {
            _temporaryForeColor = ForeColor;
            ForeColor = MouseOverColor;
        }

        private void MouseLeaveControl(object sender, MouseStateExtended e)
        {
            ForeColor = _temporaryForeColor;
            _temporaryForeColor = Color.Transparent;
        }
    }

    /// <summary>
    /// Interface for a hyperlink control
    /// </summary>
    public interface IXNAHyperLink : IXNALabel
    {
        /// <summary>
        /// Color for the link when the mouse is over the link
        /// </summary>
        Color MouseOverColor { get; set; }

        /// <summary>
        /// Invoked when a mouse button is pressed on a button control
        /// </summary>
        event EventHandler<MouseEventArgs> OnMouseDown;

        /// <summary>
        /// Invoked when a mouse button is released on a button control
        /// </summary>
        event EventHandler<MouseEventArgs> OnMouseUp;

        /// <summary>
        /// Invoked when the link is clicked
        /// </summary>
        event EventHandler<MouseEventArgs> OnClick;
    }
}
