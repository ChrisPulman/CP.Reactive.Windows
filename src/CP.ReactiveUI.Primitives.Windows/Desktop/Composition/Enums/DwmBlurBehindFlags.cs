// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Enums;
#endif
/// <summary>Configuration flags for the DwmEnableBlurBehindWindow function.</summary>
[Flags]
public enum DwmBlurBehindFlags
{
    /// <summary>No blur-behind flags are set.</summary>
    None = 0,
    /// <summary>Transparency is enabled.</summary>
    Enable = 1,
    /// <summary>The blur region is enabled.</summary>
    BlurRegion = 2,
    /// <summary>The maximized transition is enabled.</summary>
    TransitionMaximized = 4,
}
