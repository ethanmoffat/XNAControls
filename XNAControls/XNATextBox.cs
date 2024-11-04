using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls.Input;

namespace XNAControls
{
    /// <summary>
    /// Represents a text input control
    /// </summary>
    public class XNATextBox : XNAControl, IXNATextBox
    {
        internal static IXNATextBox FocusedTextbox { get; set; }

        private readonly Texture2D _textBoxBG;
        private readonly Texture2D _textBoxLeft;
        private readonly Texture2D _textBoxRight;
        private readonly Texture2D _caretTexture;

        private readonly XNATextBoxLabel _textLabel;
        private readonly XNATextBoxLabel _defaultTextLabel;

        private Rectangle _drawArea;
        private string _actualText;
        private bool _selected;

        private int _lastLeftPadding;

        private bool _multiline;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int MaxChars { get; set; }

        /// <inheritdoc />
        public int? MaxWidth
        {
            get => _textLabel.TextWidth;
            set
            {
                _textLabel.TextWidth = value;
                _defaultTextLabel.TextWidth = value;
            }
        }

        /// <inheritdoc />
        public bool PasswordBox { get; set; }

        /// <inheritdoc />
        public int LeftPadding { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public string DefaultText
        {
            get => _defaultTextLabel.Text;
            set => _defaultTextLabel.Text = value;
        }

        /// <inheritdoc />
        public Color TextColor
        {
            get => _textLabel.ForeColor;
            set => _textLabel.ForeColor = value;
        }

        /// <inheritdoc />
        public Color DefaultTextColor
        {
            get => _defaultTextLabel.ForeColor;
            set => _defaultTextLabel.ForeColor = value;
        }

        /// <inheritdoc />
        public LabelAlignment TextAlignment
        {
            get => _textLabel.TextAlign;
            set
            {
                _textLabel.TextAlign = value;
                _defaultTextLabel.TextAlign = value;
            }
        }

        /// <inheritdoc />
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                FocusedTextbox?.PostMessage(EventType.LostFocus, EventArgs.Empty);
                PostMessage(EventType.GotFocus, EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public int TabOrder { get; set; }

        /// <inheritdoc />
        public bool Multiline
        {
            get => _multiline;
            set
            {
                _multiline = value;
                _textLabel.WrapBehavior = _defaultTextLabel.WrapBehavior = 
                    _multiline ? WrapBehavior.WrapToNewLine : WrapBehavior.ScrollText;
            }
        }

        /// <inheritdoc />
        public IScrollHandler ScrollHandler
        {
            get => _textLabel.ScrollHandler;
            set => _textLabel.ScrollHandler = value;
        }

        /// <inheritdoc />
        public int? RowSpacing
        {
            get => _textLabel.RowSpacing;
            set => _textLabel.RowSpacing = value;
        }

        /// <inheritdoc />
        public event EventHandler OnGotFocus = delegate { };

        /// <inheritdoc />
        public event EventHandler OnLostFocus = delegate { };

        /// <inheritdoc />
        public event EventHandler OnTextChanged = delegate { };

        /// <inheritdoc />
        public event EventHandler OnEnterPressed = delegate { };

        /// <inheritdoc />
        public event EventHandler<MouseEventArgs> OnMouseDown = delegate { };

        /// <inheritdoc />
        public event EventHandler<MouseEventArgs> OnMouseUp = delegate { };

        /// <inheritdoc />
        public event EventHandler<MouseEventArgs> OnClicked = delegate { };

        /// <summary>
        /// Create a new text box with the specified area and font (content name)
        /// </summary>
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

            _textLabel = new XNATextBoxLabel(spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = LabelAlignment.MiddleLeft,
                DrawArea = new Rectangle(0, 0, area.Width, area.Height),
                WrapBehavior = WrapBehavior.ScrollText,
            };
            _textLabel.SetParentControl(this);

            _defaultTextLabel = new XNATextBoxLabel(spriteFontContentName)
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = LabelAlignment.MiddleLeft,
                DrawArea = new Rectangle(0, 0, area.Width, area.Height),
                WrapBehavior = WrapBehavior.ScrollText,
            };
            _defaultTextLabel.SetParentControl(this);

            DrawArea = area;

            _actualText = "";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            _textLabel.Initialize();
            _defaultTextLabel.Initialize();

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_lastLeftPadding != LeftPadding)
            {
                _lastLeftPadding = LeftPadding;
                _textLabel.DrawPosition = new Vector2(LeftPadding, 0);
                _defaultTextLabel.DrawPosition = new Vector2(LeftPadding, 0);
            }

            if (Enabled)
            {
                base.OnUpdateControl(gameTime);
            }
        }

        /// <inheritdoc />
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
                    Vector2 caretAdjust;

                    if (Multiline)
                    {
                        var textWidth = _textLabel.MeasureString(_textLabel.TextRows.Last()).X;
                        var linesToRender = Math.Min(_textLabel.ScrollHandler?.LinesToRender ?? _textLabel.TextRows.Count, _textLabel.TextRows.Count);
                        var textHeight = (linesToRender - 1) * (_textLabel.RowSpacing ?? (int)_textLabel.ActualHeight);
                        caretAdjust = new Vector2(textWidth, textHeight);
                    }
                    else
                    {
                        var textWidth = _textLabel.TextWidth.HasValue && _textLabel.ActualWidth > _textLabel.TextWidth
                            ? _textLabel.TextWidth.Value
                            : _textLabel.ActualWidth;

                        caretAdjust = new Vector2(textWidth, 0);
                    }

                    _spriteBatch.Draw(_caretTexture,
                                      _textLabel.AdjustedDrawPosition + caretAdjust,
                                      Color.White);
                }
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
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
            FocusedTextbox?.PostMessage(EventType.LostFocus, EventArgs.Empty);
            FocusedTextbox = this;
            FocusedTextbox.PostMessage(EventType.GotFocus, EventArgs.Empty);

            return true;
        }

        /// <inheritdoc />
        protected override bool HandleKeyTyped(IXNAControl control, KeyboardEventArgs eventArgs)
        {
            if (eventArgs.Key == Keys.Tab && FocusedTextbox != null)
            {
                IXNATextBox nextTextBox;

                var dialogStack = Singleton<DialogRepository>.Instance.OpenDialogs;
                var textBoxes = dialogStack.Any()
                    ? dialogStack.Peek().FlattenedChildren.OfType<IXNATextBox>()
                    : Game.Components.OfType<IXNATextBox>().Concat(Game.Components.OfType<IXNAControl>().SelectMany(x => x.FlattenedChildren.OfType<IXNATextBox>()));

                var state = KeyboardExtended.GetState();
                if (state.IsShiftDown())
                {
                    var orderTextBoxesEnumerable = textBoxes.OrderByDescending(x => x.TabOrder);
                    nextTextBox = orderTextBoxesEnumerable
                        .SkipWhile(x => x.TabOrder >= FocusedTextbox.TabOrder)
                        .FirstOrDefault();
                    nextTextBox ??= orderTextBoxesEnumerable.FirstOrDefault();
                }
                else
                {
                    var orderTextBoxesEnumerable = textBoxes.OrderBy(x => x.TabOrder);
                    nextTextBox = orderTextBoxesEnumerable
                        .SkipWhile(x => x.TabOrder <= FocusedTextbox.TabOrder)
                        .FirstOrDefault();
                    nextTextBox ??= orderTextBoxesEnumerable.FirstOrDefault();
                }

                FocusedTextbox?.PostMessage(EventType.LostFocus, EventArgs.Empty);
                FocusedTextbox = nextTextBox;
                FocusedTextbox?.PostMessage(EventType.GotFocus, EventArgs.Empty);
            }
            else if(eventArgs.Character.HasValue)
            {
                HandleTextInput(eventArgs);
            }

            return true;
        }

        /// <inheritdoc />
        protected virtual bool HandleTextInput(KeyboardEventArgs eventArgs)
        {
            switch (eventArgs.Key)
            {
                case Keys.Tab: break;
                case Keys.Enter:
                    {
                        if (Multiline)
                        {
                            Text += '\n';
                        }

                        OnEnterPressed?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case Keys.Back:
                    {
                        if (!string.IsNullOrEmpty(Text))
                        {
                            Text = Text.Remove(Text.Length - 1);
                        }
                    }
                    break;
                default:
                    {
                        if (eventArgs.Character != null)
                        {
                            Text += eventArgs.Character;
                        }
                    }
                    break;
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool HandleLostFocus(IXNAControl control, EventArgs eventArgs)
        {
            OnLostFocus?.Invoke(this, eventArgs);
            _selected = false;
            if (FocusedTextbox == this)
                FocusedTextbox = null;

            return true;
        }

        /// <inheritdoc />
        protected override bool HandleGotFocus(IXNAControl control, EventArgs eventArgs)
        {
            OnGotFocus?.Invoke(this, eventArgs);
            _selected = true;
            if (FocusedTextbox != this)
                FocusedTextbox = this;

            return true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && FocusedTextbox == this)
            {
                FocusedTextbox = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// A class wth logic to handle multiline textboxes
        /// </summary>
        protected internal class XNATextBoxLabel : XNALabel
        {
            /// <inheritdoc />
            public IReadOnlyList<string> TextRows => DrawStrings.Count == 0 ? new List<string> { Text } : DrawStrings;

            /// <inheritdoc />
            public IScrollHandler ScrollHandler { get; set; }

            /// <inheritdoc />
            public XNATextBoxLabel(string spriteFontName)
                : base(spriteFontName)
            {
            }

            /// <inheritdoc />
            public new Vector2 MeasureString(string input) => base.MeasureString(input);

            /// <inheritdoc />
            protected override void DrawMultiLine(float adjustedX, float adjustedY)
            {
                var start = ScrollHandler == null || DrawStrings.Count <= ScrollHandler.LinesToRender
                    ? 0
                    : ScrollHandler.ScrollOffset;
                var end = ScrollHandler == null || DrawStrings.Count <= ScrollHandler.LinesToRender
                    ? DrawStrings.Count
                    : ScrollHandler.LinesToRender + ScrollHandler.ScrollOffset;

                for (int i = start; i < Math.Min(DrawStrings.Count, end); i++)
                {
                    var line = DrawStrings[i];
                    DrawTextLine(line, adjustedX, adjustedY + (RowSpacing ?? LineHeight) * (i - start));
                }
            }

            /// <inheritdoc/>
            protected override void OnWrappedTextUpdated()
            {
                if (!AutoSize || !DrawStrings.Any())
                {
                    ScrollHandler?.UpdateDimensions(TextRows.Count);

                    if (ImmediateParent.Enabled)
                    {
                        ScrollHandler?.ScrollToEnd();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Interface for a text input control
    /// </summary>
    public interface IXNATextBox : IXNAControl
    {
        /// <summary>
        /// The maximum number of characters that can be entered in this text box
        /// </summary>
        int MaxChars { get; set; }

        /// <summary>
        /// The maximum width of text as measured by the font before text starts scrolling
        /// </summary>
        int? MaxWidth { get; set; }

        /// <summary>
        /// Set this textbox as a password box
        /// </summary>
        bool PasswordBox { get; set; }

        /// <summary>
        /// Width of empty space to the left of the first character displayed in this text box
        /// </summary>
        int LeftPadding { get; set; }

        /// <summary>
        /// The text to display
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The default text to display. This text shows before any text is entered.
        /// </summary>
        string DefaultText { get; set; }

        /// <summary>
        /// Color of the text
        /// </summary>
        Color TextColor { get; set; }

        /// <summary>
        /// Color of the default text
        /// </summary>
        Color DefaultTextColor { get; set; }

        /// <summary>
        /// Alignment of the text
        /// </summary>
        LabelAlignment TextAlignment { get; set; }

        /// <summary>
        /// Gets or sets whether this text box is selected. Selecting this text box gives it focus.
        /// </summary>
        bool Selected { get; set; }

        /// <summary>
        /// TabOrder of this text box. Pressing 'tab' will cycle through text boxes based on their tab order.
        /// </summary>
        int TabOrder { get; set; }

        /// <summary>
        /// Gets or sets whether the control is a multiline textbox. Text behavior will wrap instead of scrolling.
        /// </summary>
        bool Multiline { get; set; }

        /// <summary>
        /// Gets or sets the scroll handler, which handles vertical scrolling text in multiline text box controls.
        /// </summary>
        IScrollHandler ScrollHandler { get; set; }

        /// <summary>
        /// Gets or sets the spacing between rows in a multiline textbox, in pixels
        /// </summary>
        int? RowSpacing { get; set; }

        /// <summary>
        /// Event fired when this text box gets focus
        /// </summary>
        event EventHandler OnGotFocus;

        /// <summary>
        /// Event fired when this text box loses focus
        /// </summary>
        event EventHandler OnLostFocus;

        /// <summary>
        /// Event fired when text changes
        /// </summary>
        event EventHandler OnTextChanged;

        /// <summary>
        /// Event fired when the enter key is pressed
        /// </summary>
        event EventHandler OnEnterPressed;

        /// <summary>
        /// Event fired when a mouse button is pressed on a button control
        /// </summary>
        event EventHandler<MouseEventArgs> OnMouseDown;

        /// <summary>
        /// Event fired when a mouse button is released on a button control
        /// </summary>
        event EventHandler<MouseEventArgs> OnMouseUp;

        /// <summary>
        /// Event fired when this text box is clicked
        /// </summary>
        event EventHandler<MouseEventArgs> OnClicked;
    }
}