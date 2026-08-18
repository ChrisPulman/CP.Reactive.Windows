// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CP.ReactiveUI.Primitives.Windows.Example.WpfExample;

/// <summary>ReactiveUI view for the Windows Interaction Laboratory.</summary>
public partial class MainWindow : ReactiveLabWindow<MainViewModel>
{
    /// <summary>Initializes the laboratory view and scopes all view-specific resources to activation.</summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new(new WindowsLabService(this));
        DataContext = ViewModel;

        this.WhenActivated((Action<IDisposable> disposables) =>
        {
            ViewModel.CaptureDashboard.RegisterHandler(context => context.SetOutput(CaptureDashboard())).TrackWith(disposables);
            ViewModel.WhenAnyValue(static viewModel => viewModel.GuardMode)
                .Select(ViewModel.CreateWindowGuard)
                .Switch()
                .Subscribe(_ => { }, ViewModel.ReportGuardFailure)
                .TrackWith(disposables);
        });
    }

    /// <summary>Renders only this dashboard to an in-memory preview; no screen or file capture is performed.</summary>
    /// <returns>The frozen dashboard preview.</returns>
    private ImageSource CaptureDashboard()
    {
        var width = Math.Max(1, checked((int)DashboardRoot.ActualWidth));
        var height = Math.Max(1, checked((int)DashboardRoot.ActualHeight));
        RenderTargetBitmap bitmap = new(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(DashboardRoot);
        bitmap.Freeze();
        return bitmap;
    }
}
