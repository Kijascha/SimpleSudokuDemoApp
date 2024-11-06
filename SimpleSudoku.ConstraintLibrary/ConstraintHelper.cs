using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary
{
    public static class ConstraintHelper
    {
        //TODO implement all thos helper methods here instead of reimplementing them for every constraint -> less DRY
        /// <summary>
        /// Counts the number of occurrences of a candidate in a specific search unit (Row, Column, or Box) within a puzzle.
        /// </summary>
        /// <param name="_puzzle">The puzzle model implementing <see cref="IPuzzleModel"/>.</param>
        /// <param name="searchUnitType">The type of search unit to check, either Row, Column, or Box.</param>
        /// <param name="unitRow">The row index of the unit (used for Row or Box search).</param>
        /// <param name="unitCol">The column index of the unit (used for Column or Box search).</param>
        /// <param name="candidate">The candidate value to search for within the unit.</param>
        /// <returns>The number of cells that contain the candidate in the specified search unit.</returns>
        /// <remarks>
        /// This method supports counting occurrences of a candidate in rows, columns, or boxes.
        /// - If the searchUnitType is <see cref="SearchUnitType.Row"/>, the method examines the row at index `unitRow`.
        /// - If the searchUnitType is <see cref="SearchUnitType.Column"/>, the method examines the column at index `unitCol`.
        /// - If the searchUnitType is <see cref="SearchUnitType.Box"/>, the method examines the box at the given row (`unitRow`) and column (`unitCol`).
        /// It filters the unit cells by whether they contain the specified candidate and returns the count of such cells.
        /// </remarks>
        public static int CountOccurrencesInUnit(IPuzzleModel _puzzle, SearchUnitType searchUnitType, int unitRow, int unitCol, int candidate)
        {
            int count = 0;

            var unitCells = (searchUnitType == SearchUnitType.Row)
                ? _puzzle.GetRow(unitRow, false).Where(cell => cell.SolverCandidates.Contains(candidate))
                : (searchUnitType == SearchUnitType.Column) ? _puzzle.GetColumn(unitCol, false).Where(cell => cell.SolverCandidates.Contains(candidate))
                : _puzzle.GetBox(unitRow, unitCol, false).Where(cell => cell.SolverCandidates.Contains(candidate));

            count = unitCells.Count();
            return count;
        }
        /// <summary>
        /// Counts the number of occurrences of a candidate in a specific row or column of the puzzle.
        /// </summary>
        /// <param name="_puzzle">The puzzle model implementing <see cref="IPuzzleModel"/>.</param>
        /// <param name="searchUnitType">The type of search unit to check, either Row or Column.</param>
        /// <param name="unit">The index of the unit (either row index or column index).</param>
        /// <param name="candidate">The candidate value to search for within the unit.</param>
        /// <returns>The number of cells that contain the candidate in the specified row or column.</returns>
        /// <remarks>
        /// This method counts candidate occurrences only in rows or columns, not boxes.
        /// - If the searchUnitType is <see cref="SearchUnitType.Row"/>, it examines the row at the given index `unit`.
        /// - If the searchUnitType is <see cref="SearchUnitType.Column"/>, it examines the column at the given index `unit`.
        /// The method does not support counting within boxes, and it returns 0 if the searchUnitType is set to <see cref="SearchUnitType.Box"/>.
        /// </remarks>
        public static int CountOccurrencesInUnit(IPuzzleModel _puzzle, SearchUnitType searchUnitType, int unit, int candidate)
        {
            int count = 0;

            // Ensure we are not processing boxes for the skyscraper constraint
            if (searchUnitType == SearchUnitType.Box)
            {
                return 0; // Skyscrapers are not applicable to boxes, return zero occurrences
            }

            var unitCells = (searchUnitType == SearchUnitType.Row)
                ? _puzzle.GetRow(unit, false).Where(cell => cell.SolverCandidates.Contains(candidate))
                : _puzzle.GetColumn(unit, false).Where(cell => cell.SolverCandidates.Contains(candidate));

            count = unitCells.Count();
            return count;
        }


        public static IEnumerable<CellV2> GetMatchingPairsInUnit(IPuzzleModel _puzzle, int row, int col, SearchUnitType searchUnitType) => searchUnitType switch
        {
            SearchUnitType.Box => GetMatchingPairsInBox(_puzzle, row, col),
            SearchUnitType.Row => GetMatchingPairsInRow(_puzzle, row, col),
            SearchUnitType.Column => GetMatchingPairsInColumn(_puzzle, row, col),
            _ => throw new ArgumentOutOfRangeException(nameof(searchUnitType))
        };
        private static IEnumerable<CellV2> GetMatchingPairsInRow(IPuzzleModel _puzzle, int row, int col)
        {
            for (int c = 0; c < 9; c++)
            {
                if (c != col && _puzzle.Board[row, c].Digit == 0 && _puzzle.Board[row, c].SolverCandidates.Collection.ToHashSet().SetEquals(_puzzle.Board[row, col].SolverCandidates.Collection))
                {
                    yield return _puzzle.Board[row, c];
                }
            }
        }
        private static IEnumerable<CellV2> GetMatchingPairsInColumn(IPuzzleModel _puzzle, int row, int col)
        {
            for (int r = 0; r < 9; r++)
            {
                if (r != row && _puzzle.Board[r, col].Digit == 0 && _puzzle.Board[r, col].SolverCandidates.Collection.ToHashSet().SetEquals(_puzzle.Board[row, col].SolverCandidates.Collection))
                {
                    yield return _puzzle.Board[r, col];
                }
            }
        }
        private static IEnumerable<CellV2> GetMatchingPairsInBox(IPuzzleModel _puzzle, int row, int col)
        {
            int startRow = row - row % 3;
            int startCol = col - col % 3;

            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if ((r != row || c != col) && _puzzle.Board[r, c].Digit == 0 && _puzzle.Board[r, c].SolverCandidates.Collection.ToHashSet().SetEquals(_puzzle.Board[row, col].SolverCandidates.Collection))
                    {
                        yield return _puzzle.Board[r, c];
                    }
                }
            }
        }

        // Helper function to check if the blocks form a rectangular 2x2 subgrid
        public static bool IsRectangularSubgrid(int block1, int block2, int blockRoof1, int blockRoof2)
        {
            // Ensure the blocks are in distinct rows and columns (not Z-shaped or diagonal)
            int block1Row = block1 / 3;
            int block1Col = block1 % 3;
            int block2Row = block2 / 3;
            int block2Col = block2 % 3;
            int blockRoof1Row = blockRoof1 / 3;
            int blockRoof1Col = blockRoof1 % 3;
            int blockRoof2Row = blockRoof2 / 3;
            int blockRoof2Col = blockRoof2 % 3;

            // Blocks must form a proper 2x2 rectangular grid
            bool isValidRectangular =
                (block1Row == blockRoof1Row || block1Row == blockRoof2Row) &&
                (block2Row == blockRoof1Row || block2Row == blockRoof2Row) &&
                (block1Col == blockRoof1Col || block1Col == blockRoof2Col) &&
                (block2Col == blockRoof1Col || block2Col == blockRoof2Col);

            return isValidRectangular;
        }
        // Helper function to check if the blocks form a valid rectangular subgrid (2x2, 2x3, or 3x2)
        public static bool IsRectangularSubgridV2(int block1, int block2, int blockRoof1, int blockRoof2, bool isRowCheck)
        {
            // Calculate the box indices for the blocks
            int boxRowBase1 = block1 / 3;  // Row of the box for block1
            int boxColBase1 = block1 % 3;  // Column of the box for block1
            int boxRowBase2 = block2 / 3;  // Row of the box for block2
            int boxColBase2 = block2 % 3;  // Column of the box for block2
            int boxRowRoof1 = blockRoof1 / 3;  // Row of the box for roof block 1
            int boxColRoof1 = blockRoof1 % 3; // Column of the box for roof block 1
            int boxRowRoof2 = blockRoof2 / 3;  // Row of the box for roof block 2
            int boxColRoof2 = blockRoof2 % 3; // Column of the box for roof block 2

            if (isRowCheck)
            {
                // Check if both base blocks are in the same column
                // Roof blocks must be in the same column and not in the base block's column
                return boxColBase1 == boxColBase2 && boxColBase1 != boxColRoof1 && boxColRoof1 == boxColRoof2;
            }

            // Check if both base blocks are in the same row
            // Roof blocks must be in the same row and not in the base block's row
            return boxRowBase1 == boxRowBase2 && boxRowBase1 != boxRowRoof1 && boxRowRoof1 == boxRowRoof2;
        }
        // Ensure that the base and roof blocks belong to at least 4 different boxes
        public static bool IsValidSkyscraperWithDistinctBoxes(int baseRow1, int baseCol1, int baseRow2, int baseCol2, int roofRow1, int roofCol1, int roofRow2, int roofCol2)
        {
            var uniqueBoxes = new HashSet<int>
            {
                GetBlockIndex(baseRow1, baseCol1),
                GetBlockIndex(baseRow2, baseCol2),
                GetBlockIndex(roofRow1, roofCol1),
                GetBlockIndex(roofRow2, roofCol2)
            };

            // Skyscraper must span at least 4 distinct boxes
            return uniqueBoxes.Count == 4;
        }
        public static bool IsInSameUnit(SearchUnitType unitType, int row1, int col1, int row2, int col2)
        {
            return (unitType == SearchUnitType.Row) ? row1 == row2 :
                    (unitType == SearchUnitType.Column) ? col1 == col2 :
                    GetBlockIndex(row1, col1) == GetBlockIndex(row2, col2);
        }
        public static bool IsInSameUnit(int row1, int col1, int row2, int col2)
        {
            return row1 == row2 || col1 == col2 || GetBlockIndex(row1, col1) == GetBlockIndex(row2, col2);
        }
        public static int GetBlockIndex(int row, int col)
        {
            return (row / 3) * 3 + (col / 3);
        }
    }
}
