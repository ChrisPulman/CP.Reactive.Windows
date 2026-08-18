// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>Information about what the clipboard contained at the most recent clipboard update.</summary>
public class ClipboardUpdateInformation
{
    /// <summary>Gets the default clipboard-owner window handle.</summary>
    private static Func<IntPtr> _getSharedMessageWindowHandle = static () => SharedMessageWindow.NativeHandle;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.ClipboardUpdateInformation" /> class.</summary>
    /// <param name="clipboardAccessToken">The clipboard access token.</param>
    private ClipboardUpdateInformation(IClipboardAccessToken clipboardAccessToken)
    {
        List<uint> formatIds = [.. clipboardAccessToken.AvailableFormatIds()];

        FormatIds = formatIds;
    }

    /// <summary>Gets the clipboard sequence number, which starts at 0 when the Windows session starts.</summary>
    public uint Id { get; } = ClipboardNative.SequenceNumber;

    /// <summary>Gets the timestamp of the clipboard update event.</summary>
    public DateTimeOffset Timestamp { get; } = TimeProvider.System.GetUtcNow();

    /// <summary>Gets whether a window owns the clipboard content.</summary>
    public bool HasOwner { get; } = ClipboardNative.HasOwner;

    /// <summary>Gets the formats in this clipboard content as strings.</summary>
    public IEnumerable<string> Formats
    {
        get
        {
            foreach (var formatId in FormatIds)
            {
                var format = ClipboardFormatExtensions.MapIdToFormat(formatId);
                if (!string.IsNullOrEmpty(format))
                {
                    yield return format;
                }
            }
        }
    }

    /// <summary>Gets the formats in this clipboard content as identifiers.</summary>
    public IEnumerable<uint> FormatIds { get; }

    /// <summary>Creates clipboard update information.</summary>
    /// <returns>The clipboard update information.</returns>
    public static ClipboardUpdateInformation Create() => Create(_getSharedMessageWindowHandle());

    /// <summary>Creates clipboard update information.</summary>
    /// <param name="windowHandle">The window handle for the clipboard lock.</param>
    /// <returns>The clipboard update information.</returns>
    public static ClipboardUpdateInformation Create(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            windowHandle = _getSharedMessageWindowHandle();
        }

        using var clipboard = ClipboardNative.Access(windowHandle);
        return new(clipboard);
    }

    /// <summary>Overrides the default clipboard-owner window lookup for deterministic tests.</summary>
    /// <param name="getSharedMessageWindowHandle">The replacement shared message-window handle lookup.</param>
    /// <returns>A scope that restores the previous lookup.</returns>
    internal static IDisposable OverrideSharedMessageWindowHandleForTesting(Func<IntPtr> getSharedMessageWindowHandle)
    {
        Throw.IfNull(getSharedMessageWindowHandle);
        var previous = _getSharedMessageWindowHandle;
        _getSharedMessageWindowHandle = getSharedMessageWindowHandle;
        return Scope.Create(previous, static operation => _getSharedMessageWindowHandle = operation);
    }
}
