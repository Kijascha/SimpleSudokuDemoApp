using SimpleSudoku.CommonLibrary.Models;
using System.Diagnostics;

namespace SimpleSudoku.SudokuSolver
{
    public class BacktrackSolver(IPuzzleModel puzzle)
    {
        private readonly IPuzzleModel _puzzle = puzzle;

        Random _rand = new Random();

        /// <summary>
        /// Solves the Sudoku puzzle using backtracking.
        /// </summary>
        /// <returns>True if the puzzle is solved, false otherwise.</returns>
        public bool Solve()
        {
            // Find the next empty cell (null)
            (int row, int col)? emptyCell = FindEmptyCell();
            if (emptyCell == null)
            {
                // No empty cells left, the puzzle is solved
                return true;
            }

            (int row, int col) = emptyCell.Value;

            var candidates = GetRandomDigits();

            Debug.WriteLine($"[{string.Join(", ", candidates)}]");

            // Try placing each digit from 1 to 9
            for (int digit = 0; digit < candidates.Count; digit++)
            {
                // Check if the digit is valid in the current cell
                if (_puzzle.IsValidDigit(row, col, candidates[digit]))
                {
                    // Place the digit in the cell
                    _puzzle.UpdateDigit(row, col, candidates[digit], validate: false);

                    // Recursively attempt to solve the rest of the puzzle
                    if (Solve())
                    {
                        return true; // If the puzzle is solved, return true
                    }

                    // Backtrack: remove the digit (set it to null)
                    _puzzle.UpdateDigit(row, col, null, validate: false);
                }
            }

            // If no digit works, return false to trigger backtracking
            return false;
        }

        /// <summary>
        /// Finds the next empty cell (cell with a null digit).
        /// </summary>
        /// <returns>A tuple with the row and column of the empty cell, or null if no empty cell is found.</returns>
        private (int, int)? FindEmptyCell()
        {
            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int col = 0; col < PuzzleModel.Size; col++)
                {
                    if (!_puzzle.Digits[row, col].HasValue)
                    {
                        return (row, col); // Return the first empty cell found
                    }
                }
            }

            // No empty cells found
            return null;
        }

        // Helper function to generate a list of digits from 1 to 9 in random order
        private List<int> GetRandomDigits()
        {
            List<int> digits = new List<int>();
            for (int i = 1; i <= PuzzleModel.Size; i++) digits.Add(i);

            // Shuffle the digits randomly
            for (int i = digits.Count - 1; i > 0; i--)
            {
                int j = _rand.Next(i + 1);
                int temp = digits[i];
                digits[i] = digits[j];
                digits[j] = temp;
            }

            return digits;
        }
    }
}
