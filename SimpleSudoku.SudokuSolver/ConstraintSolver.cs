using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.ConstraintLibrary;
using SimpleSudoku.ConstraintLibrary.Constraints;
using System.Diagnostics;

namespace SimpleSudoku.SudokuSolver;

public class ConstraintSolver(IConstraintManager constraintManager, IPuzzleModel puzzleModel) : IConstraintSolver
{
    private readonly IConstraintManager _constraintManager = constraintManager;
    private readonly IPuzzleModel _puzzleModel = puzzleModel;

    public void InitializeConstraints()
    {
        _constraintManager.AddConstraint(new NakedSingleConstraint(_puzzleModel));
        _constraintManager.AddConstraint(new HiddenSingleConstraint(_puzzleModel));
        _constraintManager.AddConstraint(new NakedPairConstraint(_puzzleModel));


        _constraintManager.ConstraintFailed += ConstraintManager_ConstraintFailed;
    }

    private void ConstraintManager_ConstraintFailed(object? sender, ConstraintErrorEventArgs e)
    {
        Debug.WriteLine(e.ErrorMessage);
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
