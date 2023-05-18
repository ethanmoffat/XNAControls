namespace XNAControls
{
    /// <summary>
    /// Represents a handler that manages scrolling input
    /// </summary>
    public interface IScrollHandler
    {
        /// <summary>
        /// The number of lines that are visible for this scroll handler
        /// </summary>
        int LinesToRender { get; set; }

        /// <summary>
        /// The offset in the collection at which to start rendering lines
        /// </summary>
        int ScrollOffset { get; }

        /// <summary>
        /// Scroll the collection to the top
        /// </summary>
        void ScrollToTop();

        /// <summary>
        /// Scroll the collection to the bottom
        /// </summary>
        void ScrollToEnd();

        /// <summary>
        /// Update the total number of scrollable elements in the container
        /// </summary>
        void UpdateDimensions(int count);
    }
}
