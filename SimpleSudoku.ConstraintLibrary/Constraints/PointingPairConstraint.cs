using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using System.Diagnostics;
using System.Text;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class PointingPairConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;

        private HashSet<(int, int, int, int, int)> _seenPointingPairs = new();

        public override bool ApplyConstraint(out string errorMessage)
        {
            StringBuilder debugInfo = new StringBuilder();
            bool foundValidPointingPair = false;
            _seenPointingPairs.Clear();

            // Iterate through all candidates (1-9)
            for (int candidate = 1; candidate <= 9; candidate++)
            {
                //debugInfo.AppendLine($"Checking candidate {candidate} for pointing pairs...");
                foundValidPointingPair |= CheckForPointingPairInBlock(SearchUnitType.Column, candidate, debugInfo);
                foundValidPointingPair |= CheckForPointingPairInBlock(SearchUnitType.Row, candidate, debugInfo);
            }

            errorMessage = foundValidPointingPair ? "" : "No valid pointing pairs found.";

            //debugInfo.AppendLine($"foundValidPointingPair: {foundValidPointingPair}");
            Debug.WriteLine(debugInfo.ToString());

            return foundValidPointingPair;
        }

        private bool FindPointingPairInUnit(SearchUnitType unitType, int startRow, int startCol, int candidate, StringBuilder debugInfo)
        {
            var unit = (unitType == SearchUnitType.Row) ? _puzzle.GetRow(startRow, false) :
                                                                                (unitType == SearchUnitType.Column) ? _puzzle.GetColumn(startCol, false) :
                                                                                _puzzle.GetBox(startRow, startCol, false);

            var filteredUnit = unit.Where(cell => cell.SolverCandidates.Contains(candidate));
            if (filteredUnit.Any() && filteredUnit.Count() == 2)
            {
                var possiblePointingCellA = filteredUnit.ToList()[0];
                var possiblePointingCellB = filteredUnit.ToList()[1];

                if (ConstraintHelper.IsInSameUnit(SearchUnitType.Box, possiblePointingCellA.Row, possiblePointingCellA.Column, possiblePointingCellB.Row, possiblePointingCellB.Column))
                {
                    var pointingPair = (possiblePointingCellA.Row, possiblePointingCellA.Column, possiblePointingCellB.Row, possiblePointingCellB.Column, candidate);

                    if (!_seenPointingPairs.Contains(pointingPair))
                    {

                        //debugInfo.AppendLine($"Pointing Pair found in Cells ({possiblePointingCellA.Row}, {possiblePointingCellA.Column}) and ({possiblePointingCellB.Row}, {possiblePointingCellB.Column}) for candidate {candidate}.");

                        _seenPointingPairs.Add(pointingPair);
                        //TODO: Processing found pointing Pairs

                        if (unitType == SearchUnitType.Row)
                        {
                            var box = _puzzle.GetBox(possiblePointingCellA.Row, possiblePointingCellA.Column, false);

                            var nonPairCellsInBox = box.Where(c =>
                                !(c.Row == possiblePointingCellA.Row && c.Column == possiblePointingCellA.Column) &&
                                !(c.Row == possiblePointingCellB.Row && c.Column == possiblePointingCellB.Column) &&
                                c.SolverCandidates.Contains(candidate));


                            if (!nonPairCellsInBox.Any()) return false;

                            if (nonPairCellsInBox.Any())
                            {
                                //debugInfo.AppendLine($"\tnonPairCellsInBox.Count: {nonPairCellsInBox.Count()}.");

                                foreach (var cell in nonPairCellsInBox)
                                {
                                    cell.SolverCandidates.Remove(candidate);
                                }
                                return true;
                            }
                        }

                        else if (unitType == SearchUnitType.Column)
                        {
                            var box = _puzzle.GetBox(possiblePointingCellA.Row, possiblePointingCellA.Column, false);

                            var nonPairCellsInBox = box.Where(c =>
                                !(c.Row == possiblePointingCellA.Row && c.Column == possiblePointingCellA.Column) &&
                                !(c.Row == possiblePointingCellB.Row && c.Column == possiblePointingCellB.Column) &&
                                c.SolverCandidates.Contains(candidate));


                            if (!nonPairCellsInBox.Any()) return false;

                            if (nonPairCellsInBox.Any())
                            {
                                //debugInfo.AppendLine($"\tnonPairCellsInBox.Count: {nonPairCellsInBox.Count()}.");

                                foreach (var cell in nonPairCellsInBox)
                                {
                                    cell.SolverCandidates.Remove(candidate);
                                }
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        private bool CheckForPointingPairInBlock(SearchUnitType unitType, int candidate, StringBuilder debugInfo)
        {
            bool foundPointingPair = false;
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    if (ConstraintHelper.CountOccurrencesInUnit(_puzzle, unitType, row, col, candidate) == 2)
                    {
                        foundPointingPair |= FindPointingPairInUnit(unitType, row, col, candidate, debugInfo);
                    }
                }
            }
            return foundPointingPair;
        }
    }
}
