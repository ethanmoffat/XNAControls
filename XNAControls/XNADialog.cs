﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
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

    public class DialogClosingEventArgs : EventArgs
    {
        public XNADialogResult Result { get; }

        public bool Cancel { get; set; }

        public DialogClosingEventArgs(XNADialogResult result)
        {
            Result = result;
        }
    }

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

        bool IXNADialog.Modal => _modal;

        /// <inheritdoc />
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        /// <inheritdoc />
        public event EventHandler DialogClosed;

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

        /// <inheritdoc />
        public override void Initialize()
        {
            Singleton<DialogRepository>.Instance.OpenDialogs.Push(this);
            base.Initialize();
        }

        /// <inheritdoc />
        public virtual void BringToTop()
        {
            var dialogsCount = Singleton<DialogRepository>.Instance.OpenDialogs.Count;
            SetDrawOrder((dialogsCount+1) * 5 + DialogLayerOffset);
        }

        /// <inheritdoc />
        public virtual void CenterInGameView()
        {
            var viewport = Game.GraphicsDevice.Viewport;

            DrawPosition = new Vector2(viewport.Width/2 - BackgroundTexture.Width/2,
                                       viewport.Height/2 - BackgroundTexture.Height/2);
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
        protected virtual void Close(XNADialogResult result)
        {
            var eventArgs = new DialogClosingEventArgs(result);
            DialogClosing?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                Singleton<DialogRepository>.Instance.OpenDialogs.Pop();
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
    }

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
