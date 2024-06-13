using LiveCharts;
using LiveCharts.Defaults;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text;
using System.Windows;
using Workspace_Watcher_4._0.Classes;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using LiveCharts.Wpf;

namespace Workspace_Watcher_4._0.Logic
{
    public class DetectedProcess : INotifyPropertyChanged
    {
        public string Name { get; private set; }
        public TimeSpan TimeOpen { get; private set; }
        public bool displayedOnPiechart
        {
            get
            {
                var processes = App.observer.PieProcesses;
                foreach (var process in processes)
                    if(process.Title.Equals(Name))
                        return true;

                return false;
            }
        }
        public string processName { get; private set; }

        public ObservableValue SecoundsAsObservableValue { get; private set; }

        private static Dictionary<string, string> processPresetNames = new Dictionary<string, string>()
        {
            {"explorer", "Windows Explorer" },
            {"WindowsTerminal", "Windows Terminal" },
            {"sqldeveloper", "Oracle SQL Developer" },
            {"Unity", "Unity" },
            {"webstorm", "Webstorm" },
            {"datagrip", "DataGrip" },
            {"idea", "IntelliJ" },
            {"studio", "Android Studio" },
            {"ApplicationFrameHost", "Windows Process" },
            {"steam", "Steam" },
            {"WINWORD", "Microsoft Word" },
            {"Illustrator", "Adobe Illustrator" },
            {"Photoshop", "Adobe Photoshop" },
            {"Workspace Watcher 4.0", "Workspace Watcher" },
            {"firefox", "Firefox" },
            {"Acrobat", "Adobe Acrobat" },
        };

        private static LinkedList<string> blacklist = new LinkedList<string>(new string[]
        {
            "ApplicationFrameHost",
        });

        public static DetectedProcess activeProcess;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private DetectedProcess(string name, string processName, bool isNew, int sec)
        {
            Name = name;
            this.processName = processName;
            TimeOpen = new TimeSpan(0, 0, sec);
            SecoundsAsObservableValue = new ObservableValue(TimeOpen.TotalSeconds);

            if (isNew)
            {
                Task.Run(() =>
                {
                    while (TokenStorage.Tokens == null)
                    {
                        Thread.Sleep(1000);
                    }

                    HTTPPostProcess(this);
                });
            }
        }
        public static DetectedProcess? CreateNewProcess(string name, string processName, bool isNew, int seconds = 0)
        {
            if (blacklist.Contains(processName))
                return null;

            if (processPresetNames.ContainsKey(processName))
            {
                if (processPresetNames.TryGetValue(processName, out var newName))
                    return new DetectedProcess(newName, processName, isNew, seconds);
            }
            else
            {
                foreach (var key in processPresetNames.Keys)
                    if (processName.Contains(key))
                        if (processPresetNames.TryGetValue(key, out var newName))
                            return new DetectedProcess(newName, processName, isNew, seconds);
            }

            string[] splittedName = name.Split('-');

            if (splittedName.Length > 1)
                name = splittedName[splittedName.Length - 1].Substring(1);

            splittedName = name.Split(':');

            if (splittedName.Length > 1)
                name = splittedName[splittedName.Length - 1].Substring(1);

            splittedName = name.Split('|');

            if (splittedName.Length > 1)
                name = splittedName[splittedName.Length - 1].Substring(1);

            name = RemoveVersionIfPresent(name);

            return new DetectedProcess(name, processName, isNew, seconds);
        }
        public static async Task<bool> LoadProcesses()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Tokens.accessToken);

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                using HttpResponseMessage response = await client.GetAsync("http://84.113.43.172:8080/api/process/");

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException(responseBody);

                response.EnsureSuccessStatusCode();

                var list = JsonConvert.DeserializeObject<List<ProcessObject>>(responseBody);
                
                foreach (var process in list)
                {
                    var p = CreateNewProcess(process.displayedName, process.processName, false, process.sec);
                    Application.Current.Dispatcher.Invoke(() => App.observer.Processes.Add(p));
                    Application.Current.Dispatcher.Invoke(() => App.observer.PieProcesses.Add(new PieSeries
                    {
                        Title = p.Name,
                        Values = new ChartValues<ObservableValue> { p.SecoundsAsObservableValue },
                        DataLabels = true
                    }));
                }

                App.observer.Start();

                return true;
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static DetectedProcess? GetProcessByName(string name)
        {
            foreach (var process in App.observer?.Processes)
                if (process.processName == name)   
                    return process;

            return null;
        }

        public void AddSeconds(int seconds)
        {
            TimeOpen = TimeOpen.Add(new TimeSpan(0, 0, seconds));
            SecoundsAsObservableValue.Value += seconds;

            Application.Current.Dispatcher.Invoke(() => {
                if (!displayedOnPiechart)
                    if (TimeOpen.TotalSeconds > TaskObserver.GetObserver().LowestValueOnPiechart)
                        TaskObserver.GetObserver().ReplaceValueInPieProcessesList(this);
            });

            NotifyPropertyChanged("TimeOpen");
        }

        private static string RemoveVersionIfPresent(string name)
        {
            string[] nameParts = name.Split(' ');
            string lastNamePart = nameParts[nameParts.Length - 1].ToLower();

            if (nameParts.Length <= 1)
                return name;

            //Check common Version conventions
            if(lastNamePart.StartsWith('v'))
            {
                if (Char.IsDigit(lastNamePart[1]))
                    nameParts[nameParts.Length - 1] = "";
            }
            else if (Char.IsDigit(lastNamePart[0]))
            {
                if(lastNamePart.Split('.').Length > 1)
                    if (Char.IsDigit(lastNamePart[1]))
                        nameParts[nameParts.Length - 1] = "";
            }

            name = String.Empty;
            foreach(var part in nameParts)
                name += part + " ";

            return name.Remove(name.Length - 1);
                
        }

        public static DetectedProcess? FindeProcessByTitle(string title)
        {
            foreach(var process in App.observer.Processes)
                if(process.Name.Equals(title))
                    return process;

            return null;
        }

        private static async Task<bool> HTTPPostProcess(DetectedProcess p)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Tokens.accessToken);

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                ProcessObject process = new ProcessObject(p.processName, p.Name, (int)p.TimeOpen.TotalSeconds);
                var json = JsonConvert.SerializeObject(process);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PostAsync("http://84.113.43.172:8080/api/process/", data);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new HttpRequestException(responseBody);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.Message, "Fehler! - Post Process", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        public static void HTTPUpdateProcesses (object sender, ElapsedEventArgs e)
        {
            if (TokenStorage.Tokens == null)
            {
                //MessageBox.Show("No Tokens!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var process in App.observer.Processes)
            {
                HTTPUpdateProcess(process);
            }
        }

        private static async Task HTTPUpdateProcess (DetectedProcess p)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStorage.Tokens.accessToken);

                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };

                ProcessObject process = new ProcessObject(p.processName, p.Name, (int)p.TimeOpen.TotalSeconds);
                var json = JsonConvert.SerializeObject(process);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await client.PutAsync("http://84.113.43.172:8080/api/process/", data);

                string responseBody = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                //MessageBox.Show(ex.Message, "Fehler! - Update Process", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class ProcessObject
        {
            public string processName;
            public string displayedName;
            public int sec;

            [JsonConstructor]
            public ProcessObject(string processName, string displayedName, int sec)
            {
                this.processName = processName;
                this.displayedName = displayedName;
                this.sec = sec;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
