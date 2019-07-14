using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// Represents a rectangular area that may be subdivided into smaller areas, which
    /// may be occupied (when the <see cref="frame"/> is set). This is used in an algorithm that
    /// packs as many rectangles as possible.
    /// </summary>
    public class Node
    {
        #region Members
        /// <summary>
        /// If this isn't a leaf node, it has a left/top node (depending on whether it's split
        /// horizontally or vertically) and a right/bottom node for the space within it.
        /// </summary>
        public Node leftOrTopNode;

        /// <summary>
        /// If this isn't a leaf node, it has a left/top node (depending on whether it's split
        /// horizontally or vertically) and a right/bottom node for the space within it.
        /// </summary>
        public Node rightOrBottomNode;

        /// <summary>
        /// The dimensions of this node, which only matter if it's a leaf node.
        /// </summary>
        public NodeBounds bounds;

        /// <summary>
        /// The associated frame occupying this node, if given. Only leaf nodes can be occupied.
        /// </summary>
        public Frame frame = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a node with a width/height of 1 and x/y of 0. Used as the root node.
        /// </summary>
        public Node()
        {
            leftOrTopNode = null;
            rightOrBottomNode = null;
            frame = null;
            bounds = new NodeBounds(0, 0, 1, 1);
        }

        /// <summary>
        /// Creates a node with child nodes.
        /// </summary>
        /// <param name="leftOrTop">
        /// The node representing the area to the left or top of the separation in the node.
        /// </param>
        /// <param name="rightOrBottom">
        /// The node representing the area to the right or bottom of the separation in the node.
        /// </param>
        public Node(Node leftOrTop, Node rightOrBottom)
        {
            leftOrTopNode = leftOrTop;
            rightOrBottomNode = rightOrBottom;
            bounds = new NodeBounds();
            frame = null;
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
            leftOrTopNode = null;
            rightOrBottomNode = null;
            bounds = new NodeBounds(x, y, width, height);
            frame = null;
        }

        /// <summary>
        /// Creates a leaf node with the <see cref="Frame"/>'s dimensions.
        /// </summary>
        /// <param name="x">
        /// The leftmost edge of the rectangle the node occupies.
        /// </param>
        /// <param name="y">
        /// The topmost edge of the rectangle the node occupies.
        /// </param>
        /// <param name="frame">
        /// The frame whose width and height is used for this node.
        /// </param>
        public Node(int x, int y, Frame frame)
        {
            leftOrTopNode = null;
            rightOrBottomNode = null;
            bounds = new NodeBounds(x, y, frame.W, frame.H);
            this.frame = frame;
        }
        #endregion
    }
}