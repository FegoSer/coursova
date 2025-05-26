using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Coursova.Models;

namespace Coursova;

public class MainWindowViewModel : ReactiveObject
{
    private readonly MatrixService _matrixService = new();
    private readonly RandomService _randomService = new();
    private readonly FileService _fileService = new();
    private readonly PathFinderService _pathFinderService = new();

    private int _currentDistance = int.MaxValue;

    private ObservableCollection<ObservableCollection<string>> _matrix = [];
    public ObservableCollection<ObservableCollection<string>> Matrix
    {
        get => _matrix;
        private set => this.RaiseAndSetIfChanged(ref _matrix, value);
    }

    public string SizeInput { get; set; } = Constants.DefaultMatrixSize.ToString();
    public string StartVertex { get; set; } = Constants.DefaultStartVertex.ToString();
    public string EndVertex { get; set; } = Constants.DefaultEndVertex.ToString();

    private string _matrixText = "";
    public string MatrixText
    {
        get => _matrixText;
        set => this.RaiseAndSetIfChanged(ref _matrixText, value);
    }

    private bool _graphNeedsUpdate = false;
    public bool GraphNeedsUpdate
    {
        get => _graphNeedsUpdate;
        set => this.RaiseAndSetIfChanged(ref _graphNeedsUpdate, value);
    }

    private List<int> _shortestPath = [];
    public List<int> ShortestPath
    {
        get => _shortestPath;
        private set => this.RaiseAndSetIfChanged(ref _shortestPath, value);
    }

    private int _operationsCount = 0;
    public int OperationsCount
    {
        get => _operationsCount;
        private set => this.RaiseAndSetIfChanged(ref _operationsCount, value);
    }

    private List<(int, int)> _pathEdges = [];
    public List<(int, int)> PathEdges
    {
        get => _pathEdges;
        private set => this.RaiseAndSetIfChanged(ref _pathEdges, value);
    }

    public string[] Methods { get; } = [Constants.FloydWarshallAlgorithmName, Constants.DantzigAlgorithmName];
    public string SelectedMethod { get; set; } = Constants.FloydWarshallAlgorithmName;

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }
    private string _message = Constants.HelloMessage;

    public ReactiveCommand<Unit, Unit> CreateMatrixCommand { get; }
    public ReactiveCommand<Unit, Unit> GenerateRandomMatrixCommand { get; }
    public ReactiveCommand<Unit, Unit> FindPathCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveToFileCommand { get; }

    private int[,] _weights = new int[0, 0];

    public MainWindowViewModel()
    {
        CreateMatrixCommand = ReactiveCommand.Create(CreateMatrix);
        GenerateRandomMatrixCommand = ReactiveCommand.Create(GenerateRandomMatrix);
        FindPathCommand = ReactiveCommand.Create(FindPath);
        SaveToFileCommand = ReactiveCommand.Create(SaveToFile);

        Message = Constants.HelloMessage;
    }

    private void CreateMatrix()
    {
        try
        {
            MatrixText = _matrixService.CreateEmptyMatrix(SizeInput);
            Message = "Шаблон матриці створено для орієнтованого графа. Заповніть значення вагів.";
            Matrix = _matrixService.ConvertMatrixTextToObservableCollection(MatrixText);
            GraphNeedsUpdate = !GraphNeedsUpdate;
        }
        catch (ArgumentException ex)
        {
            Message = ex.Message;
        }
    }

    private void GenerateRandomMatrix()
    {
        try
        {
            MatrixText = _randomService.GenerateRandomMatrixText(SizeInput);
            Message = "Випадкову матрицю для орієнтованого графа створено.";
            Matrix = _matrixService.ConvertMatrixTextToObservableCollection(MatrixText);
            GraphNeedsUpdate = !GraphNeedsUpdate;
        }
        catch (ArgumentException ex)
        {
            Message = ex.Message;
        }
    }

    private void FindPath()
    {
        try
        {
            _weights = _matrixService.ParseMatrixFromText(MatrixText, SizeInput);
            var (startVertex, endVertex) = _matrixService.ValidateStartEndVertices(StartVertex, EndVertex, SizeInput);

            var result = _pathFinderService.FindPath(_weights, startVertex, endVertex, SelectedMethod);

            ShortestPath = result.Path;
            PathEdges = result.Edges;
            OperationsCount = result.OperationsCount;
            _currentDistance = result.Distance;
            Message = result.Message;

            GraphNeedsUpdate = !GraphNeedsUpdate;
        }
        catch (ArgumentException ex)
        {
            Message = ex.Message;
        }
        catch (Exception ex)
        {
            Message = $"Загальна помилка: {ex.Message}";
        }
    }

    public int[,]? GetWeightsMatrix()
    {
        if (string.IsNullOrWhiteSpace(MatrixText))
        {
            return null;
        }
        return _weights;
    }

    public (int start, int end) GetStartEndVertices()
    {
        try
        {
            return _matrixService.ValidateStartEndVertices(StartVertex, EndVertex, SizeInput);
        }
        catch (ArgumentException)
        {
            return (-1, -1);
        }
    }

    private void SaveToFile()
    {
        try
        {
            _fileService.SaveToFile(
                MatrixText,
                SelectedMethod,
                StartVertex,
                EndVertex,
                _currentDistance,
                ShortestPath,
                OperationsCount
            );
            Message = $"Збережено у файл {Constants.SolutionFilePath}";
        }
        catch (Exception ex)
        {
            Message = $"Помилка збереження: {ex.Message}";
        }
    }
}