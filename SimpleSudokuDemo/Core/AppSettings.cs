using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleSudokuDemo.Core
{
    public partial class AppSettings : ObservableObject
    {
        [ObservableProperty] private bool _developerMode;
        [ObservableProperty] private bool _darkMode;

    }
}
