// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using Microsoft.Xna.Framework;

namespace XNAControls.Old
{
    public class KeyboardDispatcher : IDisposable
    {
        private const char CHAR_PASTE_CODE = (char)0x16;
        internal const char CHAR_RETURNKEY_CODE = '\r';
        internal const char CHAR_BACKSPACE_CODE = '\b';
        internal const char CHAR_TAB_CODE = '\t';

        private readonly IKeyboardEvents _events;

        private IKeyboardSubscriber _subscriber;
        public IKeyboardSubscriber Subscriber
        {
            get { return _subscriber; }
            set
            {
                SetSubscriberSelected(false);
                _subscriber = value;
                SetSubscriberSelected(true);
            }
        }

        private void SetSubscriberSelected(bool selected)
        {
            if (_subscriber != null)
                _subscriber.Selected = selected;
        }

        public KeyboardDispatcher(GameWindow window)
        {
            _events = new KeyboardEvents(window);
            _events.CharEntered += EventInput_CharEntered;
        }

        private void EventInput_CharEntered(object sender, CharEnteredEventArgs e)
        {
            if (_subscriber == null)
                return;

            if (char.IsControl(e.Character))
            {
                if (e.Character == CHAR_PASTE_CODE)
                {
                    GetClipboardInfoFromThread();
                    _subscriber.ReceiveTextInput(_pasteResult);
                }
                else
                {
                    _subscriber.ReceiveCommandInput(e.Character);
                }
            }
            else
            {
                _subscriber.ReceiveTextInput(e.Character);
            }
        }

        #region Clipboard Handling

        private string _pasteResult = "";

        private void GetClipboardInfoFromThread()
        {
            var thread = new Thread(SetPasteResult);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        [STAThread]
        void SetPasteResult()
        {
            _pasteResult = System.Windows.Forms.Clipboard.ContainsText() ? System.Windows.Forms.Clipboard.GetText() : "";
        }

        #endregion

        #region IDisposable

        ~KeyboardDispatcher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _events.CharEntered -= EventInput_CharEntered;
                _events.Dispose();
            }
        }

        #endregion
    }
}
