using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenTripletConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;
        public static HashSet<((int Row, int Column, HashSet<int> Candidates), HashSet<int> Triplet)> HandledTriplets = [];
        public override bool ApplyConstraint(out string errorMessage)
        {
            var foundTriplet = FindHiddenTriplet();
            errorMessage = foundTriplet ? "" : "Couldn't find any Hidden Triplets!";
            return foundTriplet;
        }

        private bool FindHiddenTriplet()
        {
            bool foundTriplet = false;
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    //FindHiddenTripletsInCol(row, col);
                    foundTriplet |= FindTripletInUnit(row, col, SearchUnitType.Row);
                    foundTriplet |= FindTripletInUnit(row, col, SearchUnitType.Column);
                    foundTriplet |= FindTripletInUnit(row, col, SearchUnitType.Box);
                    // Break the loop if a triplet is found and processed
                    if (foundTriplet)
                    {
                        return true;
                    }
                }
            }
            return foundTriplet;
        }

        private bool FindTripletInUnit(int row, int col, SearchUnitType searchUnitType)
        {
            var unitCells = _puzzle.GetUnit(row, col, searchUnitType).ToList();

            for (int i1 = 0; i1 < PuzzleModel.Size - 2; i1++)
            {
                for (int i2 = i1 + 1; i2 < PuzzleModel.Size - 1; i2++)
                {
                    for (int i3 = i2 + 1; i3 < PuzzleModel.Size; i3++)
                    {
                        // ensure the cells we're loopint through are valid
                        if (!(unitCells[i1].Digit == 0 || unitCells[i2].Digit == 0 || unitCells[i3].Digit == 0)) continue;
                        if (unitCells[i1].SolverCandidates.Collection.Count == 0 || unitCells[i2].SolverCandidates.Collection.Count == 0 || unitCells[i3].SolverCandidates.Collection.Count == 0) continue;

                        // combine candidates from all 3 unitCells
                        var combinedCandidates = new HashSet<int>(unitCells[i1].SolverCandidates.Collection); // cell 1 canidates
                        combinedCandidates.UnionWith(unitCells[i2].SolverCandidates.Collection);  // cell 2 canidates
                        combinedCandidates.UnionWith(unitCells[i3].SolverCandidates.Collection);  // cell 3 canidates

                        // triplet found
                        if (combinedCandidates.Count == 3)
                        {
                            var unsolvedCells = unitCells.Where(c => c != unitCells[i1] && c != unitCells[i2] && c != unitCells[i3] && c.Digit == 0).ToList();

                            if (unsolvedCells.Count > 0)
                            {

                                bool anyChanges = false;

                                foreach (var cell in unsolvedCells)
                                {
                                    var cellCandidates = _puzzle.Board[cell.Row, cell.Column].SolverCandidates.Collection.ToHashSet();
                                    var relevantCandidates = cellCandidates.Intersect(combinedCandidates).ToHashSet();

                                    if (relevantCandidates.Count > 0 && !HandledTriplets.Contains(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), combinedCandidates)))
                                    {
                                        var except = cellCandidates.Except(combinedCandidates).ToHashSet();

                                        if (except.Count > 0)
                                        {
                                            cellCandidates.ExceptWith(combinedCandidates);
                                            HandledTriplets.Add(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), combinedCandidates));
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
                                        var cellCandidates = _puzzle.Board[cell.Row, cell.Column].SolverCandidates.Collection;
                                        HandledTriplets.Add(((cell.Row, cell.Column, new HashSet<int>(cellCandidates)), combinedCandidates));
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
