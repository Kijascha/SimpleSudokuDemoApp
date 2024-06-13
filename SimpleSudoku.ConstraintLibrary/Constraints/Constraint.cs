namespace SimpleSudoku.ConstraintLibrary.Constraints;

/// <summary>
/// Represents a base class for constraints.
/// </summary>
public abstract class Constraint : IConstraint
{
    /// <summary>
    /// Applies the constraint and returns whether it succeeded.
    /// </summary>
    /// <param name="errorMessage">When this method returns false, contains an error message describing why the constraint failed; otherwise, null.</param>
    /// <returns>True if the constraint was successfully applied; otherwise, false.</returns>
    public abstract bool ApplyConstraint(out string errorMessage);
}
