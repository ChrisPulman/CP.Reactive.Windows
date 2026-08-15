// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;
using CP.ReactiveUI.Primitives.Windows.Desktop.Windows;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

/// <summary>Shows environment update notifications for a form after it is shown.</summary>
internal sealed class EnvironmentUpdateDialogBehavior : IDisposable
{
    /// <summary>The form that owns the dialog.</summary>
    private readonly Form _form;

    /// <summary>The log source for diagnostics.</summary>
    private readonly ILog _log;

    /// <summary>The environment-update subscription.</summary>
    private readonly IDisposable _environmentUpdateSubscription;

    /// <summary>Indicates whether this behavior has been disposed.</summary>
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="EnvironmentUpdateDialogBehavior"/> class.</summary>
    /// <param name="form">The form that owns the dialog.</param>
    /// <param name="log">The log source for diagnostics.</param>
    public EnvironmentUpdateDialogBehavior(Form form, ILog log)
    {
#if NETFRAMEWORK
        if (form is null)
        {
            throw new ArgumentNullException(nameof(form));
        }

        if (log is null)
        {
            throw new ArgumentNullException(nameof(log));
        }
#else
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(log);
#endif

        _form = form;
        _log = log;
        _environmentUpdateSubscription = EnvironmentMonitor.EnvironmentChangeEvents.Subscribe(args =>
        {
            _log.InfoFormat("{0} - {1}", args.SystemParametersInfoAction, args.Area);
            _ = MessageBox.Show(_form, $"{args.SystemParametersInfoAction} - {args.Area}", "Change!");
        });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _environmentUpdateSubscription.Dispose();
    }
}
