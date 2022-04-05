using System;
using Microsoft.Xna.Framework;

namespace XNAControls
{
    public class KeyboardDispatcher : IDisposable
    {
        internal const char CHAR_RETURNKEY_CODE = '\r';
        internal const char CHAR_BACKSPACE_CODE = '\b';
        internal const char CHAR_TAB_CODE = '\t';

        private readonly IKeyboardEvents _events;

        private IKeyboardSubscriber _subscriber;
        public IKeyboardSubscriber Subscriber
        {
            get => _subscriber;
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
                _subscriber.ReceiveCommandInput(e.Character);
            }
            else
            {
                _subscriber.ReceiveTextInput(e.Character);
            }
        }

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
