//#define DebugList

using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using Workspace_Watcher_4._0.MVVM.View;

namespace Workspace_Watcher_4._0.Logic
{
    public class TaskObserver
    {
        private static TaskObserver observer;
        private const int TIMER_INTERVALL = 1000;
        private Timer timer = new Timer(TIMER_INTERVALL);
        private const int PIECHART_PROCESS_LIMIT = 8;
        public int LowestValueOnPiechart
        {
            get
            {
                int lowest = Int32.MaxValue;
                foreach (var process in PieProcesses)
                {
                    if ((process.Values[0] as ObservableValue).Value < lowest)
                        lowest = (int)(process.Values[0] as ObservableValue).Value;
                }
                
                return lowest;
            }
        }

        public PieSeries? LowestProcessOnPiechart
        {
            get
            {
                int lowest = LowestValueOnPiechart;
                foreach ( var process in PieProcesses)
                    if((process.Values[0] as ObservableValue).Value == lowest)
                        return process as PieSeries;
                return null;
            }
        }

        public ObservableCollection<DetectedProcess> Processes { get; set; } = new ObservableCollection<DetectedProcess>();
        public SeriesCollection PieProcesses { get; set; } = new SeriesCollection();

        //Singleton constructor
        private TaskObserver()
        {
            observer = this;
        }

        private void CheckActiveWindowChange(object source, ElapsedEventArgs e)
        {
            int idOfNewProcess = GetActiveWindowId();
            string processName = Process.GetProcessById(idOfNewProcess).ProcessName;

            if (DetectedProcess.activeProcess?.processName == Process.GetProcessById(idOfNewProcess).ProcessName)
                return;

            DetectedProcess? newDetectedProcess = DetectedProcess.GetProcessByName(processName);
            if(newDetectedProcess != null)
            {
                DetectedProcess.activeProcess = newDetectedProcess;
                return;
            }

            Process newProcess = Process.GetProcessById(idOfNewProcess);

            if (newProcess.MainWindowTitle.Equals(String.Empty))
                return;

            if(newProcess.ProcessName.Equals(String.Empty))
                return;

            newDetectedProcess = DetectedProcess.CreateNewProcess(newProcess.MainWindowTitle, newProcess.ProcessName, true);
            if(newDetectedProcess == null) return;
            DetectedProcess.activeProcess = newDetectedProcess;
            Application.Current.Dispatcher.Invoke(() => Processes.Add(newDetectedProcess));
            AddToPieProcessesList(newDetectedProcess);
        }

        private void AddToPieProcessesList(DetectedProcess process)
        {
            if (PIECHART_PROCESS_LIMIT <= PieProcesses.Count)
                return;

            Application.Current.Dispatcher.Invoke(() => PieProcesses.Add(new PieSeries
            {
                Title = process.Name,
                Values = new ChartValues<ObservableValue> { process.SecoundsAsObservableValue },
                DataLabels = false
            }));

            //process.displayedOnPiechart = true;
        }

        public void ReplaceValueInPieProcessesList(DetectedProcess process)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PieProcesses.Remove(LowestProcessOnPiechart);
                //DetectedProcess.FindeProcessByTitle(LowestProcessOnPiechart.Title).displayedOnPiechart = false;
            });
            AddToPieProcessesList(process);

        }

        private void UpdateTime(object source, ElapsedEventArgs e)
        {
            if(HomeView.DetectedFaces <= 0) 
                return;

            DetectedProcess.activeProcess.AddSeconds(TIMER_INTERVALL / 1000);
        }

        public void Start()
        {
            if (timer.Enabled)
                return;

#if DebugList
            foreach(var process in Process.GetProcesses())
            {
                if (process.MainWindowTitle.Equals(String.Empty))
                    continue;

                Processes.AddLast(new DetectedProcess(process.MainWindowTitle, process.Id, process));
            }
#endif

            int idOfCurrentProcess = GetActiveWindowId();
            string processName = Process.GetProcessById(idOfCurrentProcess).ProcessName;

            var currentDetectedProcess = DetectedProcess.GetProcessByName(processName);
            if (currentDetectedProcess == null)
            {
                Process newProcess = Process.GetProcessById(idOfCurrentProcess);
                currentDetectedProcess = DetectedProcess.CreateNewProcess(newProcess.MainWindowTitle, newProcess.ProcessName, true);
                Processes.Add(currentDetectedProcess);
                AddToPieProcessesList(currentDetectedProcess);
            }
            else

            DetectedProcess.activeProcess = currentDetectedProcess;

            timer.Elapsed += CheckActiveWindowChange;
            timer.Elapsed += UpdateTime;
            timer.Start();
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);


        private int GetActiveWindowId()
        {
            uint processId;
            IntPtr handle = GetForegroundWindow();
            GetWindowThreadProcessId(handle, out processId);

            return (int) processId;
        }

        public static TaskObserver GetObserver()
        {
            if(observer == null) 
                new TaskObserver();

            return observer;
        }
    }
}
