namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// The method used to split a node.
    /// </summary>
    public enum SplitMethod
    {
        /// <summary>
        /// Splits parallel to the longest side. If a block has greater width than height, it would
        /// split horizontally. If both sides are equal, splits vertically.
        /// </summary>
        Parallel,

        /// <summary>
        /// Splits perpendicular to the longest side. If a block has greater width than height, it
        /// would split vertically. If both sides are equal, splits vertically.
        /// </summary>
        Perpendicular
    }
}
