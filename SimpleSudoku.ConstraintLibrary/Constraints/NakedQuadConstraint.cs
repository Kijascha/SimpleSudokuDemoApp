using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class NakedQuadConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;
        public static HashSet<((int Row, int Column, HashSet<int> Candidates), HashSet<int> Quad)> HandledQuads = [];

        public override bool ApplyConstraint(out string errorMessage)
        {
            var foundQuad = FindNakedQuad();
            errorMessage = foundQuad ? "" : "Couldn't find any Naked Quads!";
            return foundQuad;
        }

        private bool FindNakedQuad()
        {
            bool foundQuad = false;
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    //FindHiddenTripletsInCol(row, col);
                    foundQuad |= FindQuadInUnit(row, col, SearchUnitType.Row);
                    foundQuad |= FindQuadInUnit(row, col, SearchUnitType.Column);
                    foundQuad |= FindQuadInUnit(row, col, SearchUnitType.Box);
                    // Break the loop if a triplet is found and processed
                    if (foundQuad)
                    {
                        return true;
                    }
                }
            }
            return foundQuad;
        }

        private bool FindQuadInUnit(int row, int col, SearchUnitType searchUnitType)
        {
            var unitCells = _puzzle.GetUnit(row, col, searchUnitType).ToList();

            for (int i1 = 0; i1 < PuzzleModel.Size - 3; i1++)
            {
                for (int i2 = i1 + 1; i2 < PuzzleModel.Size - 2; i2++)
                {
                    for (int i3 = i2 + 1; i3 < PuzzleModel.Size - 1; i3++)
                    {
                        for (int i4 = i3 + 1; i4 < PuzzleModel.Size; i4++)
                        {
                            // ensure the cells we're loopint through are valid
                            if (!(unitCells[i1].Digit == null || unitCells[i2].Digit == null || unitCells[i3].Digit == null || unitCells[i4].Digit == null)) continue;
                            if (unitCells[i1].Candidates.Count == 0 || unitCells[i2].Candidates.Count == 0 || unitCells[i3].Candidates.Count == 0 || unitCells[i4].Candidates.Count == 0) continue;

                            // combine candidates from all 3 unitCells
                            var combinedCandidates = new HashSet<int>(unitCells[i1].Candidates); // cell 1 canidates
                            combinedCandidates.UnionWith(unitCells[i2].Candidates);  // cell 2 canidates
                            combinedCandidates.UnionWith(unitCells[i3].Candidates);  // cell 3 canidates
                            combinedCandidates.UnionWith(unitCells[i4].Candidates);  // cell 3 canidates

                            // triplet found
                            if (combinedCandidates.Count == 4)
                            {
                                var unsolvedCells = unitCells.Where(c => c != unitCells[i1] && c != unitCells[i2] && c != unitCells[i3] && c != unitCells[i4] && c.Digit == null).ToList();

                                if (unsolvedCells.Count > 0)
                                {

                                    bool anyChanges = false;

                                    foreach (var cell in unsolvedCells)
                                    {
                                        var cellCandidates = _puzzle.SolverCandidates[cell.Row, cell.Column];
                                        var relevantCandidates = cellCandidates.Intersect(combinedCandidates).ToHashSet();

                                        if (relevantCandidates.Count > 0 && !HandledQuads.Contains(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), combinedCandidates)))
                                        {
                                            var except = cellCandidates.Except(combinedCandidates).ToHashSet();

                                            if (except.Count > 0)
                                            {
                                                cellCandidates.ExceptWith(combinedCandidates);
                                                HandledQuads.Add(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), combinedCandidates));
                                                anyChanges = true;
                                            }
                                        }
                                    }

                                    if (anyChanges)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        // If no changes were made, log this triplet to avoid reprocessing
                                        foreach (var cell in unsolvedCells)
                                        {
                                            var cellCandidates = _puzzle.SolverCandidates[cell.Row, cell.Column];
                                            HandledQuads.Add(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), combinedCandidates));
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
    }
}
