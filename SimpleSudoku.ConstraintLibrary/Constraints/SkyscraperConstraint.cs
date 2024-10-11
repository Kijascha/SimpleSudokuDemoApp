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

            // loop through each candidate
            for (int candidate = 1; candidate <= PuzzleModel.Size; candidate++)
            {
                if (FindSkyscraper(candidate, SearchUnitType.Row, debugInfo) ||
                   FindSkyscraper(candidate, SearchUnitType.Column, debugInfo))
                {
                    foundValidSkyscraper = true;
                    break;
                }
            }

            debugInfo.AppendLine($"\tfoundValidSkyscraper: {foundValidSkyscraper}");

            //TODO Bug fixes fehlerhafte erkennung in anderen puzzles !!!!!!!!

            errorMessage = foundValidSkyscraper ? "" : "No valid skyscrapers found.";

            // Output the accumulated debug information
            Debug.WriteLine(debugInfo.ToString());

            return foundValidSkyscraper;
        }

        private bool FindSkyscraper(int candidate, SearchUnitType searchUnitType, StringBuilder debugInfo)
        {
            HashSet<(int unitNumber, IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> unit)> strongLinks = [];

            for (int unit = 0; unit < PuzzleModel.Size; unit++)
            {
                if (ConstraintHelper.CountOccurrencesInUnit(_puzzle, searchUnitType, unit, candidate) == 2)
                {
                    var strongLinkToAdd = (searchUnitType == SearchUnitType.Row) ? _puzzle.GetRow(unit, false) :
                                                                                            (searchUnitType == SearchUnitType.Column) ? _puzzle.GetColumn(unit, false) : null;

                    if (strongLinkToAdd != null)
                        strongLinks.Add((unit, strongLinkToAdd));
                }
            }

            // Now check weak links in both row pairs
            for (int u1 = 0; u1 < strongLinks.Count - 1; u1++)
            {
                for (int u2 = u1 + 1; u2 < strongLinks.Count; u2++)
                {
                    var strongLink1 = strongLinks.ToList()[u1].unit;
                    var strongLink2 = strongLinks.ToList()[u2].unit;

                    var filteredLink1 = strongLink1.Where(c => c.Candidates.Contains(candidate));
                    var selectedLink1 = (searchUnitType == SearchUnitType.Row) ? filteredLink1.Select(c => c.Column) :
                                        (searchUnitType == SearchUnitType.Column) ? filteredLink1.Select(c => c.Row) : null;

                    var filteredLink2 = strongLink2.Where(c => c.Candidates.Contains(candidate));
                    var selectedLink2 = (searchUnitType == SearchUnitType.Row) ? filteredLink2.Select(c => c.Column) :
                                        (searchUnitType == SearchUnitType.Column) ? filteredLink2.Select(c => c.Row) : null;

                    if (selectedLink1 != null && selectedLink2 != null)
                    {
                        var weakLink = selectedLink1.Intersect(selectedLink2);


                        if (weakLink.Count() > 0)
                        {
                            foreach (var weakLinkUnit in weakLink)
                            {
                                // Get the cells in the intersecting column that contain the candidate in both rows
                                var baseCandidates1 = (searchUnitType == SearchUnitType.Row) ? strongLink1.Where(c => c.Candidates.Contains(candidate) && c.Column == weakLinkUnit) :
                                                                                            (searchUnitType == SearchUnitType.Column) ? strongLink1.Where(c => c.Candidates.Contains(candidate) && c.Row == weakLinkUnit) : null;
                                var baseCandidates2 = (searchUnitType == SearchUnitType.Row) ? strongLink2.Where(c => c.Candidates.Contains(candidate) && c.Column == weakLinkUnit) :
                                                                                            (searchUnitType == SearchUnitType.Column) ? strongLink2.Where(c => c.Candidates.Contains(candidate) && c.Row == weakLinkUnit) : null;

                                if (baseCandidates1 != null && baseCandidates2 != null)
                                {
                                    var baseCell1 = baseCandidates1.First();
                                    var baseCell2 = baseCandidates2.First();

                                    var roofCellResult1 = (searchUnitType == SearchUnitType.Row) ? strongLink1.Where(c => c.Candidates.Contains(candidate) && (c.Column != baseCell1.Column)) :
                                                                                            (searchUnitType == SearchUnitType.Column) ? strongLink1.Where(c => c.Candidates.Contains(candidate) && (c.Row != baseCell1.Row)) : null;
                                    var roofCellResult2 = (searchUnitType == SearchUnitType.Row) ? strongLink2.Where(c => c.Candidates.Contains(candidate) && (c.Column != baseCell2.Column)) :
                                                                                            (searchUnitType == SearchUnitType.Column) ? strongLink2.Where(c => c.Candidates.Contains(candidate) && (c.Row != baseCell2.Row)) : null;

                                    if (roofCellResult1 != null && roofCellResult2 != null)
                                    {
                                        var roofCell1 = roofCellResult1.First();
                                        var roofCell2 = roofCellResult2.First();

                                        // Get the blocks of the strong linked candidates in row1 and row2
                                        int block1 = ConstraintHelper.GetBlockIndex(baseCell1.Row, baseCell1.Column);
                                        int block2 = ConstraintHelper.GetBlockIndex(baseCell2.Row, baseCell2.Column);
                                        int block3 = ConstraintHelper.GetBlockIndex(roofCell1.Row, roofCell1.Column);
                                        int block4 = ConstraintHelper.GetBlockIndex(roofCell2.Row, roofCell2.Column);

                                        var isRowCheck = (searchUnitType == SearchUnitType.Row) ? true : false;

                                        // Check if the roof forms a valid rectangular structure
                                        if (IsValidSkyscraper(isRowCheck, baseCell1.Row, baseCell1.Column, baseCell2.Row, baseCell2.Column, roofCell1.Row, roofCell1.Column, roofCell2.Row, roofCell2.Column))
                                        {
                                            //debugInfo.AppendLine($"Possible valid Skyscraper framed by base cells in [r1: {candidateInUnit1.Row} c1: {candidateInUnit1.Column} r2: {candidateInUnit2.Row} c2: {candidateInUnit2.Column}] " +
                                            //    $"and roof cells in [r1: {roofCell1.Row} c1: {roofCell1.Column} r2: {roofCell2.Row} c2: {roofCell2.Column}]");
                                            // Remove the candidate from cells that are seen by both roof cells and return true if successfull removed
                                            if (RemoveCandidate(roofCell1.Row, roofCell1.Column, roofCell2.Row, roofCell2.Column, candidate, debugInfo))
                                                return true;
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

        private bool RemoveCandidate(int roofCell1Row, int roofCell1Col, int roofCell2Row, int roofCell2Col, int candidate, StringBuilder debugInfo)
        {
            // loop through the puzzle
            for (int r = 0; r < PuzzleModel.Size; r++)
            {
                for (int c = 0; c < PuzzleModel.Size; c++)
                {
                    // only handle cells which contains the candidate
                    if (_puzzle.SolverCandidates[r, c].Contains(candidate) && SeesBothRoofCells(r, c, roofCell1Row, roofCell1Col, roofCell2Row, roofCell2Col) &&
                        r != roofCell1Row && c != roofCell1Col && r != roofCell2Row && c != roofCell2Col)
                    {
                        //debugInfo.AppendLine($"Removed candidate {candidate} from cell ({r}, {c}) seen by roof cell 1 ({roofCell1Row}, {roofCell1Col}) and roof cell 2 ({roofCell2Row}, {roofCell2Col})");
                        _puzzle.SolverCandidates[r, c].Remove(candidate);
                        return true;
                    }
                }
            }
            return false;
        }

        // Helper function to check if a cell "sees" both roof cells
        private static bool SeesBothRoofCells(int row, int col, int roofRow1, int roofCol1, int roofRow2, int roofCol2)
        {
            // A cell "sees" another cell if it's in the same row, column, or block
            return ConstraintHelper.IsInSameUnit(row, col, roofRow1, roofCol1) && ConstraintHelper.IsInSameUnit(row, col, roofRow2, roofCol2);
        }
        // Final method to check for both rectangular shapes and distinct boxes
        public static bool IsValidSkyscraper(bool isRowCheck, int baseRow1, int baseCol1, int baseRow2, int baseCol2, int roofRow1, int roofCol1, int roofRow2, int roofCol2)
        {

            // Then ensure it spans at least 4 distinct 3x3 boxes
            bool hasDistinctBoxes = ConstraintHelper.IsValidSkyscraperWithDistinctBoxes(baseRow1, baseCol1, baseRow2, baseCol2, roofRow1, roofCol1, roofRow2, roofCol2);

            if (hasDistinctBoxes)
            {
                // First check if the skyscraper forms a valid rectangular subgrid (2x2, 2x3, or 3x2)
                bool isRectangular = ConstraintHelper.IsRectangularSubgridV2(
                    ConstraintHelper.GetBlockIndex(baseRow1, baseCol1),
                    ConstraintHelper.GetBlockIndex(baseRow2, baseCol2),
                    ConstraintHelper.GetBlockIndex(roofRow1, roofCol1),
                    ConstraintHelper.GetBlockIndex(roofRow2, roofCol2), isRowCheck);

                // The skyscraper is valid only if both conditions are satisfied
                return isRectangular && hasDistinctBoxes;
            }
            return false;
        }
    }
}
