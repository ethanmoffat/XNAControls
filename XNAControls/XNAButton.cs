using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace XNAControls
{
    public class XNAButton : XNAControl, IXNAButton
    {
        public enum ButtonFlashBehavior
        {
            /// <summary>
            /// Button will continue flashing even on mouseover
            /// </summary>
            FlashOnMouseOver,
            /// <summary>
            /// Button will not continue flashing on mouseover
            /// </summary>
            DoNotFlashOnMouseOver,
        }

        private readonly Texture2D _sheet;
        private readonly Rectangle _outSource;
        private readonly Rectangle _overSource;

        private Rectangle _sourceRect;
        private bool _dragging;

        private long _lastFlashTick;

        /// <summary>
        /// Invoked when the button control is clicked once
        /// </summary>
        public event EventHandler OnClick = delegate { };

        /// <summary>
        /// Invoked when the button control is being dragged
        /// </summary>
        public event EventHandler OnClickDrag = delegate { };

        /// <summary>
        /// Set the FlashSpeed which causes the over/out textures to cycle once every 'FlashSpeed' milliseconds
        /// </summary>
        public int? FlashSpeed { get; set; }

        /// <summary>
        /// Set the behavior of the button when FlashSpeed is set
        /// </summary>
        public ButtonFlashBehavior FlashBehavior { get; set; }

        /// <summary>
        /// Get/set the area that should respond to a click event relative to the top-left corner of this control.
        /// </summary>
        public Rectangle ClickArea { get; set; }

        /// <summary>
        /// Gets the click area of the control offset based on the control and all parent's X,Y coordinates
        /// </summary>
        protected Rectangle ClickAreaWithOffset =>
            new Rectangle(ClickArea.X + DrawAreaWithParentOffset.X, ClickArea.Y + DrawAreaWithParentOffset.Y,
                          ClickArea.Width, ClickArea.Height);

        /// <summary>
        /// Construct a button where the textures for over/out are a part of a sprite sheet.
        /// </summary>
        /// <param name="sheet">The sprite sheet texture</param>
        /// <param name="location">Location to draw the button</param>
        /// <param name="outSource">Source within the sprite sheet that contains the texture to draw on MouseOut</param>
        /// <param name="overSource">Source within the sprite sheet that contains the texture to draw on MouseOver</param>
        public XNAButton(Texture2D sheet, Vector2 location, Rectangle outSource, Rectangle overSource)
        {
            if (outSource == null)
                throw new ArgumentNullException(nameof(outSource));
            if (overSource == null)
                throw new ArgumentNullException(nameof(outSource));
            _sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
            _outSource = outSource;
            _overSource = overSource;

            var largerArea = outSource.Size.X*outSource.Size.Y >= overSource.Size.X*overSource.Size.Y
                ? outSource
                : overSource;

            _sourceRect = outSource;

            DrawArea = new Rectangle((int)location.X, (int)location.Y, largerArea.Width, largerArea.Height);
            ClickArea = new Rectangle(0, 0, DrawArea.Width, DrawArea.Height);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (MouseOver && ClickAreaWithOffset.ContainsPoint(CurrentMouseState.X, CurrentMouseState.Y)
                && PreviousMouseState.LeftButton == ButtonState.Pressed
                && CurrentMouseState.LeftButton == ButtonState.Released)
                OnClick(this, EventArgs.Empty);

            if (MouseOver && PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Pressed
                && ShouldClickDrag && !_dragging)
            {
                SuppressParentClickDragEvent(true);
                _dragging = true;
            }
            else if (PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released && _dragging)
            {
                _dragging = false;
                SuppressParentClickDragEvent(false);
            }

            if (_dragging)
                OnClickDrag(this, EventArgs.Empty);

            if (FlashSpeed != null && (FlashBehavior == ButtonFlashBehavior.FlashOnMouseOver || !MouseOver))
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastFlashTick > FlashSpeed)
                {
                    _lastFlashTick = (long)gameTime.TotalGameTime.TotalMilliseconds;
                    _sourceRect = _sourceRect.Equals(_overSource) ? _outSource : _overSource;
                }
            }
            else
            {
                _sourceRect = MouseOver ? _overSource : _outSource;
            }

            base.OnUpdateControl(gameTime);
        }
        
        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            if (_sheet != null)
                _spriteBatch.Draw(_sheet, DrawAreaWithParentOffset, _sourceRect, Color.White);

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }
    }

    public interface IXNAButton : IXNAControl
    {
        /// <summary>
        /// Invoked when the button control is clicked once
        /// </summary>
        event EventHandler OnClick;

        /// <summary>
        /// Invoked when the button control is being dragged
        /// </summary>
        event EventHandler OnClickDrag;

        /// <summary>
        /// Set the FlashSpeed which causes the over/out textures to cycle once every 'FlashSpeed' milliseconds
        /// </summary>
        int? FlashSpeed { get; set; }

        /// <summary>
        /// Get/set the area that should respond to a click event relative to the top-left corner of this control. 
        /// Parent offsets are adjusted automatically
        /// </summary>
        Rectangle ClickArea { get; set; }
    }
}
