using Microsoft.Extensions.DependencyInjection;
using SimpleSudokuDemo.ViewModels;
using System.Windows.Controls;

namespace SimpleSudokuDemo.Views
{
    /// <summary>
    /// Interaktionslogik für SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            if (App.AppHost != null)
                DataContext = App.AppHost!.Services.GetRequiredService<SettingsViewModel>();
        }
    }
}
