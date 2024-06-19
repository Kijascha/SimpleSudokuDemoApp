namespace SimpleSudoku.CommonLibrary.System
{
    public class SudokuSuccessEventArgs : EventArgs
    {
        public (int Row, int Column) ConflictingCell { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SudokuSuccessEventArgs"/> class.
        /// </summary>
        /// <param name="conflictingCell">Theconflicting cell in a regular sudoku.</param>
        public SudokuSuccessEventArgs(int row, int column)
        {
            ConflictingCell = (row, column);
        }
    }
}
