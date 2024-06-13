using SimpleSudoku.ConstraintLibrary.Constraints;
using System.Collections.Concurrent;

namespace SimpleSudoku.ConstraintLibrary;

/// <summary>
/// Manages a collection of constraints and applies them iteratively.
/// </summary>
public class ConstraintManager
{
    private readonly ConcurrentDictionary<Constraint, Func<(bool, string?)>> _constraintCollection;

    /// <summary>
    /// Initializes a new instance of the ConstraintManager class.
    /// </summary>
    public ConstraintManager()
    {
        _constraintCollection = new ConcurrentDictionary<Constraint, Func<(bool, string?)>>();
    }

    /// <summary>
    /// Event raised when a constraint fails during application.
    /// </summary>
    public event EventHandler<ConstraintErrorEventArgs>? ConstraintFailed;

    /// <summary>
    /// Adds a constraint to the ConstraintManager.
    /// </summary>
    /// <param name="constraint">The constraint to add.</param>
    /// <returns>True if the constraint was successfully added; otherwise, false.</returns>
    public bool AddConstraint(Constraint constraint)
    {
        return _constraintCollection.TryAdd(constraint, () => constraint.ApplyConstraint(out string errorMessage) ? (true, null) : (false, errorMessage));
    }

    /// <summary>
    /// Removes a constraint from the ConstraintManager.
    /// </summary>
    /// <param name="constraint">The constraint to remove.</param>
    /// <returns>True if the constraint was successfully removed; otherwise, false.</returns>
    public bool RemoveConstraint(Constraint constraint)
    {
        return _constraintCollection.TryRemove(constraint, out _);
    }

    /// <summary>
    /// Checks if a constraint is already present in the ConstraintManager.
    /// </summary>
    /// <param name="constraint">The constraint to check.</param>
    /// <returns>True if the constraint is present; otherwise, false.</returns>
    public bool ContainsConstraint(Constraint constraint)
    {
        return _constraintCollection.ContainsKey(constraint);
    }

    /// <summary>
    /// Applies all constraints stored in the ConstraintManager.
    /// </summary>
    /// <param name="anyConstraintApplied">True if at least one constraint was successfully applied; otherwise, false.</param>
    /// <returns>True if all constraints were successfully applied; otherwise, false.</returns>
    public bool ApplyAllConstraints(out bool anyConstraintApplied)
    {
        anyConstraintApplied = false;
        bool allConstraintsApplied = true;

        foreach (var kvp in _constraintCollection)
        {
            var (constraint, applyFunc) = kvp;

            var (success, errorMessage) = applyFunc();
            if (!success)
            {
                allConstraintsApplied = false;

                // Perform null check on errorMessage
                if (errorMessage != null)
                {
                    OnConstraintFailed(new ConstraintErrorEventArgs(constraint, errorMessage));
                }
                else
                {
                    OnConstraintFailed(new ConstraintErrorEventArgs(constraint, "Unknown error occurred."));
                }
            }
            else
            {
                anyConstraintApplied = true;
            }
        }

        return allConstraintsApplied;
    }

    /// <summary>
    /// Raises the <see cref="ConstraintFailed"/> event.
    /// </summary>
    /// <param name="e">Event arguments containing details about the failed constraint.</param>
    protected virtual void OnConstraintFailed(ConstraintErrorEventArgs e)
    {
        ConstraintFailed?.Invoke(this, e);
    }
}
