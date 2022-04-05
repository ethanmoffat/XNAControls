using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
        public event EventHandler OnClick = delegate { };

        public XNAHyperLink(string spriteFontContentName)
            : base(spriteFontContentName) { }

        /// <summary>
        /// Manually invoke the click event on the link
        /// </summary>
        public void Click()
        {
            OnClick(this, EventArgs.Empty);
        }

        public override void Initialize()
        {
            OnMouseEnter += MouseEnterControl;
            OnMouseLeave += MouseLeaveControl;

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (MouseOver &&
                PreviousMouseState.LeftButton == ButtonState.Pressed &&
                CurrentMouseState.LeftButton == ButtonState.Released)
                OnClick(this, null);

            base.OnUpdateControl(gameTime);
        }

        private void MouseEnterControl(object sender, EventArgs e)
        {
            _temporaryForeColor = ForeColor;
            ForeColor = MouseOverColor;
        }

        private void MouseLeaveControl(object sender, EventArgs e)
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
        event EventHandler OnClick;

        /// <summary>
        /// Manually invoke the click event on the link
        /// </summary>
        void Click();
    }
}
