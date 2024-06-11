using CommunityToolkit.Mvvm.ComponentModel;
using SimpleSudokuDemo.Core;
using SimpleSudokuDemo.ViewModels;

namespace SimpleSudokuDemo.Services
{
    public partial class NavigationService : ObservableObject, INavigationService
    {
        [ObservableProperty] private ViewModel? _currentViewModel;

        public void NavigateTo<TViewModel>(IAbstractFactory<TViewModel> factory)
            where TViewModel : ViewModel
        {
            CurrentViewModel = factory.Create();
        }
    }
}
