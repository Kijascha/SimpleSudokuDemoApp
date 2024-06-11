using SimpleSudokuDemo.Core;
using SimpleSudokuDemo.ViewModels;

namespace SimpleSudokuDemo.Services
{
    public interface INavigationService
    {
        ViewModel? CurrentViewModel { get; }
        void NavigateTo<TViewModel>(IAbstractFactory<TViewModel> factory) where TViewModel : ViewModel;
    }
}