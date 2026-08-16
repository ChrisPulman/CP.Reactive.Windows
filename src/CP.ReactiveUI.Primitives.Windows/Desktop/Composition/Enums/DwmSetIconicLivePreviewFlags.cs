// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Enums;
#endif
/// <summary>Configuration flags for the DwmSetIconicLivePreviewBitmap function.</summary>
[Flags]
public enum DwmSetIconicLivePreviewFlags
{
    /// <summary>No iconic live preview flags are set.</summary>
    None = 0,
    /// <summary>Displays a frame around the provided bitmap.</summary>
    DisplayFrame = 1,
}
