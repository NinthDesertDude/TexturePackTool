namespace TexturePackTool
{
    /// <summary>
    /// Represents a rectangular area for a node.
    /// </summary>
    public struct NodeBounds
    {
        /// <summary>
        /// The horizontal x-coordinate.
        /// Default 0.
        /// </summary>
        public int X;

        /// <summary>
        /// The vertical y-coordinate.
        /// Default 0.
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the represented area. Numbers less than 1 are invalid.
        /// Default 0.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the represented area. Numbers less than 1 are invalid.
        /// Default 0.
        /// </summary>
        public int Height;

        /// <summary>
        /// Instantiates a bounds object for the given dimensions.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="width">The nonzero width of the represented area.</param>
        /// <param name="height">The nonzero height of the represented area.</param>
        public NodeBounds(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}