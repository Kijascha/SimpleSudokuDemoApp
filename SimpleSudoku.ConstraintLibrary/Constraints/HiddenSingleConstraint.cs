using SimpleSudoku.CommonLibrary.Models;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenSingleConstraint : Constraint
    {
        private readonly IPuzzleModel _puzzle;

        public HiddenSingleConstraint(IPuzzleModel puzzle)
        {
            _puzzle = puzzle;
        }
        public override bool ApplyConstraint(out string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}
