using SimpleSudoku.CommonLibrary.Models;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class NakedSingleConstraint(IPuzzleModel puzzle) : Constraint
{
    private void FindNakedSingles()
    {
    }
    public override bool ApplyConstraint(out string errorMessage)
    {
        throw new NotImplementedException();
    }
}
