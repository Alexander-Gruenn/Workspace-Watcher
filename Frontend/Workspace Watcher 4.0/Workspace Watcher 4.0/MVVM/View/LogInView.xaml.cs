using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
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
using Workspace_Watcher_4._0.Classes;
using Workspace_Watcher_4._0.Logic;

namespace Workspace_Watcher_4._0.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für LogInView.xaml
    /// </summary>
    public partial class LogInView : UserControl
    {
        static MainWindow mainWindow;
        bool accessGranted = false;
        public static void GetMainWindow(MainWindow main)
        {
            mainWindow = main;
        }

        public LogInView()
        {
            InitializeComponent();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            logInButton.IsEnabled = false;
            registerButton.IsEnabled = false;

            if (!accessGranted)
                HTTPLoginRequestAsync();
            

            if (accessGranted)
            {
                mainWindow.ProfileButton.Opacity = 100;
                mainWindow.StatisticsButton.Opacity = 100;
                mainWindow.HomeButton.Opacity = 100;
                mainWindow.LogOutButton.Opacity = 100;
                mainWindow.faceDetectedText.Opacity = 100;
                mainWindow.RecordIndicator.Opacity = 100;

                mainWindow.HomeButton.IsEnabled = true;
                mainWindow.LogOutButton.IsEnabled = true;
                mainWindow.StatisticsButton.IsEnabled = true;
                mainWindow.ProfileButton.IsEnabled = true;
                mainWindow.HomeButton.IsChecked = true;
                mainWindow.LeftBottomBorder.Background = Brushes.DeepSkyBlue;
                mainWindow.RightBottomBorder.Background = Brushes.DeepSkyBlue;
                mainWindow.HomeButton.Command.Execute(null);

                App.timer.Start();
                DetectedProcess.LoadProcesses();
            }

            logInButton.IsEnabled = true;
            registerButton.IsEnabled = true;
        }

        private async Task HTTPLoginRequestAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                User user = new User(mailTxtBox.Text, passwordTxtBox.Password);

                var json = JsonConvert.SerializeObject(user);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync("http://84.113.43.172:8080/api/user/login", data);
                string responseBody = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                var tokens = JsonConvert.DeserializeObject<Tokens>(responseBody);

                TokenStorage.SetTokens(tokens);

                accessGranted = true;

                LogIn_Click(null, null);
            }
            catch (HttpRequestException ex)
            {
                accessGranted = false;

                //MessageBox.Show(ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);

                mailTxtBox.Clear();
                passwordTxtBox.Clear();
            }
        }

        private void RegistrationPage_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.LogOutButton.Command.Execute(null);
        }
        private void CheckToLogIn()
        {
            if (passwordTxtBox.Password != String.Empty && mailTxtBox.Text != String.Empty && CheckUserInputMail() && CheckUserInputPassword())
                logInButton.IsEnabled = true;
            else
                logInButton.IsEnabled = false;
        }
        private bool CheckUserInputPassword()
        {
            bool canLogIn = true;

            if (passwordTxtBox.PasswordLength < 6)
            {
                passwordErrorLabel.Content = "Das Passwort muss mindestens 6 Zeichen lang sein.";
                canLogIn = false;
            }
            else
                passwordErrorLabel.Content = String.Empty;
            
            return canLogIn;
        }
        private bool CheckUserInputMail()
        {
            bool canLogIn = true;

            if (!MailAddress.TryCreate(mailTxtBox.Text, out MailAddress? result))
            {
                mailErrorLabel.Content = "Falsche E-Mail.";
                canLogIn = false;
            }
            else
                mailErrorLabel.Content = String.Empty;

            return canLogIn;
        }

        private void passwordTxtBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckToLogIn();

        private void mailTxtBox_TextChanged(object sender, TextChangedEventArgs e) => CheckToLogIn();
    }
}
