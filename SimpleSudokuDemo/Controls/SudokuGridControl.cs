using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleSudokuDemo.Controls;

public class SudokuGridControl : Control
{
    private readonly int _cellSize = 63; // Set to a fixed size
    private Point? _selectionStart = null;

    #region Dependency Properties
    public int GridSize
    {
        get { return (int)GetValue(GridSizeProperty); }
        set { SetValue(GridSizeProperty, value); }
    }
    public bool NeedsRedraw
    {
        get { return (bool)GetValue(NeedsRedrawProperty); }
        set { SetValue(NeedsRedrawProperty, value); }
    }
    public IPuzzleModel Puzzle
    {
        get { return (IPuzzleModel)GetValue(PuzzleProperty); }
        set { SetValue(PuzzleProperty, value); }
    }
    public ObservableCollection<CellV2> SelectedCells
    {
        get { return (ObservableCollection<CellV2>)GetValue(SelectedCellsProperty); }
        set { SetValue(SelectedCellsProperty, value); }
    }
    public Brush CellColor
    {
        get { return (Brush)GetValue(CellColorProperty); }
        set { SetValue(CellColorProperty, value); }
    }
    public Brush SelectedCellColor
    {
        get { return (Brush)GetValue(SelectedCellColorProperty); }
        set { SetValue(SelectedCellColorProperty, value); }
    }
    public Brush GridLineColor
    {
        get { return (Brush)GetValue(GridLineColorProperty); }
        set { SetValue(GridLineColorProperty, value); }
    }
    public Brush DigitColor
    {
        get { return (Brush)GetValue(DigitColorProperty); }
        set { SetValue(DigitColorProperty, value); }
    }
    public Brush PredefinedDigitColor
    {
        get { return (Brush)GetValue(PredefinedDigitColorProperty); }
        set { SetValue(PredefinedDigitColorProperty, value); }
    }
    public Brush CandidateColor
    {
        get { return (Brush)GetValue(CandidateColorProperty); }
        set { SetValue(CandidateColorProperty, value); }
    }
    public GameMode GameMode
    {
        get { return (GameMode)GetValue(GameModeProperty); }
        set { SetValue(GameModeProperty, value); }
    }
    public CandidateMode CandidateMode
    {
        get { return (CandidateMode)GetValue(CandidateModeProperty); }
        set { SetValue(CandidateModeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for GridSize.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GridSizeProperty =
        DependencyProperty.Register("GridSize", typeof(int), typeof(SudokuGridControl), new PropertyMetadata(9));

    // Using a DependencyProperty as the backing store for NeedsRedraw.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NeedsRedrawProperty =
        DependencyProperty.Register("NeedsRedraw", typeof(bool), typeof(SudokuGridControl), new PropertyMetadata(false, OnNeedsRedrawChanged));

    // Using a DependencyProperty as the backing store for Puzzle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PuzzleProperty =
        DependencyProperty.Register("Puzzle", typeof(IPuzzleModel), typeof(SudokuGridControl), new PropertyMetadata(new PuzzleModel()));

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SelectedCellsProperty =
        DependencyProperty.Register("SelectedCells", typeof(ObservableCollection<CellV2>), typeof(SudokuGridControl), new PropertyMetadata(new ObservableCollection<CellV2>()));

    // Using a DependencyProperty as the backing store for CellColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CellColorProperty =
        DependencyProperty.Register("CellColor", typeof(Brush), typeof(SudokuGridControl), new PropertyMetadata(Brushes.White));

    // Using a DependencyProperty as the backing store for SelectedCellColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SelectedCellColorProperty =
        DependencyProperty.Register("SelectedCellColor", typeof(Brush), typeof(SudokuGridControl), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(187, 173, 216, 230))));

    // Using a DependencyProperty as the backing store for GridLineColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GridLineColorProperty =
        DependencyProperty.Register("GridLineColor", typeof(Brush), typeof(SudokuGridControl), new PropertyMetadata(Brushes.Black));

    // Using a DependencyProperty as the backing store for DigitColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DigitColorProperty =
        DependencyProperty.Register("DigitColor", typeof(Brush), typeof(SudokuGridControl), new PropertyMetadata(Brushes.CornflowerBlue));

    // Using a DependencyProperty as the backing store for PredefinedDigitColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PredefinedDigitColorProperty =
        DependencyProperty.Register("PredefinedDigitColor", typeof(Brush), typeof(SudokuGridControl), new PropertyMetadata(Brushes.Black));

    // Using a DependencyProperty as the backing store for CandidateColor.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CandidateColorProperty =
        DependencyProperty.Register("CandidateColor", typeof(Brush), typeof(SudokuGridControl), new PropertyMetadata(Brushes.Gray));

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty GameModeProperty =
        DependencyProperty.Register("GameMode", typeof(GameMode), typeof(SudokuGridControl), new PropertyMetadata(GameMode.Create, OnGameModeChanged));

    // Using a DependencyProperty as the backing store for CandidateMode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CandidateModeProperty =
        DependencyProperty.Register("CandidateMode", typeof(CandidateMode), typeof(SudokuGridControl), new PropertyMetadata(CandidateMode.None, OnCandidateModeChanged));

    private static void OnNeedsRedrawChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SudokuGridControl control && (bool)e.NewValue)
        {
            control.InvalidateVisual();
        }
    }
    private static void OnGameModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SudokuGridControl control && control.NeedsRedraw)
        {
            control.NeedsRedraw = true;
            control.InvalidateVisual();
        }
    }
    private static void OnCandidateModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SudokuGridControl control)
        {
            control.NeedsRedraw = true;
            control.InvalidateVisual();
        }
    }
    #endregion

    static SudokuGridControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SudokuGridControl), new FrameworkPropertyMetadata(typeof(SudokuGridControl)));
    }
    public SudokuGridControl()
    {
        // Set default size for the control
        Width = GridSize * _cellSize;
        Height = GridSize * _cellSize;

        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                Puzzle.Board[i, j] = new CellV2()
                {
                    Row = i,
                    Column = j,
                };
            }
        }
        // Add mouse event handlers
        MouseDown += SudokuGridControl_MouseDown;
        MouseMove += SudokuGridControl_MouseMove;
        MouseUp += SudokuGridControl_MouseUp;
        KeyDown += SudokuGridControl_KeyDown;
        Loaded += UserControl_Loaded;
        this.Focusable = true; // Ensure the control can be focused
        NeedsRedraw = true;
    }

    #region EventHandling

    private void SudokuGridControl_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Get the mouse position relative to the control
        var position = e.GetPosition(this);

        // Compute the cell column and row based on the clicked position
        int col = (int)(position.X / _cellSize);
        int row = (int)(position.Y / _cellSize);

        // Ensure the click is within valid grid bounds
        if (row >= 0 && row < GridSize && col >= 0 && col < GridSize)
        {
            // Start tracking the selection area for MouseMove
            _selectionStart = position;

            // Handle selection logic with Shift key pressed
            if (Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (!SelectedCells.Contains(Puzzle.Board[row, col]))
                {
                    SelectedCells.Add(Puzzle.Board[row, col]); // Select the cell if it wasn't already selected
                }
            }
            else
            {
                // Single selection mode without Shift, clear the previous selection
                SelectedCells.Clear();
                SelectedCells.Add(Puzzle.Board[row, col]); // Add the clicked cell to the selection
            }

            // Force a redraw to reflect the selection change
            NeedsRedraw = true;
            InvalidateVisual();
        }
    }

    private void SudokuGridControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _selectionStart != null)
        {
            var position = e.GetPosition(this);
            int row = (int)(position.Y / _cellSize);
            int col = (int)(position.X / _cellSize);

            // Ensure the mouse is within valid grid bounds
            if (row >= 0 && row < GridSize && col >= 0 && col < GridSize)
            {
                // Handle the selection toggle with Shift key pressed
                if (Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (!SelectedCells.Contains(Puzzle.Board[row, col]))
                    {

                        SelectedCells.Add(Puzzle.Board[row, col]); // Select the cell if it wasn't already selected
                        NeedsRedraw = true;
                        //InvalidateVisual(); // Redraw to reflect the selection/deselection
                    }
                }
            }
        }
    }
    private void SudokuGridControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _selectionStart = null;
    }
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        this.Focus();
    }
    private void SudokuGridControl_KeyDown(object sender, KeyEventArgs e)
    {
        // Handle arrow keys for moving selection
        if (e.Key == Key.Up)
        {
            MoveSelection(-1, 0); // Move up
            return;
        }
        else if (e.Key == Key.Down)
        {
            MoveSelection(1, 0); // Move down
            return;
        }
        else if (e.Key == Key.Left)
        {
            MoveSelection(0, -1); // Move left
            return;
        }
        else if (e.Key == Key.Right)
        {
            MoveSelection(0, 1); // Move right
            return;
        }

        if (Keyboard.IsKeyDown(Key.LeftCtrl))
            CandidateMode = CandidateMode.CornerCandidates;
        else if (Keyboard.IsKeyDown(Key.LeftShift))
            CandidateMode = CandidateMode.CenterCandidates;
        else if (Keyboard.IsKeyDown(Key.X))
            CandidateMode = CandidateMode.None;

        // Check for NumPad0 to clear all candidates
        if (CandidateMode == CandidateMode.CornerCandidates && e.Key == Key.NumPad0)
        {
            foreach (var cell in SelectedCells)
            {
                Puzzle.Board[cell.Row, cell.Column].CornerCandidates.Clear(); // Clear the cell
            }
            NeedsRedraw = true;
            InvalidateVisual(); // Redraw to reflect changes
            return;
        }
        // Check for NumPad0 to clear all candidates
        else if (CandidateMode == CandidateMode.CenterCandidates && e.Key == Key.NumPad0)
        {
            foreach (var cell in SelectedCells)
            {
                Puzzle.Board[cell.Row, cell.Column].CenterCandidates.Clear(); // Clear the cell
            }
            NeedsRedraw = true;
            InvalidateVisual(); // Redraw to reflect changes
            return;
        }
        else if (CandidateMode == CandidateMode.SolverCandidates && e.Key == Key.NumPad0)
        {
            foreach (var cell in SelectedCells)
            {
                Puzzle.Board[cell.Row, cell.Column].SolverCandidates.Clear(); // Clear the cell
            }
            NeedsRedraw = true;
            InvalidateVisual(); // Redraw to reflect changes
            return;
        }
        else if (e.Key == Key.NumPad0)
        {
            foreach (var cell in SelectedCells)
            {
                SetCellValue(cell.Row, cell.Column, 0); // Clear the cell
            }
            NeedsRedraw = true;
            InvalidateVisual(); // Redraw to reflect changes
            return;
        }

        // Check for number input for candidates when Ctrl is pressed
        if (CandidateMode == CandidateMode.CornerCandidates)
        {
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                int candidate = (int)(e.Key - Key.NumPad0);
                foreach (var cell in SelectedCells)
                {
                    Puzzle.UpdateCandidate(cell.Row, cell.Column, candidate, GameMode, false, CandidateMode.CornerCandidates);
                }
                NeedsRedraw = true;
                InvalidateVisual(); // Redraw to reflect changes
            }
        }
        else if (CandidateMode == CandidateMode.CenterCandidates)
        {
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                int candidate = (int)(e.Key - Key.NumPad0);
                foreach (var cell in SelectedCells)
                {
                    Puzzle.UpdateCandidate(cell.Row, cell.Column, candidate, GameMode, false, CandidateMode.CenterCandidates);
                }
                NeedsRedraw = true;
                InvalidateVisual(); // Redraw to reflect changes
            }
        }
        else if (CandidateMode == CandidateMode.SolverCandidates)
        {
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                int candidate = (int)(e.Key - Key.NumPad0);
                foreach (var cell in SelectedCells)
                {
                    Puzzle.UpdateCandidate(cell.Row, cell.Column, candidate, GameMode, true, CandidateMode.SolverCandidates);
                }
                NeedsRedraw = true;
                InvalidateVisual(); // Redraw to reflect changes
            }
        }
        // Handle number keys (1-9) for digit entry when not holding Ctrl
        else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
        {
            int digit = (int)(e.Key - Key.NumPad0); // Convert Key to corresponding digit

            foreach (var cell in SelectedCells)
            {
                SetCellValue(cell.Row, cell.Column, digit); // Implement this method
            }

            NeedsRedraw = true;
            InvalidateVisual(); // Redraw to reflect changes
        }
    }
    #endregion

    #region Drawing
    private void DrawGrid(DrawingContext dc)
    {
        // Draw the grid cells as white squares
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                // Calculate the top-left corner of the current cell
                double x = col * _cellSize;
                double y = row * _cellSize;

                // Draw the selected cell with a different color
                if (!SelectedCells.Contains(Puzzle.Board[row, col]))
                {
                    dc.DrawRectangle(CellColor, null, new Rect(x, y, _cellSize, _cellSize));
                }

                // Draw the cell value (if any)
                if (Puzzle.Board[row, col].Digit != 0)
                {
                    DrawCellValue(dc, row, col, x, y);
                }
            }
        }

        // Draw both corner and center candidates within each cell
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (Puzzle.Board[row, col].Digit == 0)
                {
                    var cornerCandidates = Puzzle.Board[row, col].CornerCandidates.Collection.ToArray();
                    var centerCandidates = Puzzle.Board[row, col].CenterCandidates.Collection.ToArray();
                    var solverCandidates = Puzzle.Board[row, col].SolverCandidates.Collection.ToArray();

                    if (CandidateMode == CandidateMode.SolverCandidates)
                    {
                        // Draw solver candidates as corner candidates
                        DrawCornerCandidates(dc, row, col, solverCandidates);
                    }
                    else
                    {
                        // Draw actual corner and center candidates as usual
                        DrawCornerCandidates(dc, row, col, cornerCandidates);
                        DrawCenterCandidates(dc, row, col, centerCandidates);
                    }
                }
            }
        }
        // Draw grid lines with varying thickness
        DrawGridLines(dc);
    }
    private void DrawSelectedCells(DrawingContext dc)
    {
        foreach (var cell in SelectedCells)
        {
            // Highlight selected cells with a light blue background
            dc.DrawRectangle(SelectedCellColor, null,
                             new Rect(cell.Column * _cellSize, cell.Row * _cellSize, _cellSize, _cellSize));
        }
    }
    private void DrawCellValue(DrawingContext dc, int row, int col, double x, double y)
    {
        // Similar logic for drawing the cell value based on whether it's predefined
        Brush digitColor = Puzzle.Board[row, col].IsPredefined ? PredefinedDigitColor : DigitColor;

        FormattedText formattedText = new FormattedText(
            Puzzle.Board[row, col].Digit.ToString(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            32, // Font size for digits
            digitColor,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        double textX = x + (_cellSize - formattedText.Width) / 2;
        double textY = y + (_cellSize - formattedText.Height) / 2;

        dc.DrawText(formattedText, new Point(textX, textY));
    }
    private void DrawCenterCandidates(DrawingContext dc, int row, int col, int[] candidates)
    {
        double cellX = col * _cellSize;
        double cellY = row * _cellSize;

        if (candidates.Length > 0)
        {
            double fontSize = 16; // Initial font size
            double letterSpacing = 0; // Initial spacing between characters
            double totalWidth;

            // Loop to adjust font size until the text fits within cell width
            List<FormattedText> formattedCharacters;
            do
            {
                formattedCharacters = candidates.Select(c => CreateFormattedText(c.ToString(), fontSize)).ToList();
                totalWidth = formattedCharacters.Sum(f => f.Width) + (formattedCharacters.Count - 1) * letterSpacing;

                if (totalWidth > _cellSize) fontSize -= 2; // Reduce font size if text overflows cell
            } while (totalWidth > _cellSize && fontSize > 8);

            // Calculate starting point for centered text
            double textX = cellX + (_cellSize - totalWidth) / 2;
            double textY = cellY + (_cellSize - formattedCharacters[0].Height) / 2 + 7;

            // Draw each character with the specified spacing
            foreach (var formattedChar in formattedCharacters)
            {
                dc.DrawText(formattedChar, new Point(textX, textY));
                textX += formattedChar.Width + letterSpacing; // Move X position for next character
            }
        }
    }
    private void DrawCornerCandidates(DrawingContext dc, int row, int col, int[] candidates)
    {
        double cellX = col * _cellSize;
        double cellY = row * _cellSize;

        double candidateXOffset = 6; // Small margin for left position
        double candidateYOffset = 0; // Small margin for top position
        double candidateSpacingX = _cellSize / 2.8; // Space between candidates horizontally
        double candidateSpacingY = _cellSize / 3; // Space between candidates vertically

        double startX = cellX + (_cellSize - (candidateSpacingX * 3)) / 2 + candidateXOffset;
        double startY = cellY + (_cellSize - (candidateSpacingY * 3)) / 2 + candidateYOffset;

        for (int i = 0; i < candidates.Length; i++)
        {
            int candidate = candidates[i];
            int index = i + 1 - 1;
            int gridRow = index / 3; // 0, 1, or 2
            int gridCol = index % 3; // 0, 1, or 2

            double textX = startX + gridCol * candidateSpacingX;
            double textY = startY + gridRow * candidateSpacingY;

            FormattedText formattedText = new FormattedText(
                candidate.ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                18, // Fixed font size for corner candidates
                CandidateColor,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(formattedText, new Point(textX, textY));
        }
    }
    private void DrawGridLines(DrawingContext dc)
    {
        Pen thinPen = new Pen(GridLineColor, 1); // Thin lines for cell borders
        Pen thickPen = new Pen(GridLineColor, 3); // Thick lines for 3x3 grid borders

        for (int i = 0; i <= GridSize; i++)
        {
            if (i % 3 == 0)
            {
                dc.DrawLine(thickPen, new Point(i * _cellSize + 1, 0), new Point(i * _cellSize + 1, GridSize * _cellSize + 2));
                dc.DrawLine(thickPen, new Point(0, i * _cellSize + 1), new Point(GridSize * _cellSize + 2, i * _cellSize + 1));
            }
            else
            {
                dc.DrawLine(thinPen, new Point(i * _cellSize + 0.5, 0), new Point(i * _cellSize + 0.5, GridSize * _cellSize));
                dc.DrawLine(thinPen, new Point(0, i * _cellSize + 0.5), new Point(GridSize * _cellSize, i * _cellSize + 0.5));
            }
        }
    }
    #endregion

    #region Helper Methods
    // Helper function to create FormattedText
    private FormattedText CreateFormattedText(string text, double fontSize)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            fontSize,
            CandidateColor,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }
    private void SetCellValue(int row, int col, int value)
    {
        if (GameMode == GameMode.Create)
        {
            Puzzle.UpdateDigit(row, col, value, GameMode, CandidateMode);

            if (Puzzle.Board[row, col].Digit != 0)
            {
                Puzzle.Board[row, col].IsPredefined = true;
            }
            else
            {
                Puzzle.Board[row, col].IsPredefined = false;
            }

            // Clear candidates when a digit is set (for simplicity)
            if (value != 0)
            {
                Puzzle.Board[row, col].CornerCandidates.Clear();
                Puzzle.Board[row, col].CenterCandidates.Clear();
            }
        }
        else if (GameMode == GameMode.Play)
        {
            if (!Puzzle.Board[row, col].IsPredefined)
            {
                Puzzle.UpdateDigit(row, col, value, GameMode, CandidateMode);

                // Clear candidates when a digit is set (for simplicity)
                if (value != 0)
                {
                    Puzzle.Board[row, col].CornerCandidates.Clear();
                    Puzzle.Board[row, col].CenterCandidates.Clear();
                }
            }
        }
    }
    private void MoveSelection(int rowDelta, int colDelta)
    {
        if (SelectedCells.Count > 0)
        {
            // Get the current selected cell
            var currentCell = SelectedCells[SelectedCells.Count - 1]; // Get the last selected cell

            // Calculate new row and column
            int newRow = Math.Clamp(currentCell.Row + rowDelta, 0, GridSize - 1);
            int newCol = Math.Clamp(currentCell.Column + colDelta, 0, GridSize - 1);

            // Update selected cells to contain the new cell
            SelectedCells.Clear(); // Clear previous selections
            SelectedCells.Add(Puzzle.Board[newRow, newCol]); // Add the new selected cell

            NeedsRedraw = true;
            InvalidateVisual(); // Redraw to reflect the change
        }
    }
    #endregion

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // Disable anti-aliasing for sharp grid lines
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        DrawSelectedCells(drawingContext);
        DrawGrid(drawingContext);

        NeedsRedraw = false; // Reset the flag after drawing
    }
}
