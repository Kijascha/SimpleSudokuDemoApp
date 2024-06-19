namespace SimpleSudoku.SudokuSolver
{
    public interface IConstraintSolver
    {
        void InitializeConstraints();
        bool Solve();
    }
}