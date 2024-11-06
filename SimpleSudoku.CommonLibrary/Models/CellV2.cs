using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleSudoku.CommonLibrary.Models;

public partial class CellV2 : ObservableObject
{
    public int Row { get; set; }
    public int Column { get; set; }
    [ObservableProperty] private int _digit;
    [ObservableProperty] private Candidates _solverCandidates = new();
    [ObservableProperty] private Candidates _centerCandidates = new();
    [ObservableProperty] private Candidates _cornerCandidates = new();
    [ObservableProperty] private bool _isPredefined;

    public CellV2()
    {
        _centerCandidates.Clear();
        _cornerCandidates.Clear();
        _digit = 0;
        _isPredefined = false;
    }
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
        var otherCell = (Cell)obj;
        return this.Row == otherCell.Row && this.Column == otherCell.Column;
    }

    // Override GetHashCode method
    public override int GetHashCode()
    {
        // Use a prime number to combine hash codes for Row and Col
        return (Row * 397) ^ Column;
    }

    public CellV2 Clone()
    {
        return new CellV2()
        {
            Row = Row,
            Column = Column,
            Digit = Digit,
            IsPredefined = IsPredefined,
            SolverCandidates = new Candidates(SolverCandidates.BitMask),
            CenterCandidates = new Candidates(CenterCandidates.BitMask),
            CornerCandidates = new Candidates(CornerCandidates.BitMask)
        };
    }
}
