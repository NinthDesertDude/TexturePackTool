namespace TexturePackTool
{
    /// <summary>
    /// Represents a rectangular area that may be subdivided into smaller areas, which
    /// may be occupied (when the <see cref="used"/> is set). This is used in an algorithm that
    /// packs as many rectangles as possible.
    /// </summary>
    public class Node
    {
        #region Members
        /// <summary>
        /// The dimensions of this node, which only matter if it's a leaf node.
        /// </summary>
        public NodeBounds bounds;

        /// <summary>
        /// A child node within this one, which is vertically-oriented. The distinction is
        /// important to growth and splitting of nodes.
        /// </summary>
        public Node down;

        /// <summary>
        /// A child node within this one, which is horizontally-oriented. The distinction is
        /// important to growth and splitting of nodes.
        /// </summary>
        public Node right;

        /// <summary>
        /// Whether this node has been associated to a frame or not.
        /// </summary>
        public bool used;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a node with a width/height of 1 and x/y of 0. Used as the root node.
        /// </summary>
        public Node()
        {
            bounds = new NodeBounds(0, 0, 0, 0);
            down = null;
            right = null;
            used = false;
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
        public Node(Node down, Node right)
        {
            bounds = new NodeBounds();
            this.down = down;
            this.right = right;
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
        public Node(int x, int y, int width, int height)
        {
            bounds = new NodeBounds(x, y, width, height);
            down = null;
            right = null;
            used = false;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">
        /// The node whose values should be copied.
        /// </param>
        public Node(Node other)
        {
            bounds = other.bounds;
            down = other.down;
            right = other.right;
            used = other.used;
        }
        #endregion
    }
}