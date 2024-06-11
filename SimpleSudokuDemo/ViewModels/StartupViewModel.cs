using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels;

public partial class StartupViewModel : ViewModel
{
    public StartupViewModel(INavigationService navigationService, IServiceProvider serviceProvider) : base(navigationService, serviceProvider)
    {
        NavigateToMenu();
    }

    public void NavigateToMenu()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<MenuViewModel>());
    }
}
