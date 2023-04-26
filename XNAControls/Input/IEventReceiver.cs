using Microsoft.Xna.Framework;

namespace XNAControls.Input
{
    /// <summary>
    /// Interface representing an entity that can receive events from the Game's InputManager
    /// </summary>
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
        /// Post a message to a control (adds this message to the message queue)
        /// </summary>
        /// <param name="eventType">The event type to send</param>
        /// <param name="eventArgs">Additional arguments for the event</param>
        void PostMessage(EventType eventType, object eventArgs);

        /// <summary>
        /// Send a message to a control for immediate processing (bypasses message queue)
        /// </summary>
        /// <param name="eventType">The event type to post</param>
        /// <param name="eventArgs">Additional arguments for the event</param>
        /// <returns>True if the event was handled, false otherwise</returns>
        bool SendMessage(EventType eventType, object eventArgs);
    }
}
