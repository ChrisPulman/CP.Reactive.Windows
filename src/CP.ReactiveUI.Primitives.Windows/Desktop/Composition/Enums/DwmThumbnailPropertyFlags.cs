// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition.Enums;
#endif
/// <summary>A flag to indicate which properties are set by the DwmUpdateThumbnailProperties method.</summary>
[Flags]
public enum DwmThumbnailPropertyFlags
{
    /// <summary>No thumbnail properties are set.</summary>
    None = 0,
    /// <summary>A value for the destination rectangle member has been specified.</summary>
    Destination = 1,
    /// <summary>A value for the source rectangle member has been specified.</summary>
    Source = 2,
    /// <summary>A value for the opacity member has been specified.</summary>
    Opacity = 4,
    /// <summary>A value for the visible member has been specified.</summary>
    Visible = 8,
    /// <summary>A value for the source-client-area-only member has been specified.</summary>
    SourceClientAreaOnly = 0x10,
}
