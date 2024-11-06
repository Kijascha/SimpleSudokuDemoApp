using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SimpleSudokuDemo.Views
{
    /// <summary>
    /// Interaktionslogik für PlayView.xaml
    /// </summary>
    public partial class PlayView : UserControl
    {
        public PlayView()
        {
            InitializeComponent();
            SaveDialogBG.Visibility = Visibility.Collapsed;
            SaveDialog.Visibility = Visibility.Collapsed;
            LoadDialog.Visibility = Visibility.Collapsed;
            NameText.Focusable = false;
            DescriptionText.Focusable = false;
        }

        private void StartSlideInAnimation()
        {
            var storyboard = (Storyboard)FindResource("SlideInStoryboard");
            storyboard.Begin();
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
            var storyboard = (Storyboard)FindResource("SlideInStoryboard");
            storyboard.Stop();

            var storyboard2 = (Storyboard)FindResource("SlideOutStoryboard");
            storyboard2.Completed += Storyboard2_Completed;
            storyboard2.Begin();
        }

        private void Storyboard2_Completed(object? sender, EventArgs e)
        {
            var storyboard2 = (Storyboard)FindResource("SlideOutStoryboard");
            storyboard2.Stop();

            LoadDialog.Visibility = Visibility.Collapsed;
            SudokuGrid.Focus();
        }

        private void OpenLoadDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LoadDialog.Visibility == Visibility.Collapsed)
            {
                LoadDialog.Visibility = Visibility.Visible;
            }
            StartSlideInAnimation();

        }
        private void OpenSettingsPanelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsDialog.Visibility == Visibility.Collapsed)
            {
                SettingsDialog.Visibility = Visibility.Visible;
            }
            var slideIn = (Storyboard)FindResource("SettingsSlideInStoryboard");
            slideIn.Begin();
        }
        private void CloseSettingsDialog_Click(object sender, RoutedEventArgs e)
        {
            var slideIn = (Storyboard)FindResource("SettingsSlideInStoryboard");
            slideIn.Stop();

            var slideOut = (Storyboard)FindResource("SettingsSlideOutStoryboard");
            slideOut.Completed += SlideOut_Completed;
            slideOut.Begin();

        }

        private void SlideOut_Completed(object? sender, EventArgs e)
        {
            var storyboard2 = (Storyboard)FindResource("SlideOutStoryboard");
            storyboard2.Stop();

            SettingsDialog.Visibility = Visibility.Collapsed;
            SudokuGrid.Focus();
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
