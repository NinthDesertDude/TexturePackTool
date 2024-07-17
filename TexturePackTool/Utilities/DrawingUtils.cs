using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using TexturePackTool.Model;

namespace TexturePackTool.Utilities
{
    /// <summary>
    /// This class provides methods to draw images.
    /// </summary>
    public static class DrawingUtils
    {
        /// <summary>
        /// A hex string like ff00ff or 000000ff
        /// </summary>
        private static readonly Regex hexColor = new Regex("^[0-9a-fA-F]{6}$|^[0-9a-fA-F]{8}$");

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
        /// Returns true if the given string is a 6 or 8 character hex color encoded as RGBA (no # or $ prefix).
        /// </summary>
        public static bool IsHexColor(string input)
        {
            return !string.IsNullOrEmpty(input) && hexColor.IsMatch(input);
        }

        /// <summary>
        /// Returns a new bitmap that resembles the original, but in the given format.
        /// Does not dispose the original.
        /// </summary>
        public static Bitmap FormatImage(Bitmap img, PixelFormat format)
        {
            Bitmap clone = new Bitmap(img.Width, img.Height, format);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                gr.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            return clone;
        }
    }
}
