using SimpleSudoku.CommonLibrary.System;
using System.Collections.ObjectModel;

namespace SimpleSudoku.CommonLibrary.Models;

public class PuzzleModel
{
    public const int Size = 9; // set to 9 for now for a regular 9x9 sudoku grid
    public int?[,] Digits { get; init; }
    public HashSet<int>[,] PlayerCandidates { get; init; }
    public HashSet<int>[,] SolverCandidates { get; init; }
    private HashSet<(int row, int column, HashSet<int> candidates)> PlayerCandidatesBackup { get; set; }

    /// <summary>
    /// Occurs when an attempt to set a digit violates Sudoku rules.
    /// Subscribers can handle this event to provide feedback to the user, such as highlighting the cell with the error.
    /// </summary>
    public event EventHandler<SudokuErrorEventArgs>? SudokuError;

    public PuzzleModel()
    {
        Digits = new int?[Size, Size];
        PlayerCandidates = new HashSet<int>[Size, Size];
        SolverCandidates = new HashSet<int>[Size, Size];
        PlayerCandidatesBackup = new HashSet<(int, int, HashSet<int>)>();

        InitializeCandidates();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PuzzleModel"/> class using an observable collection of <see cref="CellModel"/>.
    /// </summary>
    /// <param name="cells">The observable collection of cells representing the puzzle state.</param>
    public PuzzleModel(ObservableCollection<CellModel> cells) : this()
    {
        foreach (var cell in cells)
        {
            Digits[cell.Row, cell.Column] = cell.Digit;
            PlayerCandidates[cell.Row, cell.Column] = new HashSet<int>(cell.PlayerCandidates);
            SolverCandidates[cell.Row, cell.Column] = new HashSet<int>(cell.SolverCandidates);
        }
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
        var collection = new ObservableCollection<CellModel>();

        for (int row = 0; row < Size; row++)
        {
            for (int column = 0; column < Size; column++)
            {
                collection.Add(new CellModel
                {
                    Row = row,
                    Column = column,
                    Digit = Digits[row, column],
                    SolverCandidates = SolverCandidates[row, column],
                    PlayerCandidates = PlayerCandidates[row, column]
                });
            }
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
        ValidateRowColumn(row, column);
        ValidateDigit(digit);

        if (validate && digit.HasValue)
        {
            var validationResult = IsValidDigit(row, column, digit.Value);
            if (!validationResult.isValid)
            {
                SudokuError?.Invoke(this, new SudokuErrorEventArgs(row, column, validationResult.conflictingRow, validationResult.conflictingColumn));
                return;
            }
        }

        if (digit.HasValue)
        {
            // Backup player candidates before clearing
            PlayerCandidatesBackup.Add((row, column, new HashSet<int>(PlayerCandidates[row, column])));
            // Clear candidates for this cell if a digit is set
            SolverCandidates[row, column].Clear();
            PlayerCandidates[row, column].Clear();
        }
        else
        {
            // Restore player candidates when digit is removed
            var backup = PlayerCandidatesBackup.FirstOrDefault(b => b.row == row && b.column == column);
            if (backup != default)
            {
                PlayerCandidates[row, column] = new HashSet<int>(backup.candidates);
                PlayerCandidatesBackup.Remove(backup);
            }
            // Reset solver candidates for this cell
            SolverCandidates[row, column].Clear();
            for (int num = 1; num <= Size; num++)
            {
                SolverCandidates[row, column].Add(num);
            }
        }

        Digits[row, column] = digit;
    }

    /// <summary>
    /// Sets or removes the specified candidate in the solver's candidate grid.
    /// </summary>
    /// <param name="row">The row number of the cell (0-8).</param>
    /// <param name="column">The column number of the cell (0-8).</param>
    /// <param name="candidate">The candidate value to set in the cell (1-9).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the row or column is out of the valid range (0-8).</exception>
    /// <exception cref="ArgumentException">Thrown if the candidate is not between 1 and 9 (inclusive).</exception>
    /// <remarks>
    /// If a digit is already set in the specified cell, this method does nothing as candidates should be ignored.
    /// If the candidate already exists in the cell's candidate list, it is removed; otherwise, it is added.
    /// </remarks>
    public void UpdateSolverCandidate(int row, int column, int candidate)
    {
        ValidateRowColumn(row, column);
        ValidateCandidate(candidate);

        if (Digits[row, column].HasValue)
        {
            // If a digit is already set, do nothing as candidates should be ignored
            return;
        }

        if (SolverCandidates[row, column].Contains(candidate))
        {
            // Remove candidate if it exists
            SolverCandidates[row, column].Remove(candidate);
        }
        else
        {
            // Add candidate if it does not exist
            SolverCandidates[row, column].Add(candidate);
        }
    }

    /// <summary>
    /// Adds or removes a candidate in the player's candidate grid for the specified cell.
    /// </summary>
    /// <param name="row">The row number of the cell (0-8).</param>
    /// <param name="column">The column number of the cell (0-8).</param>
    /// <param name="candidate">The candidate value to add or remove (1-9).</param>
    /// <remarks>
    /// If a digit is already set in the specified cell, this method does nothing as candidates should be ignored.
    /// If the candidate already exists in the cell's candidate list, it is removed; otherwise, it is added.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the row or column is out of the valid range (0-8).</exception>
    /// <exception cref="ArgumentException">Thrown if the candidate is not between 1 and 9 (inclusive).</exception>
    public void UpdatePlayerCandidate(int row, int column, int candidate)
    {
        ValidateRowColumn(row, column);
        ValidateCandidate(candidate);

        if (Digits[row, column].HasValue)
        {
            // If a digit is already set, do nothing as candidates should be ignored
            return;
        }

        if (PlayerCandidates[row, column].Contains(candidate))
        {
            // Remove candidate if it exists
            PlayerCandidates[row, column].Remove(candidate);
        }
        else
        {
            // Add candidate if it does not exist
            PlayerCandidates[row, column].Add(candidate);
        }
    }

    private void ValidateRowColumn(int row, int column)
    {
        if (row < 0 || row >= Size || column < 0 || column >= Size)
        {
            throw new ArgumentOutOfRangeException(nameof(row), "Row and column must be between 0 and 8.");
        }
    }
    private void ValidateDigit(int? digit)
    {
        // Check if the digit has a value and if it's outside the valid range (1-9)
        if (digit.HasValue && (digit < 1 || digit > Size))
        {
            throw new ArgumentOutOfRangeException(nameof(digit), "Digit must be between 1 and 9.");
        }
    }
    private void ValidateCandidate(int candidate)
    {
        if (candidate < 1 || candidate > Size)
        {
            throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");
        }
    }

    private (bool isValid, int conflictingRow, int conflictingColumn) IsValidInRow(int row, int digit)
    {
        for (int column = 0; column < Size; column++)
        {
            if (Digits[row, column] == digit)
            {
                return (false, row, column);
            }
        }
        return (true, -1, -1);
    }
    private (bool isValid, int conflictingRow, int conflictingColumn) IsValidInColumn(int column, int digit)
    {
        for (int row = 0; row < Size; row++)
        {
            if (Digits[row, column] == digit)
            {
                return (false, row, column);
            }
        }
        return (true, -1, -1);
    }
    private (bool isValid, int conflictingRow, int conflictingColumn) IsValidInSubgrid(int row, int column, int digit)
    {
        int startRow = row / 3 * 3;
        int startColumn = column / 3 * 3;
        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startColumn; c < startColumn + 3; c++)
            {
                if (Digits[r, c] == digit)
                {
                    return (false, r, c);
                }
            }
        }
        return (true, -1, -1);
    }
    private (bool isValid, int conflictingRow, int conflictingColumn) IsValidDigit(int row, int column, int digit)
    {
        var rowCheck = IsValidInRow(row, digit);
        if (!rowCheck.isValid) return rowCheck;

        var columnCheck = IsValidInColumn(column, digit);
        if (!columnCheck.isValid) return columnCheck;

        var subgridCheck = IsValidInSubgrid(row, column, digit);
        if (!subgridCheck.isValid) return subgridCheck;

        return (true, -1, -1);
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
