using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XNAControls.Input;

namespace XNAControls
{
    /// <summary>
    /// Represents a result of a modal dialog
    /// </summary>
    public enum XNADialogResult
    {
        /// <inheritdoc />
        OK,
        /// <inheritdoc />
        Cancel,
        /// <summary>
        /// Used when the dialog is closed programmatically without a valid result
        /// </summary>
        NO_BUTTON_PRESSED
    }

    /// <summary>
    /// Event args for when a dialog is closing
    /// </summary>
    public class DialogClosingEventArgs : EventArgs
    {
        /// <summary>
        /// The dialog result
        /// </summary>
        public XNADialogResult Result { get; }

        /// <summary>
        /// Set to true to cancel the dialog closing event and stop it from being closed
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Create an instance of DialogClosingEventArgs with the given result
        /// </summary>
        public DialogClosingEventArgs(XNADialogResult result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Represents a dialog
    /// </summary>
    public abstract class XNADialog : XNAControl, IXNADialog
    {
        /// <summary>
        /// The base draw order to use for dialogs in the dialog stack.
        /// Each subsequent dialog will be displayed with a greater draw order so the most recent dialog is always shown on top.
        /// Note that setting the DrawOrder property of a dialog manually will have no effect.
        /// </summary>
        public static int DialogLayerOffset { get; set; } = 30;

        private bool _modal;

        private readonly TaskCompletionSource<XNADialogResult> _showTaskCompletionSource;

        private Texture2D _backgroundTexture;
        private Rectangle? _backgroundTextureSource;

        bool IXNADialog.Modal => _modal;

        /// <inheritdoc />
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        /// <inheritdoc />
        public event EventHandler DialogClosed;

        /// <summary>
        /// The background texture of the dialog. Setting the background texture automatically sets the dialog size.
        /// </summary>
        protected Texture2D BackgroundTexture
        {
            get => _backgroundTexture;
            set
            {
                _backgroundTexture = value;

                if (BackgroundTextureSource.HasValue)
                    SetSize(BackgroundTextureSource.Value.Width, BackgroundTextureSource.Value.Height);
                else if (BackgroundTexture != null)
                    SetSize(BackgroundTexture.Width, BackgroundTexture.Height);
            }
        }

        /// <summary>
        /// The source area for the background texture of the dialog. Setting the texture source automatically sets the dialog size.
        /// </summary>
        protected Rectangle? BackgroundTextureSource
        {
            get => _backgroundTextureSource;
            set
            {
                _backgroundTextureSource = value;

                if (_backgroundTextureSource.HasValue)
                    SetSize(_backgroundTextureSource.Value.Width, _backgroundTextureSource.Value.Height);
                else if (BackgroundTexture != null)
                    SetSize(BackgroundTexture.Width, BackgroundTexture.Height);
            }
        }

        /// <inheritdoc />
        protected XNADialog()
        {
            _showTaskCompletionSource = new TaskCompletionSource<XNADialogResult>();
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            Singleton<DialogRepository>.Instance.OpenDialogs.Push(this);
            base.Initialize();
        }

        /// <inheritdoc />
        public virtual void BringToTop()
        {
            FindAndPopThisDialogFromStack();
            var openDialogs = Singleton<DialogRepository>.Instance.OpenDialogs;
            openDialogs.Push(this);

            var maxDrawOrder = Game.Components.OfType<IEventReceiver>().Max(x => x.ZOrder);
            SetDrawOrder((100 * openDialogs.Count) + maxDrawOrder + 1);
        }

        /// <inheritdoc />
        public virtual void CenterInGameView()
        {
            var viewport = Game.GraphicsDevice.Viewport;

            var bounds = BackgroundTextureSource ?? BackgroundTexture?.Bounds ?? Rectangle.Empty;
            DrawPosition = new Vector2(viewport.Width/2 - bounds.Width/2,
                                       viewport.Height/2 - bounds.Height/2);
        }

        /// <inheritdoc />
        public void ShowDialog()
        {
            //Run the ShowDialogAsync() method on a Threadpool Thread
            //Use ConfigureAwait(false) to ignore the captured context when starting the async operation
            //See http://blog.stephencleary.com/2012/02/async-and-await.html for more info
            Task.Run(async () => await ShowDialogAsync(modal: true).ConfigureAwait(false));
        }

        /// <inheritdoc />
        public Task<XNADialogResult> ShowDialogAsync()
        {
            return ShowDialogAsync(modal: true);
        }

        /// <inheritdoc />
        public void Show()
        {
            Task.Run(async () => await ShowDialogAsync(modal: false).ConfigureAwait(false));
        }

        /// <inheritdoc />
        protected override bool HandleDrag(IXNAControl control, MouseEventArgs eventArgs)
        {
            DrawPosition += eventArgs.DistanceMoved;
            return true;
        }

        /// <summary>
        /// Draw the background texture of the dialog
        /// </summary>
        protected override void OnDrawControl(GameTime gameTime)
        {
            if (BackgroundTexture != null)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(BackgroundTexture, DrawAreaWithParentOffset, BackgroundTextureSource, Color.White);
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }

        /// <summary>
        /// Close the dialog with the specified result
        /// </summary>
        /// <param name="result">Result to return from the Show() call</param>
        protected virtual void Close(XNADialogResult result)
        {
            var eventArgs = new DialogClosingEventArgs(result);
            DialogClosing?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                FindAndPopThisDialogFromStack();
                _showTaskCompletionSource.SetResult(result);
                DialogClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task<XNADialogResult> ShowDialogAsync(bool modal)
        {
            _modal = modal;

            AddControlToDefaultGame();
            BringToTop();

            var result = await _showTaskCompletionSource.Task;

            Dispose();
            return result;
        }

        private void FindAndPopThisDialogFromStack()
        {
            var dlgStack = Singleton<DialogRepository>.Instance.OpenDialogs;

            if (!dlgStack.Contains(this))
                return;

            var tempStack = new Stack<IXNADialog>();
            do
            {
                tempStack.Push(dlgStack.Pop());
            }
            while (tempStack.Peek() != this);

            // pop 'this' dialog from temp stack
            tempStack.Pop();

            // push all other dialogs back onto dialog stacck
            while (tempStack.Any())
                dlgStack.Push(tempStack.Pop());
        }
    }

    /// <summary>
    /// Interface for a dialog
    /// </summary>
    public interface IXNADialog : IXNAControl
    {
        internal bool Modal { get; }

        /// <summary>
        /// Fired when the dialog is about to be closed
        /// </summary>
        event EventHandler<DialogClosingEventArgs> DialogClosing;

        /// <summary>
        /// Fired when the dialog has been closed
        /// </summary>
        event EventHandler DialogClosed;

        /// <summary>
        /// Adjust the draw order of the dialog such that it is brought to the top of the draw order
        /// </summary>
        void BringToTop();

        /// <summary>
        /// Center the dialog in the Game's default graphics device
        /// </summary>
        void CenterInGameView();

        /// <summary>
        /// Show the modal dialog and do processing until the dialog is closed
        /// </summary>
        void ShowDialog();

        /// <summary>
        /// Show the modal dialog asynchronously and do processing until the user makes a choice
        /// </summary>
        /// <returns>Result of the dialog based on user selection (OK or Cancel)</returns>
        Task<XNADialogResult> ShowDialogAsync();

        /// <summary>
        /// Show the dialog as modeless (does not block updating of other controls)
        /// </summary>
        void Show();
    }
}
