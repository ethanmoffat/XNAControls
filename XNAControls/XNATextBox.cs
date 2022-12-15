using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using XNAControls.Input;

namespace XNAControls
{
    public class XNATextBox : XNAControl, IXNATextBox
    {
        internal static IXNATextBox FocusedTextbox { get; set; }

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
            get => _drawArea;
            set
            {
                _drawArea = value;
                _textLabel.DrawArea = new Rectangle(LeftPadding, 0, _drawArea.Width, _drawArea.Height);
                _defaultTextLabel.DrawArea = new Rectangle(LeftPadding, 0, _drawArea.Width, _drawArea.Height);
            }
        }

        public int MaxChars { get; set; }

        public int? MaxWidth
        {
            get => _textLabel.TextWidth;
            set
            {
                _textLabel.TextWidth = value;
                _defaultTextLabel.TextWidth = value;
            }
        }

        public bool PasswordBox { get; set; }

        public int LeftPadding { get; set; }

        public string Text
        {
            get => _actualText;
            set
            {
                if (MaxChars > 0 && value.Length > MaxChars)
                    return;

                _actualText = value;
                _textLabel.Text = PasswordBox ? new string(value.Select(x => '*').ToArray()) : value;
                OnTextChanged?.Invoke(this, EventArgs.Empty);

                _textLabel.Visible = _actualText.Length > 0;
                _defaultTextLabel.Visible = _actualText.Length == 0;
            }
        }

        public string DefaultText
        {
            get => _defaultTextLabel.Text;
            set => _defaultTextLabel.Text = value;
        }

        public Color TextColor
        {
            get => _textLabel.ForeColor;
            set => _textLabel.ForeColor = value;
        }

        public Color DefaultTextColor
        {
            get => _defaultTextLabel.ForeColor;
            set => _defaultTextLabel.ForeColor = value;
        }

        public LabelAlignment TextAlignment
        {
            get => _textLabel.TextAlign;
            set
            {
                _textLabel.TextAlign = value;
                _defaultTextLabel.TextAlign = value;
            }
        }

        public int TabOrder { get; set; }

        public event EventHandler OnGotFocus = delegate { };
        public event EventHandler OnLostFocus = delegate { };

        public event EventHandler OnTextChanged = delegate { };
        public event EventHandler OnEnterPressed = delegate { };
        public event EventHandler<MouseEventArgs> OnClicked = delegate { };

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                FocusedTextbox?.SendMessage(EventType.LostFocus, EventArgs.Empty);
                SendMessage(EventType.GotFocus, EventArgs.Empty);
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
                DrawArea = new Rectangle(0, 0, area.Width, area.Height),
                WrapBehavior = WrapBehavior.ScrollText,
                HandlesEvents = EventType.None,
            };
            _textLabel.SetParentControl(this);

            _defaultTextLabel = new XNALabel(spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = LabelAlignment.MiddleLeft,
                DrawArea = new Rectangle(0, 0, area.Width, area.Height),
                WrapBehavior = WrapBehavior.ScrollText,
                HandlesEvents = EventType.None,
            };
            _defaultTextLabel.SetParentControl(this);

            DrawArea = area;

            _actualText = "";
        }

        public override void Initialize()
        {
            _textLabel.Initialize();
            _defaultTextLabel.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
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
                {
                    var textWidth = _textLabel.TextWidth.HasValue && _textLabel.ActualWidth > _textLabel.TextWidth
                        ? _textLabel.TextWidth.Value
                        : _textLabel.ActualWidth;
                    _spriteBatch.Draw(_caretTexture,
                                      new Vector2(_textLabel.AdjustedDrawPosition.X + textWidth + 2,
                                                  DrawAreaWithParentOffset.Y + (int)Math.Round((DrawArea.Height - _caretTexture.Height) / 2.0)),
                                      Color.White);
                }
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        protected override void HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            FocusedTextbox?.SendMessage(EventType.LostFocus, EventArgs.Empty);
            FocusedTextbox = this;
            FocusedTextbox.SendMessage(EventType.GotFocus, EventArgs.Empty);
        }

        protected override void HandleKeyTyped(IXNAControl control, KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Key == Keys.Tab && FocusedTextbox != null)
            {
                var orderTextBoxesEnumerable = Game.Components.OfType<IXNATextBox>().OrderBy(x => x.TabOrder);
                var nextTextBox = orderTextBoxesEnumerable
                    .SkipWhile(x => x.TabOrder <= FocusedTextbox.TabOrder)
                    .FirstOrDefault();

                nextTextBox ??= orderTextBoxesEnumerable.FirstOrDefault();

                FocusedTextbox?.SendMessage(EventType.LostFocus, EventArgs.Empty);
                FocusedTextbox = nextTextBox;
                FocusedTextbox?.SendMessage(EventType.GotFocus, EventArgs.Empty);
            }
            else if(eventArgs.Character.HasValue)
            {
                HandleTextInput(eventArgs);
            }
        }

        protected virtual void HandleTextInput(KeyboardEventArgs eventArgs)
        {
            switch (eventArgs.Key)
            {
                case Keys.Tab: break;
                case Keys.Enter:
                    {
                        OnEnterPressed?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case Keys.Back:
                    {
                        if (!string.IsNullOrEmpty(Text))
                            Text = Text.Remove(Text.Length - 1);
                    }
                    break;
                default:
                    {
                        if (eventArgs.Character != null)
                            Text += eventArgs.Character;
                    }
                    break;
            }
        }

        protected override void HandleLostFocus(IXNAControl control, EventArgs eventArgs)
        {
            _selected = false;
            if (FocusedTextbox == this)
                FocusedTextbox = null;
        }

        protected override void HandleGotFocus(IXNAControl control, EventArgs eventArgs)
        {
            _selected = true;
            if (FocusedTextbox != this)
                FocusedTextbox = this;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && FocusedTextbox == this)
            {
                FocusedTextbox = null;
            }

            base.Dispose(disposing);
        }
    }

    public interface IXNATextBox : IXNAControl
    {
        int MaxChars { get; set; }
        int? MaxWidth { get; set; }
        bool PasswordBox { get; set; }
        int LeftPadding { get; set; }
        string Text { get; set; }
        string DefaultText { get; set; }
        Color TextColor { get; set; }
        Color DefaultTextColor { get; set; }
        LabelAlignment TextAlignment { get; set; }
        bool Selected { get; set; }
        int TabOrder { get; set; }

        event EventHandler OnGotFocus;
        event EventHandler OnLostFocus;
        event EventHandler OnTextChanged;
        event EventHandler<MouseEventArgs> OnClicked;
    }
}