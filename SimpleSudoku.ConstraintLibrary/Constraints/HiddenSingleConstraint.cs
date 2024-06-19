using SimpleSudoku.CommonLibrary.Models;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenSingleConstraint : Constraint
    {
        private readonly IPuzzleModel _puzzle;

        public HiddenSingleConstraint(IPuzzleModel puzzle)
        {
            _puzzle = puzzle;
        }

        public bool FindHiddenSingles()
        {
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    var rowCells = _puzzle.GetRow(row, false);
                    var columnCells = _puzzle.GetColumn(col, false);
                    var boxCells = _puzzle.GetBox(row, col, false);

                    (int Row, int Column, int? Digit, HashSet<int> Candidates) cell = (row, col, _puzzle.Digits[row, col], _puzzle.SolverCandidates[row, col]);

                    if (cell.Digit == null)
                    {
                        var rowCandidates = rowCells.Where(c => c.Column != col).SelectMany(c => c.Candidates).ToHashSet();
                        var colCandidates = columnCells.Where(c => c.Row != row).SelectMany(c => c.Candidates).ToHashSet();
                        var boxCandidates = boxCells.Where(c => c.Row != row && c.Column != col).SelectMany(c => c.Candidates).ToHashSet();

                        // Intersection of candidate sets
                        var hiddenCandidates = cell.Candidates.Except(rowCandidates).Except(colCandidates).Except(boxCandidates);

                        if (hiddenCandidates.Count() == 1)
                        {
                            var hiddenSingle = hiddenCandidates.Single();

                            // Further actions (e.g., setting the digit) can be performed here 
                            _puzzle.SolverCandidates[row, col].Clear();
                            _puzzle.SolverCandidates[row, col].Add(hiddenSingle);

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool ApplyConstraint(out string errorMessage)
        {
            errorMessage = "";
            if (!FindHiddenSingles())
            {
                errorMessage = "Couldn't find any Hidden Singles!";
                return false;
            }
            return true;
        }
    }
}
