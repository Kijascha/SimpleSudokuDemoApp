using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class NakedQuadConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;
        public static HashSet<((int Row, int Column, int CandidateMask), int QuadMask)> HandledQuads = new();

        public override bool ApplyConstraint(out string errorMessage)
        {
            var foundQuad = FindNakedQuad();
            errorMessage = foundQuad ? "" : "Couldn't find any Naked Quads!";
            return false;
        }

        private bool FindNakedQuad()
        {
            bool foundQuad = false;
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    //FindHiddenTripletsInCol(row, col);
                    foundQuad |= FindQuadInUnit(row, col, SearchUnitType.Row);
                    foundQuad |= FindQuadInUnit(row, col, SearchUnitType.Column);
                    foundQuad |= FindQuadInUnit(row, col, SearchUnitType.Box);
                    // Break the loop if a triplet is found and processed
                    if (foundQuad)
                    {
                        return true;
                    }
                }
            }
            return foundQuad;
        }


        private bool FindQuadInUnit(int row, int col, SearchUnitType searchUnitType)
        {
            var unitCells = _puzzle.GetUnit(row, col, searchUnitType).ToList();

            for (int i1 = 0; i1 < PuzzleModel.Size - 3; i1++)
            {
                for (int i2 = i1 + 1; i2 < PuzzleModel.Size - 2; i2++)
                {
                    for (int i3 = i2 + 1; i3 < PuzzleModel.Size - 1; i3++)
                    {
                        for (int i4 = i3 + 1; i4 < PuzzleModel.Size; i4++)
                        {
                            // Get bitmask for each candidate set
                            int candidates1 = unitCells[i1].SolverCandidates.BitMask;
                            int candidates2 = unitCells[i2].SolverCandidates.BitMask;
                            int candidates3 = unitCells[i3].SolverCandidates.BitMask;
                            int candidates4 = unitCells[i4].SolverCandidates.BitMask;

                            if (candidates1 == 0 || candidates2 == 0 || candidates3 == 0 || candidates4 == 0) continue;

                            // Combine candidates using bitwise OR
                            int combinedCandidates = candidates1 | candidates2 | candidates3 | candidates4;

                            // Check if the combined candidates result in exactly 4 unique candidates
                            if (CountBits(combinedCandidates) == 4)
                            {
                                var unsolvedCells = unitCells
                                    .Where(c => c != unitCells[i1] && c != unitCells[i2] && c != unitCells[i3] && c != unitCells[i4] && c.Digit == 0)
                                    .ToList();

                                if (unsolvedCells.Count > 0)
                                {
                                    bool anyChanges = false;

                                    foreach (var cell in unsolvedCells)
                                    {
                                        int cellCandidates = cell.SolverCandidates.BitMask;
                                        int relevantCandidates = cellCandidates & combinedCandidates;

                                        if (relevantCandidates != 0 && !HandledQuads.Contains(((cell.Row, cell.Column, cellCandidates), combinedCandidates)))
                                        {
                                            // Remove the quad candidates from this cell's candidate set
                                            int newCandidates = cellCandidates & ~combinedCandidates;

                                            if (newCandidates != cellCandidates)
                                            {
                                                cell.SolverCandidates.FromBitMask(newCandidates);
                                                HandledQuads.Add(((cell.Row, cell.Column, cellCandidates), combinedCandidates));
                                                anyChanges = true;
                                            }
                                        }
                                    }

                                    if (anyChanges)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        // If no changes were made, log this quad to avoid reprocessing
                                        foreach (var cell in unsolvedCells)
                                        {
                                            int cellCandidates = cell.SolverCandidates.BitMask;
                                            HandledQuads.Add(((cell.Row, cell.Column, cellCandidates), combinedCandidates));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        // Utility function to count bits in an integer (i.e., number of candidates)
        private int CountBits(int n)
        {
            int count = 0;
            while (n != 0)
            {
                n &= (n - 1);  // Clear the least significant bit
                count++;
            }
            return count;
        }
    }
}
