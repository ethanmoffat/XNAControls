using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;

namespace XNAControls
{
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

        public XNAHyperLink(string spriteFontContentName)
            : base(spriteFontContentName) { }

        public override void Initialize()
        {
            OnMouseEnter += MouseEnterControl;
            OnMouseLeave += MouseLeaveControl;

            base.Initialize();
        }

        protected override void HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            OnClick?.Invoke(control, eventArgs);
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
