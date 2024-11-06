using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using SimpleSudokuDemo.Core;
using System.IO;
using System.Text.Json;

namespace SimpleSudokuDemo.Services
{
    public partial class AppSettingsService : ObservableObject, IAppSettingsService
    {
        private readonly string _appSettingsFilePath;
        [ObservableProperty] private AppSettings _settings;

        // Define the event
        public event EventHandler? SettingsUpdated;

        public AppSettingsService(IOptions<AppSettings> options)
        {
            _settings = options.Value;
            _appSettingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            // Load settings synchronously if the file exists
            if (File.Exists(_appSettingsFilePath))
            {
                var json = File.ReadAllText(_appSettingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json);
                if (loadedSettings != null)
                {
                    _settings = loadedSettings;
                }
                else
                {
                    _settings = options.Value;
                }
            }
            else
            {
                _settings = options.Value;
            }
        }
        public AppSettings GetSettings() => Settings;

        public async Task UpdateSettings(AppSettings settings)
        {
            Settings = settings;
            await SaveSettings();
            OnUpdateSettings();
        }
        public async Task LoadSettings()
        {
            if (File.Exists(_appSettingsFilePath))
            {
                using FileStream fileStream = File.OpenRead(_appSettingsFilePath);
                var result = await JsonSerializer.DeserializeAsync<AppSettings>(fileStream);

                if (result != null)
                {
                    Settings = result;
                }
            }

        }

        private async Task SaveSettings()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            // Serialize and save JSON to the file
            string json = JsonSerializer.Serialize(Settings, options);

            using FileStream fileStream = File.Create(_appSettingsFilePath);
            await JsonSerializer.SerializeAsync(fileStream, Settings);
            await fileStream.DisposeAsync();
        }
        protected virtual void OnUpdateSettings()
        {
            SettingsUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
