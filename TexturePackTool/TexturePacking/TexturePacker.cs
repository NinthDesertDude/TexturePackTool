using System;
using System.Collections.Generic;
using System.Linq;
using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    public class TexturePacker
    {
        Node root = null;

        public void Fit(List<Frame> frames)
        {
            List<Frame> sortedFrames = SortFramesMaxSide(frames);

            int len = sortedFrames.Count;
            root = new Node(0, 0, sortedFrames[0].W, sortedFrames[0].H);

            foreach (Frame frame in sortedFrames)
            {
                Node node = FindNode(root, frame.W, frame.H);

                frame.fit = node != null
                    ? SplitNode(node, frame.W, frame.H)
                    : GrowNode(frame.W, frame.H);
            }
        }

        private Node FindNode(Node root, int width, int height)
        {
            if (root.used)
            {
                return FindNode(root.right, width, height) ??
                    FindNode(root.down, width, height);
            }

            if (width <= root.bounds.Width &&
                height <= root.bounds.Height)
            {
                return root;
            }

            return null;
        }

        private Node SplitNode(Node node, int width, int height)
        {
            node.used = true;

            node.down = new Node(
                node.bounds.X,
                node.bounds.Y + width,
                node.bounds.Width,
                node.bounds.Height - height);

            node.right = new Node(
                node.bounds.X + width,
                node.bounds.Y,
                node.bounds.Width - width,
                node.bounds.Height);

            return node;
        }

        private Node GrowNode(int width, int height)
        {
            bool canGrowDown = width <= root.bounds.Width;
            bool canGrowRight = height <= root.bounds.Height;
            bool shouldGrowRight = canGrowRight && root.bounds.Height >= root.bounds.Width + width;
            bool shouldGrowDown = canGrowDown && root.bounds.Width >= root.bounds.Height + height;

            if (shouldGrowRight)
            {
                return GrowRight(width, height);
            }
            if (shouldGrowDown)
            {
                return GrowDown(width, height);
            }
            if (canGrowRight)
            {
                return GrowRight(width, height);
            }
            if (canGrowDown)
            {
                return GrowDown(width, height);
            }

            return null;
        }

        private Node GrowRight(int width, int height)
        {
            root = new Node(0, 0, root.bounds.Width + width, root.bounds.Height)
            {
                used = true,
                down = root,
                right = new Node(root.bounds.Width, 0, width, root.bounds.Height)
            };

            Node node = FindNode(root, width, height);
            if (node != null)
            {
                return SplitNode(node, width, height);
            }

            return node;
        }

        private Node GrowDown(int width, int height)
        {
            root = new Node(0, 0, root.bounds.Width + width, root.bounds.Height)
            {
                used = true,
                down = new Node(0, root.bounds.Height, root.bounds.Width, height),
                right = root
            };

            Node node = FindNode(root, width, height);
            if (node != null)
            {
                return SplitNode(node, width, height);
            }

            return node;
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
    }
}
