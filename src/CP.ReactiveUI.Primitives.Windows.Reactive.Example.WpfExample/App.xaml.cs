// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
using ReactiveUI.Reactive.Builder;

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.WpfExample;

/// <summary>Starts the Windows Interaction Laboratory with DPI awareness enabled.</summary>
public partial class App : Application
{
    /// <summary>Initializes a new instance of the <see cref="App"/> class and configures System.Reactive WPF services.</summary>
    public App() => RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        _ = NativeDpiMethods.EnableDpiAware();
        base.OnStartup(e);
    }
}
