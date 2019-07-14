namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// Represents a rectangular area that may be subdivided into smaller areas, which
    /// may be occupied (when the <see cref="used"/> is set). This is used in an algorithm that
    /// packs as many rectangles as possible.
    /// </summary>
    public class NodeSite
    {
        #region Members
        public NodeSite down;
        public NodeSite right;

        /// <summary>
        /// The dimensions of this node, which only matter if it's a leaf node.
        /// </summary>
        public NodeBounds bounds;

        /// <summary>
        /// Whether this node has been associated to a frame or not.
        /// </summary>
        public bool used;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a node with a width/height of 1 and x/y of 0. Used as the root node.
        /// </summary>
        public NodeSite()
        {
            down = null;
            right = null;
            used = false;
            bounds = new NodeBounds(0, 0, 1, 1);
        }

        /// <summary>
        /// Creates a node with child nodes.
        /// </summary>
        /// <param name="down">
        /// The node representing the area to the left or top of the separation in the node.
        /// </param>
        /// <param name="right">
        /// The node representing the area to the right or bottom of the separation in the node.
        /// </param>
        public NodeSite(NodeSite down, NodeSite right)
        {
            this.down = down;
            this.right = right;
            bounds = new NodeBounds();
            used = false;
        }

        /// <summary>
        /// Creates a leaf node with the given dimensions, for when a node is split.
        /// </summary>
        /// <param name="x">
        /// The leftmost edge of the rectangle the node occupies.
        /// </param>
        /// <param name="y">
        /// The topmost edge of the rectangle the node occupies.
        /// </param>
        /// <param name="width">
        /// The width of the rectangle the node occupies.
        /// </param>
        /// <param name="height">
        /// The height of the rectangle the node occupies.
        /// </param>
        public NodeSite(int x, int y, int width, int height)
        {
            down = null;
            right = null;
            bounds = new NodeBounds(x, y, width, height);
            used = false;
        }
        #endregion
    }
}