using Workspace_Watcher_4._0.Core;

namespace Workspace_Watcher_4._0.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand StatisticsViewCommand { get; set; }
        public RelayCommand ProfileViewCommand { get; set; }
        public RelayCommand RegistrationViewCommand { get; set; }
        public RelayCommand LogInViewCommand { get; set; }

        public HomeViewModel _HomeViewModel { get; set; }
        public StatisticsViewModel _StatisticsViewModel { get; set; }
        public ProfileViewModel _ProfileViewModel { get; set; }
        public RegistrationViewModel _RegistrationViewModel { get; set; }
        public LogInViewModel _LogInViewModel { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel()
        {
            _HomeViewModel = new HomeViewModel();
            _StatisticsViewModel = new StatisticsViewModel();
            _ProfileViewModel = new ProfileViewModel();
            _RegistrationViewModel = new RegistrationViewModel();
            _LogInViewModel = new LogInViewModel();
            CurrentView = _RegistrationViewModel;

            HomeViewCommand = new RelayCommand(o => CurrentView = _HomeViewModel);

            StatisticsViewCommand = new RelayCommand(o => CurrentView = _StatisticsViewModel);

            ProfileViewCommand = new RelayCommand(o => CurrentView = _ProfileViewModel);

            RegistrationViewCommand = new RelayCommand(o => CurrentView = _RegistrationViewModel);

            LogInViewCommand = new RelayCommand(o => CurrentView = _LogInViewModel);
        }

    }
}
