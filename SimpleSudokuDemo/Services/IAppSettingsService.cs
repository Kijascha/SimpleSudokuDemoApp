using SimpleSudokuDemo.Core;

namespace SimpleSudokuDemo.Services
{
    public interface IAppSettingsService
    {
        event EventHandler? SettingsUpdated;
        AppSettings Settings { get; set; }
        AppSettings GetSettings();
        Task UpdateSettings(AppSettings settings);
        Task LoadSettings();
    }
}