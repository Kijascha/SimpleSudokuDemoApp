using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.SudokuSolver;

namespace SimpleSudokuDemo.Services
{
    public interface IGameService
    {
        IBacktrackSolver BacktrackSolver { get; }
        IConstraintSolver ConstraintSolver { get; }
        IPuzzleModel CreatePuzzle(int numToRemove);
    }
}