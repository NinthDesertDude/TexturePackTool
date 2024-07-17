using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using TexturePackTool.Utilities;

namespace TexturePackTool
{
    /// <summary>
    /// Detects contiguous pixel islands and returns a list of rectangles for a given image.
    /// </summary>
    public static class TextureSplitting
    {
        /// <summary>
        /// Loads the given texture and iterates through it to create rects which will each be exported as individual
        /// tiles. For a tile to be recognized, it needs to be surrounded by background pixels (irregular shapes
        /// count). Returns null on success, or a two-string tuple representing an error message, then caption.
        /// </summary>
        /// <param name="textureUrl">The file path for the texture which will be analyzed.</param>
        /// <param name="outputDir">The output directory where tiles will be saved out of the texture.</param>
        public static unsafe Tuple<string, string> SplitByRegion(
            string textureUrl,
            string outputDir,
            string backgroundColor = "",
            bool skipSmallBounds = true,
            bool useDiagonals = true)
        {
            if (!File.Exists(textureUrl))
            {
                return new Tuple<string, string>(
                    $"The texture doesn't exist or can't be found",
                    "Splitting by regions failed to begin");
            }

            if (!string.IsNullOrEmpty(backgroundColor) &&
                !DrawingUtils.IsHexColor(backgroundColor))
            {
                return new Tuple<string, string>(
                    $"The background color given '{backgroundColor}' needs to be \"\""
                        + " or a 6 or 8 length hexadecimal string.",
                    "Splitting by regions failed to begin");
            }

            int tileNum = 0;

            try
            {
                using (Bitmap bmpRaw = (Bitmap)Image.FromFile(textureUrl))
                {
                    if (bmpRaw == null || bmpRaw.Width == 0 || bmpRaw.Height == 0)
                    {
                        return new Tuple<string, string>(
                            $"The texture didn't load correctly (or width/height is zero).",
                            "Splitting by regions failed");
                    }

                    using (var bmpPng = DrawingUtils.FormatImage(bmpRaw, PixelFormat.Format32bppArgb))
                    {
                        Directory.CreateDirectory(outputDir);

                        var pixelRegions = new Region[bmpPng.Width, bmpPng.Height];
                        var regions = new List<Region>();

                        BitmapData bmpData = bmpPng.LockBits(
                            new Rectangle(0, 0, bmpPng.Width, bmpPng.Height),
                            ImageLockMode.ReadOnly,
                            bmpPng.PixelFormat);

                        byte* row = (byte*)bmpData.Scan0;
                        Region left, up;
                        Region upleft = null;
                        Region upright = null;

                        // Handles an optional color passed in to process instead of looking for transparency.
                        bool useTargetColor = false;
                        Color targetColor = Color.Transparent;

                        if (!string.IsNullOrEmpty(backgroundColor))
                        {
                            useTargetColor = true;
                            targetColor = Color.FromArgb(
                                (backgroundColor.Length == 8)
                                    ? int.Parse(backgroundColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
                                    : 255,
                                int.Parse(backgroundColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                                int.Parse(backgroundColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                                int.Parse(backgroundColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
                            );
                        }

                        // Iterate every pixel to track regions
                        for (int y = 0; y < bmpPng.Height; y++)
                        {
                            byte* pixel = row + (y * bmpData.Stride);

                            for (int x = 0; x < bmpPng.Width; x++)
                            {
                                // If this is a background color, go to next pixel
                                if ((!useTargetColor && (*(pixel + 3) != 0)) ||
                                    (useTargetColor &&
                                        ((*pixel != targetColor.B)
                                        || (*(pixel + 1) != targetColor.G)
                                        || (*(pixel + 2) != targetColor.R)
                                        || (*(pixel + 3) != targetColor.A))))
                                {
                                    // Get the regions
                                    left = (x != 0)
                                        ? pixelRegions[x - 1, y]?.GetLargestSuperset() : null;
                                    up = (y != 0)
                                        ? pixelRegions[x, y - 1]?.GetLargestSuperset() : null;

                                    if (useDiagonals)
                                    {
                                        upleft = x != 0 && y != 0
                                            ? pixelRegions[x - 1, y - 1]?.GetLargestSuperset() : null;
                                        upright = x != bmpPng.Width - 1 && y != 0
                                            ? pixelRegions[x + 1, y - 1]?.GetLargestSuperset() : null;
                                    }

                                    // Two regions are connected via this pixel: top-left and top-right
                                    if (upleft != null && upright != null)
                                    {
                                        pixelRegions[x, y] = upleft;
                                        if (upleft != upright) { upright.SupersetRegion = upleft; }
                                        upleft.X = Math.Min(x, Math.Min(upleft.X, upright.X));
                                        upleft.Y = Math.Min(y, Math.Min(upleft.Y, upright.Y));
                                        upleft.X2 = Math.Max(x, Math.Max(upleft.X2, upright.X2));
                                        upleft.Y2 = Math.Max(y, Math.Max(upleft.Y2, upright.Y2));
                                    }

                                    // Two regions are connected via this pixel: left and top-right
                                    else if (upleft == null && left != null && upright != null)
                                    {
                                        pixelRegions[x, y] = upright;
                                        if (left != upright) { left.SupersetRegion = upright; }
                                        upright.X = Math.Min(x, Math.Min(left.X, upright.X));
                                        upright.Y = Math.Min(y, Math.Min(left.Y, upright.Y));
                                        upright.X2 = Math.Max(x, Math.Max(left.X2, upright.X2));
                                        upright.Y2 = Math.Max(y, Math.Max(left.Y2, upright.Y2));
                                    }

                                    // Simple expansion of up-left diagonal region
                                    else if (upleft != null)
                                    {
                                        upleft.X = Math.Min(upleft.X, x);
                                        upleft.Y = Math.Min(upleft.Y, y);
                                        upleft.X2 = Math.Max(upleft.X2, x);
                                        upleft.Y2 = Math.Max(upleft.Y2, y);
                                        pixelRegions[x, y] = upleft;
                                    }

                                    // Two regions are connected at a corner that looks like _| and the up region is always favored
                                    // arbitrarily as the new superset. Left region is marked as a subset for performance
                                    else if (up != null && left != null)
                                    {
                                        pixelRegions[x, y] = up;
                                        if (left != up) { left.SupersetRegion = up; }
                                        up.X = Math.Min(x, Math.Min(left.X, up.X));
                                        up.Y = Math.Min(y, Math.Min(left.Y, up.Y));
                                        up.X2 = Math.Max(x, Math.Max(left.X2, up.X2));
                                        up.Y2 = Math.Max(y, Math.Max(left.Y2, up.Y2));
                                    }

                                    // Simple expansion of up region
                                    else if (up != null)
                                    {
                                        up.Y2 = Math.Max(up.Y2, y);
                                        pixelRegions[x, y] = up;
                                    }

                                    // Simple expansion of up-right diagonal region
                                    else if (upright != null)
                                    {
                                        upright.X = Math.Min(upright.X, x);
                                        upright.Y = Math.Min(upright.Y, y);
                                        upright.X2 = Math.Max(upright.X2, x);
                                        upright.Y2 = Math.Max(upright.Y2, y);
                                        pixelRegions[x, y] = upright;
                                    }

                                    // Simple expansion of left region
                                    else if (left != null)
                                    {
                                        left.X2 = Math.Max(left.X2, x);
                                        pixelRegions[x, y] = left;
                                    }

                                    // Create a new region since left/up are null
                                    else
                                    {
                                        pixelRegions[x, y] = new Region(x, y);
                                        regions.Add(pixelRegions[x, y]);
                                    }
                                }

                                // Next pixel
                                pixel += 4;
                            }
                        }

                        // Iterate all regions to make a lookup pointing to the largest superset each unique region is in.
                        // This will simplify and speed up performance at the final per-pixel step.
                        var supersets = new Dictionary<Region, Region>();
                        for (int i = 0; i < regions.Count; i++)
                        {
                            supersets.Add(regions[i], regions[i].GetLargestSuperset());
                        }

                        #region DEBUG: Enable to visualize the bounding boxes
                        /*bmpPng.UnlockBits(bmpData);
                        bmpData = bmpPng.LockBits(
                            new Rectangle(0, 0, bmpPng.Width, bmpPng.Height),
                            ImageLockMode.ReadWrite,
                            bmpPng.PixelFormat);
                        row = (byte*)bmpData.Scan0;
                        for (int i = 0; i < regions.Count; i++)
                        {
                            if (regions[i].SupersetRegion != null) { continue; }
                            for (int y = regions[i].Y; y <= regions[i].Y2; y++)
                            {
                                byte* pixel = row + (y * bmpData.Stride) + (regions[i].X * 4);

                                for (int x = regions[i].X; x <= regions[i].X2; x++)
                                {
                                    *pixel = (byte)Math.Min(*pixel + 64, 255); // Blue
                                    *(pixel + 2) = (byte)Math.Min(*(pixel + 2) + 32, 255); // Red
                                    *(pixel + 3) = (byte)Math.Min(*(pixel + 3) + 64, 255); // Alpha

                                    pixel += 4;
                                }
                            }
                        }

                        string tempExportPath = Path.Combine(outputDir, $"debug.png");
                        try { bmpPng.Save(tempExportPath, ImageFormat.Png); } catch { }
                        bmpPng.UnlockBits(bmpData);
                        return null;*/
                        #endregion

                        // Draw each tile.
                        for (int i = 0; i < regions.Count; i++)
                        {
                            // We only want the final regions.
                            if (regions[i].SupersetRegion != null) { continue; }

                            // Optionally skip small regions up to 2x2 in size:
                            if (skipSmallBounds && (
                                (regions[i].X2 - regions[i].X + 1)
                                * (regions[i].Y2 - regions[i].Y + 1) <= 4))
                            {
                                continue;
                            }

                            tileNum++;

                            using (Bitmap tile = new Bitmap(
                                regions[i].X2 - regions[i].X + 1,
                                regions[i].Y2 - regions[i].Y + 1,
                                PixelFormat.Format32bppArgb))
                            {
                                BitmapData tileData = tile.LockBits(
                                    new Rectangle(0, 0, tile.Width, tile.Height),
                                    ImageLockMode.WriteOnly,
                                    tile.PixelFormat);

                                byte* tileRow = (byte*)tileData.Scan0;

                                // Manually draw each pixel to the tile instead of using DrawImage() since some of the pixels on
                                // the edges within the bounding box may belong to other sprites and we want to fully omit them.
                                // This is why we made a quick superset lookup, and why we have the pixelRegions array.
                                for (int y = regions[i].Y; y <= regions[i].Y2; y++)
                                {
                                    byte* bmpPixel = row + (y * bmpData.Stride) + (regions[i].X * 4);
                                    byte* tilePixel = tileRow + ((y - regions[i].Y) * tileData.Stride);

                                    for (int x = regions[i].X; x <= regions[i].X2; x++)
                                    {
                                        // This pixel belongs to this tile or is unmarked, so copy it over.
                                        // We copy unmarked pixels because background color isn't always 0 alpha.
                                        if (pixelRegions[x, y] == null || supersets[pixelRegions[x, y]] == regions[i])
                                        {
                                            *tilePixel = *bmpPixel;
                                            *(tilePixel + 1) = *(bmpPixel + 1);
                                            *(tilePixel + 2) = *(bmpPixel + 2);
                                            *(tilePixel + 3) = *(bmpPixel + 3);
                                        }

                                        // Next pixel
                                        bmpPixel += 4;
                                        tilePixel += 4;
                                    }
                                }

                                tile.UnlockBits(tileData);

                                // Saves the tile.
                                string tileExportPath = Path.Combine(outputDir, $"{tileNum}.png");
                                try
                                {
                                    tile.Save(tileExportPath, ImageFormat.Png);
                                }
                                catch
                                {
                                    return new Tuple<string, string>(
                                        $"Tile # '{tileNum}' can't be saved at: {tileExportPath}",
                                        "Splitting by regions failed partway through");
                                }
                            }
                        }

                        bmpPng.UnlockBits(bmpData);
                    }
                }

                return null;
            }
            catch
            {
                if (tileNum == 0)
                {
                    return new Tuple<string, string>(
                    $"Failed to start splitting by regions at '{textureUrl}'. No files were written.",
                    "Splitting by regions failed");
                }

                return new Tuple<string, string>(
                    $"Failed to finish splitting by regions at '{textureUrl}'. {tileNum} tiles were exported.",
                    "Splitting by regions failed while running");
            }
        }

        /// <summary>
        /// Loads the given texture and splits it into separate files with the given tile width and height. If the
        /// dimensions are not evenly divisible, outputs the remainder anyway. Returns null on success, or a two-string
        /// tuple representing an error message, then caption.
        /// </summary>
        /// <param name="textureUrl">The texture containing all the tiles.</param>
        /// <param name="outputDir">The directory to output all the new, separated tiles.</param>
        /// <param name="tileWidth">The width of each tile in the original texture.</param>
        /// <param name="tileHeight">The height of each tile in the original texture.</param>
        /// <param name="offset">The number of extra pixels in the X and Y axes between each tile, if any.</param>
        /// <param name="startOffset">The amount of padding on the X and Y axes before the grid, if any.</param>
        /// <param name="wholeTilesOnly">
        /// If true, partial tiles due to texture dimensions not being a multiple of tile size are skipped.
        /// </param>
        /// <param name="skipEmptyTiles">If true, tiles that are only fully transparent pixels are skipped.</param>
        public static Tuple<string, string> SplitByGrid(
            string textureUrl,
            string outputDir,
            int tileWidth,
            int tileHeight,
            Point offset,
            Point startOffset,
            bool wholeTilesOnly = true,
            bool skipEmptyTiles = true)
        {
            if (!File.Exists(textureUrl))
            {
                return new Tuple<string, string>(
                    $"The texture doesn't exist or can't be found",
                    "Splitting texture failed to begin");
            }
            if (tileWidth <= 0
                || tileHeight <= 0
                || offset == null
                || offset.X < 0
                || offset.Y < 0
                || startOffset == null
                || startOffset.X < 0
                || startOffset.Y < 0)
            {
                return new Tuple<string, string>(
                    $"The parameters given for how to split up the texture are not in bounds (e.g. negative width).",
                    "Splitting texture failed to begin");
            }

            int tileNum = 0;

            try
            {
                using (var bitmap = Bitmap.FromFile(textureUrl))
                {
                    if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
                    {
                        return new Tuple<string, string>(
                            $"The texture didn't load correctly (or width/height is zero).",
                            "Splitting texture failed");
                    }

                    Directory.CreateDirectory(outputDir);

                    for (int y = startOffset.Y; y < bitmap.Height; y += tileHeight + offset.Y)
                    {
                        for (int x = startOffset.X; x < bitmap.Width; x += tileWidth + offset.X)
                        {
                            tileNum++;
                            int width = (x + tileWidth > bitmap.Width) ? bitmap.Width - x : tileWidth;
                            int height = (y + tileHeight > bitmap.Height) ? bitmap.Height - y : tileHeight;

                            if ((width != tileWidth || height != tileHeight) && wholeTilesOnly)
                            {
                                continue;
                            }

                            var tileRegion = new Rectangle(x, y, width, height);
                            if (skipEmptyTiles && IsFullyTransparent((Bitmap)bitmap, tileRegion))
                            {
                                continue;
                            }

                            using (Bitmap tile = new Bitmap(width, height))
                            {
                                using (var tileGraphics = Graphics.FromImage(tile))
                                {
                                    tileGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                                    tileGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                                    tileGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                                    tileGraphics.DrawImage(
                                        bitmap,
                                        new Rectangle(0, 0, width, height),
                                        tileRegion,
                                        GraphicsUnit.Pixel);

                                    // Saves the final sprite sheet and updates the image source.
                                    string tileExportPath = Path.Combine(outputDir, $"{tileNum}.png");
                                    try
                                    {
                                        tile.Save(tileExportPath, ImageFormat.Png);
                                    }
                                    catch
                                    {
                                        return new Tuple<string, string>(
                                            $"Tile # '{tileNum}' can't be saved at: {tileExportPath}",
                                            "Splitting texture failed partway through");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                if (tileNum == 0)
                {
                    return new Tuple<string, string>(
                    $"Failed to start splitting the texture at '{textureUrl}'. No files were written.",
                    "Splitting texture failed");
                }

                return new Tuple<string, string>(
                    $"Failed to finish splitting the texture at '{textureUrl}'. {tileNum} tiles were exported.",
                    "Splitting texture failed while running");
            }

            return null;
        }

        /// <summary>
        /// Separates the rectangle defined by width and height into squares of the given size.
        /// The square size is determined by the original bounds given. The remainder is added
        /// afterwards as 2 separate rectangles. This is used for parallel rendering.
        /// </summary>
        private static Rectangle[] GetRois(int width, int height)
        {
            List<Rectangle> rois = new List<Rectangle>();
            int squareSize;

            if (width >= 384 && height >= 384) { squareSize = 128; } // 9+ chunks
            else if (width >= 128 && height >= 128) { squareSize = 64; } // 9-36 chunks
            else if (width >= 48 && height >= 48) { squareSize = 32; } // 3-16 chunks
            else
            {
                // not worth parallelizing regions < 48x48, so return whole rect.
                return new Rectangle[] { new Rectangle(0, 0, width, height) };
            }

            int chunksX = width / squareSize;
            int chunksY = height / squareSize;
            int chunkXRem = width % squareSize;
            int chunkYRem = height % squareSize;

            for (int y = 0; y < chunksY; y++)
            {
                for (int x = 0; x < chunksX; x++)
                {
                    rois.Add(new Rectangle(x * squareSize, y * squareSize, squareSize, squareSize));
                }
            }

            if (chunkYRem > 0) { rois.Add(new Rectangle(0, chunksY * squareSize, width, chunkYRem)); }
            if (chunkXRem > 0) { rois.Add(new Rectangle(chunksX * squareSize, 0, chunkXRem, height - chunkYRem)); }

            return rois.ToArray();
        }

        /// <summary>
        /// Returns true if the tile is fully transparent, false otherwise.
        /// </summary>
        private static unsafe bool IsFullyTransparent(Bitmap tile, Rectangle srcRect)
        {
            // Rectangles of zero dimension are fully transparent.
            if (srcRect == null || srcRect.Width < 0 || srcRect.Height < 0)
            {
                return true;
            }

            // Snaps out-of-bounds values.
            if (srcRect.X < 0) { srcRect.X = 0; }
            if (srcRect.Y < 0) { srcRect.Y = 0; }
            if (srcRect.X + srcRect.Width > tile.Width) { srcRect.Width = tile.Width - srcRect.X; }
            if (srcRect.Y + srcRect.Height > tile.Height) { srcRect.Height = tile.Height - srcRect.Y; }

            BitmapData bmpData = tile.LockBits(
                new Rectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height),
                ImageLockMode.ReadOnly,
                tile.PixelFormat);

            byte* row = (byte*)bmpData.Scan0;

            // Divide the region to check into rects for good parallelization.
            Rectangle[] rois = GetRois(srcRect.Width, srcRect.Height);

            bool returnValue = true;

            Parallel.For(0, rois.Length, (i, loopState) =>
            {
                Rectangle roi = rois[i];
                for (int y = roi.Y; y < roi.Y + roi.Height; y++)
                {
                    byte* alpha = row + (y * bmpData.Stride) + (roi.X * 4) + 3;

                    for (int x = roi.X; x < roi.X + roi.Width; x++)
                    {
                        if (*alpha != 0) { returnValue = false; }
                        if (!returnValue) { break; } // Separate from above if-statement so all threads can break

                        alpha++;
                    }

                    if (!returnValue) { break; }
                }
            });

            tile.UnlockBits(bmpData);
            return returnValue;
        }
    }
}