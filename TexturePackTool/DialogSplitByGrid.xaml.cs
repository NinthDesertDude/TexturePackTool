using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for DialogSplitByGrid.xaml
    /// </summary>
    public partial class DialogSplitByGrid : Window
    {
        public Action OnApply;
        public Action OnClose;

        public DialogSplitByGrid()
        {
            InitializeComponent();

            TextureUrlTxtbx.TextChanged += (a, b) => RefreshControls();
            BrowseTextureUrlBttn.Click += (a, b) => BrowseForFile();
            OutputDirTxtbx.TextChanged += (a, b) => RefreshControls();
            BrowseOutputDirBttn.Click += (a, b) => BrowseForFolder();
            TileWidthTxtbx.TextChanged += (a, b) => RefreshControls();
            TileHeightTxtbx.TextChanged += (a, b) => RefreshControls();
            OffsetXTxtbx.TextChanged += (a, b) => RefreshControls();
            OffsetYTxtbx.TextChanged += (a, b) => RefreshControls();
            StartOffsetXTxtbx.TextChanged += (a, b) => RefreshControls();
            StartOffsetYTxtbx.TextChanged += (a, b) => RefreshControls();
            ApplyBttn.Click += (a, b) => ApplyClicked();
            CloseBttn.Click += (a, b) => CloseClicked();
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
                    this.TextureUrlTxtbx.Text = String.Join(";", dlg.FileNames);
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
            string[] files = this.TextureUrlTxtbx.Text.Split(';');
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

            this.InvalidOutputDirTxt.Visibility = (
                !string.IsNullOrWhiteSpace(this.OutputDirTxtbx.Text) && !Directory.Exists(this.OutputDirTxtbx.Text))
                ? Visibility.Visible : Visibility.Collapsed;

            int result;

            this.InvalidTileSizeTxt.Visibility = (
                !Int32.TryParse(this.TileWidthTxtbx.Text, out result) || result <= 0 ||
                !Int32.TryParse(this.TileHeightTxtbx.Text, out result) || result <= 0)
                    ? Visibility.Visible : Visibility.Collapsed;

            this.InvalidOffsetTxt.Visibility = (
                !Int32.TryParse(this.OffsetXTxtbx.Text, out result) || result < 0 ||
                !Int32.TryParse(this.OffsetYTxtbx.Text, out result) || result < 0)
                    ? Visibility.Visible : Visibility.Collapsed;

            this.InvalidStartOffsetTxt.Visibility = (
                !Int32.TryParse(this.StartOffsetXTxtbx.Text, out result) || result < 0 ||
                !Int32.TryParse(this.StartOffsetYTxtbx.Text, out result) || result < 0)
                    ? Visibility.Visible : Visibility.Collapsed;

            this.ApplyBttn.IsEnabled =
                !string.IsNullOrWhiteSpace(this.TextureUrlTxtbx.Text) &&
                !string.IsNullOrWhiteSpace(this.OutputDirTxtbx.Text) &&
                !this.InvalidTextureUrlTxt.IsVisible &&
                !this.InvalidOutputDirTxt.IsVisible &&
                !this.InvalidTileSizeTxt.IsVisible &&
                !this.InvalidOffsetTxt.IsVisible &&
                !this.InvalidStartOffsetTxt.IsVisible;
        }

        /// <summary>
        /// Calls the user callback after refreshing to ensure Apply is still enabled (params valid).
        /// </summary>
        private void ApplyClicked()
        {
            RefreshControls();
            if (this.ApplyBttn.IsEnabled)
            {
                this.OnApply?.Invoke();
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
