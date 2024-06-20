using SimpleSudoku.CommonLibrary.Models;

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
        // get matching candidate pairs within a 3x3 sudoku box
        var result = GetMatchingCells(row, col, searchUnitType).Where(c => c.Candidates.Count == 2);

        // handling the elimination of candidates of other cells within the box 
        if (result is not null && result.Any())
        {
            foreach (var matchingPair in result)
            {
                var overlappingCells = GetSearchingUnit(row, col, searchUnitType)
                .Where(c => c.Digit == null &&
                c.Candidates.Overlaps(matchingPair.Candidates) &&
                !c.Candidates.SetEquals(matchingPair.Candidates));

                if (overlappingCells.Any())
                {
                    foreach (var overlappingCell in overlappingCells)
                    {
                        overlappingCell.Candidates.RemoveWhere(x => matchingPair.Candidates.Contains(x));
                    }
                    return true;
                }
            }
        }

        return false;
    }
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingCells(int row, int col, SearchUnitType searchUnitType) => searchUnitType switch
    {
        SearchUnitType.Box => GetMatchingCellsInBox(row, col),
        SearchUnitType.Row => GetMatchingCellsInRow(row, col),
        SearchUnitType.Column => GetMatchingCellsInColumn(row, col),
        _ => throw new ArgumentOutOfRangeException(nameof(searchUnitType))
    };
    private IEnumerable<(int Row, int Column, int? Digit, HashSet<int> Candidates)> GetSearchingUnit(int row, int col, SearchUnitType searchUnitType) => searchUnitType switch
    {
        SearchUnitType.Box => _puzzle.GetBox(row, col, false),
        SearchUnitType.Row => _puzzle.GetRow(row, false),
        SearchUnitType.Column => _puzzle.GetColumn(col, false),
        _ => throw new ArgumentOutOfRangeException(nameof(searchUnitType))
    };
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingCellsInRow(int row, int col)
    {
        for (int c = 0; c < 9; c++)
        {
            if (c != col && _puzzle.Digits[row, c] == null && _puzzle.SolverCandidates[row, c].SetEquals(_puzzle.SolverCandidates[row, col]))
            {
                yield return (row, c, _puzzle.SolverCandidates[row, c]);
            }
        }
    }
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingCellsInColumn(int row, int col)
    {
        for (int r = 0; r < 9; r++)
        {
            if (r != row && _puzzle.Digits[r, col] == null && _puzzle.SolverCandidates[r, col].SetEquals(_puzzle.SolverCandidates[row, col]))
            {
                yield return (r, col, _puzzle.SolverCandidates[r, col]);
            }
        }
    }
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingCellsInBox(int row, int col)
    {
        int startRow = row - row % 3;
        int startCol = col - col % 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if ((r != row || c != col) && _puzzle.Digits[r, c] == null && _puzzle.SolverCandidates[r, c].SetEquals(_puzzle.SolverCandidates[row, col]))
                {
                    yield return (r, c, _puzzle.SolverCandidates[r, c]);
                }
            }
        }
    }
    private enum SearchUnitType { Row, Column, Box }
}
