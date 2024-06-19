using SimpleSudoku.CommonLibrary.Models;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class NakedSingleConstraint(IPuzzleModel puzzle) : Constraint
{
    private readonly IPuzzleModel _puzzle = puzzle;

    private bool FindNakedSingles()
    {
        bool foundNakedSingle = false;

        for (int r = 0; r < _puzzle.Digits.GetLength(0); r++)
        {
            for (int c = 0; c < _puzzle.Digits.GetLength(1); c++)
            {
                if (_puzzle.SolverCandidates[r, c].Count == 1)
                {
                    var candidate = _puzzle.SolverCandidates[r, c].Single();
                    _puzzle.UpdateDigit(r, c, candidate, false);
                    _puzzle.SolverCandidates[r, c].Clear();
                    _puzzle.UpdateCandidate(r, c, candidate);
                    foundNakedSingle = true;
                }
            }
        }
        return foundNakedSingle;
    }
    public override bool ApplyConstraint(out string errorMessage)
    {
        errorMessage = "";
        if (!FindNakedSingles())
        {
            errorMessage = "Couldn't find any Naked Singles!";
            return false;
        }
        return true;
    }
}
