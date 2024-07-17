using Newtonsoft.Json;

namespace TexturePackTool.CommandLine
{
    /// <summary>
    /// The expected serialization format of the split by grid command.
    /// </summary>
    public class SplitGridCommandJson
    {
        /// <summary>
        /// An absolute URL to an existing texture file that contains all the tiles to be split. Multiple textures can
        /// be specified, delimited by a semicolon.
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
        /// A positive, nonzero integer for the width of each tile.
        /// </summary>
        [JsonProperty("TileWidth")]
        public int TileWidth { get; set; }

        /// <summary>
        /// A positive, nonzero integer for the height of each tile.
        /// </summary>
        [JsonProperty("TileHeight")]
        public int TileHeight { get; set; }

        /// <summary>
        /// The distance in pixels on the X axis between each tile.
        /// </summary>
        [JsonProperty("OffsetX")]
        public int OffsetX { get; set; }

        /// <summary>
        /// The distance in pixels on the Y axis between each tile.
        /// </summary>
        [JsonProperty("OffsetY")]
        public int OffsetY { get; set; }

        /// <summary>
        /// The distance in pixels on the X axis before the grid begins.
        /// </summary>
        [JsonProperty("StartOffsetX")]
        public int StartOffsetX { get; set; }

        /// <summary>
        /// The distance in pixels on the Y axis before the grid begins.
        /// </summary>
        [JsonProperty("StartOffsetY")]
        public int StartOffsetY { get; set; }

        /// <summary>
        /// If the texture's width and height aren't multiples of the tile dimensions, the remainder tiles can be
        /// included or omitted at will.
        /// </summary>
        public bool WholeTilesOnly { get; set; }

        /// <summary>
        /// If a tile that would be exported is perfectly transparent, it's skipped over if this is true.
        /// </summary>
        public bool SkipEmptyTiles { get; set; }
    }
}