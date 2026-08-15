// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
using log4net.Config;

namespace CP.ReactiveUI.Primitives.Windows.Example.WpfExample;

/// <summary>Interaction logic for App.xaml.</summary>
public partial class App
{
    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        _ = BasicConfigurator.Configure();
        _ = NativeDpiMethods.EnableDpiAware();
        base.OnStartup(e);
    }
}
