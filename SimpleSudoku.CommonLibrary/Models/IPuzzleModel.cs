using SimpleSudoku.CommonLibrary.System;
using System.Collections.ObjectModel;

namespace SimpleSudoku.CommonLibrary.Models
{
    public interface IPuzzleModel
    {
        int?[,] Digits { get; init; }
        HashSet<int>[,] PlayerCandidates { get; init; }
        HashSet<int>[,] SolverCandidates { get; init; }

        event EventHandler<SudokuErrorEventArgs>? SudokuError;

        ObservableCollection<CellModel> ToObservableCollection();
        void UpdateDigit(int row, int column, int? digit, bool validate = true);
        void UpdatePlayerCandidate(int row, int column, int candidate);
        void UpdateSolverCandidate(int row, int column, int candidate);
    }
}