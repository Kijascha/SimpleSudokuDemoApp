using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels
{
    public partial class SettingsViewModel : ViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IConfiguration _config;

        [ObservableProperty] private bool _developerMode;
        [ObservableProperty] private bool _darkMode;


        public SettingsViewModel(INavigationService navigationService,
                                IServiceProvider serviceProvider,
                                IAppSettingsService appSettingsService,
                                IConfiguration config) : base(navigationService, serviceProvider)
        {
            _navigationService = navigationService;
            _serviceProvider = serviceProvider;
            _appSettingsService = appSettingsService;
            _config = config;
            _appSettingsService.LoadSettings();
            _developerMode = _config.GetValue<bool>("DeveloperMode");
            _darkMode = _config.GetValue<bool>("DarkMode");
        }
        partial void OnDeveloperModeChanged(bool value)
        {
            _appSettingsService.Settings.DeveloperMode = value;
            _appSettingsService.UpdateSettings(_appSettingsService.Settings);
        }
        partial void OnDarkModeChanged(bool value)
        {
            _appSettingsService.Settings.DarkMode = value;
            _appSettingsService.UpdateSettings(_appSettingsService.Settings);
        }
    }
}
