using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSudokuDemo.Services;
using SimpleSudokuDemo.ViewModels;
using SimpleSudokuDemo.Views;
using System.Windows;

namespace SimpleSudokuDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }
        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Register ViewModels
                    services.AddFactory<StartupViewModel>();
                    services.AddFactory<MenuViewModel>();
                    services.AddFactory<CreateViewModel>();
                    services.AddFactory<PlayViewModel>();

                    services.AddNavigationService();
                    services.AddSingleton((p) => p);

                    // Register Views and set DataContext for each one
                    services.AddSingleton(p => new StartupView()
                    {
                        DataContext = p.GetRequiredService<StartupViewModel>()
                    });
                    services.AddSingleton(p => new MenuView()
                    {
                        DataContext = p.GetRequiredService<MenuViewModel>()
                    });
                    services.AddSingleton(p => new CreateView()
                    {
                        DataContext = p.GetRequiredService<CreateViewModel>()
                    });
                    services.AddSingleton(p => new PlayView()
                    {
                        DataContext = p.GetRequiredService<PlayViewModel>()
                    });
                })
                .Build();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupForm = AppHost!.Services.GetRequiredService<StartupView>();
            startupForm.Show();

            base.OnStartup(e);
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();

            base.OnExit(e);
        }
    }

}
