using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.SudokuSolver.Services;
using SimpleSudokuDemo.Core;
using SimpleSudokuDemo.Services;
using SimpleSudokuDemo.ViewModels;
using SimpleSudokuDemo.Views;
using System.IO;
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
                .ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("appsettings.json");

#if DEBUG
                    configurationBuilder.AddJsonFile("appsettings.Development.json", true, true);
#else
                    configurationBuilder.AddJsonFile("appsettings.Production.json", true, true);
#endif
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Register ViewModels
                    services.AddFactory<StartupViewModel>();
                    services.AddFactory<MenuViewModel>();
                    services.AddFactory<CreateViewModel>();
                    services.AddFactory<PlayViewModel>();
                    services.AddFactory<SettingsViewModel>();

                    services.AddSingleton<IAppSettingsService, AppSettingsService>();

                    // Register configuration settings
                    services.Configure<AppSettings>(hostContext.Configuration);

                    services.AddNavigationService();
                    services.AddSingleton((p) => p);
                    services.AddSingleton<IPuzzleModel, PuzzleModel>(p => new PuzzleModel(Initialize2DCollectionV2()));
                    services.AddSingleton<IPuzzleModelV2, PuzzleModelV2>(p => new PuzzleModelV2(Initialize2DCollectionV2()));
                    // Register Modules
                    services.AddConstraintSolver();
                    services.AddBacktrackSolver();
                    services.AddGameService();

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
                    services.AddSingleton(p => new SettingsView()
                    {
                        DataContext = p.GetRequiredService<SettingsViewModel>()
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
        private static CellV2[,] Initialize2DCollectionV2()
        {
            CellV2[,] collection2D = new CellV2[PuzzleModel.Size, PuzzleModel.Size];

            for (int row = 0; row < PuzzleModelV2.Size; row++)
            {
                for (int column = 0; column < PuzzleModelV2.Size; column++)
                {
                    collection2D[row, column] = new CellV2
                    {
                        Row = row,
                        Column = column
                    };
                }
            }

            return collection2D;
        }
    }

}
