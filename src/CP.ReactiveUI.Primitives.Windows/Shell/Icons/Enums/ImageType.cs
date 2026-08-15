// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Enums;
#endif
/// <summary>Used for the type in LoadImage or CopyImage.</summary>
public enum ImageType : uint
{
    /// <summary>Copies or loads a bitmap.</summary>
    IMAGE_BITMAP,
    /// <summary>Copies or loads a cursor.</summary>
    IMAGE_ICON,
    /// <summary>Copies or loads an icon.</summary>
    IMAGE_CURSOR
}
