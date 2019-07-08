using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// Represents a rectangular area that may be subdivided into smaller areas, which
    /// may be occupied (when the <see cref="frame"/> is set). This is used in an algorithm that
    /// packs as many rectangles as possible.
    /// </summary>
    public class PackingNode
    {
        #region Members
        /// <summary>
        /// Whether this is the root node or not.
        /// </summary>
        public bool isRoot;

        /// <summary>
        /// TODO: The name?
        /// </summary>
        public string name;

        /// <summary>
        /// The leftmost edge of the rectangle the node occupies.
        /// </summary>
        public int x;

        /// <summary>
        /// The topmost edge of the rectangle the node occupies.
        /// </summary>
        public int y;

        /// <summary>
        /// The width of the rectangle the node occupies.
        /// </summary>
        public int width;

        /// <summary>
        /// The height of the rectangle the node occupies.
        /// </summary>
        public int height;

        /// <summary>
        /// The frame associated with this node.
        /// </summary>
        public Frame frame;

        /// <summary>
        /// The sibling node to the right of the rectangle this node occupies.
        /// </summary>
        public PackingNode right;

        /// <summary>
        /// The sibling node below the rectangle this node occupies.
        /// </summary>
        public PackingNode down;

        /// <summary>
        /// TODO: ???
        /// </summary>
        public PackingNode fit;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an empty node.
        /// </summary>
        public PackingNode()
        {
            isRoot = false;
            name = string.Empty;
            x = y = width = height = 0;
            frame = null;
            right = null;
            down = null;
            fit = null;
        }

        /// <summary>
        /// Creates a node with the given name and dimensions.
        /// </summary>
        /// <param name="name">
        /// The name of the node.
        /// </param>
        /// <param name="width">
        /// The width of the node.
        /// </param>
        /// <param name="height">
        /// The height of the node.
        /// </param>
        public PackingNode(string name, int width, int height)
        {
            isRoot = false;
            this.name = name;
            this.width = width;
            this.height = height;
            x = y = 0;
            frame = null;
            right = null;
            down = null;
            fit = null;
        }

        /// <summary>
        /// Creates a node with the given dimensions.
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
        public PackingNode(int x, int y, int width, int height)
        {
            isRoot = false;
            name = string.Empty;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            frame = null;
            right = null;
            down = null;
            fit = null;
        }
        #endregion
    }
}