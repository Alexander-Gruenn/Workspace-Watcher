using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
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

namespace Workspace_Watcher_4._0.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        private static User User { get; set; }
        private bool editButtonActivated { get; set; } = false;

        public ProfileView()
        {
            InitializeComponent();
            ProfileImage.ImageSource = new ImageSourceConverter().ConvertFromString("../../../MVVM/View/UserImage.png") as ImageSource;
            editButtonActivated = false;

            Task.Run(async () =>
            {
                User = await this.HTTPFetchUser();
                this.UpdateProperties(User);
            });
        }

        public void UpdateProperties(User user)
        {
            Dispatcher.Invoke(() =>
            {
                FirstName.Text = user.firstName;
                Surname.Text = user.surname;
                RoleTextBox.Text = user.role;
                MailTextBox.Text = user.email;
                PasswordTextBox.Text = "********";
            });            
        }

        private void ChangeClick(TextBox box)
        {
            if (box.IsEnabled)
                box.IsEnabled = false;            
            else
                box.IsEnabled = true;
        }

        private async void EditChangeClick(object sender, RoutedEventArgs e)
        {
            ChangeClick(FirstName);
            ChangeClick(Surname);
            ChangeClick(PasswordTextBox);
            ChangeClick(MailTextBox);
            editButtonActivated = !editButtonActivated;

            if (editButtonActivated)
            {
                Dispatcher.Invoke(() => PasswordTextBox.Text = String.Empty);
            }
            else
            {
                User newUser = User;

                if (!FirstName.Text.Equals(String.Empty))
                    newUser.firstName = FirstName.Text;
                if (!Surname.Text.Equals(String.Empty))
                    newUser.surname = Surname.Text;
                if (!PasswordTextBox.Text.Equals(String.Empty))
                    newUser.password = PasswordTextBox.Text;
                if (!MailTextBox.Text.Equals(String.Empty))
                    newUser.email = MailTextBox.Text;

                await HTTPUpdateUser(newUser);
                await HTTPGetNewTokens(newUser);
                User = await HTTPFetchUser();
                UpdateProperties(User);
            }
        }

        private void EditTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                FirstName.IsEnabled = false;
                Surname.IsEnabled = false;
                PasswordTextBox.IsEnabled = false;
                MailTextBox.IsEnabled = false;
            }
        }

        private void ImageChangeClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif;)|*.jpg;*.jpeg;*.png;*.gif;|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string sourceFile = openFileDialog.FileName;

                // Pfad zum Zielverzeichnis
                string targetDirectory = @"../../../MVVM/View/";

                // Name der Ziel-Datei
                string targetFileName = "UserImage.png";

                // Kombinieren Sie das Zielverzeichnis und den Dateinamen
                string destFile = System.IO.Path.Combine(targetDirectory, targetFileName);


                // Verwenden Sie die Methode File.Copy() zum Kopieren der Datei
                ProfileImage.ImageSource = new ImageSourceConverter().ConvertFromString("../../../MVVM/View/Replace.png") as ImageSource;

                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                File.Copy(sourceFile, destFile, true);

                ProfileImage.ImageSource = new ImageSourceConverter().ConvertFromString("../../../MVVM/View/UserImage.png") as ImageSource;
            }
        }

        private async Task HTTPUpdateUser(User user)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Tokens.accessToken);

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                var json = JsonConvert.SerializeObject(user);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PutAsync("http://84.113.43.172:8080/api/user/", data);

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

        private async Task<User> HTTPFetchUser()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Tokens.accessToken);

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                using HttpResponseMessage response = await client.GetAsync("http://84.113.43.172:8080/api/user/");

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException(responseBody);

                response.EnsureSuccessStatusCode();

                var jsonElement = JsonConvert.DeserializeObject<User>(responseBody);

                return jsonElement;
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        private async Task HTTPGetNewTokens(User param)
        {
            try
            {
                HttpClient client = new HttpClient();
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                User user = new User(param.email, param.password);

                var json = JsonConvert.SerializeObject(user);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync("http://84.113.43.172:8080/api/user/login", data);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException(responseBody);

                response.EnsureSuccessStatusCode();

                var tokens = JsonConvert.DeserializeObject<Tokens>(responseBody);

                TokenStorage.SetTokens(tokens);
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
