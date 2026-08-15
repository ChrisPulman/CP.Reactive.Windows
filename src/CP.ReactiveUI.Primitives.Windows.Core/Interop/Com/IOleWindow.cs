// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>Provides methods that allow an application to obtain activation window handles.</summary>
[Guid("00000114-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IOleWindow
{
    /// <summary>Retrieves a handle to one of the windows participating in activation.</summary>
    /// <param name="windowHandle">A pointer to a variable that receives the window handle.</param>
    void GetWindow(out IntPtr windowHandle);

    /// <summary>Determines whether context-sensitive help mode should be entered.</summary>
    /// <param name="enterMode"><see langword="true" /> to enter help mode; otherwise, exits help mode.</param>
    void ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool enterMode);
}
