namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public interface IConstraint
    {
        bool ApplyConstraint(out string errorMessage);
    }
}