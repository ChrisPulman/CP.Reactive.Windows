// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Flags for the CURSOR_INFO "flags" field, see: https://msdn.microsoft.com/en-us/library/windows/desktop/ms648381.aspx.</summary>
[Flags]
public enum CursorInfoFlags : uint
{
    /// <summary>The cursor is hidden.</summary>
    None = 0U,
    /// <summary>Cursor is showing.</summary>
    Showing = 1U,
    /// <summary>Cursor is suppressed.</summary>
    Suppressed = 2U
}
