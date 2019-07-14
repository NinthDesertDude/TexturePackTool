using System;
using System.Collections.Generic;
using System.Linq;
using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    public class TexturePackerSite
    {
        NodeSite root = null;

        private void Fit(List<Frame> frames)
        {
            int len = frames.Count;
            root = new NodeSite(0, 0, frames[0].W, frames[0].H);

            foreach (Frame frame in frames)
            {
                NodeSite node = FindNode(root, frame.W, frame.H);

                frame.fit = node != null
                    ? SplitNode(node, frame.W, frame.H)
                    : GrowNode(frame.W, frame.H);
            }
        }

        private NodeSite FindNode(NodeSite root, int width, int height)
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

        private NodeSite SplitNode(NodeSite node, int width, int height)
        {
            node.used = true;

            node.down = new NodeSite(
                node.bounds.X,
                node.bounds.Y + width,
                node.bounds.Width,
                node.bounds.Height - height);

            node.right = new NodeSite(
                node.bounds.X + width,
                node.bounds.Y,
                node.bounds.Width - width,
                node.bounds.Height);

            return node;
        }

        public NodeSite GrowNode(int width, int height)
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

        public NodeSite GrowRight(int width, int height)
        {
            root = new NodeSite(0, 0, root.bounds.Width + width, root.bounds.Height)
            {
                used = true,
                down = root,
                right = new NodeSite(root.bounds.Width, 0, width, root.bounds.Height)
            };

            NodeSite node = FindNode(root, width, height);
            if (node != null)
            {
                return SplitNode(node, width, height);
            }

            return node;
        }

        public NodeSite GrowDown(int width, int height)
        {
            root = new NodeSite(0, 0, root.bounds.Width + width, root.bounds.Height)
            {
                used = true,
                down = new NodeSite(0, root.bounds.Height, root.bounds.Width, height),
                right = root
            };

            NodeSite node = FindNode(root, width, height);
            if (node != null)
            {
                return SplitNode(node, width, height);
            }

            return node;
        }
    }
}
