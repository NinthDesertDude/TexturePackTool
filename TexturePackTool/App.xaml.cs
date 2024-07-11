using System;
using System.Linq;
using System.Windows;

namespace TexturePackTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
 
            // Runs the program as usual if it has no additional command line arguments.
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length == 1)
            {
                var window = new MainWindow();
                window.Show();
            }

            // If the program has any extra command line arguments, it runs in a windowless mode.
            else
            {
                string errorResponse = CommandLine.CommandLine.ProcessCommands(commandLineArgs);

                if (errorResponse != null)
                {
                    if (!string.IsNullOrEmpty(errorResponse))
                    {
                        MessageBox.Show(
                            errorResponse,
                            "Command line failure",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    Environment.Exit(-1);
                }

                Environment.Exit(0);
            }
        }
    }
}
