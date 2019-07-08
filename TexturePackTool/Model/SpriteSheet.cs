using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace TexturePackTool.Model
{
    /// <summary>
    /// Represents a collection of sprites to be packed together into a single sprite sheet.
    /// </summary>
    public class SpriteSheet
    {
        #region Members
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
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        [JsonConstructor]
        public SpriteSheet(ObservableCollection<Frame> Frames, string Name)
        {
            this.Frames = Frames;
            this.Name = Name;
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
        #endregion
    }
}
