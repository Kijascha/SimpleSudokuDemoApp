namespace SimpleSudoku.CommonLibrary.System;

/// <summary>
/// Provides data for the <see cref="PuzzleModel.SudokuError"/> event.
/// </summary>
public class SudokuErrorEventArgs : EventArgs
{
    /// <summary>
    /// The row number of the cell that caused the error.
    /// </summary>
    public int Row { get; }

    /// <summary>
    /// The column number of the cell that caused the error.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// The row number of the conflicting cell.
    /// </summary>
    public int ConflictingRow { get; }

    /// <summary>
    /// The column number of the conflicting cell.
    /// </summary>
    public int ConflictingColumn { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SudokuErrorEventArgs"/> class.
    /// </summary>
    /// <param name="row">The row number of the cell that caused the error.</param>
    /// <param name="column">The column number of the cell that caused the error.</param>
    /// <param name="conflictingRow">The row number of the conflicting cell.</param>
    /// <param name="conflictingColumn">The column number of the conflicting cell.</param>
    public SudokuErrorEventArgs(int row, int column, int conflictingRow, int conflictingColumn)
    {
        Row = row;
        Column = column;
        ConflictingRow = conflictingRow;
        ConflictingColumn = conflictingColumn;
    }
}
