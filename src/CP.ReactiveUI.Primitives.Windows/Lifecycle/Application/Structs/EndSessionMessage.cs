// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Lifecycle;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Lifecycle;
#endif
/// <summary>Represents a message indicating that a Windows session is ending.</summary>
/// <remarks>Use this type to encapsulate information about session end events received from the Windows message
/// loop. The message can be handled to prevent further processing by the default window procedure.</remarks>
/// <param name="Msg">The Windows message type associated with the session end event.</param>
/// <param name="EndSessionReason">The reason for the session termination, specifying why the session is ending.</param>
public readonly record struct EndSessionMessage(WindowsMessages Msg, EndSessionReasons EndSessionReason)
{
    /// <summary>Gets or sets a value indicating whether the message has been handled.</summary>
    public bool Handled { get; init; }

    /// <summary>Gets or sets the native result returned to the system.</summary>
    public int Result { get; init; }
}
