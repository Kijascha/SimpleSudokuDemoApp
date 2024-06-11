using CommunityToolkit.Mvvm.Input;
using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels;

public partial class MenuViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    : ViewModel(navigationService, serviceProvider)
{

    [RelayCommand]
    public void NavigateToPlayView()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<PlayViewModel>());
    }
    [RelayCommand]
    public void NavigateToCreateView()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<CreateViewModel>());
    }
}
