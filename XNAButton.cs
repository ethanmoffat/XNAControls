// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
    public class XNAButton : XNAControl
    {
        public string Text
        {
            get { return _textLabel.Text; }
            set { _textLabel.Text = value; }
        }

        private bool _dragging;
        private readonly Texture2D _out; //texture for mouse out (primary display texture)
        private readonly Texture2D _over; //texture for mouse over
        private Texture2D _drawTexture; //texture to be drawn, selected based on MouseOver in Update method
        private readonly XNALabel _textLabel;

        /// <summary>
        /// If FlashSpeed is set, the button will change between over/out textures once every 'FlashSpeed' milliseconds
        /// </summary>
        public int? FlashSpeed { get; set; }

        private Rectangle? area; //nullable rectangle for optional area that will respond to mouse click/hover
        /// <summary>
        /// Get/set the area that should respond to a click event.
        /// If null, the entire button area responds to a click event.
        /// </summary>
        public Rectangle? ClickArea
        {
            get { return area; }
            set
            {
                area = value;
                if(area != null)
                {
                    //factor in the draw coordinates and the offset from the parent to the new click area
                    area = new Rectangle(area.Value.X + DrawAreaWithParentOffset.X,
                        area.Value.Y + DrawAreaWithParentOffset.Y,
                        area.Value.Width,
                        area.Value.Height);
                }
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

            DrawArea = new Rectangle((int) location.X,
                (int) location.Y,
                largerArea.Width, largerArea.Height);

            ClickArea = null;

            var outData = new Color[outSource.Width * outSource.Height];
            _out = new Texture2D(sheet.GraphicsDevice, outSource.Width, outSource.Height);
            sheet.GetData(0, outSource, outData, 0, outData.Length);
            _out.SetData(outData);

            var overData = new Color[overSource.Width * overSource.Height];
            _over = new Texture2D(sheet.GraphicsDevice, overSource.Width, overSource.Height);
            sheet.GetData(0, overSource, overData, 0, overData.Length);
            _over.SetData(overData);

            _drawTexture = _out;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (MouseOver && PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released)
                OnClick(this, EventArgs.Empty);

            //todo: handle dragging
            //if (MouseOver && PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Pressed
            //    && shouldClickDrag && !_dragging)
            //{
            //    SuppressParentClickDrag(true);
            //    _dragging = true;
            //}
            //else if (PreviousMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released && _dragging)
            //{
            //    _dragging = false;
            //    SuppressParentClickDrag(false);
            //}

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

        public event EventHandler OnClick = delegate { };
        public event EventHandler OnClickDrag = delegate { };
    }
}
