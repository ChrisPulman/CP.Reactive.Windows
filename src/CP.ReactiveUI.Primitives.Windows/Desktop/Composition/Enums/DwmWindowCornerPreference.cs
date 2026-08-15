// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Enums;
#endif
/// <summary>Flags used by the DwmSetWindowAttribute function to specify the rounded corner preference for a window.</summary>
public enum DwmWindowCornerPreference : uint
{
    /// <summary>Let the system decide when to round window corners.</summary>
    Default,
    /// <summary>Never round window corners.</summary>
    DoNotRound,
    /// <summary>Round the corners, if appropriate.</summary>
    Round,
    /// <summary>Round the corners if appropriate, with a small radius.</summary>
    RoundSmall,
}
