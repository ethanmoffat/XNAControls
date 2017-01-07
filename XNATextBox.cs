// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
    public class XNATextBox : XNAControl, IXNATextBox
    {
        private readonly Texture2D _textBoxBG;
        private readonly Texture2D _textBoxLeft;
        private readonly Texture2D _textBoxRight;
        private readonly Texture2D _caretTexture;

        private readonly XNALabel _textLabel;
        private readonly XNALabel _defaultTextLabel;

        private Rectangle _drawArea;
        private string _actualText;
        private bool _selected;

        private int _lastLeftPadding;

        public override Rectangle DrawArea
        {
            get { return _drawArea; }
            set
            {
                _drawArea = value;
                _textLabel.DrawArea = new Rectangle(LeftPadding, -4, _drawArea.Width, _drawArea.Height);
                _defaultTextLabel.DrawArea = new Rectangle(LeftPadding, -4, _drawArea.Width, _drawArea.Height);
            }
        }

        public int MaxChars { get; set; }

        public bool Highlighted { get; set; }

        public bool PasswordBox { get; set; }

        public int LeftPadding { get; set; }

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
                OnTextChanged(this, EventArgs.Empty);

                _textLabel.Visible = _actualText.Length > 0;
                _defaultTextLabel.Visible = _actualText.Length == 0;
            }
        }

        public string DefaultText
        {
            get { return _defaultTextLabel.Text; }
            set { _defaultTextLabel.Text = value; }
        }

        public Color TextColor
        {
            get { return _textLabel.ForeColor; }
            set { _textLabel.ForeColor = value; }
        }

        public Color DefaultTextColor
        {
            get { return _defaultTextLabel.ForeColor; }
            set { _defaultTextLabel.ForeColor = value; }
        }

        public LabelAlignment TextAlignment
        {
            get { return _textLabel.TextAlign; }
            set
            {
                _textLabel.TextAlign = value;
                _defaultTextLabel.TextAlign = value;
            }
        }

        public event EventHandler OnFocused = delegate { };

        public event EventHandler OnEnterPressed = delegate { };
        public event EventHandler OnTabPressed = delegate { };
        public event EventHandler OnTextChanged = delegate { };
        public event EventHandler OnClicked = delegate { };

        public bool Selected
        {
            get { return _selected; }
            set
            {
                bool oldSel = _selected;
                _selected = value;
                if (!oldSel && _selected)
                    OnFocused(this, EventArgs.Empty);
            }
        }

        public XNATextBox(Rectangle area, 
                          string spriteFontContentName,
                          Texture2D backgroundTexture = null,
                          Texture2D leftSideTexture = null,
                          Texture2D rightSideTexture = null,
                          Texture2D caretTexture = null)
        {
            _textBoxBG = backgroundTexture;
            _textBoxLeft = leftSideTexture;
            _textBoxRight = rightSideTexture;
            _caretTexture = caretTexture;

            _textLabel = new XNALabel(spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = LabelAlignment.MiddleLeft,
                DrawArea = new Rectangle(0, 0, area.Width, area.Height)
            };

            _defaultTextLabel = new XNALabel(spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = LabelAlignment.MiddleLeft,
                DrawArea = new Rectangle(0, 0, area.Width, area.Height)
            };

            DrawArea = area;
        }

        public override void Initialize()
        {
            _textLabel.Initialize();
            _textLabel.SetParentControl(this);

            _defaultTextLabel.Initialize();
            _defaultTextLabel.SetParentControl(this);

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (DrawAreaWithParentOffset.Contains(CurrentMouseState.Position))
            {
                Highlighted = true;
                if (PreviousMouseState.LeftButton == ButtonState.Released &&
                    CurrentMouseState.LeftButton == ButtonState.Pressed)
                {
                    var wasSelectedBeforeOnClick = Selected;
                    OnClicked(this, new EventArgs());

                    if (Selected && !wasSelectedBeforeOnClick)
                    {
                        OnFocused(this, new EventArgs());
                    }
                }
            }
            else
            {
                Highlighted = false;
            }

            if (_lastLeftPadding != LeftPadding)
            {
                _lastLeftPadding = LeftPadding;
                _textLabel.DrawPosition = new Vector2(LeftPadding, 0);
                _defaultTextLabel.DrawPosition = new Vector2(LeftPadding, 0);
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            if(_textBoxBG != null)
                _spriteBatch.Draw(_textBoxBG, DrawAreaWithParentOffset, Color.White);

            if (_textBoxLeft != null)
                _spriteBatch.Draw(_textBoxLeft, DrawPositionWithParentOffset, Color.White);

            if (_textBoxRight != null)
            {
                var drawPosition = new Vector2(DrawPositionWithParentOffset.X + DrawArea.Width - _textBoxRight.Width,
                                               DrawPositionWithParentOffset.Y);

                _spriteBatch.Draw(_textBoxRight, drawPosition, Color.White);
            }

            if (_caretTexture != null && _textLabel != null && Selected)
            {
                var caretVisible = !(gameTime.TotalGameTime.TotalMilliseconds % 1000 < 500);
                if (caretVisible)
                    _spriteBatch.Draw(_caretTexture,
                                      new Vector2(_textLabel.AdjustedDrawPosition.X + _textLabel.ActualWidth + 2,
                                                  _textLabel.AdjustedDrawPosition.Y),
                                      Color.White);
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
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
            if (!ShouldUpdate())
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

    public interface IXNATextBox : IXNAControl, IKeyboardSubscriber
    {
        int MaxChars { get; set; }
        bool Highlighted { get; set; }
        bool PasswordBox { get; set; }
        int LeftPadding { get; set; }
        string Text { get; set; }
        string DefaultText { get; set; }
        Color TextColor { get; set; }
        Color DefaultTextColor { get; set; }
        LabelAlignment TextAlignment { get; set; }

        event EventHandler OnFocused;
        event EventHandler OnEnterPressed;
        event EventHandler OnTabPressed;
        event EventHandler OnTextChanged;
        event EventHandler OnClicked;
    }
}