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
using TexturePackTool.Utilities;

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members
        private static readonly string applicationName = "Texture Pack Tool";
        private static readonly string newSpritesheetPrefix = "Untitled_Spritesheet_";

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
        private MemoryStream displayedImage;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an instance of the main window view.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            isUserWorkUnsaved = false;
            project = new TexturePackProject();
            projectSaveUrl = string.Empty;
            Title = applicationName;
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

                project.AddSpriteSheet(new SpriteSheet($"{newSpritesheetPrefix}1"));
                SaveProject();
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
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                }

                OpenFileDialog dlg = new OpenFileDialog();
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Filter = "JSON|*.json";
                dlg.Title = "Load JSON texture pack file";

                if (dlg.ShowDialog() == true)
                {
                    projectSaveUrl = dlg.FileName;
                    Title = $"{applicationName} - {dlg.SafeFileName}";
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
                    Title = $"{applicationName} - {dlg.SafeFileName}";
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
            this.SaveAsButton.IsEnabled = true;
            this.ExportButton.IsEnabled = true;
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

                int counter = 1;
                while (project.SpriteSheets.Any(o => o.Name == $"{newSpritesheetPrefix}{counter}")) { counter++; }

                SpriteSheet addedSpriteSheet = new SpriteSheet($"{newSpritesheetPrefix}{counter}");
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
            LoadImageIfAble(imgPath);
        }

        /// <summary>
        /// Displays the given frame's associated image if able.
        /// </summary>
        private void LoadPreviewImageIfAble(Model.Frame frame)
        {
            string imgPath = $"{frame?.GetAbsolutePath(projectSaveUrl)}";
            LoadImageIfAble(imgPath);
        }

        /// <summary>
        /// Displays the given image in the image preview if able.
        /// </summary>
        private void LoadImageIfAble(string imgPath)
        {
            if (imgPath != null && File.Exists(imgPath))
            {
                // Loads the bitmap from file
                Bitmap bmp = new Bitmap(imgPath);

                // Release the last bitmap in our cache, if any
                displayedImage?.Close();
                displayedImage = new MemoryStream();

                // Save the newly-loaded bitmap to our cache
                bmp.Save(displayedImage, System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();

                displayedImage.Seek(0, SeekOrigin.Begin);

                // Use our cache as the bmp source (advantage is this avoids file locks)
                BitmapImage bmpSource = new BitmapImage();
                bmpSource.BeginInit();
                bmpSource.CacheOption = BitmapCacheOption.OnLoad;
                bmpSource.StreamSource = displayedImage;
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
            displayedImage?.Close();
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

            ctrl.RemoveSpritesheetBttn.Click += (a, b) =>
            {
                var result = MessageBox.Show(
                "This will delete this spritesheet. Are you sure?",
                "Confirm delete spritesheet",
                MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    RemoveSpriteSheetTab(newSpriteSheet);
                    project.RemoveSpriteSheet(newSpriteSheet);
                }
            };
            ctrl.AddFromFileBttn.Click += (a, b) =>
            {
                AddFiles(newSpriteSheet);
            };
            ctrl.SpriteSheetFrames.SelectionChanged += (a, b) =>
            {
                var activeFrame = (Model.Frame)ctrl.SpriteSheetFrames.SelectedItem;
                LoadPreviewImageIfAble(activeFrame);
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
        /// Draws all sprite sheets.
        /// </summary>
        private void DrawSpriteSheet(SpriteSheet spriteSheet, ExportOptions options)
        {
            var possibleErrorAndCaption = DrawingUtils.ExportPacked(projectSaveUrl, spriteSheet, options);
            if (possibleErrorAndCaption != null)
            {
                MessageBox.Show(
                    possibleErrorAndCaption.Item1,
                    possibleErrorAndCaption.Item2,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                // Refresh the preview on success
                if (GetSpriteSheetTab(spriteSheet)?.IsSelected ?? false)
                {
                    LoadExportedImageIfAble(spriteSheet);
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
            if (project.SpriteSheets.Count == 0)
            {
                MessageBox.Show(
                    "You need to create a spritesheet in order to save anything.",
                    "No data to save",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                SaveAsProject();
            }
        }

        /// <summary>
        /// Regenerates and saves each spritesheet, using a 1px offset.
        /// </summary>
        private void InvokeMenuExport(object sender, RoutedEventArgs e)
        {
            Export(ExportOptions.HalfPixelOffset);
        }

        /// <summary>
        /// Regenerates and saves each spritesheet.
        /// </summary>
        private void InvokeMenuExportNoOffset(object sender, RoutedEventArgs e)
        {
            Export(ExportOptions.NoOffset);
        }

        /// <summary>
        /// Regenerates and saves each spritesheet, using a 1px offset filled in black.
        /// </summary>
        private void InvokeMenuExportBordered(object sender, RoutedEventArgs e)
        {
            Export(ExportOptions.BlackBorders);
        }

        /// <summary>
        /// Regenerates and saves each spritesheet.
        /// </summary>
        private void Export(ExportOptions exportOptions)
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
                DrawSpriteSheet(sheet, exportOptions);
            }

            // Saves exported image dimensions.
            SaveProject();
        }

        private void InvokeMenuUtilitySplitGrid(object sender, RoutedEventArgs e)
        {
            DialogSplitByGrid dlg = new DialogSplitByGrid();
            dlg.OnClose = () => dlg.Close();

            dlg.OnApply = () =>
            {
                try
                {
                    Tuple<string, string> possibleErrorMessage = null;
                    string[] files = dlg.TextureUrlTxtbx.Text.Split(';');

                    foreach (string file in files)
                    {
                        possibleErrorMessage = DrawingUtils.SplitTextureByGrid(
                        file,
                        Path.Combine(dlg.OutputDirTxtbx.Text, Path.GetFileNameWithoutExtension(file)),
                        int.Parse(dlg.TileWidthTxtbx.Text),
                        int.Parse(dlg.TileHeightTxtbx.Text),
                        new System.Drawing.Point(int.Parse(dlg.OffsetXTxtbx.Text), int.Parse(dlg.OffsetYTxtbx.Text)),
                        new System.Drawing.Point(int.Parse(dlg.StartOffsetXTxtbx.Text), int.Parse(dlg.StartOffsetYTxtbx.Text)),
                        dlg.WholeTilesOnlyChkbx.IsChecked ?? false);

                        if (possibleErrorMessage != null)
                        {
                            MessageBox.Show(
                                possibleErrorMessage.Item1,
                                possibleErrorMessage.Item2,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }

                    if (possibleErrorMessage == null)
                    {
                        dlg.Close();
                    }
                }
                catch
                {
                    MessageBox.Show(
                        "Couldn't split the spritesheets; try checking that all files/folders exist.",
                        "Failed to split spritesheet(s)",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

            dlg.ShowDialog();
        }
        #endregion

        #endregion
    }
}
