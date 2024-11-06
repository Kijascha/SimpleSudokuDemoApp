using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.CommonLibrary.Models
{
    public partial class PuzzleModelV2 : ObservableObject, IPuzzleModelV2
    {
        public const int Size = 9; // set to 9 for now for a regular 9x9 sudoku grid
        public CellV2[,] Board { get; set; }

        // Store bitmasks for removed candidates to enable restoration
        private readonly HashSet<(int Row, int Column, int Candidate)> _removedSolverCandidates = [];
        private readonly HashSet<(int Row, int Column, int Candidate)> _removedCenterCandidates = [];
        private readonly HashSet<(int Row, int Column, int Candidate)> _removedCornerCandidates = [];

        // Stacks to store history for undo and redo actions
        private readonly Stack<CellV2[,]> _undoStack = [];
        private readonly Stack<CellV2[,]> _redoStack = [];

        // Properties to check if undo or redo is possible
        [ObservableProperty] private bool _canUndo;
        [ObservableProperty] private bool _canRedo;

        private readonly object _lockObject = new();

        // Method to notify about state changes
        private void NotifyStackChange()
        {
            CanUndo = _undoStack.Count > 0;
            CanRedo = _redoStack.Count > 0;
        }
        #region Constructors
        public PuzzleModelV2()
        {
            Board = new CellV2[Size, Size];
        }
        public PuzzleModelV2(CellV2[,] board)
        {
            Board = new CellV2[Size, Size];

            for (int row = 0; row < Size; row++)
            {
                for (int column = 0; column < Size; column++)
                {
                    Board[row, column] = board[row, column];
                }
            }
        }
        #endregion

        #region Export
        // Method to clone the board state
        private CellV2[,] CloneBoard(CellV2[,] board)
        {
            var clone = new CellV2[Size, Size];
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    clone[row, col] = board[row, col].Clone();
                }
            }
            return clone;
        }
        public CellV2[][] ToJaggedArray()
        {
            var jaggedBoard = new CellV2[Size][];

            for (int i = 0; i < Size; i++)
            {
                jaggedBoard[i] = new CellV2[Size];
                for (int j = 0; j < Size; j++)
                {
                    jaggedBoard[i][j] = Board[i, j].Clone();
                }
            }
            return jaggedBoard;
        }
        public void FromJaggedArray(CellV2[][] jaggedBoard)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Board[i, j] = jaggedBoard[i][j].Clone();
                }
            }
        }
        #endregion

        #region Update and Restore
        public void UpdateDigit(int row, int column, int digit, GameMode gameMode)
        {
            lock (_lockObject)
            {
                ValidateRowColumn(row, column);
                ValidateDigit(digit);

                int currentDigit = Board[row, column].Digit;

                if (gameMode == GameMode.Play)
                {
                    // Save the current board state to undo stack before making changes
                    _undoStack.Push(CloneBoard(Board));
                    _redoStack.Clear(); // Clear redo stack on new action
                    NotifyStackChange(); // Notify the UI that stacks have changed
                }

                if (currentDigit == digit)
                {
                    RestoreCandidates(row, column, Board[row, column].Digit);
                    // Toggle off: clear the digit and restore candidates
                    Board[row, column].Digit = 0;
                }
                else
                {
                    if (digit == 0)
                    {
                        Board[row, column].Digit = 0;
                        Board[row, column].CenterCandidates.Clear();
                    }
                    else
                    {
                        // Toggle on: set the digit and remove only existing candidates
                        Board[row, column].Digit = digit;
                        RemoveCandidatesInUnit(SearchUnitType.Row, row, column, digit);
                        RemoveCandidatesInUnit(SearchUnitType.Column, row, column, digit);
                        RemoveCandidatesInUnit(SearchUnitType.Box, row, column, digit);

                        // Clear candidates when a digit is set (for simplicity)
                        if (Board[row, column].Digit != 0)
                        {
                            Board[row, column].SolverCandidates.Clear();
                            Board[row, column].CenterCandidates.Clear();
                        }
                    }
                }
            }
        }
        public void UpdateCandidate(int row, int column, int candidate, GameMode gameMode, bool useSolverCandidates = true, CandidateMode candidateMode = CandidateMode.None)
        {
            lock (_lockObject)
            {
                ValidateRowColumn(row, column);
                ValidateCandidate(candidate);

                if (Board[row, column].Digit != 0)
                {
                    // If a digit is already set, do nothing as candidates should be ignored
                    return;
                }

                if (gameMode == GameMode.Play)
                {
                    // Save the current board state to undo stack before making changes
                    _undoStack.Push(CloneBoard(Board));
                    _redoStack.Clear(); // Clear redo stack on new action
                    NotifyStackChange(); // Notify the UI that stacks have changed
                }

                if (useSolverCandidates)
                {
                    // Remove candidate if it exists
                    if (!Board[row, column].SolverCandidates.Remove(candidate))
                    {
                        // Add candidate if it does not exist
                        Board[row, column].SolverCandidates.Add(candidate);
                    }
                }
                else
                {

                    switch (candidateMode)
                    {
                        case CandidateMode.CenterCandidates:
                            // Remove candidate if it exists
                            if (!Board[row, column].CenterCandidates.Remove(candidate))
                            {
                                // Add candidate if it does not exist
                                Board[row, column].CenterCandidates.Add(candidate);
                            }
                            break;

                        case CandidateMode.CornerCandidates:
                            // Remove candidate if it exists
                            if (!Board[row, column].CornerCandidates.Remove(candidate))
                            {
                                // Add candidate if it does not exist
                                Board[row, column].CornerCandidates.Add(candidate);
                            }
                            break;
                    }
                }
            }
        }
        // Undo the last action by restoring the previous board state
        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            // Push current state to redo stack before undoing
            _redoStack.Push(CloneBoard(Board));

            // Restore the last board state from undo stack
            var previousState = _undoStack.Pop();
            Board = previousState;

            NotifyStackChange(); // Notify UI that stacks have changed
        }

        // Redo the last undone action by restoring from redo stack
        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            // Push current state to undo stack before redoing
            _undoStack.Push(CloneBoard(Board));

            // Restore the last board state from redo stack
            var nextState = _redoStack.Pop();
            Board = nextState;

            NotifyStackChange(); // Notify UI that stacks have changed
        }
        private void RemoveCandidatesInUnit(SearchUnitType searchUnitType, int row, int column, int digit)
        {
            var cellsInUnit = (SearchUnitType.Row == searchUnitType) ? GetRow(row, false) :
                                (SearchUnitType.Column == searchUnitType) ? GetColumn(column, false) :
                                GetBox(row, column, false);

            foreach (var cell in cellsInUnit)
            {
                if (cell.SolverCandidates.Collection.Any() &&
                    cell.SolverCandidates.Contains(digit) &&
                    !_removedSolverCandidates.Contains((cell.Row, cell.Column, digit)))
                {
                    _removedSolverCandidates.Add((cell.Row, cell.Column, digit));
                    cell.SolverCandidates.Remove(digit);
                }

                if (cell.CenterCandidates.Collection.Any() &&
                    cell.CenterCandidates.Contains(digit) &&
                    !_removedCenterCandidates.Contains((cell.Row, cell.Column, digit)))
                {
                    _removedCenterCandidates.Add((cell.Row, cell.Column, digit));
                    cell.CenterCandidates.Remove(digit);
                }
            }
        }

        private void RestoreCandidates(int row, int column, int digit)
        {
            var cellsInUnit = _removedCenterCandidates
                .Where(c => IsInSameUnit(c.Row, c.Column, row, column) && c.Candidate == digit);

            foreach (var cell in cellsInUnit)
            {
                Board[cell.Row, cell.Column].SolverCandidates.Add(digit);
                _removedSolverCandidates.Remove((cell.Row, cell.Column, digit));

                Board[cell.Row, cell.Column].CenterCandidates.Add(digit);
                _removedCenterCandidates.Remove((cell.Row, cell.Column, digit));
            }
        }

        private static bool IsInSameUnit(int row1, int col1, int row2, int col2)
        {
            return row1 == row2 || col1 == col2 || GetBlockIndex(row1, col1) == GetBlockIndex(row2, col2);
        }
        private static int GetBlockIndex(int row, int col)
        {
            return (row / 3) * 3 + (col / 3);
        }
        #endregion

        #region Units
        public IEnumerable<CellV2> GetUnit(int row, int col, SearchUnitType searchUnitType) => searchUnitType switch
        {
            SearchUnitType.Box => GetBox(row, col, false),
            SearchUnitType.Row => GetRow(row, false),
            SearchUnitType.Column => GetColumn(col, false),
            _ => throw new ArgumentOutOfRangeException(nameof(searchUnitType))
        };
        public IEnumerable<CellV2> GetRow(int row, bool usePlayerCandidates) => Enumerable.Range(0, Size).Select(col => Board[row, col]);
        public IEnumerable<CellV2> GetColumn(int col, bool usePlayerCandidates) => Enumerable.Range(0, Size).Select(row => Board[row, col]);
        public IEnumerable<CellV2> GetBox(int startRow, int startCol, bool usePlayerCandidates) => GetBoxRange(startRow).SelectMany(r => GetBoxRange(startCol).Select(c => Board[r, c]));
        private static IEnumerable<int> GetBoxRange(int index) => Enumerable.Range((index / 3) * 3, 3);
        #endregion

        #region Validation
        private static void ValidateRowColumn(int row, int column)
        {
            ValidateRow(row);
            ValidateColumn(column);
        }
        private static void ValidateRow(int row)
        {
            Guard.IsBetweenOrEqualTo(row, 0, 8, nameof(row));
        }
        private static void ValidateColumn(int column)
        {
            Guard.IsBetweenOrEqualTo(column, 0, 8, nameof(column));
        }
        private static void ValidateDigit(int digit)
        {
            if (digit != 0) Guard.IsBetweenOrEqualTo(digit, 1, 9, nameof(digit));
        }
        private static void ValidateCandidate(int candidate)
        {
            Guard.IsBetweenOrEqualTo(candidate, 1, 9, nameof(candidate));
        }

        public bool IsValidInRow(int row, int digit)
        {
            ValidateRow(row);
            ValidateDigit(digit);

            for (int column = 0; column < Size; column++)
            {
                if (Board[row, column].Digit == digit) return false;
            }
            return true;
        }
        public bool IsValidInColumn(int column, int digit)
        {
            ValidateColumn(column);
            ValidateDigit(digit);

            for (int row = 0; row < Size; row++)
            {
                if (Board[row, column].Digit == digit) return false;
            }
            return true;
        }
        public bool IsValidInSubgrid(int row, int column, int digit)
        {
            ValidateRowColumn(row, column);
            ValidateDigit(digit);

            int startRow = row / 3 * 3;
            int startColumn = column / 3 * 3;

            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startColumn; c < startColumn + 3; c++)
                {
                    if (Board[r, c].Digit == digit) return false;
                }
            }
            return true;
        }
        public bool IsValidDigit(int row, int column, int digit)
        {
            return IsValidInRow(row, digit) && IsValidInColumn(column, digit) && IsValidInSubgrid(row, column, digit);
        }
        #endregion
    }
}
