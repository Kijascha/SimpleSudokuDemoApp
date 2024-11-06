using SimpleSudoku.CommonLibrary.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace SimpleSudokuDemo.Core
{
    public class Collection2DConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Cell[,] collection2D)
                return new ObservableCollection<Cell>();

            var collection1D = new ObservableCollection<Cell>();

            for (int row = 0; row < PuzzleModel.Size; row++)
            {
                for (int column = 0; column < PuzzleModel.Size; column++)
                {
                    collection1D.Add(collection2D[row, column]);
                }
            }

            return collection1D;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ObservableCollection<Cell> collection1D)
                return new Cell[PuzzleModel.Size, PuzzleModel.Size];

            var collection2D = new Cell[PuzzleModel.Size, PuzzleModel.Size];

            foreach (var cell in collection1D)
            {
                if (cell.Row < PuzzleModel.Size && cell.Column < PuzzleModel.Size)
                {
                    collection2D[cell.Row, cell.Column] = cell;
                }
            }

            return collection2D;
        }
    }
}
