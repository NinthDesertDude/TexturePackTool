namespace TexturePackTool
{
    /// <summary>
    /// Represents a rectangular area, with an additional property indicating if this rectangle has been merged into
    /// a larger superset rectangle. Using an indicator is easier than updating all values.
    /// </summary>
    public class Region
    {
        public int X;
        public int Y;
        public int X2;
        public int Y2;
        public Region SupersetRegion;

        /// <summary>
        /// Creates a new Region of 1x1 size at the given position.
        /// </summary>
        public Region(int x, int y)
        {
            X = x;
            Y = y;
            X2 = x;
            Y2 = y;
        }

        /// <summary>
        /// Returns the final superset in the chain of supersets this region is connected to.
        /// </summary>
        public Region GetLargestSuperset()
        {
            Region finalSuperset = SupersetRegion;
            while (finalSuperset != null)
            {
                if (finalSuperset.SupersetRegion == null) { return finalSuperset; }
                finalSuperset = finalSuperset.SupersetRegion;
            }

            return this;
        }
    }
}