using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using System.Diagnostics;
using System.Text;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class YWingConstraint(IPuzzleModel puzzle) : Constraint
{
    private readonly IPuzzleModel _puzzle = puzzle;
    private readonly HashSet<string> _seenYWings = [];

    public override bool ApplyConstraint(out string errorMessage)
    {
        bool removedSuccessfully = false;
        StringBuilder debugInfo = new StringBuilder();

        debugInfo.AppendLine("Starting Y-Wing constraint application.");

        // Iterate through all cells to look for potential hinge cells (A).
        for (int row = 0; row < PuzzleModel.Size; row++)
        {
            for (int col = 0; col < PuzzleModel.Size; col++)
            {
                var cellCandidates = _puzzle.Board[row, col].SolverCandidates.Collection.ToHashSet();

                debugInfo.AppendLine($"Checking cell ({row}, {col}) with digit {_puzzle.Board[row, col].Digit} and candidates: [{string.Join(", ", cellCandidates)}]");

                // A hinge cell must have exactly 2 candidates.
                if (cellCandidates.Count == 2)
                {
                    HashSet<int> hingeCandidates = new(cellCandidates);
                    debugInfo.AppendLine($"Found hinge cell at ({row}, {col}) with candidates: [{string.Join(", ", hingeCandidates)}]");

                    // Look for potential wings in rows, columns, and blocks.
                    removedSuccessfully |= FindYWing(row, col, hingeCandidates, debugInfo);
                }
            }
        }

        errorMessage = removedSuccessfully ? "" : "Couldn't find any Y-Wings";

        // Output the debug information if any Y-Wing patterns were found.
        if (debugInfo.Length > 0)
        {
            Debug.WriteLine(debugInfo.ToString());
        }

        return removedSuccessfully;
    }

    private bool FindYWing(int hingeRow, int hingeCol, HashSet<int> hingeCandidates, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

        debugInfo.AppendLine($"Searching for wings for hinge cell at ({hingeRow}, {hingeCol})");

        // Search for wing cells in the column, row, and box.
        removedSuccessfully |= SearchPotentialWings(hingeRow, hingeCol, hingeCandidates, SearchUnitType.Column, debugInfo);
        removedSuccessfully |= SearchPotentialWings(hingeRow, hingeCol, hingeCandidates, SearchUnitType.Row, debugInfo);
        removedSuccessfully |= SearchPotentialWings(hingeRow, hingeCol, hingeCandidates, SearchUnitType.Box, debugInfo);

        return removedSuccessfully;
    }

    private bool SearchPotentialWings(int hingeRow, int hingeCol, HashSet<int> hingeCandidates, SearchUnitType unitType, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;
        var unit = _puzzle.GetUnit(hingeRow, hingeCol, unitType);

        debugInfo.AppendLine($"Searching in {unitType} for wings related to hinge cell ({hingeRow}, {hingeCol}).");

        foreach (var potentialWingCell in unit)
        {
            if (potentialWingCell.SolverCandidates.Collection.Count == 2)
            {
                var potentialWingCandidates = potentialWingCell.SolverCandidates.Collection.ToHashSet();

                debugInfo.AppendLine($"Checking potential wing cell ({potentialWingCell.Row}, {potentialWingCell.Column}) with candidates: [{string.Join(", ", potentialWingCandidates)}]");

                if (potentialWingCandidates.Count == 2 && SharesOneCandidate(hingeCandidates, potentialWingCandidates))
                {
                    debugInfo.AppendLine($"Found potential wing cell ({potentialWingCell.Row}, {potentialWingCell.Column}) sharing one candidate with hinge.");

                    removedSuccessfully |= TryFindYWingPartner(hingeRow, hingeCol,
                                                               potentialWingCell.Row, potentialWingCell.Column,
                                                               hingeCandidates, potentialWingCandidates,
                                                               unitType, debugInfo);
                }
            }
        }

        return removedSuccessfully;
    }

    private bool TryFindYWingPartner(int hingeRow, int hingeCol, int wing1Row, int wing1Col, HashSet<int> hingeCandidates, HashSet<int> wing1Candidates, SearchUnitType wing1Unit, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

        debugInfo.AppendLine($"Looking for a partner wing for hinge ({hingeRow},{hingeCol}) and first wing ({wing1Row},{wing1Col}).");

        // Search in other units depending on the current search context.
        foreach (var unitType in Enum.GetValues(typeof(SearchUnitType)).Cast<SearchUnitType>())
        {
            if (wing1Unit == SearchUnitType.Column)
            {
                var unit = _puzzle.GetUnit(hingeRow, hingeCol, unitType);
                foreach (var potentialWingCell in unit)
                {
                    var potentialWing2Candidates = potentialWingCell.SolverCandidates.Collection.ToHashSet();

                    if (potentialWing2Candidates.Count == 2 &&
                        SharesOneCandidate(hingeCandidates, potentialWing2Candidates) &&
                        SharesOneCandidate(wing1Candidates, potentialWing2Candidates))
                    {
                        var totalCandidates = new HashSet<int>(hingeCandidates);
                        totalCandidates.UnionWith(wing1Candidates);
                        totalCandidates.UnionWith(potentialWing2Candidates);

                        debugInfo.AppendLine($"Evaluating potential second wing cell ({potentialWingCell.Row}, {potentialWingCell.Column}) with candidates: [{string.Join(", ", potentialWing2Candidates)}]");

                        if (totalCandidates.Count == 3 &&
                            CandidatesAppearTwice(totalCandidates, hingeCandidates, wing1Candidates, potentialWing2Candidates) &&
                            !IsInSameUnit(wing1Row, wing1Col, potentialWingCell.Row, potentialWingCell.Column))
                        {
                            int sharedCandidate = GetSharedCandidate(wing1Candidates, potentialWing2Candidates);

                            debugInfo.AppendLine($"Y-Wing found! Hinge: ({hingeRow},{hingeCol}), Wing1: ({wing1Row},{wing1Col}), Wing2: ({potentialWingCell.Row},{potentialWingCell.Column}), Shared Candidate: {sharedCandidate}");

                            var yWingPattern = NormalizePattern((hingeRow, hingeCol), (wing1Row, wing1Col), (potentialWingCell.Row, potentialWingCell.Column));

                            if (_seenYWings.Contains(yWingPattern))
                            {
                                debugInfo.AppendLine($"Skipping duplicate Y-Wing pattern: {yWingPattern}");
                                continue;
                            }

                            _seenYWings.Add(yWingPattern);

                            removedSuccessfully |= RemoveFromAffectedCells(hingeRow, hingeCol, wing1Row, wing1Col, potentialWingCell.Row, potentialWingCell.Column, sharedCandidate, debugInfo);
                        }
                    }
                }
            }
        }
        return removedSuccessfully;
    }
    private bool CandidatesAppearTwice(HashSet<int> totalCandidates, HashSet<int> hingeCandidates, HashSet<int> wing1Candidates, HashSet<int> wing2Candidates)
    {
        // Count the occurrences of each candidate across all three sets.
        Dictionary<int, int> candidateCounts = new Dictionary<int, int>();
        foreach (var candidate in totalCandidates)
        {
            candidateCounts[candidate] = 0;
        }

        foreach (var candidate in hingeCandidates) candidateCounts[candidate]++;
        foreach (var candidate in wing1Candidates) candidateCounts[candidate]++;
        foreach (var candidate in wing2Candidates) candidateCounts[candidate]++;

        // Ensure that each candidate appears exactly twice across the three sets.
        return candidateCounts.Values.All(count => count == 2);
    }

    private bool RemoveFromAffectedCells(int hingeRow, int hingeCol, int wing1Row, int wing1Col, int wing2Row, int wing2Col, int sharedCandidate, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

        // Go through all cells to find ones that can see both wings.
        for (int row = 0; row < PuzzleModel.Size; row++)
        {
            for (int col = 0; col < PuzzleModel.Size; col++)
            {
                // The affected cell cannot be the hinge cell or one of the wing cells.
                if ((row == hingeRow && col == hingeCol) &&
                    (row == wing1Row && col == wing1Col) &&
                    (row == wing2Row && col == wing2Col))
                {
                    continue;
                }

                if (CanSeeBothWings(row, col, wing1Row, wing1Col, wing2Row, wing2Col) && _puzzle.Board[row, col].SolverCandidates.Contains(sharedCandidate))
                {
                    debugInfo.AppendLine($"Removing candidate {sharedCandidate} from cell ({row}, {col})");
                    _puzzle.Board[row, col].SolverCandidates.Remove(sharedCandidate);
                    removedSuccessfully = true;
                }
            }
        }

        return removedSuccessfully;
    }

    private bool CanSeeBothWings(int row, int col, int wing1Row, int wing1Col, int wing2Row, int wing2Col)
    {
        // A cell can see both wings if it's in the same row, column, or block as both wing cells.
        return (IsInSameUnit(row, col, wing1Row, wing1Col) && IsInSameUnit(row, col, wing2Row, wing2Col));
    }

    private bool IsInSameUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 || col1 == col2 || GetBlockIndex(row1, col1) == GetBlockIndex(row2, col2);
    }

    private int GetBlockIndex(int row, int col)
    {
        return (row / 3) * 3 + (col / 3);
    }

    private bool SharesOneCandidate(HashSet<int> set1, HashSet<int> set2)
    {
        return set1.Intersect(set2).Count() == 1;
    }

    private int GetSharedCandidate(HashSet<int> set1, HashSet<int> set2)
    {
        return set1.Intersect(set2).First();
    }
    private bool IsNotTheSameCell(int cell1Row, int cell1Col, int cell2Row, int cell2Col)
    {
        return !(cell1Row == cell2Row && cell1Col == cell2Col);
    }
    private string NormalizePattern((int, int) hinge, (int, int) wing1, (int, int) wing2)
    {
        var cells = new[] { hinge, wing1, wing2 };
        Array.Sort(cells);
        return string.Join("-", cells.Select(c => $"{c.Item1},{c.Item2}"));
    }
}
