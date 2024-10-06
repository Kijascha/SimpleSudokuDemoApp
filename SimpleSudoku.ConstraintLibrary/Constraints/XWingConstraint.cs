using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class XWingConstraint(IPuzzleModel puzzle) : Constraint
{
    private readonly IPuzzleModel _puzzle = puzzle;

    public override bool ApplyConstraint(out string errorMessage)
    {
        bool removedSuccessfully = false;
        for (int candidate = 1; candidate <= 9; candidate++)
        {
            removedSuccessfully |= FindXWingInUnit(SearchUnitType.Row, candidate);
            removedSuccessfully |= FindXWingInUnit(SearchUnitType.Column, candidate);
        }

        errorMessage = removedSuccessfully ? "" : "Couldn't find any x-wings";

        return removedSuccessfully;
    }

    private bool FindXWingInUnit(SearchUnitType searchUnitType, int candidate)
    {
        bool removedSuccessfully = false;

        for (int unitA1 = 0; unitA1 < PuzzleModel.Size - 1; unitA1++)
        {
            for (int unitA2 = unitA1 + 1; unitA2 < PuzzleModel.Size; unitA2++)
            {
                // Check if both columns contain the target candidate
                if (ConstraintHelper.CountOccurrencesInUnit(_puzzle, searchUnitType, unitA1, candidate) == 2 && ConstraintHelper.CountOccurrencesInUnit(_puzzle, searchUnitType, unitA2, candidate) == 2)
                {
                    for (int unitB1 = 0; unitB1 < PuzzleModel.Size - 1; unitB1++)
                    {
                        for (int unitB2 = unitB1 + 1; unitB2 < PuzzleModel.Size; unitB2++)
                        {
                            switch (searchUnitType)
                            {
                                case SearchUnitType.Row:
                                    // Check if both cells in each column contain the target candidate
                                    if (_puzzle.SolverCandidates[unitA1, unitB1].Contains(candidate) && _puzzle.SolverCandidates[unitA2, unitB1].Contains(candidate) &&
                                        _puzzle.SolverCandidates[unitA1, unitB2].Contains(candidate) && _puzzle.SolverCandidates[unitA2, unitB2].Contains(candidate))
                                    {
                                        // Remove the candidate from other cells in the same rows
                                        removedSuccessfully |= RemoveCandidateFromUnit(searchUnitType, candidate, unitB1, unitA1, unitA2);
                                        removedSuccessfully |= RemoveCandidateFromUnit(searchUnitType, candidate, unitB2, unitA1, unitA2);
                                    }
                                    break;
                                case SearchUnitType.Column:
                                    // Check if both cells in each column contain the target candidate
                                    if (_puzzle.SolverCandidates[unitB1, unitA1].Contains(candidate) && _puzzle.SolverCandidates[unitB2, unitA1].Contains(candidate) &&
                                        _puzzle.SolverCandidates[unitB1, unitA2].Contains(candidate) && _puzzle.SolverCandidates[unitB2, unitA2].Contains(candidate))
                                    {
                                        // Remove the candidate from other cells in the same rows
                                        removedSuccessfully |= RemoveCandidateFromUnit(searchUnitType, candidate, unitB1, unitA1, unitA2);
                                        removedSuccessfully |= RemoveCandidateFromUnit(searchUnitType, candidate, unitB2, unitA1, unitA2);
                                    }
                                    break;
                                default:
                                    throw new NotSupportedException("This SearchUnitType is currently not supported!");
                            }
                        }
                    }
                }
            }
        }
        return removedSuccessfully;
    }

    private bool RemoveCandidateFromUnit(SearchUnitType searchUnitType, int candidate, int unitA, int unitB1, int unitB2)
    {
        bool removedSuccessfully = false;

        for (int unit = 0; unit < PuzzleModel.Size; unit++)
        {
            switch (searchUnitType)
            {
                case SearchUnitType.Row:
                    if (unit != unitB1 && unit != unitB2 && _puzzle.SolverCandidates[unit, unitA].Contains(candidate))
                    {
                        _puzzle.SolverCandidates[unit, unitA].Remove(candidate);
                        removedSuccessfully = true;
                    }
                    break;
                case SearchUnitType.Column:
                    if (unit != unitB1 && unit != unitB2 && _puzzle.SolverCandidates[unitA, unit].Contains(candidate))
                    {
                        _puzzle.SolverCandidates[unitA, unit].Remove(candidate);
                        removedSuccessfully = true;
                    }
                    break;
                default:
                    throw new NotSupportedException("This SearchUnitType is currently not supported!");
            }
        }
        return removedSuccessfully;
    }
}
