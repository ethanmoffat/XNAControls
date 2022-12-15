using Microsoft.Xna.Framework;

namespace XNAControls.Input
{
    public interface IEventReceiver
    {
        /// <summary>
        /// The vertical ordering for handling this event receiver. Higher value gets priority.
        /// </summary>
        public int ZOrder { get; }

        /// <summary>
        /// The area representing this event receiver
        /// </summary>
        public Rectangle EventArea { get; }

        /// <summary>
        /// Gets or sets the input events that this control instance will handle. Useful for making child controls not process an event.
        /// </summary>
        EventType HandlesEvents { get; set; }

        /// <summary>
        /// Send this control a message
        /// </summary>
        /// <param name="eventType">The event type to send</param>
        /// <param name="eventArgs">Additional arguments for the event</param>
        void SendMessage(EventType eventType, object eventArgs);
    }
}
