using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSudoku.CommonLibrary.Models;
using SimpleSudokuDemo.Services;
using SimpleSudokuDemo.ViewModels;
using SimpleSudokuDemo.Views;
using System.Collections.ObjectModel;
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
                    services.AddSingleton<IEnumerable<CellModel>, ObservableCollection<CellModel>>((p) => InitializeCollection());

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

        private ObservableCollection<CellModel> InitializeCollection()
        {
            var cellCollection = new ObservableCollection<CellModel>();

            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int column = 0; column < PuzzleModel.Size; column++)
                {
                    double left = column % 3 == 0 ? 2 : .5;
                    double top = row % 3 == 0 ? 2 : .5;
                    double right = column == 9 - 1 ? 2 : 0;
                    double bottom = row == 9 - 1 ? 2 : 0;

                    cellCollection.Add(new CellModel
                    {
                        Row = row,
                        Column = column,
                        Digit = null,
                        SolverCandidates = [1, 2, 3, 4, 5, 6, 7, 8, 9],
                        PlayerCandidates = [],
                        CellBorderThickness = new Thickness(left, top, right, bottom)
                    });
                }
            }

            return cellCollection;
        }
    }

}
