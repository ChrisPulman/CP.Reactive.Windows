// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Builder;

namespace CP.ReactiveUI.Primitives.Windows.Example.ConsoleDemo;

/// <summary>Provides the entry point for the Windows diagnostics showcase.</summary>
internal static class Program
{
    /// <summary>Starts the interactive dashboard.</summary>
    /// <returns>The process exit code.</returns>
    [STAThread]
    private static async Task<int> Main()
    {
        _ = RxAppBuilder.CreateReactiveUIBuilder()
            .WithCoreServices()
            .BuildApp();
        using var log = new DiagnosticLog();
        using var viewModel = new DiagnosticsViewModel(log);
        using var activation = viewModel.Activator.Activate();
        using var dashboard = new ConsoleDashboard(viewModel, log);
        return await dashboard.RunAsync().ConfigureAwait(false);
    }
}
