using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TexturePackTool.Model;
using TexturePackTool.TexturePacking;

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members
        /// <summary>
        /// A numeric counter to help make generated names unique.
        /// </summary>
        private static int guidCounter;

        /// <summary>
        /// Whether the user has unsaved changes or not. Only modify this using
        /// <see cref="SetWorkUnsavedIndicator(bool)"/>, to update the GUI as well.
        /// </summary>
        private bool isUserWorkUnsaved;

        /// <summary>
        /// The project model tied to the GUI.
        /// </summary>
        private TexturePackProject project;

        /// <summary>
        /// The absolute save path of the project, set when creating a new project and on load.
        /// </summary>
        private string projectSaveUrl;

        /// <summary>
        /// Contains the data of the most recently exported image which is displayed to the user, if available.
        /// </summary>
        private MemoryStream displayedExportedImage;
        #endregion

        #region Constructors
        /// <summary>
        /// Static constructor.
        /// </summary>
        static MainWindow()
        {
            guidCounter = 0;
        }

        /// <summary>
        /// Creates an instance of the main window view.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            isUserWorkUnsaved = false;
            project = new TexturePackProject();
            projectSaveUrl = string.Empty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the project and GUI to a clean state, using the given project (from loading) or
        /// creating a new project if null. Does not reset save location as it's expected to be
        /// set before or after this call.
        /// </summary>
        private void ClearProject(TexturePackProject proj)
        {
            guidCounter = 0;
            project = proj ?? new TexturePackProject();

            project.SpriteSheetsCleared += () =>
            {
                SetWorkUnsavedIndicator(true);
                SpritesheetsList.Items.Clear();
            };
            project.SpriteSheetAdded += (SpriteSheet newSheet) =>
            {
                SetWorkUnsavedIndicator(true);
                AddSpriteSheetTab(newSheet);
            };
            project.SpriteSheetRemoved += (SpriteSheet removedSpriteSheet) =>
            {
                SetWorkUnsavedIndicator(true);
                RemoveSpriteSheetTab(removedSpriteSheet);
            };

            // Unhooking the event is necessary to avoid an internal WPF error while clearing.
            SpritesheetsList.Items.Clear();

            // Load sprite sheets if present.
            project.SpriteSheets.ForEach(sheet =>
            {
                AddSpriteSheetTab(sheet);
            });

            // Clears or fetches to preview the most recent exported version of the spritesheet.
            if (project.SpriteSheets.Count == 0) { UnloadExportedImage(); }
            else { LoadExportedImageIfAble(project.SpriteSheets[project.SpriteSheets.Count - 1]); }

            SetWorkUnsavedIndicator(false);
        }

        /// <summary>
        /// Starts a new project, updating the GUI.
        /// </summary>
        private void StartNewProject()
        {
            // Prompts for save file and takes no action on cancel or error.
            if (SaveAsProject())
            {
                ClearProject(null);

                project.AddSpriteSheet(new SpriteSheet($"Untitled_Spritesheet_{++guidCounter}"));
            }
        }

        /// <summary>
        /// Opens a dialog for the user to load a file, returning true if they pick a valid
        /// file.
        /// </summary>
        /// <returns>
        /// True if the user successfully loads a file, else false.
        /// </returns>
        private bool LoadProject()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Filter = "JSON|*.json";
                dlg.Title = "Load JSON texture pack file";

                if (dlg.ShowDialog() == true)
                {
                    projectSaveUrl = dlg.FileName;
                    var loadedProj = TexturePackProject.Load(File.ReadAllText(projectSaveUrl));
                    ClearProject(loadedProj);

                    return true;
                }
            }
            catch
            {
                MessageBox.Show(
                    "The file is corrupt, read-protected or could not be loaded.",
                    "Load failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return false;
        }

        /// <summary>
        /// Attempts to save the project if the save location is valid, returning false if the
        /// save operation fails. If not set or invalid, calls <see cref="SaveAsProject"/>.
        /// </summary>
        /// <returns>
        /// True if the file saves correctly, false in all other cases.
        /// </returns>
        private bool SaveProject()
        {
            if (projectSaveUrl == string.Empty || !File.Exists(projectSaveUrl))
            {
                return SaveAsProject();
            }

            try
            {
                File.WriteAllText(projectSaveUrl, project.Save());
                SetWorkUnsavedIndicator(false);

                return true;
            }
            catch
            {
                MessageBox.Show(
                    "Something went wrong and the project could not be saved.",
                    "Save failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        /// <summary>
        /// Opens a dialog for the user to save the file, returning true if they pick a valid
        /// location. Returns false if there is an exception, the dialog is canceled or the user
        /// picks an invalid location.
        /// </summary>
        /// <param name="locatePathOnly">
        /// Does not save the file, just locates the path to save the file instead if true.
        /// </param>
        /// <returns>
        /// True if the project was saved, otherwise false.
        /// </returns>
        private bool SaveAsProject()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save new project";
            dlg.AddExtension = true;
            dlg.Filter = "JSON|*.json";
            dlg.CheckPathExists = true;

            try
            {
                if (dlg.ShowDialog() == true)
                {
                    projectSaveUrl = dlg.FileName;
                    File.WriteAllText(projectSaveUrl, project.Save());
                    SetWorkUnsavedIndicator(false);

                    return true;
                }
            }
            catch
            {
                MessageBox.Show(
                    "Something went wrong and the project could not be saved.",
                    "Save failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return false;
        }

        /// <summary>
        /// Displays the window title with an asterisk if the user has unsaved changes, otherwise
        /// removes the asterisk. Updates whether work is unsaved or not.
        /// </summary>
        /// <param name="isUnsaved">
        /// True if the user made unsaved changes, else false.
        /// </param>
        private void SetWorkUnsavedIndicator(bool isUnsaved)
        {
            isUserWorkUnsaved = isUnsaved;

            if (isUnsaved && !Title.EndsWith("*"))
            {
                Title += "*";
            }
            else if (!isUnsaved && Title.EndsWith("*"))
            {
                Title = Title.Substring(0, Title.Length - 1);
            }

            this.SaveButton.IsEnabled = isUnsaved;
            if (!this.SaveAsButton.IsEnabled) { this.SaveAsButton.IsEnabled = true; }
        }

        /// <summary>
        /// Adds a new sprite sheet to the GUI.
        /// </summary>
        private void AddSpriteSheetTab(SpriteSheet newSpriteSheet)
        {
            // Creates and inserts the new sprite sheet tab.
            TabItem newSpriteTab = new TabItem();
            newSpriteTab.Header = newSpriteSheet.Name;
            newSpriteTab.Tag = newSpriteSheet;
            SpritesheetsList.Items.Add(newSpriteTab);

            // Displays the most recently-exported image of the spritesheet associated with the tab, if any.
            newSpriteTab.PreviewMouseLeftButtonDown += (a, b) =>
            {
                LoadExportedImageIfAble(newSpriteSheet);
            };
            newSpriteTab.KeyDown += (a, b) =>
            {
                if (b.Key == Key.Enter)
                {
                    LoadExportedImageIfAble(newSpriteSheet);
                }
            };

            newSpriteSheet.NameUpdated += (string name) =>
            {
                SetWorkUnsavedIndicator(true);
                newSpriteTab.Header = name;
            };

            // Populates the page details for the new tab.
            AddSpriteSheetTabControls(newSpriteSheet, newSpriteTab);

            // Moves the tab for adding a new sprite sheet to the end, if it exists.
            for (int i = 0; i < SpritesheetsList.Items.Count; i++)
            {
                TabItem tab = (TabItem)SpritesheetsList.Items.GetItemAt(i);

                if (tab.Tag == null)
                {
                    SpritesheetsList.Items.Remove(tab);
                    SpritesheetsList.Items.Add(tab);
                    newSpriteTab.IsSelected = true;

                    return;
                }
            }

            // Creates and appends the tab if it doesn't exist.
            Action createNewSpriteSheet = new Action(() =>
            {
                SetWorkUnsavedIndicator(true);
                SpriteSheet addedSpriteSheet = new SpriteSheet($"Untitled_Spritesheet_{++guidCounter}");
                project.AddSpriteSheet(addedSpriteSheet);
                Dispatcher.BeginInvoke((Action)(() => SpritesheetsList.SelectedIndex = SpritesheetsList.Items.Count - 2));
            });

            TabItem newTab = new TabItem();
            newTab.Header = "+";

            newTab.PreviewMouseLeftButtonDown += (a, b) =>
            {
                createNewSpriteSheet();
            };
            newTab.KeyDown += (a, b) =>
            {
                if (b.Key == Key.Enter)
                {
                    createNewSpriteSheet();
                }
            };

            SpritesheetsList.Items.Add(newTab);
        }

        /// <summary>
        /// Displays the most recently-exported image of the given spritesheet for a tab, if any.
        /// </summary>
        private void LoadExportedImageIfAble(SpriteSheet spritesheet)
        {
            string imgPath = $"{spritesheet.GetAbsolutePath(projectSaveUrl)}.png";

            if (File.Exists(imgPath))
            {
                // Loads the bitmap from file
                Bitmap bmp = new Bitmap(imgPath);

                // Release the last bitmap in our cache, if any
                displayedExportedImage?.Close();
                displayedExportedImage = new MemoryStream();

                // Save the newly-loaded bitmap to our cache
                bmp.Save(displayedExportedImage, System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();

                displayedExportedImage.Seek(0, SeekOrigin.Begin);

                // Use our cache as the bmp source (advantage is this avoids file locks)
                BitmapImage bmpSource = new BitmapImage();
                bmpSource.BeginInit();
                bmpSource.CacheOption = BitmapCacheOption.OnLoad;
                bmpSource.StreamSource = displayedExportedImage;
                bmpSource.EndInit();
                bmpSource.Freeze();
                SpritesheetImage.Source = bmpSource;
                SpritesheetImage.MaxWidth = bmpSource.Width;
                SpritesheetImage.MaxHeight = bmpSource.Height;
            }
        }

        /// <summary>
        /// Unloads/stops displaying any exported image in memory, as applicable.
        /// </summary>
        private void UnloadExportedImage()
        {
            displayedExportedImage?.Close();
            SpritesheetImage.Source = null;
        }

        /// <summary>
        /// Sets up the content on the given tab, taking both the tab and associated sprite sheet.
        /// </summary>
        /// <param name="newSpriteSheet">
        /// The sprite sheet associated with the tab.
        /// </param>
        /// <param name="newTab">
        /// The tab that contains the newly-generated controls.
        /// </param>
        private void AddSpriteSheetTabControls(SpriteSheet newSpriteSheet, TabItem newTab)
        {
            SpritesheetControls ctrl = new SpritesheetControls();
            ctrl.SpriteSheetName.Text = newSpriteSheet.Name;
            ctrl.SpriteSheetPath.Text = newSpriteSheet.ExportUrl;
            ctrl.SpriteSheetFrames.ItemsSource = newSpriteSheet.Frames;

            ctrl.AddFromFileBttn.Click += (a, b) =>
            {
                AddFiles(newSpriteSheet);
            };
            ctrl.SpriteSheetName.TextChanged += (a, b) =>
            {
                SetWorkUnsavedIndicator(true);
                newSpriteSheet.SetName(ctrl.SpriteSheetName.Text);
            };
            ctrl.SpriteSheetPath.TextChanged += (a, b) =>
            {
                SetWorkUnsavedIndicator(true);
                newSpriteSheet.ExportUrl = ctrl.SpriteSheetPath.Text;
            };

            newTab.Content = ctrl;
        }

        /// <summary>
        /// Opens a dialog for the user to add images as frames.
        /// </summary>
        /// <param name="spriteSheet">
        /// The sprite sheet to add files to if the user successfully adds images.
        /// </param>
        private void AddFiles(SpriteSheet spriteSheet)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                CheckPathExists = true,
                DefaultExt = ".png",
                Filter = "png files|*.png",
                Multiselect = true
            };

            try
            {
                if (dlg.ShowDialog() == true)
                {
                    foreach (string fname in dlg.FileNames)
                    {
                        string file = System.IO.Path.GetFileNameWithoutExtension(fname);
                        Model.Frame newFrame = new Model.Frame($"{file}");
                        newFrame.SetRelativePath(new Uri(fname), new Uri(projectSaveUrl));
                        spriteSheet.Frames.Add(newFrame);
                    }

                    SetWorkUnsavedIndicator(true);
                }
            }
            catch
            {
                MessageBox.Show(
                    "One or more of the indicated files could not be loaded.",
                    "Add files failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Removes the tab linked to the given sprite sheet.
        /// </summary>
        private void RemoveSpriteSheetTab(SpriteSheet removedSpriteSheet)
        {
            TabItem tab = GetSpriteSheetTab(removedSpriteSheet);
            SpritesheetsList.Items.Remove(tab);
        }

        /// <summary>
        /// Returns the tab associated to the given sprite sheet.
        /// </summary>
        /// <param name="associatedSheet">
        /// A sprite sheet that is associated to the tab to be returned.
        /// </param>
        private TabItem GetSpriteSheetTab(SpriteSheet associatedSheet)
        {
            for (int i = 0; i < SpritesheetsList.Items.Count; i++)
            {
                TabItem tab = (TabItem)SpritesheetsList.Items.GetItemAt(i);
                if (tab.Tag == associatedSheet)
                {
                    return tab;
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to read the width and height metadata of all frames in the spritesheet, and
        /// updates the associated frames' width and height dimensions.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="IOException"/>
        private void RefreshImageDimensions(SpriteSheet spriteSheet)
        {
            for (int i = 0; i < spriteSheet.Frames.Count; i++)
            {
                Model.Frame frame = spriteSheet.Frames[i];

                using (FileStream imageStream = File.OpenRead(frame.GetAbsolutePath(projectSaveUrl)))
                {
                    BitmapDecoder decoder = BitmapDecoder.Create(
                        imageStream,
                        BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.Default);

                    frame.W = decoder.Frames[0].PixelWidth;
                    frame.H = decoder.Frames[0].PixelHeight;
                }
            }
        }

        /// <summary>
        /// Draws all sprite sheets.
        /// </summary>
        private void DrawSpriteSheet(SpriteSheet spriteSheet, bool addOnePixelBorder)
        {
            // Updates texture locations in the sprite sheet.
            try
            {
                RefreshImageDimensions(spriteSheet);
            }
            catch
            {
                MessageBox.Show(
                    $"Getting image dimensions for all frames failed for the sprite sheet named '{spriteSheet.Name}'.",
                    "Export failed before draw",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            TexturePacker texPacker = new TexturePacker(addOnePixelBorder);

            try
            {
                texPacker.Pack(spriteSheet.Frames.ToList());
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(
                    $"Packing the texture failed for the sprite sheet named '{spriteSheet.Name}'.",
                    "Export failed before draw",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            int offset = addOnePixelBorder ? 1 : 0;

            if (texPacker.Root != null)
            {
                // Loads each image and draws it into the sprite sheet.
                using (Bitmap texSheet = new Bitmap(texPacker.Root.bounds.Width + offset, texPacker.Root.bounds.Height + offset))
                {
                    try
                    {
                        using (var canvas = Graphics.FromImage(texSheet))
                        {
                            canvas.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                            canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                            canvas.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

                            foreach (var frame in spriteSheet.Frames)
                            {
                                using (var bitmap = Bitmap.FromFile(frame.GetAbsolutePath(projectSaveUrl)))
                                {
                                    // Specify width and height explicitly in case of image resolution differences.
                                    canvas.DrawImage(bitmap,
                                        frame.X + offset, frame.Y + offset,
                                        frame.W - offset, frame.H - offset);
                                }

                                frame.W += offset;
                                frame.H += offset;
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show(
                            $"Drawing the sprite sheet named '{spriteSheet.Name}' failed.",
                            "Export failed to draw",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        return;
                    }

                    // Saves the final sprite sheet and updates the image source.
                    try
                    {
                        string savePath = $"{spriteSheet.GetAbsolutePath(projectSaveUrl)}.png";
                        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                        texSheet.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                        LoadExportedImageIfAble(spriteSheet);
                    }
                    catch
                    {
                        MessageBox.Show(
                            $"The sprite sheet named '{spriteSheet.Name}' can't be saved with its current file path: /{spriteSheet.ExportUrl}.png",
                            "Export failed to save",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        #region Command handlers
        /// <summary>
        /// Starts a new project.
        /// </summary>
        private void InvokeCommandNew(object sender, ExecutedRoutedEventArgs e)
        {
            if (!isUserWorkUnsaved)
            {
                StartNewProject();
                return;
            }

            var result = MessageBox.Show(
                "Starting a new project will clear any unsaved changes. Are you sure?",
                "Confirm start new project",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                StartNewProject();
            }
        }

        /// <summary>
        /// Opens an existing project file.
        /// </summary>
        private void InvokeCommandOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (!isUserWorkUnsaved)
            {
                LoadProject();
                return;
            }

            var result = MessageBox.Show(
                "Opening a project will clear any unsaved changes. Are you sure?",
                "Confirm open project",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                LoadProject();
            }
        }

        /// <summary>
        /// Saves the current project file, opening a dialog if needed.
        /// </summary>
        private void InvokeCommandSave(object sender, ExecutedRoutedEventArgs e)
        {
            SaveProject();
        }

        /// <summary>
        /// Opens a dialog to save the current project file.
        /// </summary>
        private void InvokeCommandSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAsProject();
        }

        /// <summary>
        /// Regenerates and saves each spritesheet, using a 1px offset.
        /// </summary>
        private void InvokeMenuExport(object sender, RoutedEventArgs e)
        {
            Export(true);
        }

        /// <summary>
        /// Regenerates and saves each spritesheet.
        /// </summary>
        private void InvokeMenuExportNoOffset(object sender, RoutedEventArgs e)
        {
            Export(false);
        }

        /// <summary>
        /// Regenerates and saves each spritesheet.
        /// </summary>
        private void Export(bool addOnePixelBorder)
        {
            if (project.SpriteSheets.Count == 0)
            {
                MessageBox.Show(
                    "You need to create a spritesheet in order to export anything.",
                    "No spritesheets to export",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (project.SpriteSheets.All((spritesheet) => spritesheet.Frames.Count == 0))
            {
                MessageBox.Show(
                    "You need to add frames to at least one spritesheet to export anything.",
                    "No frames to export",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            foreach (SpriteSheet sheet in project.SpriteSheets)
            {
                DrawSpriteSheet(sheet, addOnePixelBorder);
            }

            // Saves exported image dimensions.
            SaveProject();
        }
        #endregion

        #endregion
    }
}
