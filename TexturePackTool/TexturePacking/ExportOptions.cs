namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// These options are passed on to the texture packing algorithm to modify how it behaves.
    /// </summary>
    public enum ExportOptions
    {
        /// <summary>
        /// Exports the images using a simple greedy packing algorithm. 
        /// </summary>
        NoOffset,

        /// <summary>
        /// Exports the images with all bordering transparent areas colored black. Some users might prefer this for 
        /// visual ease, or debugging.
        /// </summary>
        BlackBorders,

        /// <summary>
        /// Many game engines borrow the pixels just one to the top, right, left, or bottom of a sprite to speed up
        /// resize scaling. If there are opaque colors present, they will bleed into the edges of the resulting resized
        /// image, so it's common to produce spritesheets that leave a transparent border. This option adds the offset
        /// to avoid that negative result.
        /// </summary>
        HalfPixelOffset,
    }
}