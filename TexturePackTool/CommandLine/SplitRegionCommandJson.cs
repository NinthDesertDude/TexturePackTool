using Newtonsoft.Json;

namespace TexturePackTool.CommandLine
{
    /// <summary>
    /// The expected serialization format of the split by region command.
    /// </summary>
    public class SplitRegionCommandJson
    {
        /// <summary>
        /// An absolute URL to an existing texture file that contains all the tiles to be exported. Multiple textures
        /// can be specified, delimited by a semicolon.
        /// </summary>
        [JsonProperty("TextureUrl")]
        [JsonRequired]
        public string TextureUrl { get; set; }

        /// <summary>
        /// An absolute directory path which all resulting tiles will be placed in.
        /// </summary>
        [JsonProperty("OutputDir")]
        [JsonRequired]
        public string OutputDir { get; set; }

        /// <summary>
        /// The color in the image to treat as the background.
        /// If empty, the transparent color will be *any* color with 0 alpha. If specified, it must be a 6 or 8
        /// character hexadecimal string.
        /// </summary>
        [JsonProperty("BackgroundColor")]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// If true, diagonally-touching pixels are considered part of the same image. True by default.
        /// </summary>
        [JsonProperty("ConnectDiagonalPixels")]
        public bool ConnectDiagonalPixels { get; set; }

        /// <summary>
        /// Skips really small islands (area of 4 or less). These are usually tiny, unintended artifacts.
        /// </summary>
        [JsonProperty("SkipSmallBounds")]
        public bool SkipSmallBounds { get; set; }
    }
}