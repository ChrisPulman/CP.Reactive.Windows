// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>Extensions for Windows Cloud Clipboard and Clipboard History support.</summary>
public static class ClipboardCloudExtensions
{
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>
        /// Sets cloud clipboard options on the clipboard to control clipboard history and cloud sync behavior.
        /// This is a simplified method that sets all three cloud clipboard formats at once.
        /// </summary>
        public void SetCloudClipboardOptions() => clipboardAccessToken.SetCloudClipboardOptions(canIncludeInHistory: true, canUploadToCloud: true, excludeFromMonitoring: false);

        /// <summary>
        /// Sets cloud clipboard options on the clipboard to control clipboard history and cloud sync behavior.
        /// This is a simplified method that sets all three cloud clipboard formats at once.
        /// </summary>
        /// <param name="canIncludeInHistory">
        /// When true (default), allows the clipboard content to be included in clipboard history.
        /// When false, prevents the content from appearing in clipboard history.
        /// </param>
        /// <param name="canUploadToCloud">
        /// When true (default), allows the clipboard content to be uploaded to the cloud clipboard.
        /// When false, prevents the content from being synced to cloud.
        /// </param>
        /// <param name="excludeFromMonitoring">
        /// When true, excludes the clipboard content from being processed by clipboard monitoring applications.
        /// When false (default), allows monitoring applications to process the content.
        /// </param>
        public void SetCloudClipboardOptions(bool canIncludeInHistory, bool canUploadToCloud, bool excludeFromMonitoring)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            SetDWordFormat(clipboardAccessToken, CanIncludeInClipboardHistoryFormat, canIncludeInHistory ? 1U : 0U);
            SetDWordFormat(clipboardAccessToken, CanUploadToCloudClipboardFormat, canUploadToCloud ? 1U : 0U);
            SetDWordFormat(clipboardAccessToken, ExcludeClipboardContentFromMonitorProcessingFormat, excludeFromMonitoring ? 1U : 0U);
        }

        /// <summary>Sets whether clipboard content can be included in clipboard history.</summary>
        public void SetCanIncludeInClipboardHistory() => clipboardAccessToken.SetCanIncludeInClipboardHistory(canInclude: true);

        /// <summary>Sets whether clipboard content can be included in clipboard history.</summary>
        /// <param name="canInclude">True to allow inclusion in history, false to prevent it.</param>
        public void SetCanIncludeInClipboardHistory(bool canInclude)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            SetDWordFormat(clipboardAccessToken, CanIncludeInClipboardHistoryFormat, canInclude ? 1U : 0U);
        }

        /// <summary>Sets whether clipboard content can be uploaded to cloud clipboard.</summary>
        public void SetCanUploadToCloudClipboard() => clipboardAccessToken.SetCanUploadToCloudClipboard(canUpload: true);

        /// <summary>Sets whether clipboard content can be uploaded to cloud clipboard.</summary>
        /// <param name="canUpload">True to allow cloud upload, false to prevent it.</param>
        public void SetCanUploadToCloudClipboard(bool canUpload)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            SetDWordFormat(clipboardAccessToken, CanUploadToCloudClipboardFormat, canUpload ? 1U : 0U);
        }

        /// <summary>Sets whether clipboard content should be excluded from clipboard monitor processing.</summary>
        public void SetExcludeClipboardContentFromMonitorProcessing() => clipboardAccessToken.SetExcludeClipboardContentFromMonitorProcessing(exclude: true);

        /// <summary>Sets whether clipboard content should be excluded from clipboard monitor processing.</summary>
        /// <param name="exclude">True to exclude from monitoring, false to allow monitoring.</param>
        public void SetExcludeClipboardContentFromMonitorProcessing(bool exclude)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            SetDWordFormat(clipboardAccessToken, ExcludeClipboardContentFromMonitorProcessingFormat, exclude ? 1U : 0U);
        }
    }

    /// <summary>Gets the format name controlling whether clipboard content can be included in clipboard history.</summary>
    public static string CanIncludeInClipboardHistoryFormat { get; } = "CanIncludeInClipboardHistory";

    /// <summary>Gets the format name for excluding clipboard content from monitor processing.</summary>
    public static string ExcludeClipboardContentFromMonitorProcessingFormat { get; } = "ExcludeClipboardContentFromMonitorProcessing";

    /// <summary>Gets the format name controlling whether clipboard content can be uploaded to cloud clipboard.</summary>
    public static string CanUploadToCloudClipboardFormat { get; } = "CanUploadToCloudClipboard";

    /// <summary>Helper method to set a DWORD (uint32) value on the clipboard for a specific format.</summary>
    /// <param name="clipboardAccessToken">The clipboard access token.</param>
    /// <param name="format">The clipboard format name.</param>
    /// <param name="value">The DWORD value to set.</param>
    private static void SetDWordFormat(IClipboardAccessToken clipboardAccessToken, string format, uint value)
    {
        clipboardAccessToken.ThrowWhenNoAccess();
        uint formatId = ClipboardFormatExtensions.MapFormatToId(format);
        using ClipboardNativeInfo writeInfo = clipboardAccessToken.WriteInfo(formatId, 4L);
        Marshal.WriteInt32(writeInfo.MemoryPtr, checked((int)value));
    }
}
