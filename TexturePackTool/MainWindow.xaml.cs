using System;
using System.Collections.Generic;
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

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Handlers for menu -> File
        private void InvokeCommandNew(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void InvokeCommandOpen(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void InvokeCommandSave(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void InvokeCommandSaveAs(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region Handlers for menu -> Edit
        private void ChkbxPreventInterpolationArtifacts_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveAcrossSpritesheets_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
