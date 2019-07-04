using System;
using System.Collections.Generic;

namespace TexturePackTool.Model
{
    /// <summary>
    /// Represents a collection of sprites to be packed together into a single sprite sheet.
    /// </summary>
    public class SpriteSheet
    {
        #region Members
        /// <summary>
        /// Backing field for <see cref="Frames"/>.
        /// </summary>
        private List<Frame> frames;

        /// <summary>
        /// A list of <see cref="Frame"/>s.
        /// </summary>
        public List<Frame> Frames
        {
            get => new List<Frame>(frames);
            private set => frames = value;
        }

        /// <summary>
        /// The unique name of this sprite sheet.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
        #endregion

        #region Events
        /// <summary>
        /// Called when any frames are changed.
        /// </summary>
        public event Action<List<Frame>> FramesChanged;

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
            Frames = new List<Frame>();
            Name = uniqueName;
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
        /// Adds a frame, calling <see cref="FramesChanged"/> afterwards.
        /// </summary>
        /// <param name="newFrame">
        /// The frame to add.
        /// </param>
        public void AddFrame(Frame newFrame)
        {
            frames.Add(newFrame);
            FramesChanged?.Invoke(Frames);
        }

        /// <summary>
        /// Removes a frame, calling <see cref="FramesChanged"/> afterwards.
        /// </summary>
        /// <param name="newFrame">
        /// The frame to remove.
        /// </param>
        public void RemoveFrame(Frame newFrame)
        {
            frames.Remove(newFrame);
            FramesChanged?.Invoke(Frames);
        }

        /// <summary>
        /// Overwrites the list of existing frames with a different list, calling
        /// <see cref="FramesChanged"/> afterwards.
        /// </summary>
        /// <param name="frames">
        /// A list of frames.
        /// </param>
        public void SetFrames(List<Frame> frames)
        {
            this.frames = frames;
            FramesChanged?.Invoke(frames);
        }

        /// <summary>
        /// Removes all frames.
        /// </summary>
        public void ClearFrames()
        {
            frames.Clear();
            FramesChanged?.Invoke(Frames);
        }
        #endregion
    }
}
