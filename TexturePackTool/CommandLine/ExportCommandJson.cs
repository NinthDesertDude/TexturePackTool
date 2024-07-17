using Newtonsoft.Json;

namespace TexturePackTool.CommandLine
{
    /// <summary>
    /// The expected serialization format of the export command.
    /// </summary>
    public class ExportCommandJson
    {
        /// <summary>
        /// An absolute URL directory path used to resolve relative paths in <see cref="Files"/>.
        /// </summary>
        [JsonProperty("ProjectUrl")]
        [JsonRequired]
        public string ProjectUrl { get; set; }

        /// <summary>
        /// A name with no file extension where the output PNG is (over)written.
        /// </summary>
        [JsonProperty("ExportUrl")]
        [JsonRequired]
        public string ExportUrl { get; set; }

        /// <summary>
        /// A list of filenames with file extensions.
        /// </summary>
        [JsonProperty("Files")]
        public string[] Files { get; set; }

        /// <summary>
        /// A string matching a value in the enum <see cref="ExportOptions"/>, used to inform the texture pack tool how
        /// to act.
        /// </summary>
        [JsonProperty("ExportOptions")]
        public string ExportOptions { get; set; }
    }
}