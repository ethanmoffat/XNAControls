// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls.Old
{
    //From top answer on: http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game
    //Modifications made by Ethan Moffat and Brian Gradin

    public class XNATextBox : XNAControl, IKeyboardSubscriber
    {
        private readonly Texture2D _textBoxBG;
        private readonly Texture2D _textBoxLeft;
        private readonly Texture2D _textBoxRight;
        private readonly Texture2D _caretTexture;

        private XNALabel _textLabel, _defaultTextLabel;
        private string _actualText;
        
        private bool _selected;
        private int _leftPadding;

        public int MaxChars { get; set; }

        public bool Highlighted { get; set; }

        public bool PasswordBox { get; set; }

        public int LeftPadding
        {
            get { return _leftPadding; }
            set
            {
                _leftPadding = value;
                _textLabel.DrawLocation = new Vector2(_leftPadding, 0);
                _defaultTextLabel.DrawLocation = new Vector2(_leftPadding, 0);
            }
        }

        public string Text
        {
            get
            {
                return _actualText;
            }
            set
            {
                if (MaxChars > 0 && value.Length > MaxChars)
                    return;

                _actualText = value;
                _textLabel.Text = PasswordBox ? new string(value.Select(x => '*').ToArray()) : value;
                if (OnTextChanged != null)
                    OnTextChanged(this, new EventArgs());

                if (_actualText.Length == 0)
                {
                    _textLabel.Visible = false;
                    _defaultTextLabel.Visible = true;
                }
                else
                {
                    _textLabel.Visible = true;
                    _defaultTextLabel.Visible = false;
                }
            }
        }

        public string DefaultText
        {
            get { return _defaultTextLabel.Text; }
            set
            {
                if (MaxChars > 0 && value.Length > MaxChars)
                    return;

                _defaultTextLabel.Text = value;
            }
        }

        public Color TextColor
        {
            get { return _textLabel.ForeColor; }
            set { _textLabel.ForeColor = value; }
        }

        public event EventHandler OnFocused;

        public event EventHandler OnEnterPressed;
        public event EventHandler OnTabPressed;
        public event EventHandler OnTextChanged;
        public event EventHandler OnClicked;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                bool oldSel = _selected;
                _selected = value;
                if (!oldSel && _selected && OnFocused != null)
                    OnFocused(this, new EventArgs());
            }
        }

        //accepts array with following:
        //	length 4: background texture, leftEnd, rightEnd, caret
        /// <summary>
        /// Construct an XNATextBox UI control.
        /// </summary>
        /// <param name="area">The area of the screen in which the TextBox should be rendered (x, y)</param>
        /// <param name="textures">Array of four textures. 0=background, 1=leftEdge, 2=rightEdge, 3=caret</param>
        /// <param name="spriteFontContentName">Name of the SpriteFont content resource</param>
        public XNATextBox(Rectangle area, Texture2D[] textures, string spriteFontContentName)
            : base(new Vector2(area.X, area.Y), area)
        {
            if (textures.Length != 4)
                throw new ArgumentException("The textures array is invalid. Please pass in an array that contains 4 textures.");
            _textBoxBG = textures[0];
            _textBoxLeft = textures[1];
            _textBoxRight = textures[2];
            _caretTexture = textures[3];

            _setupLabels(area, spriteFontContentName);

            LeftPadding = 0;
            drawArea.Height = _textBoxBG.Height;
        }

        public XNATextBox(Rectangle area, Texture2D cursor, string spriteFontContentName)
            :base(new Vector2(area.X, area.Y), area)
        {
            _textBoxBG = null;
            _textBoxLeft = null;
            _textBoxRight = null;
            _caretTexture = cursor;

            _setupLabels(area, spriteFontContentName);

            LeftPadding = 0;
        }

        private void _setupLabels(Rectangle area, string spriteFontContentName)
        {
            _textLabel = new XNALabel(new Rectangle(0, 0, area.Width, area.Height), spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                TextAlign = LabelAlignment.MiddleLeft,
                Visible = true,
                Enabled = true
            };
            _textLabel.SetParent(this);

            _defaultTextLabel = new XNALabel(new Rectangle(0, 0, area.Width, area.Height), spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                ForeColor = Color.FromNonPremultiplied(80, 80, 80, 0xff),
                TextAlign = LabelAlignment.MiddleLeft,
                Visible = true,
                Enabled = true
            };
            _defaultTextLabel.SetParent(this);
        }

        public override void Update(GameTime gameTime)
        {
            if (!ShouldUpdate())
                return;
            MouseState mouse = Mouse.GetState();
            Point mousePoint = new Point(mouse.X, mouse.Y);

            if (DrawAreaWithOffset.Contains(mousePoint))
            {
                Highlighted = true;
                if (PreviousMouseState.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed)
                {
                    if (OnClicked != null)
                    {
                        bool prevSel = Selected;
                        OnClicked(this, new EventArgs());

                        //if clicking selected the TB
                        if (Selected && !prevSel && OnFocused != null)
                        {
                            OnFocused(this, new EventArgs());
                        }
                    }
                }
            }
            else
            {
                Highlighted = false;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            bool caretVisible = !((gameTime.TotalGameTime.TotalMilliseconds % 1000) < 500);

            SpriteBatch.Begin();
                        
            //draw bg tiled
            if(_textBoxBG != null)
                SpriteBatch.Draw(_textBoxBG, DrawAreaWithOffset, Color.White);
            
            //draw left side
            if (_textBoxLeft != null)
            {
                Rectangle leftDrawArea = new Rectangle(DrawArea.X, DrawArea.Y, _textBoxLeft.Width, DrawArea.Height);
                SpriteBatch.Draw(_textBoxLeft, leftDrawArea, Color.White);
            }

            //draw right side
            if (_textBoxRight != null)
            {
                Rectangle rightDrawArea = new Rectangle(DrawArea.X + DrawAreaWithOffset.Width - _textBoxRight.Width, DrawAreaWithOffset.Y, _textBoxRight.Width, DrawAreaWithOffset.Height);
                SpriteBatch.Draw(_textBoxRight, rightDrawArea, Color.White);
            }

            if (caretVisible && Selected)
            {
                SpriteBatch.Draw(_caretTexture,
                    new Vector2(DrawAreaWithOffset.X + LeftPadding + _textLabel.ActualWidth + 2, DrawAreaWithOffset.Y + 4),
                    Color.White);
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public virtual void ReceiveTextInput(char inputChar)
        {
            Text = Text + inputChar;
        }

        public virtual void ReceiveTextInput(string text)
        {
            Text = Text + text;
        }

        public virtual void ReceiveCommandInput(char command)
        {
            if (!XNAControls.IgnoreEnterForDialogs && Dialogs.Count != 0 && Dialogs.Peek() != TopParent as XNADialog)
                return;

            switch (command)
            {
                case KeyboardDispatcher.CHAR_BACKSPACE_CODE:
                    if (Text.Length > 0)
                        Text = Text.Substring(0, Text.Length - 1);
                    break;
                case KeyboardDispatcher.CHAR_RETURNKEY_CODE:
                    if (OnEnterPressed != null)
                        OnEnterPressed(this, new EventArgs());
                    break;
                case KeyboardDispatcher.CHAR_TAB_CODE:
                    if (OnTabPressed != null)
                        OnTabPressed(this, new EventArgs());
                    break;
            }
        }
    }
}