// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Enums;
#endif
/// <summary>Options to specify the size of icons to return.</summary>
public enum IconSize
{
    /// <summary>Specify large icon, 32 pixels by 32 pixels.</summary>
    Large,
    /// <summary>Specify small icon, 16 pixels by 16 pixels.</summary>
    Small,
}
