using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleSudoku.CommonLibrary.Models;
using SimpleSudoku.CommonLibrary.System;
using SimpleSudokuDemo.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace SimpleSudokuDemo.ViewModels;

public partial class PlayViewModel : ViewModel
{
    [ObservableProperty] private IPuzzleModel _puzzle;
    [ObservableProperty] private ObservableCollection<CellV2> _selectedCells = [];
    [ObservableProperty] private bool _needsRedraw = false;
    [ObservableProperty] private CandidateMode _candidateMode;
    [ObservableProperty] private GameMode _gameMode = GameMode.Play;
    [ObservableProperty] private string _puzzleName = "Default Puzzle";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private PuzzleEntry? _selectedPuzzle;
    [ObservableProperty] private ObservableCollection<PuzzleEntry> _puzzleEntries = [];
    [ObservableProperty] private bool _developerMode;

    private readonly IGameService _gameService;
    private readonly IConfiguration _config;
    private readonly IAppSettingsService _appSettingsService;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public PlayViewModel(
        INavigationService navigationService,
        IServiceProvider serviceProvider,
        IPuzzleModel puzzle,
        IGameService gameService,
        IConfiguration config,
        IAppSettingsService appSettingsService) : base(navigationService, serviceProvider)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        Puzzle = puzzle;
        _gameService = gameService;
        _config = config;
        _appSettingsService = appSettingsService;
        _developerMode = _appSettingsService.Settings.DeveloperMode;

        _candidateMode = DeveloperMode ? CandidateMode.SolverCandidates : CandidateMode.None;

        _appSettingsService.SettingsUpdated += OnSettingsUpdated;
    }
    ~PlayViewModel()
    {
        _appSettingsService.SettingsUpdated -= OnSettingsUpdated;
    }

    private void OnSettingsUpdated(object? sender, EventArgs e)
    {
        DeveloperMode = _appSettingsService.Settings.DeveloperMode;
        CandidateMode = DeveloperMode ? CandidateMode.SolverCandidates : CandidateMode.None;
    }

    [RelayCommand]
    public void NavigateToMenuView()
    {
        NavigationService.NavigateTo(ServiceProvider.GetAbstractFactory<MenuViewModel>());
    }
    [RelayCommand]
    public void SetDigit(string value)
    {
        var result = int.TryParse(value, out int digit);
        if (result)
        {

            foreach (var cell in SelectedCells)
            {
                NeedsRedraw = false;

                Puzzle.UpdateDigit(cell.Row, cell.Column, digit, GameMode, CandidateMode);

                // TODO If gameMode = create
                if (GameMode == GameMode.Create)
                {
                    if (Puzzle.Board[cell.Row, cell.Column].Digit != 0)
                    {
                        Puzzle.Board[cell.Row, cell.Column].IsPredefined = true;
                    }
                    else
                    {
                        Puzzle.Board[cell.Row, cell.Column].IsPredefined = false;
                    }
                }

                NeedsRedraw = true;
            }
        }
    }
    [RelayCommand]
    public void UpdateCandidateMode(CandidateMode candidateMode)
    {
        NeedsRedraw = false;
        NeedsRedraw = candidateMode switch
        {
            CandidateMode.CenterCandidates => true,
            CandidateMode.CornerCandidates => true,
            CandidateMode.SolverCandidates => true,
            CandidateMode.None => true,
            _ => true,
        };

    }
    [RelayCommand]
    public void UpdateGameMode()
    {
        NeedsRedraw = false;

        if (GameMode == GameMode.Create)
            GameMode = GameMode.Play;
        else
            GameMode = GameMode.Create;

        NeedsRedraw = true;

    }
    [RelayCommand]
    public void Undo()
    {
        NeedsRedraw = false;
        if (Puzzle.CanUndo)
        {
            Puzzle.Undo();
            NeedsRedraw = true;
        }
    }
    [RelayCommand]
    public void Redo()
    {
        NeedsRedraw = false;
        if (Puzzle.CanRedo)
        {
            Puzzle.Redo();
            NeedsRedraw = true;
        }

    }

    [RelayCommand]
    public async Task SavePuzzle()
    {
        PuzzleEntry puzzleEntry = new()
        {
            Id = Guid.NewGuid(),
            Name = PuzzleName,
            Board = Puzzle.ToJaggedArray(),
            Description = Description,
        };

        var result = PuzzleEntries.Where(p => p.Name == puzzleEntry.Name);

        if (!result.Any())
        {

            var options = new JsonSerializerOptions { WriteIndented = true };

            // Define the directory and file path
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "SavedPuzzles");
            string filePath = Path.Combine(directoryPath, "savedPuzzles.json");

            if (PuzzleEntries.Count == 0)
                await OpenPuzzles(filePath);

            PuzzleEntries.Add(puzzleEntry);

            // Ensure the directory exists
            Directory.CreateDirectory(directoryPath);

            // Serialize and save JSON to the file
            string json = JsonSerializer.Serialize(PuzzleEntries, options);

            using FileStream fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, PuzzleEntries);
            await fileStream.DisposeAsync();

            MessageBox.Show($"Puzzle '{puzzleEntry.Name}' Saved");
        }
        else
        {
            MessageBox.Show($"Puzzle '{puzzleEntry.Name}' already exists!");
        }
    }
    [RelayCommand]
    public async Task OpenPuzzles()
    {
        // Define the directory and file path
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "SavedPuzzles");
        string filePath = Path.Combine(directoryPath, "savedPuzzles.json");

        await OpenPuzzles(filePath);
    }
    [RelayCommand]
    public void LoadPuzzle()
    {
        NeedsRedraw = false;
        if (SelectedPuzzle != null)
        {
            Puzzle.FromJaggedArray(SelectedPuzzle.Board);
            NeedsRedraw = true;
        }
    }

    private async Task OpenPuzzles(string filePath)
    {
        if (File.Exists(filePath))
        {
            using FileStream fileStream = File.OpenRead(filePath);
            var result = await JsonSerializer.DeserializeAsync<List<PuzzleEntry>>(fileStream);

            if (result != null)
            {
                PuzzleEntries.Clear();
                foreach (var entry in result)
                {
                    PuzzleEntries.Add(entry);
                }
            }
        }
    }
    [RelayCommand]
    public async Task Solve()
    {
        NeedsRedraw = false;
        //TODO implement Solve method call from the solver class
        _gameService.ConstraintSolver.InitializeConstraints();
        var result = await Task.Run(_gameService.ConstraintSolver.Solve);

        MessageBox.Show($"Solvable: {result}");

        NeedsRedraw = true;
    }
    [RelayCommand]
    public void OpenSettings()
    {
    }
}
