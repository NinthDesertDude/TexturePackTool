using System;
using System.Collections.Generic;
using System.Linq;
using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    /// <summary>
    /// Takes many frames with rectangular areas and packs them using a simple, efficient
    /// heuristic.
    /// </summary>
    public class TexturePacker
    {
        /// <summary>
        /// Provides special instructions to the texture packer, e.g. adding a 1px transparent border while packing.
        /// </summary>
        public ExportOptions Options;

        /// <summary>
        /// The top-level node, or null if not set. The bounds of this node should encompass the
        /// full resulting collection of rectangles after packing.
        /// </summary>
        public Node Root;

        /// <summary>
        /// Creates a new texture packer.
        /// </summary>
        public TexturePacker(ExportOptions options)
        {
            Root = null;
            Options = options;
        }

        #region Methods
        /// <summary>
        /// Sorts frames largest first via max(width, height). This couples with the packing method
        /// so most texture edges align along the top-left to bottom-right diagonal, meaning errors
        /// will manifest there. Prevents having to grow down and to the right at the same time.
        /// The output will also be more square.
        /// </summary>
        /// <param name="frames">
        /// The rectangles representing all the images.
        /// </param>
        /// <returns>
        /// A sorted list of the frames.
        /// </returns>
        private List<Frame> SortFramesMaxSide(IEnumerable<Frame> frames)
        {
            return frames
                .OrderByDescending(o => Math.Max(o.W, o.H))
                .ThenByDescending(o => Math.Min(o.W, o.H))
                .ToList();
        }

        /// <summary>
        /// Packs all frames into the smallest rectangle possible using a fast heuristic. The
        /// results are reflected by changing the x,y coordinates of the frames passed in.
        /// </summary>
        /// <param name="frames">
        /// The frames to be packed.
        /// </param>
        public void Pack(List<Frame> frames)
        {
            if (frames.Count == 0)
            {
                return;
            }

            // Pads each frame by 2 pixels to center the image with a transparent border.
            if (Options == ExportOptions.HalfPixelOffset || Options == ExportOptions.BlackBorders)
            {
                frames.ForEach((frame) => { frame.W += 1; frame.H += 1; });
            }

            // Sorts to simplify packing textures.
            List<Frame> sortedFrames = SortFramesMaxSide(frames);

            // Starts with the root node already set.
            Root = new Node(0, 0, sortedFrames[0].W, sortedFrames[0].H);
            Root.used = true;
            sortedFrames[0].X = Root.bounds.X;
            sortedFrames[0].Y = Root.bounds.Y;

            // Adds each frame by finding a node that fits the frame and either placing the frame
            // at top/left and splitting the remaining space to a separate node, or growing the
            // total space first and then splitting.
            for (int i = 1; i < sortedFrames.Count; i++)
            {
                Frame frame = sortedFrames[i];
                Node node = FindNode(Root, frame.W, frame.H);

                Node fitNode = node != null
                    ? SplitNode(node, frame)
                    : GrowNode(frame);

                frame.X = fitNode.bounds.X;
                frame.Y = fitNode.bounds.Y;
            }
        }

        /// <summary>
        /// Recursively tries to find a node that fits the given width/height requirements. Returns
        /// null on failure.
        /// </summary>
        /// <param name="node">
        /// The node to recursively look within.
        /// </param>
        /// <param name="width">
        /// The required minimum width of a matching node.
        /// </param>
        /// <param name="height">
        /// The required minimum height of a matching node.
        /// </param>
        /// <returns>
        /// True if an unused node meeting the required minimum width/height is found, else false.
        /// </returns>
        private Node FindNode(Node node, int width, int height)
        {
            if (node == null)
            {
                return null;
            }

            // Recursively digs into child nodes searching for space if present. Arbitrarily
            // looks to the right first.
            if (node.used)
            {
                return FindNode(node.right, width, height) ??
                    FindNode(node.down, width, height);
            }

            // For unused nodes, returns after the first suitable match. This can be improved to
            // find all suitable matches and select the best.
            if (width <= node.bounds.Width &&
                height <= node.bounds.Height)
            {
                return node;
            }

            return null;
        }

        /// <summary>
        /// Marks the node as used, separating into three logical spaces: everything below, to
        /// the right, and diagonally adjacent. The diagonal area is included as part of the space
        /// to the right.
        /// </summary>
        /// <param name="node">
        /// The node to split.
        /// </param>
        /// <param name="width">
        /// The width of a frame.
        /// </param>
        /// <param name="height">
        /// The height of a frame.
        /// </param>
        /// <returns>
        /// The same node passed in, for convenience.
        /// </returns>
        private Node SplitNode(Node node, Frame frame)
        {
            node.used = true;

            node.down = new Node(
                node.bounds.X,
                node.bounds.Y + frame.H,
                node.bounds.Width,
                node.bounds.Height - frame.H);

            node.right = new Node(
                node.bounds.X + frame.W,
                node.bounds.Y,
                node.bounds.Width - frame.W,
                node.bounds.Height);

            return node; // Redundant return keeps code clean elsewhere and matches GrowNode.
        }

        /// <summary>
        /// Increases the size of the rectangle in order to place a node that doesn't otherwise
        /// fit.
        /// </summary>
        /// <param name="frame">
        /// The frame to be placed.
        /// </param>
        /// <returns>
        /// The new root node that encapsulates the old root and expands the space.
        /// </returns>
        private Node GrowNode(Frame frame)
        {
            bool canGrowDown = frame.W <= Root.bounds.Width;
            bool canGrowRight = frame.H <= Root.bounds.Height;
            bool shouldGrowRight = canGrowRight && Root.bounds.Height >= Root.bounds.Width + frame.W;
            bool shouldGrowDown = canGrowDown && Root.bounds.Width >= Root.bounds.Height + frame.H;

            if (shouldGrowRight)
            {
                return GrowRight(frame);
            }
            if (shouldGrowDown)
            {
                return GrowDown(frame);
            }
            if (canGrowRight)
            {
                return GrowRight(frame);
            }
            if (canGrowDown)
            {
                return GrowDown(frame);
            }

            return null;
        }

        /// <summary>
        /// Creates a node encapsulating the root, creating additional space to the right to place
        /// the given frame into. Splits that space after adding the given frame into it.
        /// </summary>
        /// <param name="frame">
        /// The frame to place.
        /// </param>
        /// <returns>
        /// The new root node.
        /// </returns>
        private Node GrowRight(Frame frame)
        {
            Root = new Node(0, 0, Root.bounds.Width + frame.W, Root.bounds.Height)
            {
                used = true,
                down = new Node(Root),
                right = new Node(Root.bounds.Width, 0, frame.W, Root.bounds.Height)
            };

            Node node = FindNode(Root, frame.W, frame.H);
            if (node != null)
            {
                SplitNode(node, frame);
            }

            return node;
        }

        /// <summary>
        /// Creates a node encapsulating the root, creating additional space below to place the
        /// given frame into. Splits that space after adding the given frame into it.
        /// </summary>
        /// <param name="frame">
        /// The frame to place.
        /// </param>
        /// <returns>
        /// The new root node.
        /// </returns>
        private Node GrowDown(Frame frame)
        {
            Root = new Node(0, 0, Root.bounds.Width, Root.bounds.Height + frame.H)
            {
                used = true,
                down = new Node(0, Root.bounds.Height, Root.bounds.Width, frame.H),
                right = new Node(Root)
            };

            Node node = FindNode(Root, frame.W, frame.H);
            if (node != null)
            {
                SplitNode(node, frame);
            }

            return node;
        }
        #endregion
    }
}
