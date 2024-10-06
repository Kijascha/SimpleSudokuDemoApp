using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class NakedPairConstraint(IPuzzleModel puzzle) : Constraint
{
    private readonly IPuzzleModel _puzzle = puzzle;

    public override bool ApplyConstraint(out string errorMessage)
    {
        errorMessage = "";

        if (!FindNakedPair())
        {
            errorMessage = "Couldn't find any Naked Pairs!";
            return false;
        }
        return true;
    }

    private bool FindNakedPair()
    {
        for (int row = 0; row < PuzzleModel.Size; row++)
        {
            for (int col = 0; col < PuzzleModel.Size; col++)
            {
                if (FindNakedPairsInUnit(row, col, SearchUnitType.Box) ||
                    FindNakedPairsInUnit(row, col, SearchUnitType.Row) ||
                    FindNakedPairsInUnit(row, col, SearchUnitType.Column))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool FindNakedPairsInUnit(int row, int col, SearchUnitType searchUnitType)
    {
        // get matching candidate pairs within a 3x3 sudoku box
        var matchingCells = ConstraintHelper.GetMatchingPairsInUnit(_puzzle, row, col, searchUnitType)
            .Where(c => c.Candidates.Count == 2);

        // handling the elimination of candidates of other cells within the box 
        if (matchingCells.Any())
        {
            foreach (var matchingPair in matchingCells)
            {
                var overlappingCells = _puzzle.GetUnit(row, col, searchUnitType)
                    .Where(cell => cell.Digit == null &&
                                                                                cell.Candidates.Overlaps(matchingPair.Candidates) &&
                                                                                !cell.Candidates.SetEquals(matchingPair.Candidates));

                if (overlappingCells.Any())
                {
                    foreach (var overlappingCell in overlappingCells)
                    {
                        overlappingCell.Candidates.RemoveWhere(x => matchingPair.Candidates.Contains(x));
                    }
                    return true;
                }
            }
        }

        return false;
    }
}
