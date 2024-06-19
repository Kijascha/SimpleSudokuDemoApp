namespace SimpleSudoku.CommonLibrary.System
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SudokuSuccessEventArgs"/> class.
    /// </summary>
    /// <param name="conflictingCell">Theconflicting cell in a regular sudoku.</param>
    public class SudokuSuccessEventArgs(int row, int column) : EventArgs
    {
        public (int Row, int Column) ConflictingCell { get; } = (row, column);
    }
}
