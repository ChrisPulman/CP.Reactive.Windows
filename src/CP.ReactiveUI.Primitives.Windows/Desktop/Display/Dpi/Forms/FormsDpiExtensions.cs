// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi.Forms;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi.Forms;
#endif
/// <summary>Extensions for Windows Form.</summary>
public static class FormsDpiExtensions
{
    /// <summary>Creates the window-message stream for a Windows Forms control.</summary>
    private static Func<Control, IObservable<WindowMessageInfo>> _windowMessageSource =
        static control => control.ObserveWindowMessages();

    /// <summary>Extension methods for context menu strips.</summary>
    /// <param name="contextMenuStrip">The context menu strip to attach DPI behavior to.</param>
    extension(ContextMenuStrip contextMenuStrip)
    {
        /// <summary>Handle DPI changes for the specified ContextMenuStrip.</summary>
        /// <returns>DpiHandler.</returns>
        public DpiHandler AttachDpiHandler()
        {
            DpiHandler dpiHandler = new(needsListenerWorkaround: true);
            dpiHandler.MessageHandler = _windowMessageSource(contextMenuStrip).Subscribe(message =>
            {
                _ = dpiHandler.HandleContextMenuMessages(message);
            });
            return dpiHandler;
        }
    }

    /// <summary>Extension methods for forms.</summary>
    /// <param name="form">The form to attach DPI behavior to.</param>
    extension(Form form)
    {
        /// <summary>
        ///     Handle DPI changes for the specified Form
        ///     Using this DOES NOT enable dpi scaling in the non client area, for this you will need to call:
        ///     DpiHandler.TryEnableNonClientDpiScaling(this.Handle) from the WndProc in the WM_NCCREATE message.
        ///     Prefer AttachDpiAwareBehavior when the form handle should be created in a DPI-aware context.
        /// </summary>
        /// <returns>DpiHandler.</returns>
        public DpiHandler AttachDpiHandler()
        {
            DpiHandler dpiHandler = new(needsListenerWorkaround: true);
            dpiHandler.MessageHandler = _windowMessageSource(form).Subscribe(message =>
            {
                _ = dpiHandler.HandleWindowMessages(message);
            });
            return dpiHandler;
        }

        /// <summary>Attach DPI-aware behavior to the specified Form.</summary>
        /// <returns>DPI-aware form behavior.</returns>
        public DpiAwareFormBehavior AttachDpiAwareBehavior() => new(form);

        /// <summary>Attach DPI-unaware behavior to the specified Form.</summary>
        /// <returns>DPI-unaware form behavior.</returns>
        public DpiUnawareFormBehavior AttachDpiUnawareBehavior() => CreateDpiUnawareBehavior(form);
    }

    /// <summary>Exchanges the Forms message source for deterministic tests.</summary>
    /// <param name="windowMessageSource">The replacement message source.</param>
    /// <returns>The previous message source.</returns>
    internal static Func<Control, IObservable<WindowMessageInfo>> ExchangeWindowMessageSource(
        Func<Control, IObservable<WindowMessageInfo>> windowMessageSource)
    {
        Throw.IfNull(windowMessageSource);
        var previousWindowMessageSource = _windowMessageSource;
        _windowMessageSource = windowMessageSource;
        return previousWindowMessageSource;
    }

    /// <summary>Creates DPI-unaware behavior for the specified form.</summary>
    /// <param name="form">The form to attach to.</param>
    /// <returns>DPI-unaware form behavior.</returns>
    private static DpiUnawareFormBehavior CreateDpiUnawareBehavior(Form form) => new(form);
}
