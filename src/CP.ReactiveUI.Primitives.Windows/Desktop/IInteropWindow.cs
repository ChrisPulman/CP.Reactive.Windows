// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>This is the interface of all classes which represent a native window.</summary>
public interface IInteropWindow
{
    /// <summary>Gets or sets whether a WindowScroller can work with this window.</summary>
    bool? CanScroll { get; set; }

    /// <summary>Gets or sets the title of the window, if any.</summary>
    string Caption { get; set; }

    /// <summary>Gets or sets the children of this window.</summary>
    IEnumerable<IInteropWindow> Children { get; set; }

    /// <summary>Gets or sets the internal class name for the window.</summary>
    string Classname { get; set; }

    /// <summary>Gets the handle of the window.</summary>
    IntPtr Handle { get; }

    /// <summary>Gets or sets whether the children are retrieved in z-order.</summary>
    bool HasZOrderedChildren { get; set; }

    /// <summary>Gets a value indicating whether there are any children.</summary>
    bool HasChildren { get; }

    /// <summary>Gets a value indicating whether the window has a class name.</summary>
    bool HasClassname { get; }

    /// <summary>Gets a value indicating whether this window has a parent.</summary>
    bool HasParent { get; }

    /// <summary>Gets or sets the WindowInfo for the window.</summary>
    WindowInfo? Info { get; set; }

    /// <summary>Gets or sets whether the window is maximized.</summary>
    bool? IsMaximized { get; set; }

    /// <summary>Gets or sets whether the window is minimized.</summary>
    bool? IsMinimized { get; set; }

    /// <summary>Gets or sets whether the window is visible.</summary>
    bool? IsVisible { get; set; }

    /// <summary>Gets or sets the handle for the parent to which this window belongs.</summary>
    IntPtr? Parent { get; set; }

    /// <summary>
    ///     Gets or sets the actual IInteropWindow for the parent.
    ///     This is filled when this window was retrieved via parent.GetChildren or parent.GetZOrderChildren.
    /// </summary>
    IInteropWindow ParentWindow { get; set; }

    /// <summary>Gets or sets the WindowPlacement for the window.</summary>
    WindowPlacement? Placement { get; set; }

    /// <summary>Gets or sets the thread ID this window belongs to, read with GetProcessId().</summary>
    int? ThreadId { get; set; }

    /// <summary>Gets or sets the process ID this window belongs to, read with GetProcessId().</summary>
    int? ProcessId { get; set; }

    /// <summary>Gets or sets the text (not title) of the window, if any.</summary>
    string Text { get; set; }

    /// <summary>Dump the information in the InteropWindow for debugging.</summary>
    /// <returns>StringBuilder.</returns>
    StringBuilder Dump();

    /// <summary>Dump the information in the InteropWindow for debugging.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify what to dump.</param>
    /// <returns>StringBuilder.</returns>
    StringBuilder Dump(InteropWindowRetrieveSettings retrieveSettings);

    /// <summary>Dump the information in the InteropWindow for debugging.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify what to dump.</param>
    /// <param name="dump">StringBuilder to dump to.</param>
    /// <returns>StringBuilder.</returns>
    StringBuilder Dump(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump);

    /// <summary>Dump the information in the InteropWindow for debugging.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify what to dump.</param>
    /// <param name="dump">StringBuilder to dump to.</param>
    /// <param name="indentation">int.</param>
    /// <returns>StringBuilder.</returns>
    StringBuilder Dump(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump, string indentation);
}
