using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using System.Diagnostics;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class NakedSingleConstraint(IPuzzleModel puzzle) : Constraint
{
    private readonly IPuzzleModel _puzzle = puzzle;

    private bool FindNakedSingles()
    {
        bool foundNakedSingle = false;

        for (int r = 0; r < PuzzleModel.Size; r++)
        {
            for (int c = 0; c < PuzzleModel.Size; c++)
            {

                if (_puzzle.Board[r, c].Digit != 0) continue;
                if (_puzzle.Board[r, c].SolverCandidates.Collection.Count == 1)
                {
                    var candidate = _puzzle.Board[r, c].SolverCandidates.Collection.Single();
                    Debug.WriteLine($"Naked Single: {candidate} in ({r}|{c})");
                    _puzzle.UpdateDigit(r, c, candidate, GameMode.Play, CandidateMode.SolverCandidates);
                    _puzzle.Board[r, c].SolverCandidates.Clear();
                    _puzzle.UpdateCandidate(r, c, candidate, GameMode.Play, true, CandidateMode.SolverCandidates);
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
