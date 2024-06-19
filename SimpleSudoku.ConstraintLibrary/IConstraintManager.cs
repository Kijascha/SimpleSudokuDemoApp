using SimpleSudoku.ConstraintLibrary.Constraints;

namespace SimpleSudoku.ConstraintLibrary
{
    public interface IConstraintManager
    {
        event EventHandler<ConstraintErrorEventArgs>? ConstraintFailed;

        bool AddConstraint(Constraint constraint);
        bool ApplyAllConstraints(out bool anyConstraintApplied);
        bool ApplyAllConstraintsV2(out bool anyConstraintApplied);
        bool ContainsConstraint(Constraint constraint);
        bool RemoveConstraint(Constraint constraint);
    }
}