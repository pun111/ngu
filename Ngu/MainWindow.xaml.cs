using System.Collections.Generic;
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
using System.Drawing.Design;
using System.Threading;

namespace Ngu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource         farmCTS = new CancellationTokenSource();

        public                          MainWindow()
        {
            InitializeComponent();
        }
        private void                    Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Helpers.MessageBoxNonBlocking("Loaded");
        }
        private void                    Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            //farmCTS?.Cancel();
            Thread.Sleep(100);
            farmCTS?.Dispose();
            Model.Shutdown();
        }

        private async void              Start_PMLBD(object sender, MouseButtonEventArgs e)
        {
            //Helpers.MessageBoxNonBlocking("Farming star requested");
            await Task.Run(() => Model.Snipe(farmCTS.Token), farmCTS.Token);
        }
        private async void              Stop_PMLBD(object sender, MouseButtonEventArgs e)
        {
            farmCTS.Cancel();
            await Task.Delay(1000);
            farmCTS = new CancellationTokenSource();
        }
        private void                    SetDefault_PMLBD(object sender, MouseButtonEventArgs e)
        {
            Model.SetDesiredPixel();
        }
        private async void              Boost_PMLBD(object sender, MouseButtonEventArgs e)
        {
            await Task.Run(() => Model.Boost(farmCTS.Token), farmCTS.Token);
            //Model.SendMouseKey(Model?.BossPixel?.Point);
        }
        private void                    BoostSetupComplete_PMLBD(object sender, MouseButtonEventArgs e)
        {
            Model.SetupComplete = true;
        }
    }
}
