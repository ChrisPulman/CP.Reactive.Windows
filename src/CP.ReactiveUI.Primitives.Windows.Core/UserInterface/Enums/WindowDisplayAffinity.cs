// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>GetWindowsDisplayAffinity Enum values are described here: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity.</summary>
public enum WindowDisplayAffinity
{
    /// <summary>Non affinity.</summary>
    None,
    /// <summary>Enable window contents to be displayed on a monitor.</summary>
    Monitor
}
