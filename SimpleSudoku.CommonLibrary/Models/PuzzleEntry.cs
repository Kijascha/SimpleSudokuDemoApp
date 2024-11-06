namespace SimpleSudoku.CommonLibrary.Models;

public class PuzzleEntry
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required CellV2[][] Board { get; set; }

    // Override Equals method
    public override bool Equals(object? obj)
    {
        // Null check
        if (obj == null)
        {
            return false;
        }

        // Reference equality check
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        // Type check
        if (obj.GetType() != typeof(Cell))
        {
            return false;
        }

        // Cast the object to Cell and compare Row and Col
        var otherEntry = (PuzzleEntry)obj;
        return this.Id == otherEntry.Id && this.Name == otherEntry.Name && this.Board == otherEntry.Board;
    }

    // Override GetHashCode method
    public override int GetHashCode()
    {
        // Use a prime number to combine hash codes for Row and Col
        return (Id.GetHashCode() * 397) ^ Name.GetHashCode();
    }
}
