using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using TexturePackTool.TexturePacking;

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
        [JsonProperty("name")]
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
        [JsonProperty("path")]
        public string RelativePath { get; private set; }

        /// <summary>
        /// The x-position of the texture when placed in the sprite sheet.
        /// </summary>
        [JsonProperty("x")]
        public int X { get; set; }

        /// <summary>
        /// The y-position of the texture when placed in the sprite sheet.
        /// </summary>
        [JsonProperty("y")]
        public int Y { get; set; }

        /// <summary>
        /// The width of the texture loaded from the path.
        /// </summary>
        [JsonProperty("w")]
        public int W { get; set; }

        /// <summary>
        /// The height of the texture loaded from the path.
        /// </summary>
        [JsonProperty("h")]
        public int H { get; set; }
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
            X = 0;
            Y = 0;
            W = 0;
            H = 0;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        [JsonConstructor]
        public Frame(string Name, string RelativePath, int x, int y, int w, int h)
        {
            this.Name = Name;
            this.RelativePath = RelativePath;
            X = x;
            Y = y;
            W = w;
            H = h;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to fetch the image associated with the frame, returning null on failure or
        /// exceptions. Updates dimensions on success.
        /// </summary>
        /// <param name="rootPath">
        /// The absolute path of the saved project file, which the relative path is appended to.
        /// </param>
        /// <returns>
        /// A <see cref="BitmapImage"/> containing the loaded texture, or null on failure.
        /// </returns>
        public BitmapImage LoadImage(string rootPath)
        {
            try
            {
                string path = GetAbsolutePath(rootPath);
                if (File.Exists(path))
                {
                    var image = new BitmapImage(new Uri($"file://{path}"));
                    W = image.PixelWidth;
                    H = image.PixelHeight;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

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
            return Path.Combine(Path.GetDirectoryName(root), RelativePath);
        }
        #endregion
    }
}
