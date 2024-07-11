using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TexturePackTool.Model;
using TexturePackTool.TexturePacking;

namespace TexturePackTool.Utilities
{
    /// <summary>
    /// This class provides methods to draw images.
    /// </summary>
    public static class DrawingUtils
    {
        /// <summary>
        /// Draws all frames for a given spritesheet with texture packing to the provided url.
        /// Returns null on success, or a two-string tuple representing an error message, then caption.
        /// </summary>
        /// <param name="spriteSheet">The spritesheet to draw all the frames of.</param>
        /// <param name="projectSaveUrl">The absolute path to export the drawings to.</param>
        /// <param name="options">Instructions used in texture packing.</param>
        public static Tuple<string, string> ExportPacked(string projectSaveUrl, SpriteSheet spriteSheet, ExportOptions options)
        {
            try
            {
                // Updates texture locations in the sprite sheet.
                for (int i = 0; i < spriteSheet.Frames.Count; i++)
                {
                    Frame frame = spriteSheet.Frames[i];

                    using (FileStream imageStream = File.OpenRead(frame.GetAbsolutePath(projectSaveUrl)))
                    {
                        BitmapDecoder decoder = BitmapDecoder.Create(
                            imageStream,
                            BitmapCreateOptions.IgnoreColorProfile,
                            BitmapCacheOption.Default);

                        frame.W = decoder.Frames[0].PixelWidth;
                        frame.H = decoder.Frames[0].PixelHeight;
                    }
                }
            }
            catch
            {
                return new Tuple<string, string>(
                    $"Getting image dimensions for all frames failed for the sprite sheet named '{spriteSheet.Name}'.",
                    "Export failed before draw");
            }

            TexturePacker texPacker = new TexturePacker(options);

            try
            {
                texPacker.Pack(spriteSheet.Frames.ToList());
            }
            catch (NullReferenceException)
            {
                return new Tuple<string, string>(
                    $"Packing the texture failed for the sprite sheet named '{spriteSheet.Name}'.",
                    "Export failed before draw");
            }

            int offset = (options == ExportOptions.HalfPixelOffset || options == ExportOptions.BlackBorders)
                ? 1 : 0;

            if (texPacker.Root != null)
            {
                // Loads each image and draws it into the sprite sheet.
                using (Bitmap texSheet = new Bitmap(texPacker.Root.bounds.Width + offset, texPacker.Root.bounds.Height + offset))
                {
                    try
                    {
                        using (var canvas = Graphics.FromImage(texSheet))
                        {
                            canvas.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                            canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                            canvas.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

                            foreach (var frame in spriteSheet.Frames)
                            {
                                using (var bitmap = Bitmap.FromFile(frame.GetAbsolutePath(projectSaveUrl)))
                                {
                                    // Specify width and height explicitly in case of image resolution differences.
                                    canvas.DrawImage(bitmap,
                                        frame.X + offset, frame.Y + offset,
                                        frame.W - offset, frame.H - offset);
                                }

                                // Draws black rectangles in all the shared space.
                                if (options == ExportOptions.BlackBorders)
                                {
                                    canvas.DrawRectangle(Pens.Black, new Rectangle(frame.X, frame.Y, frame.W, frame.H));
                                }

                                frame.W += offset;
                                frame.H += offset;
                            }
                        }
                    }
                    catch
                    {
                        return new Tuple<string, string>(
                            $"Drawing the sprite sheet named '{spriteSheet.Name}' failed.",
                            "Export failed to draw");
                    }

                    // Saves the final sprite sheet and updates the image source.
                    try
                    {
                        string saveUrlWithExt = $"{spriteSheet.GetAbsolutePath(projectSaveUrl)}.png";
                        Directory.CreateDirectory(Path.GetDirectoryName(saveUrlWithExt));
                        texSheet.Save(saveUrlWithExt, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch
                    {
                        return new Tuple<string, string>(
                            $"The sprite sheet named '{spriteSheet.Name}' can't be saved with its current file path: /{spriteSheet.ExportUrl}.png",
                            "Export failed to save");
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Loads the given texture and splits it into separate files with the given tile width and height. If the
        /// dimensions are not evenly divisible, outputs the remainder anyway.
        /// </summary>
        /// <param name="tileWidth">The width of each tile in the original texture.</param>
        /// <param name="tileHeight">The height of each tile in the original texture.</param>
        /// <param name="offset">The number of extra pixels in the X and Y axes between each tile, if any.</param>
        /// <param name="startOffset">The amount of padding on the X and Y axes before the grid, if any.</param>
        public static Tuple<string, string> SplitTextureByGrid(
            string textureUrl,
            string outputDir,
            int tileWidth,
            int tileHeight,
            Point offset,
            Point startOffset,
            bool wholeTilesOnly = false)
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
                                        new Rectangle(x, y, width, height),
                                        GraphicsUnit.Pixel);

                                    // Saves the final sprite sheet and updates the image source.
                                    string tileExportPath = Path.Combine(outputDir, $"{tileNum}.png");
                                    try
                                    {
                                        tile.Save(tileExportPath, System.Drawing.Imaging.ImageFormat.Png);
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
    }
}
