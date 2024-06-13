using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Workspace_Watcher_4._0.Classes;
using Workspace_Watcher_4._0.Logic;
using Workspace_Watcher_4._0.MVVM.View;
using System.Windows.Threading;

namespace Workspace_Watcher_4._0
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HomeView.GetMainWindow(this);
            LogInView.GetMainWindow(this);
            RegistrationView.GetMainWindow(this);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            HTTPLogOutRequestAsync();

            ProfileButton.Opacity = 0;
            StatisticsButton.Opacity = 0;
            HomeButton.Opacity = 0;
            LogOutButton.Opacity = 0;
            faceDetectedText.Opacity = 0;
            RecordIndicator.Opacity = 0;

            HomeButton.IsEnabled = false;
            LogOutButton.IsEnabled = false;
            StatisticsButton.IsEnabled = false;
            ProfileButton.IsEnabled = false;
            LeftBottomBorder.Background = Brushes.AliceBlue;
            RightBottomBorder.Background = Brushes.White;


            App.timer.Stop();

            DetectedProcess.HTTPUpdateProcesses(this, null);
            App.observer.Processes.Clear();
            App.observer.PieProcesses?.Clear();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            //change the WindowStyle to single border just before minimising it
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Minimized;
        }

        private async Task HTTPLogOutRequestAsync()
        {
            try
            {
                HttpClient client = new HttpClient();

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + TokenStorage.Tokens.refreshToken);

                using HttpResponseMessage response = await client.DeleteAsync("http://84.113.43.172:8080/api/user/logout");
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException(responseBody);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HomeView.StopRecording();

            App.timer.Stop();
            App.timer.Close();

            DetectedProcess.HTTPUpdateProcesses(this, null);
            HomeView.videoCapture.Dispose();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //change the WindowStyle back to None, but only after the Window has been activated
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => WindowStyle = WindowStyle.None));
        }
    }
}
