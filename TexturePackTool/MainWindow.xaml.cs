using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TexturePackTool.Model;

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int guidCounter = 0;
        private TexturePackProject project = new TexturePackProject();
        private string projSaveLoc = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

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

            project.SpriteSheetsCleared += () => { SpritesheetsList.Items.Clear(); };
            project.SpriteSheetAdded += (SpriteSheet newSheet) =>
            {
                AddSpriteSheetTab(newSheet);
            };
            project.SpriteSheetRemoved += (SpriteSheet removedSpriteSheet) =>
            {
                RemoveSpriteSheetTab(removedSpriteSheet);
            };

            SpritesheetsList.Items.Clear();

            // Load sprite sheets if present.
            project.SpriteSheets.ForEach(sheet =>
            {
                AddSpriteSheetTab(sheet);
            });
        }

        /// <summary>
        /// Starts a new project, updating the GUI.
        /// </summary>
        private void StartNewProject()
        {
            // Prompt for save file and take no action on cancel or error.
            if (SaveAsProject(true))
            {
                ClearProject(null);

                project.AddSpriteSheet(new SpriteSheet($"Untitled Spritesheet {++guidCounter}"));
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
                    string test = File.ReadAllText(dlg.FileName);
                    var loadedProj = TexturePackProject.Load(File.ReadAllText(dlg.FileName));
                    ClearProject(loadedProj);
                    projSaveLoc = dlg.FileName;
                    return true;
                }
            }
            catch
            {
                MessageBox.Show("The file is corrupt, read-protected or could not be loaded.");
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
            if (projSaveLoc == string.Empty || !File.Exists(projSaveLoc))
            {
                return SaveAsProject(false);
            }

            try
            {
                File.WriteAllText(projSaveLoc, project.Save());
                return true;
            }
            catch
            {
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
        private bool SaveAsProject(bool locatePathOnly)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Title = "Save new project";
                dlg.AddExtension = true;
                dlg.Filter = "JSON|*.json";
                dlg.CheckPathExists = true;
                if (dlg.ShowDialog() == true)
                {
                    projSaveLoc = dlg.FileName;
                    File.WriteAllText(projSaveLoc, project.Save());
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
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

            newSpriteSheet.NameUpdated += (string name) =>
            {
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
                SpriteSheet addedSpriteSheet = new SpriteSheet($"Untitled Spritesheet {++guidCounter}");
                project.AddSpriteSheet(addedSpriteSheet);
                Dispatcher.BeginInvoke((Action)(() => SpritesheetsList.SelectedIndex = SpritesheetsList.Items.Count - 2));
            });

            TabItem newTab = new TabItem();
            newTab.Header = "+";
            newTab.PreviewMouseLeftButtonDown += (a, b) => createNewSpriteSheet();
            newTab.KeyDown += (a, b) => { if (b.Key == Key.Enter) { createNewSpriteSheet(); } };
            SpritesheetsList.Items.Add(newTab);
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
            ctrl.SpriteSheetFrames.ItemsSource = newSpriteSheet.Frames;

            ctrl.AddFromFileBttn.Click += (a, b) =>
            {
                AddFiles(newSpriteSheet);
            };
            ctrl.SpriteSheetName.TextChanged += (a, b) =>
            {
                newSpriteSheet.SetName(ctrl.SpriteSheetName.Text);
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

            if (dlg.ShowDialog() == true)
            {
                foreach (string fname in dlg.FileNames)
                {
                    string file = System.IO.Path.GetFileNameWithoutExtension(fname);
                    Model.Frame newFrame = new Model.Frame($"{file}");
                    newFrame.SetRelativePath(new Uri(fname), new Uri(projSaveLoc));
                    spriteSheet.Frames.Add(newFrame);
                }
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

        #region Command handlers
        private void InvokeCommandNew(object sender, ExecutedRoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Starting a new project will clear any unsaved changes. Are you sure?",
                "Confirm start new project",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                StartNewProject();
            }
        }

        private void InvokeCommandOpen(object sender, ExecutedRoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Opening a project will clear any unsaved changes. Are you sure?",
                "Confirm open project",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                LoadProject();
            }
        }

        private void InvokeCommandSave(object sender, ExecutedRoutedEventArgs e)
        {
            SaveProject();
        }

        private void InvokeCommandSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAsProject(false);
        }

        private void InvokeMenuExport(object sender, RoutedEventArgs e)
        {
            // TODO: Handle exporting.
        }
        #endregion
        #endregion
    }
}
