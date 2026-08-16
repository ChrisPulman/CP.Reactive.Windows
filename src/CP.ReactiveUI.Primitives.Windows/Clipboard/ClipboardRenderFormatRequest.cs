// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>Information about the render format request.</summary>
public class ClipboardRenderFormatRequest
{
    /// <summary>Gets the requested format identifier.</summary>
    public uint RequestedFormatId { get; internal set; }

    /// <summary>Gets the requested format name.</summary>
    public string RequestedFormat => ClipboardFormatExtensions.MapIdToFormat(RequestedFormatId);

    /// <summary>Gets a value indicating whether this request specifies that the clipboard is destroyed.</summary>
    public bool IsDestroyClipboard { get; internal set; }

    /// <summary>Gets a value indicating whether all formats should be rendered.</summary>
    public bool RenderAllFormats => RequestedFormatId == 0;

    /// <summary>Gets the access token for clipboard access.</summary>
    public IClipboardAccessToken AccessToken { get; internal set; }
}
