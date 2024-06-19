using CommunityToolkit.Mvvm.ComponentModel;
using SimpleSudokuDemo.Services;

namespace SimpleSudokuDemo.ViewModels;

public partial class ViewModel(INavigationService navigationService, IServiceProvider serviceProvider) : ObservableObject
{
    protected IServiceProvider ServiceProvider { get; init; } = serviceProvider;

    [ObservableProperty] private INavigationService _navigationService = navigationService;
}
