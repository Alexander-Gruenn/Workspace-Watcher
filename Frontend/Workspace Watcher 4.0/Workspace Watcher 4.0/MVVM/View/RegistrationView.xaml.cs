using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
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
using Workspace_Watcher_4._0;
using Workspace_Watcher_4._0.Classes;
using Workspace_Watcher_4._0.Logic;
using Workspace_Watcher_4._0.MVVM.ViewModel;

namespace Workspace_Watcher_4._0.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für RegistrationView.xaml
    /// </summary>
    public partial class RegistrationView : UserControl
    {
        static MainWindow mainWindow;
        bool accessGranted = false;

        public static void GetMainWindow(MainWindow main)
        {
            mainWindow = main;
        }

        public RegistrationView()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            registerButton.IsEnabled = false;
            loginButton.IsEnabled = false;

            if (!accessGranted)
                HTTPRegisterRequestAsync();

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

            registerButton.IsEnabled = true;
            loginButton.IsEnabled = true;
        }

        private async Task HTTPRegisterRequestAsync()
        {
            try
            {
                HttpClient client = new HttpClient();

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                User user = new User(firstnameTxtBox.Text, lastnameTxtBox.Text, mailTxtBox.Text, passwordTxtBox.Password);

                var json = JsonConvert.SerializeObject(user);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync("http://84.113.43.172:8080/api/user/register", data);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException(responseBody);

                response.EnsureSuccessStatusCode();

                var tokens = JsonConvert.DeserializeObject<Tokens>(responseBody);

                TokenStorage.SetTokens(tokens);

                accessGranted = true;

                Register_Click(null, null);
            }
            catch (HttpRequestException ex)
            {
                accessGranted = false;
                
                //MessageBox.Show(ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);

                firstnameTxtBox.Clear();
                lastnameTxtBox.Clear();
                mailTxtBox.Clear();
                passwordTxtBox.Clear();
            }
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.LogInButton.Command.Execute(null);
        }
        private void CheckToRegister()
        {
            if(passwordTxtBox.Password != String.Empty && mailTxtBox.Text != String.Empty && lastnameTxtBox.Text != String.Empty && firstnameTxtBox.Text != String.Empty && CheckUserInputPassword() && CheckUserInputMail())
                registerButton.IsEnabled = true;
            else
                registerButton.IsEnabled = false;
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
                passwordErrorLabel.Content = " ";

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
                mailErrorLabel.Content = " ";

            return canLogIn;
        }

        private void passwordTxtBox_PasswordChanged(object sender, RoutedEventArgs e) => CheckToRegister();

        private void mailTxtBox_TextChanged(object sender, RoutedEventArgs e) => CheckToRegister();

        private void lastnameTxtBox_TextChanged(object sender, RoutedEventArgs e) => CheckToRegister();

        private void firstnameTxtBox_TextChanged(object sender, RoutedEventArgs e) => CheckToRegister();
    }
}
