// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows;
using CP.ReactiveUI.Primitives.Windows.Desktop.Devices;
using CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Wpf;
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Enums;
using CP.ReactiveUI.Primitives.Windows.Desktop.Input.Keyboard;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;

namespace CP.ReactiveUI.Primitives.Windows.Example.WpfExample;

/// <summary>Interaction logic for MainWindow.xaml.</summary>
public partial class MainWindow : IDisposable
{
    /// <summary>Tracks Windows session changes.</summary>
    private WindowsSessionListener _sessionListener;

    /// <summary>Tracks the lock/unlock subscription.</summary>
    private IDisposable _sessionLockSubscription;

    /// <summary>Tracks the logon/logoff subscription.</summary>
    private IDisposable _sessionLogonSubscription;

    /// <inheritdoc/>
    public MainWindow()
    {
        InitializeComponent();
        _ = this.AttachDpiHandler();

        _ = this.ObserveWindowMessages()
            .Where(static m => m.Message == WindowsMessages.WM_DESTROY)
            .Subscribe(static m => _ = MessageBox.Show($"{m.Message}"));

        _ = KeyboardHook.KeyboardHookEvents.Subscribe(static (args) =>
        {
            if (args.IsKeyDown && args.Key == VirtualKeyCode.PrintScreen)
            {
                args.Handled = true; // Prevent the Print Screen key from being processed by the system
            }
        });
        _ = DeviceNotification.ObserveVolumeAdditions().Subscribe(static volumeInfo => Debug.WriteLine($"Drives {volumeInfo.Volume.Drives} were added"));
        _ = DeviceNotification.ObserveVolumeRemovals().Subscribe(static volumeInfo => Debug.WriteLine($"Drives {volumeInfo.Volume.Drives} were removed"));

        _ = DeviceNotification
            .ObserveDeviceArrivals()
            .Subscribe(static deviceInterfaceChangeInfo =>
                Debug.WriteLine(
                    "Device added: {0}, for more information goto {1}",
                    deviceInterfaceChangeInfo.Device.FriendlyDeviceName,
                    deviceInterfaceChangeInfo.Device.UsbDeviceInfoUri));

        // A small example to lock the PC when a YubiKey is removed
        _ = DeviceNotification.ObserveDeviceRemovals()
            .Where(static deviceInterfaceChangeInfo => deviceInterfaceChangeInfo.Device.Name.Contains("Yubi"))
            .Subscribe(static deviceInterfaceChangeInfo => _ = User32Api.LockWorkStation());

        // Example of using WindowsSessionListener to handle session changes
        _sessionListener = new();
        _sessionLockSubscription = _sessionListener.ObserveSessionLockChanges().Subscribe(static args =>
        {
            Debug.WriteLine($"Session lock/unlock event: {args.EventType}, Session ID: {args.SessionId}");
        });
        _sessionLogonSubscription = _sessionListener.ObserveSessionLogonChanges().Subscribe(static args =>
        {
            Debug.WriteLine($"Session logon/logoff event: {args.EventType}, Session ID: {args.SessionId}");
        });
        _sessionListener.Start();

        // Make sure to dispose the listener when the window closes
        Closing += OnClosing;
    }

    /// <summary>Disposes this window's managed resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes resources owned by this window.</summary>
    /// <param name="disposing">Whether managed resources should be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sessionLockSubscription?.Dispose();
            _sessionLockSubscription = null;
            _sessionLogonSubscription?.Dispose();
            _sessionLogonSubscription = null;
            _sessionListener?.Dispose();
            _sessionListener = null;
        }
    }

    /// <summary>Disposes managed resources as the window starts closing.</summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The cancellation event data.</param>
    private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) => Dispose();
}
