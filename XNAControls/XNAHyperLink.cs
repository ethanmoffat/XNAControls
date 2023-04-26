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

        /// <summary>
        /// Color for the link when the mouse is over the link
        /// </summary>
        public Color MouseOverColor { get; set; }

        /// <summary>
        /// Event that is invoked when the link is clicked
        /// </summary>
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
        /// Event that is invoked when the link is clicked
        /// </summary>
        event EventHandler<MouseEventArgs> OnClick;
    }
}
