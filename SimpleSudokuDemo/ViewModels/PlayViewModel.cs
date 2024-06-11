using CommunityToolkit.Mvvm.Input;
using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels;

public partial class PlayViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    : ViewModel(navigationService, serviceProvider)
{
    [RelayCommand]
    public void NavigateToMenuView()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<MenuViewModel>());
    }
}
