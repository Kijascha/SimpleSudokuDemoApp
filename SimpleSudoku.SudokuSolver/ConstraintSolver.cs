using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.ConstraintLibrary;
using SimpleSudoku.ConstraintLibrary.Constraints;

namespace SimpleSudoku.SudokuSolver;

public class ConstraintSolver(IConstraintManager constraintManager, IPuzzleModel puzzleModel) : IConstraintSolver
{
    private readonly IConstraintManager _constraintManager = constraintManager;
    private readonly IPuzzleModel _puzzleModel = puzzleModel;

    public void InitializeConstraints()
    {
        _constraintManager.AddConstraint(new NakedSingleConstraint(_puzzleModel));
    }
    public bool Solve()
    {
        bool anyConstraints;
        do
        {
            // execute and loop constraints until the puzzle is correctly solved
            _constraintManager.ApplyAllConstraints(out anyConstraints);

        } while (anyConstraints);
        return true;
    }
}
