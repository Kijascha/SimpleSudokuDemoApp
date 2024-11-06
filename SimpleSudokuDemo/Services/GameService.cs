using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.SudokuSolver;

namespace SimpleSudokuDemo.Services;

public class GameService(IPuzzleModel puzzleModel, IBacktrackSolver backtrackSolver, IConstraintSolver constraintSolver) : IGameService
{
    private readonly IPuzzleModel _puzzleModel = puzzleModel;

    public IBacktrackSolver BacktrackSolver { get; } = backtrackSolver;
    public IConstraintSolver ConstraintSolver { get; } = constraintSolver;

    // Create Game
    // Method to create a puzzle with a symmetric pattern
    public IPuzzleModel CreatePuzzle(int numToRemove)
    {
        // Create a copy of the solved puzzle from _puzzleModel
        var puzzle = _puzzleModel;  // Assuming Clone() deep copies the puzzle grid

        int gridSize = PuzzleModel.Size;  // Assuming puzzle.Grid is a 2D array representing the board
        int removedPairs = 0;
        int maxAttempts = 1000;  // Safety mechanism to prevent infinite loops
        Random random = new Random();
        int attempts = 0;

        // Continue removing pairs until we reach the desired number of removals
        while (removedPairs < numToRemove / 2 && attempts < maxAttempts)
        {
            attempts++;

            // Randomly select a position in the grid
            int i = random.Next(0, gridSize);
            int j = random.Next(0, gridSize);

            // Ensure the selected cell and its symmetric counterpart are not already empty
            if (puzzle.Board[i, j].Digit != 0 && puzzle.Board[gridSize - 1 - i, gridSize - 1 - j].Digit != 0)
            {
                // Remove the digit at (i, j) and its symmetric counterpart
                puzzle.Board[i, j].Digit = 0;
                puzzle.Board[gridSize - 1 - i, gridSize - 1 - j].Digit = 0;

                // Increment the count of removed pairs
                removedPairs++;
                attempts = 0;  // Reset attempts since a successful removal was made
            }
        }

        if (removedPairs < numToRemove / 2)
        {
            // If unable to remove enough pairs, consider throwing an exception, returning partial board, or reattempting.
            throw new InvalidOperationException("Could not remove enough digits to create the puzzle.");
        }

        return puzzle;  // Return the puzzle with digits removed symmetrically
    }
    // Save Game

    // Load Game

    // Difficulty Rating

    // GameTimer

    // Solvability Check

    // Board States for saving solving steps
}
