using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SimpleSudoku.CommonLibrary.Models
{
    public partial class Cell : ObservableObject
    {
        public int Row { get; set; }
        public int Column { get; set; }
        [ObservableProperty] private int? _digit;
        [ObservableProperty] private int _digitValue;
        public Candidates PlayerCandidates { get; set; } = new Candidates();
        [ObservableProperty] private Candidates _solverCandidates = new Candidates();
        [ObservableProperty] private Candidates _centerCandidates = new Candidates();
        [ObservableProperty] private Candidates _cornerCandidates = new Candidates();
        [ObservableProperty] private bool _isPredefined;

        public Thickness BorderThickness { get; set; } = new Thickness(.5, .5, .5, .5);
        public SolidColorBrush Background { get; set; } = Brushes.White;

        public Cell()
        {
            PlayerCandidates.Clear();
            _centerCandidates.Clear();
            _cornerCandidates.Clear();
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

        public Cell Clone()
        {
            return new Cell()
            {
                Row = Row,
                Column = Column,
                Digit = Digit,
                DigitValue = DigitValue,
                IsPredefined = IsPredefined,
                SolverCandidates = new Candidates(SolverCandidates.BitMask),
                CenterCandidates = new Candidates(CenterCandidates.BitMask),
                CornerCandidates = new Candidates(CornerCandidates.BitMask)
            };
        }

    }
}
