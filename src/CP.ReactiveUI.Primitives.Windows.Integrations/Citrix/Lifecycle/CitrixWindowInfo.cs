// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Describes a Citrix client window.</summary>
public sealed class CitrixWindowInfo
{
    /// <summary>Initializes a new instance of the <see cref="CitrixWindowInfo" /> class.</summary>
    /// <param name="windowHandle">The native window handle, or 0 when the source does not expose one.</param>
    /// <param name="windowId">The source-specific Citrix window identifier, or 0 when one is not known.</param>
    /// <param name="title">The window title, when known.</param>
    /// <param name="className">The native window class name, when known.</param>
    public CitrixWindowInfo(long windowHandle, int windowId, string title, string className)
    {
        WindowHandle = windowHandle;
        WindowId = windowId;
        Title = title;
        ClassName = className;
    }

    /// <summary>Gets an empty window payload.</summary>
    public static CitrixWindowInfo Empty { get; } = new(0, 0, null, null);

    /// <summary>Gets the native window handle, or 0 when the source does not expose one.</summary>
    public long WindowHandle { get; }

    /// <summary>Gets the source-specific Citrix window identifier, or 0 when one is not known.</summary>
    public int WindowId { get; }

    /// <summary>Gets the window title, when known.</summary>
    public string Title { get; }

    /// <summary>Gets the native window class name, when known.</summary>
    public string ClassName { get; }
}
