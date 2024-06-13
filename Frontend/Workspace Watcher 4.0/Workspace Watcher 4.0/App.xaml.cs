using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Workspace_Watcher_4._0.Logic;
using Workspace_Watcher_4._0.MVVM.View;

namespace Workspace_Watcher_4._0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TaskObserver observer { get; private set; }
        private const int PUSH_INTERVAL = 10000; // in msec
        public static System.Timers.Timer timer;
		private static HomeView homeView;
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            observer = TaskObserver.GetObserver();
            
            GetVideoCapture();

            timer = new System.Timers.Timer(PUSH_INTERVAL);
            timer.Elapsed += DetectedProcess.HTTPUpdateProcesses;
        }

        public static void GetVideoCapture()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM win32_PNPEntity WHERE PnPClass='Camera'");

            Task.Run(async () =>
            {
                var collection = await Task.Run(() => { return searcher.Get(); });
                if (collection.Count > 0)
                {
                    HomeView.videoCapture = new VideoCapture();
                }
                else
                {
                    HomeView.videoCapture = null;
                }

                
                if(homeView != null)
                {
                    homeView.RecordButton.Dispatcher.Invoke(() =>
                    {
                        homeView.RecordButton.IsEnabled = true;
                    });
                }
            });
        }


        public static void SetHomeView(HomeView view)
        {
            homeView = view;
        }
    }
}
