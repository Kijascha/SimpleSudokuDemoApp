using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

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
        var matchingCells = GetMatchingPairsInUnit(row, col, searchUnitType)
            .Where(c => c.Candidates.Count == 2);

        // handling the elimination of candidates of other cells within the box 
        if (matchingCells.Any())
        {
            foreach (var matchingPair in matchingCells)
            {
                var overlappingCells = _puzzle.GetUnit(row, col, searchUnitType)
                    .Where(cell => cell.Digit == null &&
                                                                                cell.Candidates.Overlaps(matchingPair.Candidates) &&
                                                                                !cell.Candidates.SetEquals(matchingPair.Candidates));

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
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingPairsInUnit(int row, int col, SearchUnitType searchUnitType) => searchUnitType switch
    {
        SearchUnitType.Box => GetMatchingPairsInBox(row, col),
        SearchUnitType.Row => GetMatchingPairsInRow(row, col),
        SearchUnitType.Column => GetMatchingPairsInColumn(row, col),
        _ => throw new ArgumentOutOfRangeException(nameof(searchUnitType))
    };
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingPairsInRow(int row, int col)
    {
        for (int c = 0; c < 9; c++)
        {
            if (c != col && _puzzle.Digits[row, c] == null && _puzzle.SolverCandidates[row, c].SetEquals(_puzzle.SolverCandidates[row, col]))
            {
                yield return (row, c, _puzzle.SolverCandidates[row, c]);
            }
        }
    }
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingPairsInColumn(int row, int col)
    {
        for (int r = 0; r < 9; r++)
        {
            if (r != row && _puzzle.Digits[r, col] == null && _puzzle.SolverCandidates[r, col].SetEquals(_puzzle.SolverCandidates[row, col]))
            {
                yield return (r, col, _puzzle.SolverCandidates[r, col]);
            }
        }
    }
    private IEnumerable<(int Row, int Col, HashSet<int> Candidates)> GetMatchingPairsInBox(int row, int col)
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

}
