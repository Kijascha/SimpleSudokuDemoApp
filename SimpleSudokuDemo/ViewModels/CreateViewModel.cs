using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using SimpleSudoku.SudokuSolver;
using SimpleSudokuDemo.Services;
using System.Windows.Media;

namespace SimpleSudokuDemo.ViewModels;

public partial class CreateViewModel : ViewModel
{
    [ObservableProperty] private IEnumerable<CellModel> _cellCollection;
    private readonly IConstraintSolver _constraintSolver;

    public IPuzzleModel Puzzle { get; set; }
    private HashSet<(int Row, int Column, int Candidate)> _removedCandidates = [];
    [ObservableProperty] private CellModel? _selectedCell;

    public CreateViewModel(INavigationService navigationService,
                            IServiceProvider serviceProvider,
                            IEnumerable<CellModel> cellCollection,
                            IPuzzleModel puzzle,
                            IConstraintSolver constraintSolver) : base(navigationService, serviceProvider)
    {
        _cellCollection = cellCollection;
        Puzzle = puzzle;
        _constraintSolver = constraintSolver;
        Puzzle.SudokuError += Puzzle_SudokuError;
        Puzzle.SudokuSuccess += Puzzle_SudokuSuccess;
    }

    private void Puzzle_SudokuSuccess(object? sender, SudokuSuccessEventArgs e)
    {
        foreach (var cell in CellCollection)
        {
            if (cell.Row == e.ConflictingCell.Row && cell.Column == e.ConflictingCell.Column)
            {
                cell.CellBackground = Brushes.White;
            }
        }
    }

    private void Puzzle_SudokuError(object? sender, SudokuErrorEventArgs e)
    {
        foreach (var cell in CellCollection)
        {
            if (cell.Row == e.ConflictingCell.Row && cell.Column == e.ConflictingCell.Column)
            {
                cell.CellBackground = Brushes.Red;
            }
        }
    }

    [RelayCommand]
    public void NavigateToMenuView()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<MenuViewModel>());
    }

    [RelayCommand]
    public void AddDigit(Digits digit)
    {
        var val = ConvertToInt(digit);

        if (SelectedCell != null)
        {
            Puzzle.UpdateDigit(SelectedCell.Row, SelectedCell.Column, val, true);
            RefreshCellCollection();
        }
    }
    [RelayCommand]
    public void Solve()
    {
        //TODO implement Solve method call from the solver class
        _constraintSolver.InitializeConstraints();
        _constraintSolver.Solve();
        RefreshCellCollection();
    }

    private void RefreshCellCollection()
    {
        var newCollection = Puzzle.ToObservableCollection();
        int i = 0;
        foreach (var cell in CellCollection)
        {
            cell.Row = newCollection[i].Row;
            cell.Column = newCollection[i].Column;
            cell.Digit = newCollection[i].Digit;
            cell.PlayerCandidates = [.. newCollection[i].PlayerCandidates];
            cell.SolverCandidates = [.. newCollection[i].SolverCandidates];
            i++;
        }
    }

    private static int? ConvertToInt(Digits values)
    {
        return (values == Digits.One) ? 1 :
               (values == Digits.Two) ? 2 :
               (values == Digits.Three) ? 3 :
               (values == Digits.Four) ? 4 :
               (values == Digits.Five) ? 5 :
               (values == Digits.Six) ? 6 :
               (values == Digits.Seven) ? 7 :
               (values == Digits.Eight) ? 8 :
               (values == Digits.Nine) ? 9 :
               (values == Digits.Zero) ? 0 :
               null;
    }
}
