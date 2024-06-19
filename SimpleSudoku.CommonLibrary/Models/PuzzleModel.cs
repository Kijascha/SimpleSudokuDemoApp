using CommunityToolkit.Diagnostics;
using SimpleSudoku.CommonLibrary.System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SimpleSudoku.CommonLibrary.Models;

public class PuzzleModel : IPuzzleModel
{
    public const int Size = 9; // set to 9 for now for a regular 9x9 sudoku grid
    public int?[,] Digits { get; init; }
    public HashSet<int>[,] PlayerCandidates { get; init; }
    public HashSet<int>[,] SolverCandidates { get; init; }

    private static HashSet<(int Row, int Column, int Candidate)> _removedCandidates = [];
    private IEnumerable<CellModel>? cells;
    /// <summary>
    /// Occurs when an attempt to set a digit violates Sudoku rules.
    /// Subscribers can handle this event to provide feedback to the user, such as highlighting the cell with the error.
    /// </summary>
    public event EventHandler<SudokuErrorEventArgs>? SudokuError;
    public event EventHandler<SudokuSuccessEventArgs>? SudokuSuccess;

    public PuzzleModel()
    {
        Digits = new int?[Size, Size];
        PlayerCandidates = new HashSet<int>[Size, Size];
        SolverCandidates = new HashSet<int>[Size, Size];

        InitializeCandidates();
    }
    private readonly object lockObject = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="PuzzleModel"/> class using an observable collection of <see cref="CellModel"/>.
    /// </summary>
    /// <param name="cells">The observable collection of cells representing the puzzle state.</param>
    public PuzzleModel(IEnumerable<CellModel> cells) : this()
    {
        foreach (var cell in cells)
        {
            Digits[cell.Row, cell.Column] = cell.Digit;
            PlayerCandidates[cell.Row, cell.Column] = new HashSet<int>(cell.PlayerCandidates);
            SolverCandidates[cell.Row, cell.Column] = new HashSet<int>(cell.SolverCandidates);
        }
        this.cells = cells;
    }

    /// <summary>
    /// Converts the current state of the puzzle into an observable collection of <see cref="CellModel"/> objects.
    /// </summary>
    /// <returns>
    /// An <see cref="ObservableCollection{T}"/> containing <see cref="CellModel"/> objects that represent the current state of the puzzle grid.
    /// Each <see cref="CellModel"/> object includes the <c>Row</c>, <c>Column</c>, <c>Digit</c>, <c>SolverCandidates</c>, and <c>PlayerCandidates</c> for a cell.
    /// </returns>
    /// <remarks>
    /// This method iterates through the puzzle grid and creates a new <see cref="CellModel"/> object for each cell.
    /// It initializes each <see cref="CellModel"/> with the corresponding <c>Row</c>, <c>Column</c>, <c>Digit</c>, <c>SolverCandidates</c>, and <c>PlayerCandidates</c>
    /// from the puzzle model, and then adds it to the <see cref="ObservableCollection{T}"/>.
    /// </remarks>
    public ObservableCollection<CellModel> ToObservableCollection()
    {
        ObservableCollection<CellModel> collection = [.. this.cells];

        if (this.cells != null)
            foreach (var cell in this.cells)
            {
                cell.Digit = Digits[cell.Row, cell.Column];
                cell.PlayerCandidates = PlayerCandidates[cell.Row, cell.Column];
                cell.SolverCandidates = SolverCandidates[cell.Row, cell.Column];
            }

        return collection;
    }

    /// <summary>
    /// Sets or removes the specified digit in the specified cell.
    /// If validation is enabled, checks if the move is valid according to Sudoku rules.
    /// </summary>
    /// <param name="row">The row number of the cell (0-8).</param>
    /// <param name="column">The column number of the cell (0-8).</param>
    /// <param name="digit">The digit to set in the cell (1-9) or <c>null</c> to clear the cell.</param>
    /// <param name="validate">Indicates whether to validate the move according to Sudoku rules.</param>
    /// <remarks>
    /// When a digit is set, this method clears the solver and player candidates for that cell and backs up the player candidates.
    /// If the digit is removed, the player candidates are restored from the backup, and the solver candidates are reset.
    /// If validation is enabled and the move is invalid, a <see cref="SudokuErrorEventArgs"/> event is raised with details of the conflict.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the row or column is out of the valid range (0-8).</exception>
    /// <exception cref="ArgumentException">Thrown if the digit is not between 1 and 9 (inclusive) or null.</exception>
    public void UpdateDigit(int row, int column, int? digit, bool validate = true)
    {
        lock (lockObject)
        {
            ValidateRowColumn(row, column);
            ValidateDigit(digit);

            (int? digit, int row, int column) previousDigit = (Digits[row, column], row, column);

            var beforeCheck = IsValidDigit(row, column, digit) && Digits[row, column] == null;
            var afterCheck = IsValidDigit(row, column, digit) && Digits[row, column] != null;

            // Check if the same digit is being entered
            if (Digits[row, column] == digit)
            {
                // If the same digit is entered, remove it by setting to null
                Digits[row, column] = null;

                UpdateCandidatesInCurrentCell(row, column, clearCandidates: false);
            }
            else
            {
                // Otherwise, set the new digit
                Digits[row, column] = digit;
                UpdateCandidatesInCurrentCell(row, column, clearCandidates: true);
            }

            if (previousDigit.digit.HasValue)
            {
                // Restore candidates for the previous digit
                UpdateCandidatesInRow(ref _removedCandidates, (row, column, null), previousDigit.digit);
                UpdateCandidatesInColumn(ref _removedCandidates, (row, column, null), previousDigit.digit);
                UpdateCandidatesInBox(ref _removedCandidates, (row, column, null), previousDigit.digit);
            }

            else if (digit.HasValue)
            {
                // Update candidates for the new digit
                UpdateCandidatesInRow(ref _removedCandidates, (row, column, digit), null);
                UpdateCandidatesInColumn(ref _removedCandidates, (row, column, digit), null);
                UpdateCandidatesInBox(ref _removedCandidates, (row, column, digit), null);
            }

            if (validate)
            {

                if (beforeCheck)
                {
                    SudokuSuccess?.Invoke(this, new SudokuSuccessEventArgs(row, column));
                    return;
                }

                if (Digits[row, column] == null && SolverCandidates[row, column].Count() > 0)
                {
                    SudokuSuccess?.Invoke(this, new SudokuSuccessEventArgs(row, column));
                    return;
                }

                if (Digits[row, column] != null && !IsValidDigit(row, column, Digits[row, column]))
                {
                    SudokuError?.Invoke(this, new SudokuErrorEventArgs(row, column));
                    return;
                }
            }
        }
    }

    public void UpdateCandidate(int row, int column, int candidate, bool useSolverCandidates = true)
    {
        lock (lockObject)
        {
            ValidateRowColumn(row, column);
            ValidateCandidate(candidate);

            if (Digits[row, column].HasValue)
            {
                // If a digit is already set, do nothing as candidates should be ignored
                return;
            }

            if (useSolverCandidates)
            {
                // Remove candidate if it exists
                if (!SolverCandidates[row, column].Remove(candidate))
                {
                    // Add candidate if it does not exist
                    SolverCandidates[row, column].Add(candidate);
                }
            }
            else
            {
                // Remove candidate if it exists
                if (!PlayerCandidates[row, column].Remove(candidate))
                {
                    // Add candidate if it does not exist
                    PlayerCandidates[row, column].Add(candidate);
                }
            }
        }
    }

    private void UpdateCandidatesInRow(ref HashSet<(int Row, int Column, int Candidate)> removedCandidates,
        (int row, int col, int? digit) currentCell, int? candidateToRestore)
    {
        var currentRow = GetRow(currentCell.row, false);

        if (currentCell.digit.HasValue)
        {
            foreach (var cell in currentRow)
            {
                if (!Digits[cell.Row, cell.Column].HasValue)
                {
                    SolverCandidates[cell.Row, cell.Column].Remove(currentCell.digit.Value);
                    removedCandidates.Add((cell.Row, cell.Column, currentCell.digit.Value));
                }
            }
        }
        else if (candidateToRestore.HasValue)
        {
            var candidatesToRestore = removedCandidates.Where(c => c.Row == currentCell.row);
            foreach (var candidate in candidatesToRestore)
            {
                if (candidate.Candidate == candidateToRestore && !Digits[candidate.Row, candidate.Column].HasValue)
                {
                    RestoreCandidate(candidate.Row, candidate.Column, candidate.Candidate);
                }
            }
        }
        Debug.WriteLine($"removedCandidates.Count: {_removedCandidates.Count}");
    }

    private void UpdateCandidatesInColumn(ref HashSet<(int Row, int Column, int Candidate)> removedCandidates,
        (int row, int col, int? digit) currentCell, int? candidateToRestore)
    {

        var currentColumn = GetColumn(currentCell.col, false);

        if (currentCell.digit.HasValue)
        {
            foreach (var cell in currentColumn)
            {
                if (!Digits[cell.Row, cell.Column].HasValue)
                {
                    SolverCandidates[cell.Row, cell.Column].Remove(currentCell.digit.Value);
                    removedCandidates.Add((cell.Row, cell.Column, currentCell.digit.Value));
                }
            }
        }
        else if (candidateToRestore.HasValue)
        {
            var candidatesToRestore = removedCandidates.Where(c => c.Column == currentCell.col);
            foreach (var candidate in candidatesToRestore)
            {
                if (candidate.Candidate == candidateToRestore && !Digits[candidate.Row, candidate.Column].HasValue)
                {
                    RestoreCandidate(candidate.Row, candidate.Column, candidate.Candidate);
                }
            }
        }
        Debug.WriteLine($"removedCandidates.Count: {_removedCandidates.Count}");
    }

    private void UpdateCandidatesInBox(ref HashSet<(int Row, int Column, int Candidate)> removedCandidates,
        (int row, int col, int? digit) currentCell, int? candidateToRestore)
    {
        var currentBox = GetBox(currentCell.row, currentCell.col, false);

        if (currentCell.digit.HasValue)
        {
            foreach (var cell in currentBox)
            {
                if (!Digits[cell.Row, cell.Column].HasValue)
                {
                    SolverCandidates[cell.Row, cell.Column].Remove(currentCell.digit.Value);
                    removedCandidates.Add((cell.Row, cell.Column, currentCell.digit.Value));
                }
            }
        }
        else if (candidateToRestore.HasValue)
        {
            foreach (var cell in currentBox)
            {
                var candidateToRemove = removedCandidates.FirstOrDefault(c => c.Row == cell.Row &&
                                                                               c.Column == cell.Column &&
                                                                               c.Candidate == candidateToRestore);

                if (candidateToRemove.Candidate == candidateToRestore && !Digits[cell.Row, cell.Column].HasValue)
                {
                    RestoreCandidate(candidateToRemove.Row, candidateToRemove.Column, candidateToRemove.Candidate);
                }
            }
        }
        Debug.WriteLine($"removedCandidates.Count: {_removedCandidates.Count}");
    }
    private void UpdateCandidatesInCurrentCell(int row, int column, bool clearCandidates)
    {
        if (clearCandidates)
        {
            // Store current candidates in _removedCandidates before clearing
            foreach (var candidate in SolverCandidates[row, column])
            {
                _removedCandidates.Add((row, column, candidate));
            }
            // Clear candidates in the current cell
            SolverCandidates[row, column].Clear();
        }
        else
        {
            // Restore candidates in the current cell from removedCandidates
            var candidatesToRestore = _removedCandidates.Where(c => c.Row == row && c.Column == column);
            foreach (var (Row, Column, Candidate) in candidatesToRestore)
            {
                RestoreCandidate(Row, Column, Candidate);
            }
        }
    }
    private void RestoreCandidate(int row, int column, int candidate)
    {
        if (IsValidDigit(row, column, candidate))
        {
            SolverCandidates[row, column].Add(candidate);
            _removedCandidates.Remove((row, column, candidate));
        }
    }

    /// <summary>
    /// Retrieves the digits and candidate sets for each cell in the specified row.
    /// </summary>
    /// <param name="row">The row index (0-8) of the Sudoku grid.</param>
    /// <param name="usePlayerCandidates">True to include player candidates, false to include solver candidates instead.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ValueTuple{T1, T2, T3, T4}"/> containing the row, column, digit and candidate set for each cell in the row.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the row or column is out of the valid range (0-8).</exception>
    public IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetRow(int row, bool usePlayerCandidates)
    {
        for (int column = 0; column < Size; column++)
        {
            ValidateRowColumn(row, column);
            yield return (row, column, Digits[row, column], usePlayerCandidates ? PlayerCandidates[row, column] : SolverCandidates[row, column]);
        }
    }

    /// <summary>
    /// Retrieves the digits and candidate sets for each cell in the specified column.
    /// </summary>
    /// <param name="col">The column index (0-8) of the Sudoku grid.</param>
    /// <param name="usePlayerCandidates">True to include player candidates, false to include solver candidates instead.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ValueTuple{T1, T2}"/> containing the digit and candidate set for each cell in the column.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the row or column is out of the valid range (0-8).</exception>
    public IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetColumn(int column, bool usePlayerCandidates)
    {
        for (int row = 0; row < Size; row++)
        {
            ValidateRowColumn(row, column);
            yield return (row, column, Digits[row, column], usePlayerCandidates ? PlayerCandidates[row, column] : SolverCandidates[row, column]);
        }
    }

    /// <summary>
    /// Retrieves the digits and candidate sets for each cell in the specified 3x3 subgrid (box).
    /// </summary>
    /// <param name="startRow">The starting row index (0-6) of the subgrid.</param>
    /// <param name="startCol">The starting column index (0-6) of the subgrid.</param>
    /// <param name="usePlayerCandidates">True to include player candidates, false to include solver candidates instead.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ValueTuple{T1, T2}"/> containing the digit and candidate set for each cell in the subgrid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the row or column is out of the valid range (0-8).</exception>
    public IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetBox(int startRow, int startCol, bool usePlayerCandidates)
    {
        // Calculate the top-left cell of the box based on startRow and startCol
        int boxStartRow = (startRow / 3) * 3; // Example: If startRow is 3, boxStartRow should be 3
        int boxStartCol = (startCol / 3) * 3; // Example: If startCol is 6, boxStartCol should be 6

        // Iterate over the cells in the 3x3 box
        for (int row = boxStartRow; row < boxStartRow + 3; row++)
        {
            for (int column = boxStartCol; column < boxStartCol + 3; column++)
            {
                ValidateRowColumn(row, column);
                yield return (row, column, Digits[row, column], usePlayerCandidates ? PlayerCandidates[row, column] : SolverCandidates[row, column]);
            }
        }
    }


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
    private static void ValidateDigit(int? digit)
    {
        // Check if the digit has a value and if it's outside the valid range (1-9)
        if (digit.HasValue)
        {
            Guard.IsBetweenOrEqualTo(digit.Value, 1, 9, nameof(digit));
        }
    }
    private static void ValidateCandidate(int candidate)
    {
        Guard.IsBetweenOrEqualTo(candidate, 1, 9, nameof(candidate));
    }

    public bool IsValidInRow(int row, int? digit)
    {
        ValidateRow(row);
        ValidateDigit(digit);

        for (int column = 0; column < Size; column++)
        {
            if (Digits[row, column] == digit) return false;
        }
        return true;
    }
    public bool IsValidInColumn(int column, int? digit)
    {
        ValidateColumn(column);
        ValidateDigit(digit);

        int currentRow = -1;
        int currentColumn = -1;

        for (int row = 0; row < Size; row++)
        {
            if (Digits[row, column] == digit) return false;
        }
        return true;
    }
    public bool IsValidInSubgrid(int row, int column, int? digit)
    {
        ValidateRowColumn(row, column);
        ValidateDigit(digit);

        int startRow = row / 3 * 3;
        int startColumn = column / 3 * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startColumn; c < startColumn + 3; c++)
            {
                if (Digits[r, c] == digit) return false;
            }
        }
        return true;
    }
    public bool IsValidDigit(int row, int column, int? digit)
    {
        return !IsValidInRow(row, digit) ? false :
                !IsValidInColumn(column, digit) ? false :
                !IsValidInSubgrid(row, column, digit) ? false : true;
    }

    private void InitializeCandidates()
    {

        for (int row = 0; row < Size; row++)
        {
            for (int column = 0; column < Size; column++)
            {
                PlayerCandidates[row, column] = new HashSet<int>(); // player candidates should be empty at start and be set/update by the player
                SolverCandidates[row, column] = new HashSet<int>();
                for (int num = 1; num <= Size; num++)
                {
                    SolverCandidates[row, column].Add(num);
                }
            }
        }
    }
}
