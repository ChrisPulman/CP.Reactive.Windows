// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>A monitor for environment changes.</summary>
public class EnvironmentMonitor
{
    /// <summary>The lazily-created singleton monitor.</summary>
    private static readonly Lazy<EnvironmentMonitor> Singleton = new(static () => new EnvironmentMonitor());

    /// <summary>Used to store the observable.</summary>
    private readonly IObservable<EnvironmentChangedEventArgs> _environmentObservable;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.EnvironmentMonitor" /> class.</summary>
    private EnvironmentMonitor()
    {
        _environmentObservable = CreateEnvironmentChangeEvents(SharedMessageWindow.WindowMessageEvents);
    }

    /// <summary>Gets an observable sequence of environment-change notifications.</summary>
    public static IObservable<EnvironmentChangedEventArgs> EnvironmentChangeEvents => Singleton.Value._environmentObservable;

    /// <summary>Creates environment-change notifications from a supplied window-message stream.</summary>
    /// <param name="windowMessageEvents">The stream of window messages to inspect.</param>
    /// <returns>A shared stream of environment-change notifications.</returns>
    internal static IObservable<EnvironmentChangedEventArgs> CreateEnvironmentChangeEvents(IObservable<WindowMessage> windowMessageEvents) =>
        windowMessageEvents
            .Where(static message => message.Msg == WindowsMessages.WM_WININICHANGE)
            .Select(CreateChangedEventArgs)
            .Publish()
            .RefCount();

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
