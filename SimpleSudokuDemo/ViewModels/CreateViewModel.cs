using CommunityToolkit.Mvvm.Input;
using SimpleSudoku.CommonLibrary.Models;
using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels;

public partial class CreateViewModel(INavigationService navigationService, IServiceProvider serviceProvider, IEnumerable<CellModel> cellCollection)
    : ViewModel(navigationService, serviceProvider)
{
    public IEnumerable<CellModel> CellCollection { get; set; } = cellCollection;
    [RelayCommand]
    public void NavigateToMenuView()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<MenuViewModel>());
    }
}
