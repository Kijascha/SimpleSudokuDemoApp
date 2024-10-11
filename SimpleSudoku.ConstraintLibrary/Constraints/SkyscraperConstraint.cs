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

                    var selectedLink1 = SelectLink(searchUnitType, strongLink1, candidate);
                    var selectedLink2 = SelectLink(searchUnitType, strongLink1, candidate);

                    if (selectedLink1 != null && selectedLink2 != null)
                    {
                        var weakLink = selectedLink1.Intersect(selectedLink2);

                        if (weakLink.Count() > 0)
                        {
                            foreach (var weakLinkUnit in weakLink)
                            {
                                // Get the cells in the intersecting column that contain the candidate in both rows
                                var baseCell1 = FindBaseCell(searchUnitType, strongLink1, candidate, weakLinkUnit);
                                var baseCell2 = FindBaseCell(searchUnitType, strongLink2, candidate, weakLinkUnit);

                                if (baseCell1 != null && baseCell2 != null)
                                {
                                    var roofCell1 = FindRoofCell(searchUnitType, strongLink1, candidate, baseCell1.Value.Row, baseCell1.Value.Column);
                                    var roofCell2 = FindRoofCell(searchUnitType, strongLink2, candidate, baseCell1.Value.Row, baseCell1.Value.Column);

                                    if (roofCell1.HasValue && roofCell2.HasValue)
                                    {
                                        // Get the blocks of the strong linked candidates in row1 and row2
                                        int block1 = ConstraintHelper.GetBlockIndex(baseCell1.Value.Row, baseCell1.Value.Column);
                                        int block2 = ConstraintHelper.GetBlockIndex(baseCell2.Value.Row, baseCell2.Value.Column);
                                        int block3 = ConstraintHelper.GetBlockIndex(roofCell1.Value.Row, roofCell1.Value.Column);
                                        int block4 = ConstraintHelper.GetBlockIndex(roofCell2.Value.Row, roofCell2.Value.Column);

                                        var isRowCheck = (searchUnitType == SearchUnitType.Row) ? true : false;

                                        // Check if the roof forms a valid rectangular structure
                                        if (IsValidSkyscraper(isRowCheck, baseCell1.Value.Row, baseCell1.Value.Column, baseCell2.Value.Row, baseCell2.Value.Column, roofCell1.Value.Row, roofCell1.Value.Column, roofCell2.Value.Row, roofCell2.Value.Column))
                                        {
                                            //debugInfo.AppendLine($"Possible valid Skyscraper framed by base cells in [r1: {candidateInUnit1.Row} c1: {candidateInUnit1.Column} r2: {candidateInUnit2.Row} c2: {candidateInUnit2.Column}] " +
                                            //    $"and roof cells in [r1: {roofCell1.Row} c1: {roofCell1.Column} r2: {roofCell2.Row} c2: {roofCell2.Column}]");
                                            // Remove the candidate from cells that are seen by both roof cells and return true if successfull removed
                                            if (RemoveCandidate(roofCell1.Value.Row, roofCell1.Value.Column, roofCell2.Value.Row, roofCell2.Value.Column, candidate, debugInfo))
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

        private (int Row, int Column, int? Digit, HashSet<int> Candidates)? FindBaseCell(SearchUnitType searchUnitType, IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> strongLink, int candidate, int weakLinkUnit)
        {
            return (searchUnitType == SearchUnitType.Row) ? strongLink.FirstOrDefault(c => c.Candidates.Contains(candidate) && c.Column == weakLinkUnit) :
                   (searchUnitType == SearchUnitType.Column) ? strongLink.FirstOrDefault(c => c.Candidates.Contains(candidate) && c.Row == weakLinkUnit) :
                   null;
        }
        private (int Row, int Column, int? Digit, HashSet<int> Candidates)? FindRoofCell(SearchUnitType searchUnitType, IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> strongLink, int candidate, int baseCellRow, int baseCellColumn)
        {
            return (searchUnitType == SearchUnitType.Row) ? strongLink.FirstOrDefault(c => c.Candidates.Contains(candidate) && (c.Column != baseCellColumn)) :
                   (searchUnitType == SearchUnitType.Column) ? strongLink.FirstOrDefault(c => c.Candidates.Contains(candidate) && (c.Row != baseCellRow)) :
                   null;
        }
        private IEnumerable<int>? SelectLink(SearchUnitType searchUnitType, IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> strongLink, int candidate)
        {
            var filteredLink = strongLink.Where(c => c.Candidates.Contains(candidate));
            return (searchUnitType == SearchUnitType.Row) ? filteredLink.Select(c => c.Column) :
                   (searchUnitType == SearchUnitType.Column) ? filteredLink.Select(c => c.Row) :
                   null;
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
