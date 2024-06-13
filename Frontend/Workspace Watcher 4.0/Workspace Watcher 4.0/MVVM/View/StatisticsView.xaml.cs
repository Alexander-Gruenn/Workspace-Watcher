using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Windows.Controls;
using Workspace_Watcher_4._0.Logic;
using LiveCharts.Definitions.Charts;

namespace Workspace_Watcher_4._0.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für StatisticsView.xaml
    /// </summary>
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            
            DataContext = App.observer;
            InitializeComponent();
        }

       

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            //var chart = (PieChart)chartpoint.ChartView;

            ////clear selected slice.
            //foreach (PieSeries series in chart.Series)
            //    series.PushOut = 0;

            //var selectedSeries = (PieSeries)chartpoint.SeriesView;
            //selectedSeries.PushOut = 8;
        }
    }
}
