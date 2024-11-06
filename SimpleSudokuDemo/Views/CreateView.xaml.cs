using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleSudokuDemo.Views
{
    /// <summary>
    /// Interaktionslogik für CreateView.xaml
    /// </summary>
    public partial class CreateView : UserControl
    {
        public CreateView()
        {
            InitializeComponent();
            SaveDialogBG.Visibility = Visibility.Collapsed;
            SaveDialog.Visibility = Visibility.Collapsed;
            LoadDialogBG.Visibility = Visibility.Collapsed;
            LoadDialog.Visibility = Visibility.Collapsed;
            NameText.Focusable = false;
            DescriptionText.Focusable = false;
        }

        private void OpenSaveDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDialogBG.Visibility == Visibility.Collapsed)
            {
                SaveDialogBG.Visibility = Visibility.Visible;
            }
            if (SaveDialog.Visibility == Visibility.Collapsed)
            {
                SaveDialog.Visibility = Visibility.Visible;
                NameText.Focusable = true;
                DescriptionText.Focusable = true;
            }
        }

        private void CloseSaveDialog_Click(object sender, RoutedEventArgs e)
        {
            SaveDialogBG.Visibility = Visibility.Collapsed;
            SaveDialog.Visibility = Visibility.Collapsed;
            NameText.Focusable = false;
            DescriptionText.Focusable = false;

            SudokuGrid.Focus();
        }

        private void CloseLoadDialog_Click(object sender, RoutedEventArgs e)
        {
            LoadDialogBG.Visibility = Visibility.Collapsed;
            LoadDialog.Visibility = Visibility.Collapsed;

            SudokuGrid.Focus();
        }

        private void OpenLoadDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LoadDialogBG.Visibility == Visibility.Collapsed)
            {
                LoadDialogBG.Visibility = Visibility.Visible;
            }
            if (LoadDialog.Visibility == Visibility.Collapsed)
            {
                LoadDialog.Visibility = Visibility.Visible;
            }

        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up) { SudokuGrid.Focus(); }
            else if (e.Key == Key.Down) { SudokuGrid.Focus(); }
            else if (e.Key == Key.Left) { SudokuGrid.Focus(); }
            else if (e.Key == Key.Right) { SudokuGrid.Focus(); }
        }
    }
}
