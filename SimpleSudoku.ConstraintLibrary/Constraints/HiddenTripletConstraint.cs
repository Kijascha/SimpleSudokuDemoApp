﻿using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;

namespace SimpleSudoku.ConstraintLibrary.Constraints
{
    public class HiddenTripletConstraint(IPuzzleModel puzzle) : Constraint
    {
        private readonly IPuzzleModel _puzzle = puzzle;

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
                        if (!(unitCells[i1].Digit == null || unitCells[i2].Digit == null || unitCells[i3].Digit == null)) continue;
                        if (unitCells[i1].Candidates.Count == 0 || unitCells[i2].Candidates.Count == 0 || unitCells[i3].Candidates.Count == 0) continue;

                        // combine candidates from all 3 unitCells
                        var combindedCandidates = new HashSet<int>(unitCells[i1].Candidates); // cell 1 canidates
                        combindedCandidates.UnionWith(unitCells[i2].Candidates);  // cell 2 canidates
                        combindedCandidates.UnionWith(unitCells[i3].Candidates);  // cell 3 canidates

                        // triplet found
                        if (combindedCandidates.Count == 3)
                        {
                            var unsolvedCells = unitCells.Where(c => c != unitCells[i1] && c != unitCells[i2] && c != unitCells[i3] && c.Digit == null);

                            // handle removal from other valid cells in the unit
                            foreach (var cell in unsolvedCells)
                            {
                                _puzzle.SolverCandidates[cell.Row, cell.Column].ExceptWith(combindedCandidates);
                            }
                            // found triplet
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
