namespace SimpleSudoku.CommonLibrary.System;

/// <summary>
/// Provides data for the <see cref="PuzzleModel.SudokuError"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SudokuSuccessEventArgs"/> class.
/// </remarks>
/// <param name="conflictingCell">Theconflicting cell in a regular sudoku.</param>
public class SudokuErrorEventArgs(int row, int column) : EventArgs
{
    public (int Row, int Column) ConflictingCell { get; } = (row, column);
}
