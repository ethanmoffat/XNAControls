// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAControls
{
    public enum XNADialogResult
    {
        OK,
        Cancel,
        NO_BUTTON_PRESSED
    }

    public abstract class XNADialog : XNAControl, IXNADialog
    {
        /// <summary>
        /// The base draw order to use for dialogs in the dialog stack.
        /// Each subsequent dialog will be displayed with a greater draw order so the most recent dialog is always shown on top.
        /// Note that setting the DrawOrder property of a dialog manually will have no effect.
        /// </summary>
        public static int DialogLayerOffset { get; set; } = 30;

        internal bool Modal { get; private set; }

        private readonly TaskCompletionSource<XNADialogResult> _showTaskCompletionSource;

        private Texture2D _backgroundTexture;

        /// <summary>
        /// The background texture of the dialog. Setting the background texture automatically sets the dialog size
        /// </summary>
        protected Texture2D BackgroundTexture
        {
            get => _backgroundTexture;
            set
            {
                _backgroundTexture = value;
                SetSize(_backgroundTexture.Width, _backgroundTexture.Height);
            }
        }

        protected XNADialog()
        {
            _showTaskCompletionSource = new TaskCompletionSource<XNADialogResult>();
        }

        /// <summary>
        /// Add this dialog to tracked OpenDialogs
        /// </summary>
        public override void Initialize()
        {
            Singleton<DialogRepository>.Instance.OpenDialogs.Push(this);
            base.Initialize();
        }

        /// <summary>
        /// Adjust the draw order of the dialog such that it is brought to the top of the draw order
        /// </summary>
        public virtual void BringToTop()
        {
            var dialogsCount = Singleton<DialogRepository>.Instance.OpenDialogs.Count;
            SetDrawOrder((dialogsCount+1) * 5 + DialogLayerOffset);
        }

        /// <summary>
        /// Center the dialog in the Game's default graphics device
        /// </summary>
        public virtual void CenterInGameView()
        {
            var viewport = Game.GraphicsDevice.Viewport;

            DrawPosition = new Vector2(viewport.Width/2 - BackgroundTexture.Width/2,
                                       viewport.Height/2 - BackgroundTexture.Height/2);
        }

        /// <summary>
        /// Show the modal dialog and do processing until the dialog is closed
        /// </summary>
        public void ShowDialog()
        {
            //Run the ShowDialogAsync() method on a Threadpool Thread
            //Use ConfigureAwait(false) to ignore the captured context when starting the async operation
            //See http://blog.stephencleary.com/2012/02/async-and-await.html for more info
            Task.Run(async () => await ShowDialogAsync(modal: true).ConfigureAwait(false));
        }

        /// <summary>
        /// Show the modal dialog asynchronously and do processing until the user makes a choice
        /// </summary>
        /// <returns>Result of the dialog based on user selection (OK or Cancel)</returns>
        public Task<XNADialogResult> ShowDialogAsync()
        {
            return ShowDialogAsync(modal: true);
        }

        public void Show()
        {
            Task.Run(async () => await ShowDialogAsync(modal: false).ConfigureAwait(false));
        }

        /// <summary>
        /// Handle click+drag for the dialog
        /// </summary>
        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (PreviousMouseState.LeftButton == ButtonState.Pressed &&
                CurrentMouseState.LeftButton == ButtonState.Pressed &&
                DrawAreaWithParentOffset.Contains(CurrentMouseState.X, CurrentMouseState.Y) && ShouldClickDrag)
            {
                DrawPosition = new Vector2(
                    DrawPositionWithParentOffset.X + (CurrentMouseState.X - PreviousMouseState.X),
                    DrawPositionWithParentOffset.Y + (CurrentMouseState.Y - PreviousMouseState.Y));
            }

            base.OnUpdateControl(gameTime);
        }

        /// <summary>
        /// Draw the background texture of the dialog
        /// </summary>
        protected override void OnDrawControl(GameTime gameTime)
        {
            if (BackgroundTexture != null)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(BackgroundTexture, DrawAreaWithParentOffset, Color.White);
                _spriteBatch.End();
            }

            base.OnDrawControl(gameTime);
        }

        /// <summary>
        /// Close the dialog with the specified result
        /// </summary>
        /// <param name="result">Result to return from the Show() call</param>
        protected void Close(XNADialogResult result)
        {
            Singleton<DialogRepository>.Instance.OpenDialogs.Pop();
            _showTaskCompletionSource.SetResult(result);
        }

        private async Task<XNADialogResult> ShowDialogAsync(bool modal)
        {
            Modal = modal;

            AddControlToDefaultGame();
            BringToTop();

            var result = await _showTaskCompletionSource.Task;

            Dispose();
            return result;
        }
    }

    public interface IXNADialog : IXNAControl
    {
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
