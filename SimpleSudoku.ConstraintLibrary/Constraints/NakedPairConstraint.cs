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
        // Get cells with exactly two candidates
        var matchingCells = ConstraintHelper.GetMatchingPairsInUnit(_puzzle, row, col, searchUnitType)
            .Where(c => CountBits(c.SolverCandidates.BitMask) == 2);

        //Debug.WriteLine($"Checking for Naked Pairs in {searchUnitType} at ({row}, {col})");

        foreach (var matchingPair in matchingCells)
        {
            int nakedPairMask = matchingPair.SolverCandidates.BitMask;
            //Debug.WriteLine($"Found Naked Pair with mask {Convert.ToString(nakedPairMask, 2).PadLeft(9, '0')} at ({matchingPair.Row}, {matchingPair.Column})");

            // Log all cells in the current unit
            var allCellsInUnit = _puzzle.GetUnit(row, col, searchUnitType);

            // Identify overlapping cells in the unit
            var overlappingCells = allCellsInUnit
                .Where(cell => cell.Digit == 0 &&
                               (cell.SolverCandidates.BitMask & nakedPairMask) != 0 &&
                               cell.SolverCandidates.BitMask != nakedPairMask);

            bool changed = false;

            // Attempt to eliminate naked pair candidates
            foreach (var overlappingCell in overlappingCells)
            {
                int originalMask = overlappingCell.SolverCandidates.BitMask;
                int newCandidates = originalMask & ~nakedPairMask;

                if (newCandidates != originalMask)
                {
                    overlappingCell.SolverCandidates = new Candidates(newCandidates); // Update cell's candidates
                    changed = true;

                }
            }

            if (changed)
            {
                return true;
            }
        }

        return false;
    }

    // Helper method to count set bits (candidates) in a bitmask
    private int CountBits(int bitmask)
    {
        int count = 0;
        while (bitmask != 0)
        {
            count += bitmask & 1;
            bitmask >>= 1;
        }
        return count;
    }
}
