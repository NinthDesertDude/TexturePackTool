using System;
using System.Collections.Generic;
using System.Linq;
using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    public class TexturePacker
    {
        #region Members
        private Node rootNode;
        List<Node> largerNodes;
        #endregion

        #region Constructors
        /// <summary>
        /// Packs the given frames into a small rectangle based on simple heuristics with the
        /// given options. The results are accessible from the created object.
        /// </summary>
        /// <param name="frames">
        /// A list of frames with nonzero width and height (in pixels).
        /// </param>
        /// <param name="sortMethod">
        /// Sorts textures by size before insertion. <see cref="SortMethod.MaxSide"/> is the best
        /// choice for general-purpose texture packing.
        /// </param>
        /// <param name="splitMethod">
        /// When a node is split, the left or right side is filled first, but the remainder is
        /// split horizontally or vertically based on the given method.
        /// </param>
        public TexturePacker(List<Frame> frames, SortMethod sortMethod, SplitMethod splitMethod)
        {
            rootNode = new Node();
            List<Frame> sortedFrames;

            // Initially sorts the frames based on their dimensions and the chosen method.
            switch (sortMethod)
            {
                case SortMethod.MaxSide:
                    sortedFrames = SortFramesMaxSide(frames);
                    break;
                case SortMethod.Height:
                    sortedFrames = SortFramesGivenSide(frames, false);
                    break;
                case SortMethod.Width:
                    sortedFrames = SortFramesGivenSide(frames, true);
                    break;
                default:
                    sortedFrames = new List<Frame>(frames);
                    break;
            }

            // Adds each frame.
            for (int i = 0; i < sortedFrames.Count; i++)
            {
                largerNodes = new List<Node>();
                Frame frame = sortedFrames[i];
                bool success = ExactlyFillNode(rootNode, frame);

                // If the frame did not exactly fill an empty leaf node.
                if (!success)
                {
                    // Fill the smallest node that has a larger space.
                    if (largerNodes.Count > 0)
                    {
                        Node smallestNode = null;
                        int smallestArea = int.MaxValue;

                        largerNodes.ForEach(candidate =>
                        {
                            int w = candidate.bounds.Width - frame.W;
                            int h = candidate.bounds.Height - frame.H;
                            if (w * h < smallestArea)
                            {
                                smallestArea = w * h;
                                smallestNode = candidate;
                            }
                        });

                        SplitFillNode(smallestNode, frame, splitMethod);
                    }

                    // Fill a new space on the outside, growing the rectangle.
                    else
                    {
                        GrowFillNode(rootNode, frame);
                    }
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to associate the given frame with an existing packing node, keeping track of
        /// all larger open spaces to pick the tightest fit.
        /// </summary>
        /// <param name="node">
        /// The root node.
        /// </param>
        /// <param name="frame">
        /// The associated frame to store, which is expected to have width and height defined.
        /// </param>
        private bool ExactlyFillNode(Node node, Frame frame)
        {
            bool isLeaf = node.leftOrTopNode == null && node.rightOrBottomNode == null;

            // Attempts to associate the frame with a node that hasn't been filled.
            if (isLeaf)
            {
                // Can't associate the frame with a node that has been filled.
                if (frame != null)
                {
                    return false;
                }

                // If the leaf node has exact dimensions, fill it.
                if (node.bounds.Width == frame.W &&
                    node.bounds.Height == frame.H)
                {
                    node.frame = frame;
                    return true;
                }

                // If the leaf node has larger dimensions, mark it as a candidate to fill.
                else if (node.bounds.Width >= frame.W &&
                    node.bounds.Height >= frame.H)
                {
                    largerNodes.Add(node);
                }

                // If the leaf node has smaller dimensions, the frame can't be associated.
                return false;
            }

            // Recursively iterates over child nodes for non-leaf nodes.
            else
            {
                // Skip traversing child nodes when parent is already smaller than required.
                if (node.bounds.Width < frame.W ||
                    node.bounds.Height < frame.H)
                {
                    return false;
                }

                if (node.leftOrTopNode != null)
                {
                    if (ExactlyFillNode(node.leftOrTopNode, frame))
                    {
                        return true;
                    }
                }

                if (node.rightOrBottomNode != null)
                {
                    if (ExactlyFillNode(node.rightOrBottomNode, frame))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Converts the given leaf node into a parent node with two children, splitting according
        /// to the given splitting method. The leftmost or topmost node created is always the one
        /// filled.
        /// </summary>
        /// <param name="node">
        /// A leaf node to split.
        /// </param>
        /// <param name="frame">
        /// The associated frame to store, which is expected to have width and height defined.
        /// </param>
        private void SplitFillNode(Node node, Frame frame, SplitMethod splitMethod)
        {
            // Inverts the result for perpendicular split so results are opposite.
            bool widerThanTall = splitMethod == SplitMethod.Parallel
                ? frame.W > frame.H
                : frame.W < frame.H;

            int extraWidth = node.bounds.Width - frame.W;
            int extraHeight = node.bounds.Height - frame.H;

            // The new left/top node is set to the frame and its dimensions.
            node.leftOrTopNode = new Node(node.bounds.X, node.bounds.Y, frame);

            // The new right/bottom node has all remaining space.
            node.rightOrBottomNode = new Node(
                widerThanTall ? node.bounds.X : node.bounds.X + frame.W,
                widerThanTall ? node.bounds.Y + frame.H : node.bounds.Y,
                widerThanTall ? frame.W + extraWidth : extraWidth,
                widerThanTall ? extraHeight : frame.H + extraHeight);
        }

        /// <summary>
        /// Places the root node in a parent node as the left/top child, creating a new node for
        /// the right/bottom child. Splits that node to add the texture to it. Returns the new
        /// container node.
        /// </summary>
        /// <param name="node">
        /// The root node.
        /// </param>
        /// <param name="frame">
        /// The associated frame to store, which is expected to have width and height defined.
        /// </param>
        /// <returns>
        /// The new root node.
        /// </returns>
        private Node GrowFillNode(Node root, Frame frame)
        {
            bool canGrowDown = frame.W <= root.bounds.Width;
            bool canGrowRight = frame.H <= root.bounds.Height;

            Node rightOrBottomNode = frame.W > frame.H
                ? new Node(
                    root.bounds.X, root.bounds.Y + root.bounds.Height,
                    root.bounds.Width, frame.H)
                : new Node(
                    root.bounds.X + root.bounds.Width, root.bounds.Y,
                    frame.W, root.bounds.Height);

            Node containingNode = new Node(root, rightOrBottomNode);
            SplitFillNode(rightOrBottomNode, frame, SplitMethod.Perpendicular);

            return containingNode;
        }
 
        /// <summary>
        /// Sorts frames largest first via max(width, height). This couples with the packing method
        /// so most texture edges align along the top-left to bottom-right diagonal, meaning errors
        /// will manifest there.
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
        /// Sorts frames from largest to smallest with the given 
        /// </summary>
        /// <param name="frames">
        /// The frames representing all the images.
        /// </param>
        /// <param name="widthOverHeight">
        /// Whether to sort by width (true) or height (false).
        /// </param>
        /// <returns>
        /// A sorted list of the frames.
        /// </returns>
        private List<Frame> SortFramesGivenSide(IEnumerable<Frame> frames, bool widthOverHeight)
        {
            return frames
                .OrderByDescending(o => widthOverHeight ? o.W : o.H)
                .ThenByDescending(o => widthOverHeight ? o.H : o.W)
                .ToList();
        }
        #endregion
    }
}
