// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Windows.InteropWindow" /> class.</summary>
    /// <param name="handle">IntPtr.</param>
    public InteropWindow(IntPtr handle)
    {
        _nativeHandle = handle;
        Handle = SafeNativeWindowHandle.FromUnowned(handle);
    }

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
    public bool HasParent => Parent.HasValue && Parent != IntPtr.Zero;

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
    public bool Equals(IInteropWindow other) => other is not null && (ReferenceEquals(this, other) || _nativeHandle.Equals(other.Handle));

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is IInteropWindow other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _nativeHandle.GetHashCode();

    /// <summary>Appends child window dumps when the requested settings include them.</summary>
    /// <param name="retrieveSettings">InteropWindowRetrieveSettings to specify what to dump.</param>
    /// <param name="dump">StringBuilder to dump to.</param>
    /// <param name="indentation">Output indentation.</param>
    private void AppendChildWindows(InteropWindowRetrieveSettings retrieveSettings, StringBuilder dump, string indentation)
    {
        if (HasParent)
        {
            return;
        }

        if ((retrieveSettings & InteropWindowRetrieveSettings.Children) != InteropWindowRetrieveSettings.None)
        {
            AppendWindowDump(this.GetChildren());
        }

        if ((retrieveSettings & InteropWindowRetrieveSettings.ZOrderedChildren) != InteropWindowRetrieveSettings.None)
        {
            AppendWindowDump(this.GetZOrderedChildren());
        }

        void AppendWindowDump(IEnumerable<IInteropWindow> windows)
        {
            foreach (var window in windows)
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
        _ = dump.AppendLine($"{indentation}{nameof(Handle)}={_nativeHandle}");
        AppendValue(InteropWindowRetrieveSettings.Classname, nameof(Classname), Classname);
        AppendValue(InteropWindowRetrieveSettings.Caption, nameof(Caption), Caption);
        AppendValue(InteropWindowRetrieveSettings.Text, nameof(Text), Text);
        AppendValue(InteropWindowRetrieveSettings.Info, nameof(Info), Info);
        AppendValue(InteropWindowRetrieveSettings.Maximized, nameof(IsMaximized), IsMaximized);
        AppendValue(InteropWindowRetrieveSettings.Minimized, nameof(IsMinimized), IsMinimized);
        AppendValue(InteropWindowRetrieveSettings.Visible, nameof(IsVisible), IsVisible);
        AppendValue(InteropWindowRetrieveSettings.Parent, nameof(Parent), Parent);
        AppendValue(InteropWindowRetrieveSettings.ScrollInfo, nameof(CanScroll), CanScroll);
        void AppendValue(InteropWindowRetrieveSettings requestedSetting, string name, object value)
        {
            if ((retrieveSettings & requestedSetting) != InteropWindowRetrieveSettings.None)
            {
                _ = dump.AppendLine($"{indentation}{name}={value}");
            }
        }
    }
}
