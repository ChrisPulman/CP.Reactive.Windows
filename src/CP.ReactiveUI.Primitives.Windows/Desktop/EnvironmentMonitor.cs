// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>A monitor for environment changes.</summary>
public class EnvironmentMonitor
{
    /// <summary>The singleton of the KeyboardHook.</summary>
    private static readonly Lazy<EnvironmentMonitor> Singleton = new(() => new EnvironmentMonitor());

    /// <summary>Used to store the observable.</summary>
    private readonly IObservable<EnvironmentChangedEventArgs> _environmentObservable;

    /// <summary>Gets the actual clipboard hook observable.</summary>
    public static IObservable<EnvironmentChangedEventArgs> EnvironmentChangeEvents => Singleton.Value._environmentObservable;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.EnvironmentMonitor" /> class.</summary>
    private EnvironmentMonitor()
    {
        _environmentObservable = SharedMessageWindow.WindowMessageEvents.Where((m) => m.Msg == WindowsMessages.WM_WININICHANGE).Select(CreateChangedEventArgs).Publish()
            .RefCount();
    }

    /// <summary>Creates environment-change arguments from a window message.</summary>
    /// <param name="message">The source message.</param>
    /// <returns>The translated environment-change arguments.</returns>
    internal static EnvironmentChangedEventArgs CreateChangedEventArgs(WindowMessage message)
    {
        uint systemParametersInfoAction = checked((uint)(int)message.WParam);
        string area = Marshal.PtrToStringAuto(new(message.LParam));
        return EnvironmentChangedEventArgs.Create((SystemParametersInfoActions)systemParametersInfoAction, area);
    }
}
