// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Media;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Wpf;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Wpf;
#endif
/// <summary>Extensions for the WPF Window class.</summary>
public static class WindowDpiExtensions
{
    /// <summary>The logger for WPF DPI extension operations.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WindowDpiExtensions));

    /// <summary>Creates the window-message stream for a WPF window.</summary>
    private static Func<Window, IObservable<WindowMessageInfo>> _windowMessageSource =
        static window => window.ObserveWindowMessages();

    /// <summary>Extension methods for framework elements.</summary>
    /// <param name="frameworkElement">The framework element to scale.</param>
    extension(FrameworkElement frameworkElement)
    {
        /// <summary>This can be used to change the scaling of the FrameworkElement.</summary>
        /// <param name="scaleFactor">double with the factor (1.0 = 100% = 96 dpi).</param>
        public void UpdateLayoutTransform(double scaleFactor)
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Updating dpi for {0} to a scale factor {1}", frameworkElement.GetType(), scaleFactor);
            }

            if (VisualTreeHelper.GetChildrenCount(frameworkElement) == 0)
            {
                return;
            }

            var child = VisualTreeHelper.GetChild(frameworkElement, 0);
            if (Math.Abs(scaleFactor - 1.0) > double.Epsilon)
            {
                ScaleTransform scaleTransform = new(scaleFactor, scaleFactor);
                child.SetValue(FrameworkElement.LayoutTransformProperty, scaleTransform);
            }
            else
            {
                child.SetValue(FrameworkElement.LayoutTransformProperty, null);
            }
        }
    }

    /// <summary>Extension methods for WPF windows.</summary>
    /// <param name="window">The WPF window to attach DPI behavior to.</param>
    extension(Window window)
    {
        /// <summary>Handle DPI changes for the specified Window, this is actually not really needed for WPF.</summary>
        /// <returns>DpiHandler.</returns>
        public DpiHandler AttachDpiHandler()
        {
            Log.DebugFormat("Creating a dpi handler for {0}", window.GetType());
            DpiHandler dpiHandler = new();
            AttachDpiHandlerCore(
                window,
                dpiHandler,
                _windowMessageSource(window),
                static (targetWindow, scaleFactor) => targetWindow.UpdateLayoutTransform(scaleFactor));
            return dpiHandler;
        }
    }

    /// <summary>Attach a DpiHandler to the specified window using supplied message and layout hooks.</summary>
    /// <param name="window">Windows.</param>
    /// <param name="dpiHandler">DpiHandler.</param>
    /// <param name="windowMessages">The window message observable.</param>
    /// <param name="updateLayoutTransform">The layout transform callback.</param>
    internal static void AttachDpiHandlerCore(Window window, DpiHandler dpiHandler, IObservable<WindowMessageInfo> windowMessages, Action<Window, double> updateLayoutTransform)
    {
        if (Log.IsDebugEnabled)
        {
            Log.DebugFormat("Registering the UpdateLayoutTransform subscription for {0}", window.GetType());
        }

        var transformSubscription = dpiHandler.ObserveDpiChanges().Subscribe(dpiChangeInfo =>
        {
            updateLayoutTransform(window, (double)dpiChangeInfo.NewDpi / (double)DpiCalculator.DefaultScreenDpi);
        });
        var messageSubscription = windowMessages.Subscribe(message =>
        {
            _ = dpiHandler.HandleWindowMessages(message);
            if (message.Message == WindowsMessages.WM_NCCREATE)
            {
                updateLayoutTransform(
                    window,
                    (double)NativeDpiMethods.GetDpi((nint)message.Handle) / (double)DpiCalculator.DefaultScreenDpi);
            }
            else if (message.Message == WindowsMessages.WM_DESTROY)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Removing the UpdateLayoutTransform subscription for {0}", window.GetType());
                }

                transformSubscription.Dispose();
            }
        });
        dpiHandler.MessageHandler = Scope.Create(
            Tuple.Create(transformSubscription, messageSubscription),
            static subscriptions =>
        {
            subscriptions.Item2.Dispose();
            subscriptions.Item1.Dispose();
        });
    }

    /// <summary>Exchanges the WPF message source for deterministic tests.</summary>
    /// <param name="windowMessageSource">The replacement message source.</param>
    /// <returns>The previous message source.</returns>
    internal static Func<Window, IObservable<WindowMessageInfo>> ExchangeWindowMessageSource(
        Func<Window, IObservable<WindowMessageInfo>> windowMessageSource)
    {
        Throw.IfNull(windowMessageSource);
        var previousWindowMessageSource = _windowMessageSource;
        _windowMessageSource = windowMessageSource;
        return previousWindowMessageSource;
    }
}
