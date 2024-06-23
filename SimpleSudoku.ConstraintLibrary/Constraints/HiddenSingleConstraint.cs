using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenSingleConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;

        // TODO: redo this stuff, this doesn't work as intended
        public bool FindHiddenSingles(SearchUnitType searchUnitType)
        {
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    var unitCells = _puzzle.GetUnit(row, col, searchUnitType)
                        .Where(cell => cell.Digit == null);

                    for (int candidate = 1; candidate <= PuzzleModel.Size; candidate++)
                    {
                        int count = 0;

                        // count every candidate in the specified unit
                        foreach (var unitCell in unitCells)
                        {
                            if (unitCell.Candidates.Contains(candidate))
                                count++;

                            if (count > 1)
                                break;
                        }

                        // if a candidate appears only once in the unit then proceed
                        if (count == 1)
                        {
                            var singleCandidate = unitCells.Where(c => c.Candidates.Contains(candidate)).Single();

                            if (singleCandidate.Candidates.Contains(candidate))
                            {
                                // transform the hidden single into a naked single
                                _puzzle.SolverCandidates[singleCandidate.Row, singleCandidate.Column].Clear();
                                _puzzle.SolverCandidates[singleCandidate.Row, singleCandidate.Column].Add(candidate);

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override bool ApplyConstraint(out string errorMessage)
        {
            errorMessage = "";
            if (FindHiddenSingles(SearchUnitType.Row) || FindHiddenSingles(SearchUnitType.Column) || FindHiddenSingles(SearchUnitType.Box))
            {
                return true;
            }
            errorMessage = "Couldn't find any Hidden Singles!";
            return false;
        }
    }
}
