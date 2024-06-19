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
        public event EventHandler<SudokuSuccessEventArgs>? SudokuSuccess;

        ObservableCollection<CellModel> ToObservableCollection();
        void UpdateDigit(int row, int column, int? digit, bool validate = true);
        void UpdateCandidate(int row, int column, int candidate, bool useSolverCandidates = true);

        bool IsValidInRow(int row, int? digit);
        bool IsValidInColumn(int column, int? digit);
        bool IsValidInSubgrid(int row, int column, int? digit);
        bool IsValidDigit(int row, int column, int? digit);

        IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetRow(int row, bool usePlayerCandidates);
        IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetColumn(int column, bool usePlayerCandidates);
        IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetBox(int startRow, int startCol, bool usePlayerCandidates);
    }
}