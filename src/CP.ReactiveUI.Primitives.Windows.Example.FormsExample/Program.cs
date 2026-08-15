// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
using log4net.Config;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

/// <summary>Contains the application entry point.</summary>
internal static class Program
{
    /// <summary>The main entry point for the application.</summary>
    /// <param name="args">The command-line arguments.</param>
    [STAThread]
    private static void Main(string[] args)
    {
        if (Array.IndexOf(args, "--restart") >= 0)
        {
            _ = MessageBox.Show("Application restarted by Windows Restart Manager, exiting now.", "Restarted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _ = SharedMessageWindow.ObserveWindowMessages().Subscribe(static message =>
        {
            Debug.WriteLine($"Received windows message {message.Msg}");
        });
        ApplicationRestartManager.RegisterForRestart("--restart");
        _ = ApplicationRestartManager.ObserveEndSessionMessages(
                onQuerySession: static (endSessionReason) => true,
                onEndSession: static (endSessionReason) =>
                {
                    Debug.WriteLine($"Shutting down application due to {endSessionReason}");
                    Application.Exit();
                    return true;
                }).Subscribe(static endSessionMessage =>
                {
                    Debug.WriteLine($"{endSessionMessage.Msg} with session reason: {endSessionMessage.EndSessionReason}");
                });

        _ = BasicConfigurator.Configure();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using var formDpiUnaware = new FormDpiUnaware();
        formDpiUnaware.Show();
        using var formWithAttachedDpiHandler = new FormWithAttachedDpiHandler();
        formWithAttachedDpiHandler.Show();
        using var formExtendsDpiAwareForm = new FormExtendsDpiAwareForm();
        formExtendsDpiAwareForm.Show();
        using var webBrowserForm = new WebBrowserForm();
        webBrowserForm.Show();
        Application.Run();
    }
}
