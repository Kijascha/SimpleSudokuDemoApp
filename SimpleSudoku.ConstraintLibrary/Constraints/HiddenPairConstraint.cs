using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenPairConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;
        public static HashSet<((int Row, int Column, HashSet<int> Candidates), HashSet<int> Pair)> HandledPairs = new();

        public override bool ApplyConstraint(out string errorMessage)
        {
            var foundPair = FindHiddenPair();
            errorMessage = foundPair ? "" : "Couldn't find any Hidden Pairs!";
            return foundPair;
        }

        private bool FindHiddenPair()
        {
            bool foundPair = false;
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    foundPair |= FindPairInUnit(row, col, SearchUnitType.Row);
                    foundPair |= FindPairInUnit(row, col, SearchUnitType.Column);
                    foundPair |= FindPairInUnit(row, col, SearchUnitType.Box);

                    // Break the loop if a pair is found and processed
                    if (foundPair)
                    {
                        return true;
                    }
                }
            }
            return foundPair;
        }

        private bool FindPairInUnit(int row, int col, SearchUnitType searchUnitType)
        {
            var unitCells = _puzzle.GetUnit(row, col, searchUnitType).ToList();

            // Loop through all possible candidates (usually 1 to 9 in Sudoku)
            for (int candidate1 = 1; candidate1 <= PuzzleModel.Size; candidate1++)
            {
                // Count occurrences of candidate1 in the unit
                int occurrences1 = ConstraintHelper.CountOccurrencesInUnit(_puzzle, searchUnitType, row, col, candidate1);

                // Only proceed if candidate1 appears exactly twice in the unit
                if (occurrences1 == 2)
                {
                    for (int candidate2 = candidate1 + 1; candidate2 <= PuzzleModel.Size; candidate2++)
                    {
                        // Count occurrences of candidate2 in the unit
                        int occurrences2 = ConstraintHelper.CountOccurrencesInUnit(_puzzle, searchUnitType, row, col, candidate2);

                        // Only proceed if candidate2 also appears exactly twice
                        if (occurrences2 == 2)
                        {
                            // Get the cells where candidate1 and candidate2 both appear
                            var candidateCells = unitCells
                                .Where(cell => cell.SolverCandidates.Contains(candidate1) && cell.SolverCandidates.Contains(candidate2))
                                .ToList();

                            // If exactly two cells contain both candidate1 and candidate2, it's a hidden pair
                            if (candidateCells.Count == 2)
                            {
                                // Process hidden pair: eliminate other candidates from these two cells
                                bool anyChanges = false;
                                foreach (var cell in candidateCells)
                                {
                                    var cellCandidates = _puzzle.Board[cell.Row, cell.Column].SolverCandidates.Collection.ToHashSet();
                                    var otherCandidates = cellCandidates.Except(new[] { candidate1, candidate2 }).ToHashSet();

                                    if (otherCandidates.Count > 0)
                                    {
                                        cellCandidates.ExceptWith(otherCandidates);
                                        HandledPairs.Add(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), new HashSet<int> { candidate1, candidate2 }));
                                        anyChanges = true;
                                    }
                                }

                                if (anyChanges)
                                {
                                    return true;
                                }
                                else
                                {
                                    // If no changes were made, log this pair to avoid reprocessing
                                    foreach (var cell in candidateCells)
                                    {
                                        var cellCandidates = _puzzle.Board[cell.Row, cell.Column].SolverCandidates.Collection;
                                        HandledPairs.Add(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), new HashSet<int> { candidate1, candidate2 }));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
