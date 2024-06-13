using SimpleSudoku.ConstraintLibrary.Constraints;

namespace SimpleSudoku.ConstraintLibrary;

public class ConstraintErrorEventArgs(IConstraint constraint, string errorMessage) : EventArgs
{
    public IConstraint Constraint { get; } = constraint;
    public string ErrorMessage { get; } = errorMessage;
}
