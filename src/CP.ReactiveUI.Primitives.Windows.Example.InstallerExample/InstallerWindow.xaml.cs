// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using System.Windows;
using System.Windows.Documents;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;

namespace CP.ReactiveUI.Primitives.Windows.Example.InstallerExample;

/// <summary>Displays installation and restart progress for the example executable.</summary>
public partial class InstallerWindow : Window
{
    /// <summary>Gets the executable installed by this example.</summary>
    private const string ExeToInstall = @"..\..\..\..\CP.ReactiveUI.Primitives.Windows.Example.FormsExample\bin\Debug\net480\CP.ReactiveUI.Primitives.Windows.Example.FormsExample.exe";

    /// <summary>Initializes a new instance of the <see cref="InstallerWindow"/> class.</summary>
    public InstallerWindow()
    {
        InitializeComponent();
        DataContext = this;
        Start.IsEnabled = File.Exists(ExeToInstall);
    }

    /// <summary>Adds a line to the installation log.</summary>
    /// <param name="line">The line to add.</param>
    private void AddLine(string line)
    {
        LogText.Inlines.Add(new Run(line));
        LogText.Inlines.Add(new LineBreak());
    }

    /// <summary>Shuts down processes using the executable and restarts them.</summary>
    private void TryRestart()
    {
        using var session = RestartManager.CreateSession();
        session.RegisterFile(ExeToInstall);
        var processes = session.GetProcessesUsingResources();

        foreach (var process in processes)
        {
            AddLine($"Process {process.ApplicationName} (PID: {process.Process.ProcessId}) is using the file, status: {process.ApplicationStatus}");
        }

        try
        {
            session.Shutdown(Kernel32.Enums.RmShutdownType.RmShutdownOnlyRegistered, progress =>
            {
                _ = Dispatcher.BeginInvoke(() =>
                {
                    AddLine($"Shutdown progress {progress}");
                });
            });
        }
        catch (Exception)
        {
            processes = session.GetProcessesUsingResources();
            foreach (var process in processes)
            {
                AddLine($"Process {process.ApplicationName} (Status: {process.ApplicationStatus})");
            }

            return;
        }

        session.Restart(progress =>
        {
            _ = Dispatcher.BeginInvoke(() => AddLine($"Restart progress {progress}"));
        });
    }

    /// <summary>Starts the restart workflow when the button is clicked.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void Button_Click(object sender, RoutedEventArgs e) => TryRestart();
}
