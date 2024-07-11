using Newtonsoft.Json;
using System;
using System.Linq;
using TexturePackTool.Model;
using TexturePackTool.TexturePacking;
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

                // Handles all commands.
                if (nameLowercase.Equals("split-grid", StringComparison.OrdinalIgnoreCase))
                {
                    commandLineFailure = SplitByGrid(nameValuePair[1]);
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
                            WholeTilesOnly = false
                        });
            }

            string[] files = obj.TextureUrl.Split(';');
            foreach (string file in files)
            {
                var result = DrawingUtils.SplitTextureByGrid(
                    file,
                    obj.OutputDir,
                    obj.TileWidth,
                    obj.TileHeight,
                    new System.Drawing.Point(obj.OffsetX, obj.OffsetY),
                    new System.Drawing.Point(obj.StartOffsetX, obj.StartOffsetY),
                    obj.WholeTilesOnly)?.Item1;

                if (result != null) { return result; }
            }

            return null;
        }
    }
}