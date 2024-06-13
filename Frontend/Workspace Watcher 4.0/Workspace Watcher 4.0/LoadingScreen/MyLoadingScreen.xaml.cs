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
using System.Windows.Shapes;
using System.Timers;

namespace Workspace_Watcher_4._0.LoadingScreen
{
    /// <summary>
    /// Interaktionslogik für MyLoadingScreen.xaml
    /// </summary>
    public partial class MyLoadingScreen : Window
    {
        MainWindow main;
        Timer timer = new Timer(3000);
        public MyLoadingScreen()
        {
            InitializeComponent();
            main = new MainWindow();
            timer.Elapsed += CloseThis;
            timer.Start();
        }

        private void CloseThis(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                main.Show();
                timer.Dispose();
                timer.Close();

                this.Close();
            });

        }
    }
}
