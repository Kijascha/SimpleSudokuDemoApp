using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using System.Diagnostics;
using System.Text;

namespace SimpleSudoku.ConstraintLibrary.Constraints;

public class YWingConstraint(IPuzzleModel puzzle) : Constraint
{
    private readonly IPuzzleModel _puzzle = puzzle;
    private readonly HashSet<string> _seenYWings = new();

    public override bool ApplyConstraint(out string errorMessage)
    {
        bool removedSuccessfully = false;
        StringBuilder debugInfo = new StringBuilder();

        // Iterate through all cells to look for potential hinge cells (A).
        for (int row = 0; row < PuzzleModel.Size; row++)
        {
            for (int col = 0; col < PuzzleModel.Size; col++)
            {
                var cellCandidates = _puzzle.SolverCandidates[row, col];

                // A hinge cell must have exactly 2 candidates.
                if (cellCandidates.Count == 2)
                {
                    HashSet<int> hingeCandidates = new HashSet<int>(cellCandidates);

                    // Look for potential wings in rows, columns, and blocks.
                    removedSuccessfully |= FindYWingV2(row, col, hingeCandidates, debugInfo);
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

    private bool FindYWingV2(int hingeRow, int hingeCol, HashSet<int> hingeCandidates, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;

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

        foreach (var potentialWingCell in unit)
        {
            if (potentialWingCell.Candidates.Count == 2)
            {
                if (SharesOneCandidate(hingeCandidates, potentialWingCell.Candidates))
                {
                    removedSuccessfully |= TryFindYWingPartnerV2(hingeRow, hingeCol, potentialWingCell.Row, potentialWingCell.Column, hingeCandidates, potentialWingCell.Candidates, unitType, debugInfo);
                }
            }
        }

        return removedSuccessfully;
    }

    private bool TryFindYWingPartnerV2(int hingeRow, int hingeCol, int wing1Row, int wing1Col, HashSet<int> hingeCandidates, HashSet<int> wing1Candidates, SearchUnitType wing1Unit, StringBuilder debugInfo)
    {
        bool removedSuccessfully = false;
        // Search in other units depending on the current search context.
        foreach (var unitType in Enum.GetValues(typeof(SearchUnitType)).Cast<SearchUnitType>())
        {
            if (wing1Unit == SearchUnitType.Column)
            {
                var unit = _puzzle.GetUnit(hingeRow, hingeCol, unitType);
                foreach (var potentialWingCell in unit)
                {
                    if (potentialWingCell.Candidates.Count == 2 &&
                        SharesOneCandidate(hingeCandidates, potentialWingCell.Candidates) &&
                        SharesOneCandidate(wing1Candidates, potentialWingCell.Candidates))
                    {

                        // Ensure that the combined candidates across the hinge, wing1, and wing2 total exactly 3 unique candidates.
                        var totalCandidates = new HashSet<int>(hingeCandidates);
                        totalCandidates.UnionWith(wing1Candidates);
                        totalCandidates.UnionWith(potentialWingCell.Candidates);

                        if (totalCandidates.Count == 3 &&
                            CandidatesAppearTwice(totalCandidates, hingeCandidates, wing1Candidates, potentialWingCell.Candidates) &&
                            !IsInSameUnit(wing1Row, wing1Col, potentialWingCell.Row, potentialWingCell.Column))
                        {

                            int sharedCandidate = GetSharedCandidate(wing1Candidates, potentialWingCell.Candidates);

                            // Normalize the Y-Wing pattern by sorting the cells.
                            var yWingPattern = NormalizePattern((hingeRow, hingeCol), (wing1Row, wing1Col), (potentialWingCell.Row, potentialWingCell.Column));

                            if (_seenYWings.Contains(yWingPattern)) continue;

                            debugInfo.AppendLine($"yWingPattern: {yWingPattern}");
                            debugInfo.AppendLine($"Potential Hinge Cell: [{hingeRow}, {hingeCol}] with candidates {string.Join(", ", hingeCandidates)}");
                            debugInfo.AppendLine($"Potential Wing 1 Cell: [{potentialWingCell.Row}, {potentialWingCell.Column}] with candidates {string.Join(", ", potentialWingCell.Candidates)}");
                            debugInfo.AppendLine($"Potential Wing 2 Cell: [{potentialWingCell.Row}, {potentialWingCell.Column}] with candidates {string.Join(", ", potentialWingCell.Candidates)}");
                            debugInfo.AppendLine();

                            // Add the current Y-Wing to the seen set.
                            _seenYWings.Add(yWingPattern);

                            // Perform candidate removal from affected cells.
                            removedSuccessfully |= RemoveFromAffectedCells(hingeRow, hingeCol, wing1Row, wing1Col, potentialWingCell.Row, potentialWingCell.Column, sharedCandidate, debugInfo);

                            if (removedSuccessfully)
                            {
                                // Add debug information about the Y-Wing found.
                                debugInfo.AppendLine("Y-Wing Found:");
                                debugInfo.AppendLine($"Hinge Cell: [{hingeRow}, {hingeCol}] with candidates {string.Join(", ", hingeCandidates)}");
                                debugInfo.AppendLine($"Wing 1 Cell: [{wing1Row}, {wing1Col}] with candidates {string.Join(", ", wing1Candidates)}");
                                debugInfo.AppendLine($"Wing 2 Cell: [{potentialWingCell.Row}, {potentialWingCell.Column}] with candidates {string.Join(", ", potentialWingCell.Candidates)}");
                                debugInfo.AppendLine($"Shared Candidate {sharedCandidate}");
                                debugInfo.AppendLine();
                            }
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

                if (CanSeeBothWings(row, col, wing1Row, wing1Col, wing2Row, wing2Col) && _puzzle.SolverCandidates[row, col].Contains(sharedCandidate))
                {
                    _puzzle.SolverCandidates[row, col].Remove(sharedCandidate);
                    removedSuccessfully = true;

                    // Add debug information about the candidate removal.
                    debugInfo.AppendLine($"Removed candidate {sharedCandidate} from cell [{row}, {col}].");
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
