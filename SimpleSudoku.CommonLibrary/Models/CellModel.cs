using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace SimpleSudoku.CommonLibrary.Models;

public partial class CellModel : ObservableObject
{
    public int Row { get; set; }
    public int Column { get; set; }
    [ObservableProperty] private int? _digit;
    public HashSet<int> SolverCandidates { get; set; }
    public HashSet<int> PlayerCandidates { get; set; }
    public Thickness CellBorderThickness { get; set; } = new Thickness(.5, .5, .5, .5);

}
