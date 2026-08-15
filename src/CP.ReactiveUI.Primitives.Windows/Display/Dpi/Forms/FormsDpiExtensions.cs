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
    extension(ContextMenuStrip contextMenuStrip)
    {
        /// <summary>Handle DPI changes for the specified ContextMenuStrip.</summary>
        /// <returns>DpiHandler.</returns>
        public DpiHandler AttachDpiHandler()
        {
            DpiHandler dpiHandler = new(needsListenerWorkaround: true);
            dpiHandler.MessageHandler = contextMenuStrip.ObserveWindowMessages().Subscribe(delegate(WindowMessageInfo message)
            {
                _ = dpiHandler.HandleContextMenuMessages(message);
            });
            return dpiHandler;
        }
    }

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
            dpiHandler.MessageHandler = form.ObserveWindowMessages().Subscribe(delegate(WindowMessageInfo message)
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

    /// <summary>Creates DPI-unaware behavior for the specified form.</summary>
    /// <param name="form">The form to attach to.</param>
    /// <returns>DPI-unaware form behavior.</returns>
    private static DpiUnawareFormBehavior CreateDpiUnawareBehavior(Form form) => new(form);
}
