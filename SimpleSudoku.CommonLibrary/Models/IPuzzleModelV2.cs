using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.CommonLibrary.Models
{
    public interface IPuzzleModelV2
    {
        CellV2[,] Board { get; set; }
        CellV2[][] ToJaggedArray();
        void FromJaggedArray(CellV2[][] jaggedBoard);
        bool CanUndo { get; }
        bool CanRedo { get; }
        void Undo();
        void Redo();
        IEnumerable<CellV2> GetBox(int startRow, int startCol, bool usePlayerCandidates);
        IEnumerable<CellV2> GetColumn(int column, bool usePlayerCandidates);
        IEnumerable<CellV2> GetRow(int row, bool usePlayerCandidates);
        IEnumerable<CellV2> GetUnit(int row, int col, SearchUnitType searchUnitType);
        bool IsValidDigit(int row, int column, int digit);
        bool IsValidInColumn(int column, int digit);
        bool IsValidInRow(int row, int digit);
        bool IsValidInSubgrid(int row, int column, int digit);
        void UpdateCandidate(int row, int column, int candidate, GameMode gameMode, bool useSolverCandidates = true, CandidateMode candidateMode = CandidateMode.None);
        void UpdateDigit(int row, int column, int digit, GameMode gameMode);
    }
}
