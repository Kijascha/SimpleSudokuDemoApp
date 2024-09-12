using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using System.Diagnostics;
using System.Text;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class SkyscraperConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;

        public override bool ApplyConstraint(out string errorMessage)
        {
            StringBuilder debugInfo = new StringBuilder();
            bool foundValidSkyscraper = false;

            // Iterate through all candidates (1-9)
            for (int candidate = 1; candidate <= 9; candidate++)
            {
                debugInfo.AppendLine($"Checking candidate {candidate} for skyscraper pattern...");

                // Find rows where the candidate appears exactly twice
                var rowsWithTwoOccurrences = FindRowsWithExactlyTwoOccurrences(SearchUnitType.Row, candidate, debugInfo);

                // Check for valid skyscraper patterns by comparing columns in those rows
                foundValidSkyscraper |= CheckForSkyscraper(rowsWithTwoOccurrences, candidate, debugInfo);
            }

            errorMessage = foundValidSkyscraper ? "" : "No valid skyscrapers found.";

            // Output the accumulated debug information
            Debug.WriteLine(debugInfo.ToString());

            return foundValidSkyscraper;
        }

        private List<int> FindRowsWithExactlyTwoOccurrences(SearchUnitType searchUnitType, int candidate, StringBuilder debugInfo)
        {
            List<int> rowsWithTwoOccurrences = new List<int>();

            // Loop through each row
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                int occurrenceCount = CountOccurrencesInUnit(searchUnitType, row, candidate);

                // Collect rows where candidate appears exactly twice
                if (occurrenceCount == 2)
                {
                    var rowUnit = _puzzle.GetRow(row, false)
                        .Where(x => x.Candidates.Contains(candidate)).ToList();

                    if (GetBlockIndex(rowUnit[0].Row, rowUnit[0].Column) != GetBlockIndex(rowUnit[1].Row, rowUnit[1].Column))
                    {
                        rowsWithTwoOccurrences.Add(row);
                        debugInfo.AppendLine($"Candidate {candidate} appears exactly twice in row {row}. Count: {rowUnit.Count()}x");
                    }
                }
            }

            return rowsWithTwoOccurrences;
        }

        private bool CheckForSkyscraper(List<int> rowsWithTwoOccurrences, int candidate, StringBuilder debugInfo)
        {
            bool foundSkyscraper = false;

            // Compare each pair of rows where the candidate appears exactly twice
            for (int i = 0; i < rowsWithTwoOccurrences.Count - 1; i++)
            {
                for (int j = i + 1; j < rowsWithTwoOccurrences.Count; j++)
                {
                    var row1 = _puzzle.GetRow(rowsWithTwoOccurrences[i], false)
                        .Where(x => x.Candidates.Contains(candidate));
                    var row2 = _puzzle.GetRow(rowsWithTwoOccurrences[j], false)
                        .Where(x => x.Candidates.Contains(candidate));

                    for (int col = 0; col < PuzzleModel.Size; col++)
                    {
                        var intersectCol = _puzzle.GetColumn(col, false);

                        var inRow1 = row1.Any(c => c.Column == col && c.Candidates.Contains(candidate));
                        var inRow2 = row2.Any(c => c.Column == col && c.Candidates.Contains(candidate));

                        if (inRow1 && inRow2)
                        {
                            if (GetBlockIndex(rowsWithTwoOccurrences[i], col) != GetBlockIndex(rowsWithTwoOccurrences[j], col))
                            {
                                debugInfo.AppendLine($"\tResult1: Intersection found at cells ({rowsWithTwoOccurrences[i]}, {col}) and ({rowsWithTwoOccurrences[j]}, {col}).");

                                var roofCellInRow1 = row1
                                    .Where(c => c.Column != col && c.Candidates.Contains(candidate));
                                var roofCellInRow2 = row2
                                    .Where(c => c.Column != col && c.Candidates.Contains(candidate));

                                if (roofCellInRow1.Count() == 1 && roofCellInRow2.Count() == 1)
                                {
                                    int roofRow1 = roofCellInRow1.First().Row;
                                    int roofCol1 = roofCellInRow1.First().Column;
                                    int roofRow2 = roofCellInRow2.First().Row;
                                    int roofCol2 = roofCellInRow2.First().Column;

                                    debugInfo.AppendLine($"\t\tRoof Cell 1 found at row {roofRow1}, column {roofCol1}.");
                                    debugInfo.AppendLine($"\t\tRoof Cell 2 found at row {roofRow2}, column {roofCol2}.");

                                    // Check if the roof cells are in neighboring blocks
                                    if (AreBlocksAdjacent((rowsWithTwoOccurrences[i], col), (rowsWithTwoOccurrences[j], col), (roofRow1, roofCol1), (roofRow2, roofCol2)))
                                    {
                                        foundSkyscraper = true;
                                        debugInfo.AppendLine($"\t\tValid Skyscraper: Roof cells are in neighboring blocks.");

                                        // Find cells that see both roof cells and remove the candidate
                                        RemoveCandidateFromCommonVisibleCells(roofRow1, roofCol1, roofRow2, roofCol2, candidate, debugInfo);

                                    }
                                }
                            }
                        }
                    }
                }
            }

            return foundSkyscraper;
        }

        // Function to remove the candidate from cells that see both roof cells
        private void RemoveCandidateFromCommonVisibleCells(int roofRow1, int roofCol1, int roofRow2, int roofCol2, int candidate, StringBuilder debugInfo)
        {
            // Iterate through all cells in the puzzle
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    // Check if the current cell "sees" both roof cells
                    if (SeesBothRoofCells(row, col, roofRow1, roofCol1, roofRow2, roofCol2))
                    {
                        // If the cell contains the candidate, remove it
                        if (_puzzle.SolverCandidates[row, col].Contains(candidate))
                        {
                            _puzzle.SolverCandidates[row, col].Remove(candidate);
                            debugInfo.AppendLine($"\tRemoved candidate {candidate} from cell ({row}, {col}) as it sees both roof cells.");
                        }
                    }
                }
            }
        }

        // Helper function to check if a cell "sees" both roof cells
        private bool SeesBothRoofCells(int row, int col, int roofRow1, int roofCol1, int roofRow2, int roofCol2)
        {
            // A cell "sees" another cell if it's in the same row, column, or block
            return IsInSameUnit(row, col, roofRow1, roofCol1) && IsInSameUnit(row, col, roofRow2, roofCol2);
        }
        // Helper function to check if two blocks are adjacent
        private bool AreBlocksAdjacent((int row, int col) baseCell1, (int row, int col) baseCell2, (int row, int col) roofCell1, (int row, int col) roofCell2)
        {
            // Get the block indices for each of the cells
            int baseBlock1 = GetBlockIndex(baseCell1.row, baseCell1.col);
            int baseBlock2 = GetBlockIndex(baseCell2.row, baseCell2.col);
            int roofBlock1 = GetBlockIndex(roofCell1.row, roofCell1.col);
            int roofBlock2 = GetBlockIndex(roofCell2.row, roofCell2.col);

            // Extract row and column indices of the blocks
            int baseBlock1Row = baseBlock1 / 3;
            int baseBlock1Col = baseBlock1 % 3;
            int baseBlock2Row = baseBlock2 / 3;
            int baseBlock2Col = baseBlock2 % 3;
            int roofBlock1Row = roofBlock1 / 3;
            int roofBlock1Col = roofBlock1 % 3;
            int roofBlock2Row = roofBlock2 / 3;
            int roofBlock2Col = roofBlock2 % 3;

            // Check if the roof and base cells in both rows are in adjacent blocks
            bool isRoofBasePair1Adjacent = Math.Abs(baseBlock1Row - roofBlock1Row) <= 1 && Math.Abs(baseBlock1Col - roofBlock1Col) <= 1;
            bool isRoofBasePair2Adjacent = Math.Abs(baseBlock2Row - roofBlock2Row) <= 1 && Math.Abs(baseBlock2Col - roofBlock2Col) <= 1;

            // Both roof-base pairs must be adjacent
            return isRoofBasePair1Adjacent && isRoofBasePair2Adjacent;
        }
        private int CountOccurrencesInUnit(SearchUnitType searchUnitType, int unit, int candidate)
        {
            int count = 0;

            // Count occurrences of candidate in the row or column using helper methods
            var unitCandidates = _puzzle.GetRow(unit, false).Where(cell => cell.Candidates.Contains(candidate));
            count = unitCandidates.Count();

            return count;
        }
        private bool IsInSameUnit(int row1, int col1, int row2, int col2)
        {
            return row1 == row2 || col1 == col2 || GetBlockIndex(row1, col1) == GetBlockIndex(row2, col2);
        }
        private int GetBlockIndex(int row, int col)
        {
            return (row / 3) * 3 + (col / 3);
        }
    }
}
