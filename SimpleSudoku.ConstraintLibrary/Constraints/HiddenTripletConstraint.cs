using SimpleSudoku.CommonLibrary.Models;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenTripletConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;

        public override bool ApplyConstraint(out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
