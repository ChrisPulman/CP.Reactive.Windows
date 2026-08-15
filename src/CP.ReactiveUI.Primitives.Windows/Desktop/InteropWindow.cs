// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>
///     Information about a native window
///     Note: This is a dumb container, and doesn't retrieve anything about the window itself.
/// </summary>
public class InteropWindow : IEquatable<IInteropWindow>, IInteropWindow
{
    /// <summary>The native window handle value used by the compatibility interface bridge.</summary>
    private readonly IntPtr _nativeHandle;

    /// <summary>Gets the safe native window handle wrapper.</summary>
    public SafeNativeWindowHandle Handle { get; }

    /// <inheritdoc />
    IntPtr IInteropWindow.Handle => _nativeHandle;

    /// <inheritdoc />
    public bool HasZOrderedChildren { get; set; }

    /// <inheritdoc />
    public WindowInfo? Info { get; set; }

    /// <inheritdoc />
    public IEnumerable<IInteropWindow> Children { get; set; }

    /// <inheritdoc />
    public bool HasChildren => Children?.Any() ?? false;

    /// <inheritdoc />
    public string Classname { get; set; }

    /// <inheritdoc />
    public bool HasClassname => !string.IsNullOrEmpty(Classname);

    /// <inheritdoc />
    public bool HasParent
    {
        get
        {
            if (Parent.HasValue)
            {
                return Parent != IntPtr.Zero;
            }

            return false;
        }
    }

    /// <inheritdoc />
    public IntPtr? Parent { get; set; }

    /// <inheritdoc />
    public IInteropWindow ParentWindow { get; set; }

    /// <inheritdoc />
    public string Caption { get; set; }

    /// <inheritdoc />
    public string Text { get; set; }

    /// <inheritdoc />
    public bool? IsVisible { get; set; }

    /// <inheritdoc />
    public bool? IsMinimized { get; set; }

    /// <inheritdoc />
    public bool? IsMaximized { get; set; }

    /// <inheritdoc />
    public int? ThreadId { get; set; }

    /// <inheritdoc />
    public int? ProcessId { get; set; }

    /// <inheritdoc />
    public WindowPlacement? Placement { get; set; }

    /// <inheritdoc />
    public bool? CanScroll { get; set; }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.InteropWindow" /> class.</summary>
    /// <param name="handle">IntPtr.</param>
    public InteropWindow(IntPtr handle)
    {
        _nativeHandle = handle;
        Handle = SafeNativeWindowHandle.FromUnowned(handle);
    }

    /// <inheritdoc />
    public StringBuilder Dump() => Dump(InteropWindowRetrieveSettings.CacheAll, new(), string.Empty);

    /// <inheritdoc />
    public StringBuilder Dump(InteropWindowRetrieveSettings retrieveSettings) => Dump(retrieveSettings, new(), string.Empty);

    /// <inheritdoc />
    public StringBuilder Dump(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump) => Dump(retrieveSettings, dump, string.Empty);

    /// <inheritdoc />
    public StringBuilder Dump(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump, string indentation)
    {
        _ = this.Fill(retrieveSettings);
        dump ??= new();

        AppendWindowState(retrieveSettings, dump, indentation);
        AppendChildWindows(retrieveSettings, dump, indentation);
        return dump;
    }

    /// <inheritdoc />
    public bool Equals(IInteropWindow other)
    {
        if (other is not null)
        {
            if (this != other)
            {
                return _nativeHandle.Equals(other.Handle);
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is not null)
        {
            if (this != obj)
            {
                if (obj.GetType() == GetType())
                {
                    return Equals((IInteropWindow)obj);
                }

                return false;
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => _nativeHandle.GetHashCode();

    /// <summary>Appends child window dumps when the requested settings include them.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify what to dump.</param>
    /// <param name="dump">StringBuilder to dump to.</param>
    /// <param name="indentation">Output indentation.</param>
    private void AppendChildWindows(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump, string indentation)
    {
        if (!HasParent)
        {
            if ((retrieveSettings & InteropWindowRetrieveSettings.Children) != InteropWindowRetrieveSettings.None)
            {
                AppendWindowDump(this.GetChildren());
            }

            if ((retrieveSettings & InteropWindowRetrieveSettings.ZOrderedChildren) != InteropWindowRetrieveSettings.None)
            {
                AppendWindowDump(this.GetZOrderedChildren());
            }
        }

        void AppendWindowDump(IEnumerable<IInteropWindow> windows)
        {
            foreach (IInteropWindow window in windows)
            {
                _ = window.Dump(retrieveSettings, dump, $"{indentation}\t");
            }
        }
    }

    /// <summary>Appends the selected state fields for this window.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify what to dump.</param>
    /// <param name="dump">StringBuilder to dump to.</param>
    /// <param name="indentation">Output indentation.</param>
    private void AppendWindowState(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump, string indentation)
    {
        _ = dump.AppendLine($"{indentation}{"Handle"}={_nativeHandle}");
        AppendValue(InteropWindowRetrieveSettings.Classname, "Classname", Classname);
        AppendValue(InteropWindowRetrieveSettings.Caption, "Caption", Caption);
        AppendValue(InteropWindowRetrieveSettings.Text, "Text", Text);
        AppendValue(InteropWindowRetrieveSettings.Info, "Info", Info);
        AppendValue(InteropWindowRetrieveSettings.Maximized, "IsMaximized", IsMaximized);
        AppendValue(InteropWindowRetrieveSettings.Minimized, "IsMinimized", IsMinimized);
        AppendValue(InteropWindowRetrieveSettings.Visible, "IsVisible", IsVisible);
        AppendValue(InteropWindowRetrieveSettings.Parent, "Parent", Parent);
        AppendValue(InteropWindowRetrieveSettings.ScrollInfo, "CanScroll", CanScroll);
        void AppendValue(InteropWindowRetrieveSettings requestedSetting, string name, object value)
        {
            if ((retrieveSettings & requestedSetting) != InteropWindowRetrieveSettings.None)
            {
                _ = dump.AppendLine($"{indentation}{name}={value}");
            }
        }
    }
}
