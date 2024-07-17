using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TexturePackTool.Model;
using TexturePackTool.Utilities;

namespace TexturePackTool.CommandLine
{
    /// <summary>
    /// Utilities to parse and execute expected command line arguments. Command-line args are a public API, so modify
    /// carefully to preserve backwards compatibility.
    /// </summary>
    public class CommandLine
    {
        /// <summary>
        /// Interprets command-line instructions from the provided args. Returns null on success, an empty string if an
        /// error occurred but it's suppressed, or an error message.
        /// </summary>
        public static string ProcessCommands(string[] args)
        {
            bool commandLineSilenceErrors = false;
            string commandLineFailure = null;

            string[] nameValuePair = String
                .Join(" ", args.Skip(1))
                ?.Split(new[] { ' ' }, 2);

            if (nameValuePair == null || nameValuePair.Length != 2)
            {
                commandLineFailure = "Error. Make sure to give one command name, then a space, then a value.";
            }
            else
            {
                string nameLowercase = nameValuePair[0].ToLower();

                // Prepend silent- to any command to suppress error dialogs.
                if (nameLowercase.StartsWith("silent-"))
                {
                    nameLowercase = nameLowercase.Substring(7);
                    commandLineSilenceErrors = true;
                }

                // Handles all commands.
                if (nameLowercase.Equals("export", StringComparison.OrdinalIgnoreCase))
                {
                    commandLineFailure = Export(nameValuePair[1]);
                }
                else if (nameLowercase.Equals("split-grid", StringComparison.OrdinalIgnoreCase))
                {
                    commandLineFailure = SplitByGrid(nameValuePair[1]);
                }
                else if (nameLowercase.Equals("split-region", StringComparison.OrdinalIgnoreCase))
                {
                    commandLineFailure = SplitByRegion(nameValuePair[1]);
                }
                else
                {
                    commandLineFailure = "Command not recognized. Possible commands are export, split-grid, split-region.";
                }
            }

            if (commandLineFailure != null && commandLineSilenceErrors)
            {
                commandLineFailure = "";
            }

            return commandLineFailure;
        }

        /// <summary>
        /// Exports a packed texture image using the instructions provided by <paramref name="value"/>, overwriting the
        /// existing file in the export path, if any.
        /// Returns null on success, or a two-string tuple representing error message, then caption.
        /// </summary>
        /// <param name="value">
        /// A string formatted according to the JSON structure of <see cref="ExportCommandJson"/>. See that for
        /// an explanation of arguments.
        /// </param>
        public static string Export(string value)
        {
            var obj = JsonConvert.DeserializeObject<ExportCommandJson>(value);
            if (obj == null)
            {
                return "export command: JSON provided was in an unexpected format. Schema: "
                    + JsonConvert.SerializeObject(
                        new ExportCommandJson()
                        {
                            ProjectUrl = "C:/directory_path/",
                            ExportUrl = "filename_no_extension",
                            ExportOptions = "an empty string, or one of these: BorderBlack BorderTransp",
                            Files = new string[] { "file1.png", "file2.png", "..." }
                        });
            }

            SpriteSheet newSheet = new SpriteSheet("")
            {
                ExportUrl = obj.ExportUrl
            };

            for (int i = 0; i < obj.Files.Length; i++)
            {
                newSheet.Frames.Add(new Frame("", obj.Files[i], 0, 0, 0, 0));
            }

            ExportOptions options = 
                obj.ExportOptions.Equals("BorderBlack", StringComparison.OrdinalIgnoreCase) ? ExportOptions.BlackBorders
                : obj.ExportOptions.Equals("BorderTransp", StringComparison.OrdinalIgnoreCase) ? ExportOptions.HalfPixelOffset
                : ExportOptions.NoOffset;

            return DrawingUtils.ExportPacked(obj.ProjectUrl, newSheet, options)?.Item1;
        }

        /// <summary>
        /// Exports evenly-sized tiles taken from a source image, each to a file using the instructions provided by
        /// <paramref name="value"/>, overwriting the existing file in the export path, if any.
        /// Returns null on success, or a two-string tuple representing error message, then caption.
        /// </summary>
        /// <param name="value">
        /// A string formatted according to the JSON structure of <see cref="SplitGridCommandJson"/>. See that for
        /// an explanation of arguments.
        /// </param>
        public static string SplitByGrid(string value)
        {
            var obj = JsonConvert.DeserializeObject<SplitGridCommandJson>(value);
            if (obj == null)
            {
                return "split by grid command: JSON provided was in an unexpected format. Schema: "
                    + JsonConvert.SerializeObject(
                        new SplitGridCommandJson()
                        {
                            TextureUrl = "C:/path/source.png;C:/path/source2.png;...",
                            OutputDir = "C:/path/some_directory/",
                            TileWidth = 32,
                            TileHeight = 32,
                            OffsetX = 0,
                            OffsetY = 0,
                            StartOffsetX = 0,
                            StartOffsetY = 0,
                            WholeTilesOnly = true,
                            SkipEmptyTiles = true
                        });
            }

            var files = SplitEscape(obj.TextureUrl, new char[] { ';' });
            foreach (string file in files)
            {
                var result = TextureSplitting.SplitByGrid(
                    file,
                    obj.OutputDir,
                    obj.TileWidth,
                    obj.TileHeight,
                    new System.Drawing.Point(obj.OffsetX, obj.OffsetY),
                    new System.Drawing.Point(obj.StartOffsetX, obj.StartOffsetY),
                    obj.WholeTilesOnly,
                    obj.SkipEmptyTiles)?.Item1;

                if (result != null) { return result; }
            }

            return null;
        }

        /// <summary>
        /// Exports from the source image all islands of non-transparent pixels, including islands bordering an image
        /// edge, each to a file. Where bounding boxes of islands intersect, the pixels belonging to other islands are
        /// omitted from the results for a clean output. Uses instructions provided by <paramref name="value"/>,
        /// overwriting the existing file in the export path, if any.
        /// Returns null on success, or a two-string tuple representing error message, then caption.
        /// </summary>
        /// <param name="value">
        /// A string formatted according to the JSON structure of <see cref="SplitGridCommandJson"/>. See that for
        /// an explanation of arguments.
        /// </param>
        public static string SplitByRegion(string value)
        {
            var obj = JsonConvert.DeserializeObject<SplitRegionCommandJson>(value);
            if (obj == null)
            {
                return "split by region command: JSON provided was in an unexpected format. Schema: "
                    + JsonConvert.SerializeObject(
                        new SplitRegionCommandJson()
                        {
                            TextureUrl = "C:/path/source.png;C:/path/source2.png;...",
                            OutputDir = "C:/path/some_directory/",
                            BackgroundColor = "ff0000ff",
                            ConnectDiagonalPixels = true,
                            SkipSmallBounds = true
                        });
            }

            var files = SplitEscape(obj.TextureUrl, new char[] { ';' });
            foreach (string file in files)
            {
                var result = TextureSplitting.SplitByRegion(
                    file,
                    obj.OutputDir,
                    obj.BackgroundColor,
                    obj.SkipSmallBounds,
                    obj.ConnectDiagonalPixels)?.Item1;

                if (result != null) { return result; }
            }

            return null;
        }

        /// <summary>
        /// Utility function to handle escape sequences e.g. semicolons separating urls.
        /// </summary>
        public static List<string> SplitEscape(string input, params char[] delimiters)
        {
            bool escapeActive = false;
            List<string> segments = new List<string>();
            string currentSegment = "";
            for (int i = 0; i < input.Length; i++)
            {
                // Handles double-escapes with \\.
                if (input[i] == '\\')
                {
                    if (escapeActive) { currentSegment += '\\'; }
                    escapeActive = !escapeActive;
                }

                // Escapes on delimiters.
                else if (!escapeActive && delimiters.Contains(input[i]))
                {
                    segments.Add(currentSegment);
                    currentSegment = "";
                }

                // Adds normal characters (and escaped delimiters).
                else
                {
                    currentSegment += input[i];
                    escapeActive = false;
                }
            }

            //Add remaining segment, if any.
            if (currentSegment != "")
            {
                segments.Add(currentSegment);
            }

            return segments;
        }
    }
}