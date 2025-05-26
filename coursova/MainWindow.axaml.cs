using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Coursova;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using Coursova.Models;

namespace coursova;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private readonly Canvas _graphCanvas;

    public MainWindow()
    {
        InitializeComponent();

        if (DataContext == null)
        {
            DataContext = new MainWindowViewModel();
        }
        ViewModel = DataContext as MainWindowViewModel;

        if (ViewModel == null)
        {
            throw new InvalidOperationException("ViewModel не може бути null.");
        }

        _graphCanvas = this.FindControl<Canvas>("GraphCanvas")!;
        if (_graphCanvas == null)
        {
            throw new InvalidOperationException("GraphCanvas не може бути null.");
        }

        this.WhenActivated((disposables) =>
        {
            this.WhenAnyValue(x => x.ViewModel!.GraphNeedsUpdate)
                .Subscribe(_ =>
                {
                    new DrawService(_graphCanvas, ViewModel!).DrawGraph();
                })
                .DisposeWith(disposables);
        });
    }
}