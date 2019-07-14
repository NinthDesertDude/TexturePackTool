namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// The method used to pre-sort frames based on their dimensions.
    /// </summary>
    public enum SortMethod
    {
        /// <summary>
        /// Sorts by Math.Max(width, height), largest to smallest. This places most gaps along a
        /// diagonal from top-left to bottom-right of the texture, where frames frequently meet.
        /// </summary>
        MaxSide,

        /// <summary>
        /// Sorts by height (then width), largest to smallest. This is more efficient than
        /// <see cref="MaxSide"/> when most blocks have small widths.
        /// </summary>
        Height,

        /// <summary>
        /// Sorts by width (then height), largest to smallest. This is more efficient than
        /// <see cref="MaxSide"/> when most blocks have small heights.
        /// </summary>
        Width
    }
}
