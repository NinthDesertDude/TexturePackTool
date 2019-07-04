using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TexturePackTool.Model
{
    /// <summary>
    /// Projects are used to organize the metadata about sprite sheets and frames in them.
    /// </summary>
    public class TexturePackProject
    {
        #region Members
        /// <summary>
        /// Backing field for <see cref="SpriteSheets"/>.
        /// </summary>
        private List<SpriteSheet> spriteSheets;

        /// <summary>
        /// A list of <see cref="SpriteSheet"/>s.
        /// </summary>
        public List<SpriteSheet> SpriteSheets
        {
            get => new List<SpriteSheet>(spriteSheets);
            private set => spriteSheets = value;
        }
        #endregion

        #region Events
        /// <summary>
        /// Called when a sprite sheet is added.
        /// </summary>
        public event Action<SpriteSheet> SpriteSheetAdded;

        /// <summary>
        /// Called when a sprite sheet is removed.
        /// </summary>
        public event Action<SpriteSheet> SpriteSheetRemoved;

        /// <summary>
        /// Called when the sprite sheet list is cleared.
        /// </summary>
        public event Action SpriteSheetsCleared;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new texture pack project.
        /// </summary>
        public TexturePackProject()
        {
            SpriteSheets = new List<SpriteSheet>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a sprite sheet, calling <see cref="SpriteSheetAdded"/> afterwards.
        /// </summary>
        /// <param name="newSpriteSheet">
        /// The sprite sheet to add.
        /// </param>
        public void AddSpriteSheet(SpriteSheet newSpriteSheet)
        {
            spriteSheets.Add(newSpriteSheet);
            SpriteSheetAdded?.Invoke(newSpriteSheet);
        }

        /// <summary>
        /// Removes a sprite sheet, calling <see cref="SpriteSheetRemoved"/> afterwards.
        /// </summary>
        /// <param name="sheetToRemove">
        /// The sprite sheet to remove.
        /// </param>
        public void RemoveSpriteSheet(SpriteSheet sheetToRemove)
        {
            spriteSheets.Remove(sheetToRemove);
            SpriteSheetRemoved?.Invoke(sheetToRemove);
        }

        /// <summary>
        /// Removes all sprite sheets.
        /// </summary>
        public void ClearSpriteSheets()
        {
            spriteSheets.Clear();
            SpriteSheetsCleared?.Invoke();
        }

        /// <summary>
        /// Returns the serialized version of this object as stringified JSON.
        /// </summary>
        public string Save()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Deserializes a stringified JSON object, returning it as
        /// <see cref="TexturePackProject"/>.
        /// </summary>
        /// <param name="data">
        /// The stringified JSON, from e.g. a file.
        /// </param>
        public static TexturePackProject Load(string data)
        {
            return JsonConvert.DeserializeObject<TexturePackProject>(data);
        }
        #endregion
    }
}
