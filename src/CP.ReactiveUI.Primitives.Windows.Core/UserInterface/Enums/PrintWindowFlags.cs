// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Defines flags for the PrintWindow function.</summary>
public enum PrintWindowFlags : uint
{
    /// <summary>Copy the complete window.</summary>
    PW_COMPLETE,

    /// <summary>Only the client area of the window is copied. By default, the entire window is copied.</summary>
    PW_CLIENTONLY,

    /// <summary>Works on windows that use DirectX or DirectComposition.</summary>
    PW_RENDERFULLCONTENT,
}
