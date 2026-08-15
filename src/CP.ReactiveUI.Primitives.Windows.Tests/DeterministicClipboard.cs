// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides an in-memory clipboard backend behind the production extension-method surface.</summary>
internal sealed class DeterministicClipboard : IDisposable
{
    /// <summary>The first custom clipboard format identifier.</summary>
    private const uint FirstCustomFormatId = 0xC000U;

    /// <summary>The deterministic monitor test format identifier.</summary>
    private const uint MonitorTestFormatId = FirstCustomFormatId;

    /// <summary>The deterministic cloud history format identifier.</summary>
    private const uint CloudHistoryFormatId = 0xC001U;

    /// <summary>The deterministic cloud upload format identifier.</summary>
    private const uint CloudUploadFormatId = 0xC002U;

    /// <summary>The deterministic cloud monitor exclusion format identifier.</summary>
    private const uint CloudMonitorExclusionFormatId = 0xC003U;

    /// <summary>The first generated custom clipboard format identifier.</summary>
    private const uint FirstGeneratedFormatId = 0xC004U;

    /// <summary>The deterministic message-window handle.</summary>
    private const int MessageWindowHandle = 42;

    /// <summary>The delay before clipboard update messages are published.</summary>
    private const int UpdateNotificationDelayMilliseconds = 10;

    /// <summary>The custom clipboard format used by clipboard monitor tests.</summary>
    private const string ClipboardMonitorTestFormat = "TEST_FORMAT";

    /// <summary>The deterministic message-window handle.</summary>
    private static readonly IntPtr WindowHandle = new(MessageWindowHandle);

    /// <summary>Serializes installation of process-wide clipboard test overrides.</summary>
    private static readonly SemaphoreSlim OverrideGate = new(1, 1);

    /// <summary>Synchronizes access to the clipboard maps.</summary>
    private readonly Lock _syncRoot = new();

    /// <summary>The deterministic message stream.</summary>
    private readonly ManualObservable<WindowMessage> _messages = new();

    /// <summary>The deterministic handle-change stream.</summary>
    private readonly ManualObservable<nint> _handles = new(WindowHandle, emitInitial: true);

    /// <summary>The registered format names by identifier.</summary>
    private readonly Dictionary<uint, string> _formatNames = new();

    /// <summary>The registered format identifiers by name.</summary>
    private readonly Dictionary<string, uint> _formatIds = new(StringComparer.Ordinal);

    /// <summary>The clipboard memory handles by format identifier.</summary>
    private readonly Dictionary<uint, IntPtr> _formatHandles = new();

    /// <summary>The installed override scopes.</summary>
    private readonly List<IDisposable> _scopes = new();

    /// <summary>The next custom clipboard format identifier.</summary>
    private uint _nextFormatId = FirstGeneratedFormatId;

    /// <summary>The deterministic clipboard sequence number.</summary>
    private uint _sequence;

    /// <summary>A value indicating whether this instance is already disposed.</summary>
    private bool _disposed;

    /// <summary>A value indicating whether this instance owns the process-wide override gate.</summary>
    private bool _ownsOverrideGate;

    /// <summary>Initializes a new instance of the <see cref="DeterministicClipboard" /> class.</summary>
    public DeterministicClipboard()
    {
        OverrideGate.Wait();
        _ownsOverrideGate = true;
        try
        {
            ClipboardNative.ResetObservablesForTesting();
            _scopes.Add(SharedMessageWindow.OverrideStreamsForTesting(_messages, _handles, WindowHandle));
            _scopes.Add(ClipboardSemaphore.OverrideOperationsForTesting(static _ => true, static () => true, static (_, _) => WindowHandle));
            _scopes.Add(ClipboardNative.OverrideOperationsForTesting(static () => WindowHandle, GetSequenceNumber, HasFormat));
            _scopes.Add(ClipboardFormatExtensions.OverrideOperationsForTesting(EnumFormats, RegisterFormat, GetFormatName, static () => 0));
            RegisterKnownFormats();
            _scopes.Add(ClipboardInfoExtensions.OverrideOperationsForTesting(
                GetClipboardData,
                HasFormat,
                Kernel32Api.GlobalAlloc,
                Kernel32Api.GlobalLock,
                NativeMethods.GlobalFree));
            _scopes.Add(NativeMethods.OverrideSetClipboardDataForTesting(SetClipboardData));
            _scopes.Add(NativeMethods.OverrideEmptyClipboardForTesting(Clear));
            _scopes.Add(NativeMethods.OverrideClipboardListenerOperationsForTesting(static _ => true, static _ => true));
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ClipboardNative.ResetObservablesForTesting();
        _ = ClearCore(publishUpdate: false);
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            _scopes[i].Dispose();
        }

        _scopes.Clear();
        _messages.Dispose();
        _handles.Dispose();
        ClipboardNative.ResetObservablesForTesting();
        if (_ownsOverrideGate)
        {
            _ownsOverrideGate = false;
            _ = OverrideGate.Release();
        }
    }

    /// <summary>Empties deterministic clipboard content.</summary>
    /// <returns><see langword="true" />.</returns>
    private bool Clear() => ClearCore(publishUpdate: false);

    /// <summary>Empties deterministic clipboard content.</summary>
    /// <param name="publishUpdate">A value indicating whether to publish an update message.</param>
    /// <returns><see langword="true" />.</returns>
    private bool ClearCore(bool publishUpdate)
    {
        lock (_syncRoot)
        {
            foreach (IntPtr handle in _formatHandles.Values)
            {
                if (handle != IntPtr.Zero)
                {
                    _ = NativeMethods.GlobalFree(handle);
                }
            }

            _formatHandles.Clear();
            _sequence = checked(_sequence + 1U);
        }

        if (publishUpdate)
        {
            PublishClipboardUpdate();
        }

        return true;
    }

    /// <summary>Enumerates deterministic clipboard formats.</summary>
    /// <param name="previousFormatId">The previous format identifier.</param>
    /// <returns>The next format identifier, or zero.</returns>
    private uint EnumFormats(uint previousFormatId)
    {
        lock (_syncRoot)
        {
            List<uint> formats = new(_formatHandles.Keys);
            formats.Sort();
            if (previousFormatId == 0U)
            {
                return formats.Count == 0 ? 0U : formats[0];
            }

            int previousIndex = formats.IndexOf(previousFormatId);
            int nextIndex = checked(previousIndex + 1);
            return previousIndex >= 0 && nextIndex < formats.Count ? formats[nextIndex] : 0U;
        }
    }

    /// <summary>Gets clipboard data for a deterministic format.</summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>The global memory handle, or zero.</returns>
    private IntPtr GetClipboardData(uint formatId)
    {
        lock (_syncRoot)
        {
            if (!_formatHandles.TryGetValue(formatId, out IntPtr handle))
            {
                return IntPtr.Zero;
            }

            if (handle != IntPtr.Zero)
            {
                return handle;
            }
        }

        _messages.OnNext(new(WindowHandle, WindowsMessages.WM_RENDERFORMAT, new IntPtr(checked((long)formatId)), IntPtr.Zero));
        lock (_syncRoot)
        {
            return _formatHandles.TryGetValue(formatId, out IntPtr renderedHandle) ? renderedHandle : IntPtr.Zero;
        }
    }

    /// <summary>Gets a deterministic custom format name.</summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns>The registered format name, or <see langword="null" />.</returns>
    private string GetFormatName(uint formatId)
    {
        lock (_syncRoot)
        {
            return _formatNames.TryGetValue(formatId, out string formatName) ? formatName : null;
        }
    }

    /// <summary>Gets whether a deterministic format is available.</summary>
    /// <param name="formatId">The format identifier.</param>
    /// <returns><see langword="true" /> when the format is available.</returns>
    private bool HasFormat(uint formatId)
    {
        lock (_syncRoot)
        {
            return _formatHandles.ContainsKey(formatId);
        }
    }

    /// <summary>Gets the deterministic clipboard sequence number.</summary>
    /// <returns>The sequence number.</returns>
    private uint GetSequenceNumber()
    {
        lock (_syncRoot)
        {
            return _sequence;
        }
    }

    /// <summary>Publishes a deterministic clipboard update message.</summary>
    private void PublishClipboardUpdate() =>
        _messages.OnNext(new(WindowHandle, WindowsMessages.WM_CLIPBOARDUPDATE, IntPtr.Zero, IntPtr.Zero));

    /// <summary>Registers deterministic names that may already exist in static format caches.</summary>
    private void RegisterKnownFormats()
    {
        RegisterKnownFormat(ClipboardMonitorTestFormat, MonitorTestFormatId);
        RegisterKnownFormat(ClipboardCloudExtensions.CanIncludeInClipboardHistoryFormat, CloudHistoryFormatId);
        RegisterKnownFormat(ClipboardCloudExtensions.CanUploadToCloudClipboardFormat, CloudUploadFormatId);
        RegisterKnownFormat(ClipboardCloudExtensions.ExcludeClipboardContentFromMonitorProcessingFormat, CloudMonitorExclusionFormatId);
    }

    /// <summary>Registers a deterministic name for the current public format identifier.</summary>
    /// <param name="format">The known format name.</param>
    /// <param name="formatId">The known format identifier.</param>
    private void RegisterKnownFormat(string format, uint formatId)
    {
        ClipboardFormatExtensions.RegisterCachedFormatForTesting(format, formatId);
        lock (_syncRoot)
        {
            _formatIds[format] = formatId;
            _formatNames[formatId] = format;
        }
    }

    /// <summary>Publishes a deterministic clipboard update message asynchronously.</summary>
    /// <returns>A task representing the asynchronous publish.</returns>
    private async Task PublishClipboardUpdateAsync()
    {
        await Task.Delay(UpdateNotificationDelayMilliseconds).ConfigureAwait(false);
        PublishClipboardUpdate();
    }

    /// <summary>Registers or resolves a deterministic format name.</summary>
    /// <param name="format">The format name.</param>
    /// <returns>The format identifier.</returns>
    private uint RegisterFormat(string format)
    {
        lock (_syncRoot)
        {
            if (_formatIds.TryGetValue(format, out uint existingFormatId))
            {
                return existingFormatId;
            }

            uint formatId = _nextFormatId;
            _nextFormatId = checked(_nextFormatId + 1U);
            _formatIds[format] = formatId;
            _formatNames[formatId] = format;
            return formatId;
        }
    }

    /// <summary>Stores deterministic clipboard data.</summary>
    /// <param name="formatId">The format identifier.</param>
    /// <param name="memory">The clipboard memory handle.</param>
    /// <returns>The accepted clipboard memory handle.</returns>
    private IntPtr SetClipboardData(uint formatId, IntPtr memory)
    {
        lock (_syncRoot)
        {
            if (_formatHandles.TryGetValue(formatId, out IntPtr previousHandle) && previousHandle != IntPtr.Zero && previousHandle != memory)
            {
                _ = NativeMethods.GlobalFree(previousHandle);
            }

            _formatHandles[formatId] = memory;
            _sequence = checked(_sequence + 1U);
        }

        _ = Task.Run(PublishClipboardUpdateAsync);
        return memory == IntPtr.Zero ? new(1) : memory;
    }
}
