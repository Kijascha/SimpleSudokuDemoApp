using CommunityToolkit.Mvvm.ComponentModel;
using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels;

public partial class ViewModel : ObservableObject
{
    protected IServiceProvider ServiceProvider { get; init; }

    [ObservableProperty] private INavigationService _navigationService;

    public ViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        _navigationService = navigationService;
    }
}
