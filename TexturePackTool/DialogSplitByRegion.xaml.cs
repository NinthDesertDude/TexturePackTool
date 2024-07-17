using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using TexturePackTool.CommandLine;
using TexturePackTool.Utilities;

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for DialogSplitByRegion.xaml
    /// </summary>
    public partial class DialogSplitByRegion : Window
    {
        /// <summary>
        /// Called when the user clicks Apply, returning a bool indicating success or failure.
        /// </summary>
        public Func<bool> OnApply;

        /// <summary>
        /// Called when the user closes the dialog.
        /// </summary>
        public Action OnClose;

        /// <summary>
        /// Initializes with automatic defaults.
        /// </summary>
        public DialogSplitByRegion()
        {
            InitializeComponent();

            TextureUrlTxtbx.TextChanged += (a, b) => RefreshControls();
            BrowseTextureUrlBttn.Click += (a, b) => BrowseForFile();
            OutputDirTxtbx.TextChanged += (a, b) => RefreshControls();
            BrowseOutputDirBttn.Click += (a, b) => BrowseForFolder();
            BgColorTxtbx.TextChanged += (a, b) => RefreshControls();
            ApplyBttn.Click += (a, b) => ApplyClicked();
            CloseBttn.Click += (a, b) => CloseClicked();
            ConnectDiagonalPixelsChkbx.Checked += (a, b) => RefreshControls();
            SkipSmallBoundsChkbx.Checked += (a, b) => RefreshControls();
        }

        /// <summary>
        /// Initializes with defaults pre-populated in the GUI.
        /// </summary>
        public DialogSplitByRegion(SplitRegionCommandJson values) : this()
        {
            TextureUrlTxtbx.Text = values.TextureUrl;
            OutputDirTxtbx.Text = values.OutputDir;
            BgColorTxtbx.Text = values.BackgroundColor;
            ConnectDiagonalPixelsChkbx.IsChecked = values.ConnectDiagonalPixels;
            SkipSmallBoundsChkbx.IsChecked = values.SkipSmallBounds;
        }

        /// <summary>
        /// Allows the user to select a file or files and sets the texture url textbox's text to it.
        /// </summary>
        private void BrowseForFile()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Images|*.*",
                    Multiselect = true,
                    Title = "Load an image or images"
                };

                if (dlg.ShowDialog() == true)
                {
                    string allFiles = "";
                    for (int i = 0; i < dlg.FileNames.Length; i++)
                    {
                        allFiles += dlg.FileNames[i].Replace(@"\", @"\\");
                        if (i + 1 != dlg.FileNames.Length) { allFiles += ";"; }
                    }
                    this.TextureUrlTxtbx.Text = allFiles;
                }
            }
            catch
            {
                // Swallow unnecessary errors, the user can see if browsing worked or not.
            }
        }

        /// <summary>
        /// Allows the user to select a folder and sets it to the output directory textbox's text to it.
        /// </summary>
        private void BrowseForFolder()
        {
            try
            {
                using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dlg.ShowNewFolderButton = true;
                    System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        this.OutputDirTxtbx.Text = dlg.SelectedPath;
                    }
                }
            }
            catch
            {
                // Swallow unnecessary errors, the user can see if browsing worked or not.
            }
        }

        /// <summary>
        /// Switches which controls are enabled or not based on the state of data.
        /// </summary>
        private void RefreshControls()
        {
            bool isTextureUrlValid = true;
            var files = CommandLine.CommandLine.SplitEscape(TextureUrlTxtbx.Text, new char[] { ';' });
            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    isTextureUrlValid = false;
                    break;
                }
            }
            this.InvalidTextureUrlTxt.Visibility =
                !string.IsNullOrWhiteSpace(this.TextureUrlTxtbx.Text) && !isTextureUrlValid
                ? Visibility.Visible : Visibility.Collapsed;

            this.InvalidOutputDirTxt.Visibility =
                !string.IsNullOrWhiteSpace(this.OutputDirTxtbx.Text) && !Directory.Exists(this.OutputDirTxtbx.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            this.InvalidBgColorTxt.Visibility =
                this.BgColorTxtbx.Text != "" && !DrawingUtils.IsHexColor(this.BgColorTxtbx.Text)
                ? Visibility.Visible : Visibility.Collapsed;

            this.ApplyBttn.IsEnabled =
                !string.IsNullOrWhiteSpace(this.TextureUrlTxtbx.Text) &&
                !string.IsNullOrWhiteSpace(this.OutputDirTxtbx.Text) &&
                !this.InvalidTextureUrlTxt.IsVisible &&
                !this.InvalidOutputDirTxt.IsVisible &&
                !this.InvalidBgColorTxt.IsVisible;

            if (this.ApplyBttn.IsEnabled)
            {
                SuccessTxt.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Calls the user callback after refreshing to ensure Apply is still enabled (params valid).
        /// </summary>
        private void ApplyClicked()
        {
            RefreshControls();
            if (this.ApplyBttn.IsEnabled)
            {
                SuccessTxt.Visibility = Visibility.Hidden;
                bool? success = this.OnApply?.Invoke();

                if (success == true)
                {
                    SuccessTxt.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Calls the user callback for closing.
        /// </summary>
        private void CloseClicked()
        {
            this.OnClose?.Invoke();
        }
    }
}
