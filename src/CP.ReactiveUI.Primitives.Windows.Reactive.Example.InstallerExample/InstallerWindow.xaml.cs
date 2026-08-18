// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
using ReactiveUI.Reactive;

namespace CPDeploymentStudio.Reactive.Example;

/// <summary>Hosts the reactive, simulation-only deployment experience.</summary>
public partial class InstallerWindow : ReactiveWindow<InstallerViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="InstallerWindow"/> class.</summary>
    public InstallerWindow()
    {
        InitializeComponent();
        ViewModel = new InstallerViewModel();
        DataContext = ViewModel;

        this.WhenActivated(disposables =>
        {
            var viewModel = ViewModel;
            if (viewModel is null)
            {
                return;
            }

            disposables.Add(viewModel.Events.Subscribe(deploymentEvent =>
            {
                EventStreamText.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:HH:mm:ss}  {1,-10}  {2}  ·  {3:0}%",
                    deploymentEvent.Timestamp,
                    deploymentEvent.Stage,
                    deploymentEvent.Message,
                    deploymentEvent.Progress);
            }));

            disposables.Add(EnvironmentMonitor.EnvironmentChangeEvents
                .Subscribe(change => viewModel.RecordEnvironmentChange(change.Area)));

            disposables.Add(viewModel.InspectEnvironment.Execute().Subscribe());
        });

        Closed += (_, _) => ViewModel?.Dispose();
    }
}
