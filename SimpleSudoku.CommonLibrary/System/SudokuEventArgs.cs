namespace SimpleSudoku.CommonLibrary.System;

/// <summary>
/// Provides data for the <see cref="PuzzleModel.SudokuError"/> event.
/// </summary>
public class SudokuErrorEventArgs : EventArgs
{
    public (int Row, int Column) ConflictingCell { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SudokuSuccessEventArgs"/> class.
    /// </summary>
    /// <param name="conflictingCell">Theconflicting cell in a regular sudoku.</param>
    public SudokuErrorEventArgs(int row, int column)
    {
        ConflictingCell = (row, column);
    }
}
