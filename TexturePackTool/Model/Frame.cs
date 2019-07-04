using System;
using System.ComponentModel;
using System.IO;

namespace TexturePackTool.Model
{
    /// <summary>
    /// Couples the path for a texture with a reliable name for the image that should only change
    /// when the image is updated, thus maximizing reliability in referencing the frame from code
    /// that uses it.
    /// </summary>
    public class Frame : INotifyPropertyChanged
    {
        #region Members
        private string name;

        /// <summary>
        /// The unique name of this frame.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                NameChanged?.Invoke();
            }
        }

        /// <summary>
        /// The path of the frame, which should be at the topmost parent of all
        /// sprites included in any sprite sheet in the project.
        /// </summary>
        public string RelativePath { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Called when the name changes.
        /// </summary>
        public event Action NameChanged;

        /// <summary>
        /// Called when the path changes.
        /// </summary>
        public event Action RelativePathChanged;

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new frame with the unique name given.
        /// </summary>
        /// <param name="uniqueName">
        /// A name unique across frames in all sprite sheets.
        /// </param>
        public Frame(string uniqueName)
        {
            Name = uniqueName;
            RelativePath = string.Empty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets a relative path from the absolute path based on the root.
        /// </summary>
        /// <param name="absPath">
        /// The path indicated.
        /// </param>
        /// <param name="root">
        /// The save location of the file containing this project, or where it will be located.
        /// </param>
        public void SetRelativePath(Uri absPath, Uri root)
        {
            RelativePath = root.MakeRelativeUri(absPath).ToString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelativePath"));
            RelativePathChanged?.Invoke();
        }

        /// <summary>
        /// Returns the absolute path from the relative path based on the root.
        /// </summary>
        /// <param name="relativePath">
        /// The path indicated.
        /// </param>
        /// <param name="root">
        /// The save location of the file containing this project, or where it will be located.
        /// </param>
        /// <returns>
        /// The absolute path from the relative path.
        /// </returns>
        public string GetAbsolutePath(string relativePath, string root)
        {
            if (string.IsNullOrEmpty(RelativePath))
            {
                return string.Empty;
            }

            return Path.Combine(root, relativePath);
        }
        #endregion
    }
}
