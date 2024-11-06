using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SimpleSudoku.CommonLibrary.Models;

public partial class CellModel : ObservableObject
{
    public int Row { get; set; }
    public int Column { get; set; }
    [ObservableProperty] private int? _digit;
    [ObservableProperty] private HashSet<int> _solverCandidates;
    public HashSet<int> PlayerCandidates { get; set; }
    public Thickness CellBorderThickness { get; set; } = new Thickness(.5, .5, .5, .5);
    [ObservableProperty] private SolidColorBrush _cellBackground;
    public bool IsPredefined { get; set; } = false;
    public CellModel()
    {
        PlayerCandidates = [];
        SolverCandidates = [];
        CellBackground = Brushes.White;
    }

}
