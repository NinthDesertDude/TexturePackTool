using System;
using System.Collections.Generic;
using System.Linq;
using TexturePackTool.Model;

namespace TexturePackTool.TexturePacking
{
    public class TexturePacker
    {
        #region Members
        /// <summary>
        /// TODO: ???
        /// </summary>
        private List<PackingNode> root;

        private 
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new texture packer from a list of frames with nonzero width/height.
        /// </summary>
        /// <param name="frames">
        /// A list of frames that have positive nonzero width and height in pixels.
        /// </param>
        public TexturePacker(List<Frame> frames)
        {
            root = new List<PackingNode>();
            frames.ForEach(frame =>
            {
                root.Add(new PackingNode()
                {
                    frame = frame,
                    width = frame.packedWidth,
                    height = frame.packedHeight,
                });
            });
        }
        #endregion

        public void Pack()
        {

        }

        #region Methods
        private void Fit(List<PackingNode> blocks)
        {
            PackingNode node;
            int n = 0;

            blocks.ForEach(block =>
            {
                node = FindNode(this.root.ElementAt(n), block.width, block.height);
                if (node != null)
                {
                    block.fit = SplitNode(node, block.width, block.height);
                    if (node.isRoot)
                    {
                        block.fit.isRoot = true;
                    }
                }
                else
                {
                    n++;
                }
            });
        }

        private PackingNode FindNode(PackingNode root, int width, int height)
        {
            if (root.frame != null)
            {
                PackingNode right = FindNode(root.right, width, height);

                return right != null
                    ? right
                    : FindNode(root.down, width, height);
            }

            return (width <= root.width && height <= root.height)
                ? root
                : null;
        }

        private PackingNode SplitNode(PackingNode node, int width, int height)
        {
            node.frame = stuff; //TODO
            node.down = new PackingNode(node.x, node.y + height, node.width, node.height - height);
            node.right = new PackingNode(node.x + width, node.y, node.width - width, node.height);
            return node; // TODO: Not sure why we're returning an object we already have access to.
        }

        /// <summary>
        /// Sorts rectangles largest first via max(width, height).
        /// </summary>
        /// <param name="rects">
        /// The rectangles representing all the images.
        /// </param>
        /// <returns>
        /// A sorted list of the rectangles.
        /// </returns>
        private List<Frame> SortRects(List<Frame> rects)
        {
            return rects
                .OrderBy(o => Math.Max(o.packedWidth, o.packedHeight))
                .Reverse()
                .ToList();
        }
        #endregion
    }
}
