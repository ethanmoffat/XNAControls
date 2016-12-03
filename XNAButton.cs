// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
    public class XNAButton : XNAControl, IXNAButton
    {
        private readonly Texture2D _out;
        private readonly Texture2D _over;

        private bool _dragging;
        private Texture2D _drawTexture;

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
        /// Get/set the area that should respond to a click event relative to the top-left corner of this control.
        /// </summary>
        public Rectangle ClickArea { get; set; }

        /// <summary>
        /// Gets the click area of the control offset based on the control and all parent's X,Y coordinates
        /// </summary>
        protected Rectangle ClickAreaWithOffset
        {
            get
            {
                return new Rectangle(ClickArea.X + DrawAreaWithParentOffset.X,
                                     ClickArea.Y + DrawAreaWithParentOffset.Y,
                                     ClickArea.Width,
                                     ClickArea.Height);
            }
        }

        /// <summary>
        /// Construct a button where the textures for over/out are a part of a sprite sheet.
        /// </summary>
        /// <param name="sheet">The sprite sheet texture</param>
        /// <param name="location">Location to draw the button</param>
        /// <param name="outSource">Source within the sprite sheet that contains the texture to draw on MouseOut</param>
        /// <param name="overSource">Source within the sprite sheet that contains the texture to draw on MouseOver</param>
        public XNAButton(Texture2D sheet, Vector2 location, Rectangle outSource, Rectangle overSource)
        {
            if (sheet == null)
                throw new ArgumentNullException("sheet");
            if (outSource == null)
                throw new ArgumentNullException("outSource");
            if (overSource == null)
                throw new ArgumentNullException("outSource");

            var largerArea = outSource.Size.X*outSource.Size.Y >= overSource.Size.X*overSource.Size.Y
                ? outSource
                : overSource;

            var outData = new Color[outSource.Width * outSource.Height];
            _out = new Texture2D(sheet.GraphicsDevice, outSource.Width, outSource.Height);
            sheet.GetData(0, outSource, outData, 0, outData.Length);
            _out.SetData(outData);

            _drawTexture = _out;

            var overData = new Color[overSource.Width * overSource.Height];
            _over = new Texture2D(sheet.GraphicsDevice, overSource.Width, overSource.Height);
            sheet.GetData(0, overSource, overData, 0, overData.Length);
            _over.SetData(overData);

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

            if (!MouseOver && FlashSpeed != null && (int)gameTime.TotalGameTime.TotalMilliseconds % FlashSpeed == 0)
                _drawTexture = _drawTexture == _over ? _out : _over;
            else if (MouseOver)
                _drawTexture = _over;
            else if (FlashSpeed == null)
                _drawTexture = _out;

            base.OnUpdateControl(gameTime);
        }
        
        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            if (_drawTexture != null)
                _spriteBatch.Draw(_drawTexture, DrawAreaWithParentOffset, Color.White);

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_over != null)
                    _over.Dispose();
                if (_out != null)
                    _out.Dispose();
            }

            base.Dispose(disposing);
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
