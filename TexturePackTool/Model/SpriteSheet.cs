using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace TexturePackTool.Model
{
    /// <summary>
    /// Represents a collection of sprites to be packed together into a single sprite sheet.
    /// </summary>
    public class SpriteSheet
    {
        #region Members
        /// <summary>
        /// The relative path to save this file under, which will have .png appended if not
        /// specified.
        /// </summary>
        [JsonProperty("export-url")]
        public string ExportUrl { get; set; }

        /// <summary>
        /// A list of <see cref="Frame"/>s.
        /// </summary>
        [JsonProperty("frames")]
        public ObservableCollection<Frame> Frames { get; private set; }

        /// <summary>
        /// The unique name of this sprite sheet.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Called when the name changes.
        /// </summary>
        public event Action<string> NameUpdated;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a sprite sheet with a unique name.
        /// </summary>
        /// <param name="uniqueName">
        /// A name that is unique among other sprite sheets in this project.
        /// </param>
        public SpriteSheet(string uniqueName)
        {
            Frames = new ObservableCollection<Frame>();
            Name = uniqueName;
            ExportUrl = uniqueName.Replace(' ', '-').ToLower();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        [JsonConstructor]
        public SpriteSheet(string exportUrl, ObservableCollection<Frame> frames, string name)
        {
            ExportUrl = exportUrl;
            Frames = frames;
            Name = name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the unique name of the sprite sheet.
        /// </summary>
        /// <param name="uniqueName">
        /// A name that is unique among sprite sheets in the project.
        /// </param>
        public void SetName(string uniqueName)
        {
            Name = uniqueName;
            NameUpdated?.Invoke(uniqueName);
        }

        /// <summary>
        /// Returns the absolute path from the relative path based on the root.
        /// </summary>
        /// <param name="root">
        /// The save location of the file containing this project, or where it will be located.
        /// </param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <returns>
        /// The absolute path from the relative path.
        /// </returns>
        public string GetAbsolutePath(string root)
        {
            return Path.Combine(Path.GetDirectoryName(root), ExportUrl);
        }
        #endregion
    }
}
