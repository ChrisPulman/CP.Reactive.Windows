// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs;
#endif
/// <summary>Represents the ICONDIR structure in an ICO file.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct IconDir
{
    /// <summary>Gets the size of the ICONDIR structure in bytes.</summary>
    public static int Size => 6;

    /// <summary>Gets or sets the reserved value, which must be zero.</summary>
    public ushort Reserved { get; init; }

    /// <summary>Gets or sets the resource type.</summary>
    public ushort Type { get; init; }

    /// <summary>Gets or sets the number of image entries in this file.</summary>
    public ushort Count { get; init; }

    /// <summary>Defines the cursor resource type value.</summary>
    private const ushort CursorResourceType = 2;

    /// <summary>Defines the icon resource type value.</summary>
    private const ushort IconResourceType = 1;

    /// <summary>Defines the native structure size.</summary>
    private const int StructureSize = 6;

    /// <summary>Creates a new ICONDIR structure for an icon file.</summary>
    /// <param name="count">Number of icon entries.</param>
    /// <returns>The initialized ICONDIR structure.</returns>
    public static IconDir CreateIcon(ushort count) => new IconDir { Type = 1, Count = count };

    /// <summary>Creates a new ICONDIR structure for a cursor file.</summary>
    /// <param name="count">Number of cursor entries.</param>
    /// <returns>The initialized ICONDIR structure.</returns>
    public static IconDir CreateCursor(ushort count) => new IconDir { Type = 2, Count = count };
}
