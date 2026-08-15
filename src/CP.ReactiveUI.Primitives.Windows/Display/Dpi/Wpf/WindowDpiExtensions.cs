// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Windows;
using System.Windows.Media;
using log4net;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Wpf;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Wpf;
#endif
/// <summary>Extensions for the WPF Window class.</summary>
public static class WindowDpiExtensions
{
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

            DependencyObject child = VisualTreeHelper.GetChild(frameworkElement, 0);
            if (Math.Abs(scaleFactor) > 1.0)
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

    extension(Window window)
    {
        /// <summary>Handle DPI changes for the specified Window, this is actually not really needed for WPF.</summary>
        /// <returns>DpiHandler.</returns>
        public DpiHandler AttachDpiHandler()
        {
            if (Log.IsDebugEnabled)
            {
                Log.DebugFormat("Creating a dpi handler for {0}", window.GetType());
            }

            DpiHandler dpiHandler = new();
            AttachDpiHandlerCore(window, dpiHandler);
            return dpiHandler;
        }
    }

    /// <summary>The logger for WPF DPI extension operations.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(WindowDpiExtensions));

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

        IDisposable transformSubscription = dpiHandler.ObserveDpiChanges().Subscribe(delegate(DpiChangeInfo dpiChangeInfo)
        {
            updateLayoutTransform(window, (double)dpiChangeInfo.NewDpi / (double)DpiCalculator.DefaultScreenDpi);
        });
        _ = windowMessages.Subscribe(delegate(WindowMessageInfo message)
        {
            _ = dpiHandler.HandleWindowMessages(message);
            if (message.Message == WindowsMessages.WM_NCCREATE)
            {
                updateLayoutTransform(window, (double)NativeDpiMethods.GetDpi((nint)message.Handle) / (double)DpiCalculator.DefaultScreenDpi);
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
    }

    /// <summary>Attach a DpiHandler to the specified window.</summary>
    /// <param name="window">Windows.</param>
    /// <param name="dpiHandler">DpiHandler.</param>
    private static void AttachDpiHandlerCore(Window window, DpiHandler dpiHandler) => AttachDpiHandlerCore(window, dpiHandler, window.ObserveWindowMessages(), delegate(Window targetWindow, double scaleFactor)
        {
            targetWindow.UpdateLayoutTransform(scaleFactor);
        });
}
