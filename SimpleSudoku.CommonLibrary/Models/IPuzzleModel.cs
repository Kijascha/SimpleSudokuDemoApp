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

        (bool isValid, int conflictingRow, int conflictingColumn) IsValidInRow(int row, int digit);
        (bool isValid, int conflictingRow, int conflictingColumn) IsValidInColumn(int column, int digit);
        (bool isValid, int conflictingRow, int conflictingColumn) IsValidInSubgrid(int row, int column, int digit);
        (bool isValid, int conflictingRow, int conflictingColumn) IsValidDigit(int row, int column, int digit);

        IEnumerable<(int? Digits, HashSet<int> Candidates)> GetRow(int row, bool usePlayerCandidates);
        IEnumerable<(int? Digits, HashSet<int> Candidates)>? GetColumn(int column, bool usePlayerCandidates);
        IEnumerable<(int? Digits, HashSet<int> Candidates)> GetBox(int startRow, int startCol, bool usePlayerCandidates);
    }
}