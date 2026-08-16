// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Release coverage for deterministic Windows system reactive primitives.</summary>
public sealed class CoverageReleaseWindowsSystemTests
{
    /// <summary>The deterministic clipboard window handle.</summary>
    private const int ClipboardWindowHandle = 0x3201;

    /// <summary>The deterministic clipboard sequence number.</summary>
    private const uint ClipboardSequenceNumber = 0x3202U;

    /// <summary>The deterministic clipboard format identifier.</summary>
    private const uint ClipboardFormatId = 0xC321U;

    /// <summary>The deterministic native clipboard format name.</summary>
    private const string NativeClipboardFormatName = "CP_REACTIVE_RELEASE_NATIVE_CLIPBOARD_FORMAT";

    /// <summary>The deterministic clipboard text payload.</summary>
    private const string ClipboardText = "Release clipboard text";

    /// <summary>The expected update count including the initial notification.</summary>
    private const int ExpectedUpdateCount = 2;

    /// <summary>The expected render-request notification count.</summary>
    private const int ExpectedRenderRequestCount = 3;

    /// <summary>Covers native clipboard format-name success and miss branches through a deterministic seam.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardNativeFormatNameLookupCoversSuccessAndMissingBranchesAsync()
    {
        using var nativeFormatNameScope = ClipboardFormatExtensions.OverrideNativeFormatNameForTesting(CopyNativeClipboardFormatName);

        await Assert.That(ClipboardFormatExtensions.GetNativeFormatNameForTesting(ClipboardFormatId)).IsEqualTo(NativeClipboardFormatName);
        await Assert.That(ClipboardFormatExtensions.GetNativeFormatNameForTesting((uint)StandardClipboardFormats.UnicodeText)).IsNull();
    }

    /// <summary>Covers the pinned clipboard format-name buffer without invoking the Windows clipboard API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardPinnedFormatNameBufferUsesTheDeterministicOperationAsync()
    {
        using var nativeFormatNameScope = CreateNativeClipboardFormatNameScope();

        await Assert.That(ClipboardFormatExtensions.GetNativeFormatNameForTesting(ClipboardFormatId)).IsEqualTo(NativeClipboardFormatName);
        await Assert.That(ClipboardFormatExtensions.GetNativeFormatNameForTesting((uint)StandardClipboardFormats.UnicodeText)).IsNull();
    }

    /// <summary>Covers deferred clipboard message-window lifetime composition without creating a Windows message window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardMessageWindowLifetimeUsesTheDeterministicFactoryAsync()
    {
        var createCount = 0;
        using var lifetimeScope = ClipboardSemaphore.OverrideMessageWindowLifetimeFactoryForTesting(
            () =>
            {
                createCount++;
                return new NoopDisposable();
            });
        using var lifetime = ClipboardSemaphore.GetMessageWindowLifetimeForTesting();

        await Assert.That(createCount).IsEqualTo(1);
    }

    /// <summary>Covers the production clipboard lifetime factory against an injected message stream.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardDefaultMessageWindowLifetimeSubscribesWithoutCreatingANativeWindowAsync()
    {
        using var messages = new ManualObservable<WindowMessage>();
        using var handles = new ManualObservable<nint>();
        using var streams = SharedMessageWindow.OverrideStreamsForTesting(messages, handles, 0);
        using var lifetime = ClipboardSemaphore.GetMessageWindowLifetimeForTesting();

        await Assert.That(lifetime).IsNotNull();
    }

    /// <summary>Covers power-broadcast projection from a deterministic message stream.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PowerBroadcastMessageProjectionFiltersAndProjectsWithoutWindowsPowerCallsAsync()
    {
        using var messages = new ManualObservable<WindowMessage>();
        var observer = new CoreInteropCoverageTests.RecordingObserver<PowerBroadcastEvent>();
        using var subscription = PowerBroadcastListener.CreatePowerBroadcastEventsForTesting(messages).Subscribe(observer);

        messages.OnNext(new(IntPtr.Zero, WindowsMessages.WM_NULL, IntPtr.Zero, IntPtr.Zero));
        messages.OnNext(new(
            IntPtr.Zero,
            WindowsMessages.WM_POWERBROADCAST,
            (nint)(uint)PowerBroadcastEvent.PBT_APMSUSPEND,
            IntPtr.Zero));

        await Assert.That(observer.Error).IsNull();
        await Assert.That(observer.Values.Count).IsEqualTo(1);
        await Assert.That(observer.Values[0]).IsEqualTo(PowerBroadcastEvent.PBT_APMSUSPEND);
    }

    /// <summary>Covers clipboard update listener error, update de-duplication, and render-request filtering.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardNativeObservablesCoverListenerErrorsDuplicateUpdatesAndRenderFilteringAsync()
    {
        var windowHandle = new IntPtr(ClipboardWindowHandle);

        ClipboardNative.ResetObservablesForTesting();
        using (var messages = new ManualObservable<WindowMessage>())
        using (var handles = new ManualObservable<nint>(windowHandle, emitInitial: true))
        using (SharedMessageWindow.OverrideStreamsForTesting(messages, handles, windowHandle))
        using (NativeMethods.OverrideClipboardListenerOperationsForTesting(static _ => false, static _ => true))
        {
            var updateObserver = new CoreInteropCoverageTests.RecordingObserver<ClipboardUpdateInformation>();
            using var subscription = ClipboardNative.ClipboardUpdateEvents.Subscribe(updateObserver);

            await Assert.That(updateObserver.Error).IsTypeOf<Win32Exception>();
        }

        ClipboardNative.ResetObservablesForTesting();
        using (var messages = new ManualObservable<WindowMessage>())
        using (var handles = new ManualObservable<nint>(windowHandle, emitInitial: true))
        using (SharedMessageWindow.OverrideStreamsForTesting(messages, handles, windowHandle))
        using (ClipboardNative.OverrideOperationsForTesting(static () => new(ClipboardWindowHandle), static () => ClipboardSequenceNumber, static _ => true))
        using (ClipboardSemaphore.OverrideOperationsForTesting(static _ => true, static () => true, static (_, _) => new(ClipboardWindowHandle)))
        using (ClipboardFormatExtensions.OverrideOperationsForTesting(static _ => 0U, static _ => ClipboardFormatId, static _ => null, static () => 0))
        using (NativeMethods.OverrideClipboardListenerOperationsForTesting(static _ => true, static _ => true))
        {
            var updateObserver = new CoreInteropCoverageTests.RecordingObserver<ClipboardUpdateInformation>();
            using var subscription = ClipboardNative.ClipboardUpdateEvents.Subscribe(updateObserver);

            messages.OnNext(new(windowHandle, WindowsMessages.WM_CLIPBOARDUPDATE, IntPtr.Zero, IntPtr.Zero));
            messages.OnNext(new(windowHandle, WindowsMessages.WM_CLIPBOARDUPDATE, IntPtr.Zero, IntPtr.Zero));

            await Assert.That(updateObserver.Error).IsNull();
            await Assert.That(updateObserver.Values.Count).IsEqualTo(ExpectedUpdateCount);
            await Assert.That(updateObserver.Values[1].Id).IsEqualTo(ClipboardSequenceNumber);
        }

        ClipboardNative.ResetObservablesForTesting();
        using (var messages = new ManualObservable<WindowMessage>())
        using (var handles = new ManualObservable<nint>(windowHandle, emitInitial: true))
        using (SharedMessageWindow.OverrideStreamsForTesting(messages, handles, windowHandle))
        using (ClipboardSemaphore.OverrideOperationsForTesting(static _ => true, static () => true, static (_, _) => new(ClipboardWindowHandle)))
        {
            var renderObserver = new CoreInteropCoverageTests.RecordingObserver<ClipboardRenderFormatRequest>();
            using var subscription = ClipboardNative.ClipboardRenderFormatRequests.Subscribe(renderObserver);

            messages.OnNext(new(windowHandle, WindowsMessages.WM_NULL, IntPtr.Zero, IntPtr.Zero));
            messages.OnNext(new(windowHandle, WindowsMessages.WM_RENDERFORMAT, new IntPtr(ClipboardFormatId), IntPtr.Zero));
            messages.OnNext(new(windowHandle, WindowsMessages.WM_DESTROYCLIPBOARD, IntPtr.Zero, IntPtr.Zero));
            messages.OnNext(new(windowHandle, WindowsMessages.WM_RENDERALLFORMATS, IntPtr.Zero, IntPtr.Zero));

            await Assert.That(renderObserver.Error).IsNull();
            await Assert.That(renderObserver.Values.Count).IsEqualTo(ExpectedRenderRequestCount);
            await Assert.That(renderObserver.Values[0].RequestedFormatId).IsEqualTo(ClipboardFormatId);
            await Assert.That(renderObserver.Values[1].IsDestroyClipboard).IsTrue();
            await Assert.That(renderObserver.Values[2].AccessToken.CanAccess).IsTrue();
            renderObserver.Values[2].AccessToken.Dispose();
        }

        ClipboardNative.ResetObservablesForTesting();
    }

    /// <summary>Covers render-request switch branches directly without touching the real clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardRenderFormatRequestFactoryCoversAllSwitchBranchesAsync()
    {
        var windowHandle = new IntPtr(ClipboardWindowHandle);
        using var semaphoreScope = ClipboardSemaphore.OverrideOperationsForTesting(static _ => true, static () => true, static (_, _) => new(ClipboardWindowHandle));

        var allFormats = ClipboardNative.CreateRenderFormatRequestForTesting(
            new(windowHandle, WindowsMessages.WM_RENDERALLFORMATS, IntPtr.Zero, IntPtr.Zero));
        var singleFormat = ClipboardNative.CreateRenderFormatRequestForTesting(
            new(windowHandle, WindowsMessages.WM_RENDERFORMAT, new IntPtr(ClipboardFormatId), IntPtr.Zero));
        var destroyClipboard = ClipboardNative.CreateRenderFormatRequestForTesting(
            new(windowHandle, WindowsMessages.WM_DESTROYCLIPBOARD, IntPtr.Zero, IntPtr.Zero));

        await Assert.That(allFormats.AccessToken.CanAccess).IsTrue();
        await Assert.That(singleFormat.RequestedFormatId).IsEqualTo(ClipboardFormatId);
        await Assert.That(singleFormat.AccessToken.IsLockTimeout).IsTrue();
        await Assert.That(destroyClipboard.IsDestroyClipboard).IsTrue();
        await Assert.That(() => ClipboardNative.CreateRenderFormatRequestForTesting(
            new(windowHandle, WindowsMessages.WM_NULL, IntPtr.Zero, IntPtr.Zero))).Throws<InvalidOperationException>();

        allFormats.AccessToken.Dispose();
        singleFormat.AccessToken.Dispose();
    }

    /// <summary>Covers the standard-format unicode string read overload with deterministic global memory.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardStringStandardFormatReadOverloadReadsUnicodeGlobalMemoryAsync()
    {
        byte[] unicodeBytes = Encoding.Unicode.GetBytes($"{ClipboardText}\0");
        IntPtr globalHandle = Kernel32Api.GlobalAlloc(
            GlobalMemorySettings.Movable | GlobalMemorySettings.ZeroInit,
            new((uint)unicodeBytes.Length));

        try
        {
            await Assert.That(globalHandle).IsNotEqualTo(IntPtr.Zero);
            IntPtr memoryPointer = Kernel32Api.GlobalLock(globalHandle);
            try
            {
                await Assert.That(memoryPointer).IsNotEqualTo(IntPtr.Zero);
                Marshal.Copy(unicodeBytes, 0, memoryPointer, unicodeBytes.Length);
            }
            finally
            {
                _ = Kernel32Api.GlobalUnlock(globalHandle);
            }

            using var infoScope = ClipboardInfoExtensions.OverrideOperationsForTesting(
                formatId => formatId == (uint)StandardClipboardFormats.UnicodeText ? globalHandle : IntPtr.Zero,
                static formatId => formatId == (uint)StandardClipboardFormats.UnicodeText,
                Kernel32Api.GlobalAlloc,
                Kernel32Api.GlobalLock,
                NativeMethods.GlobalFree);
            using var token = new ClipboardAccessToken();

            await Assert.That(token.GetAsUnicodeString(StandardClipboardFormats.UnicodeText)).IsEqualTo(ClipboardText);
        }
        finally
        {
            if (globalHandle != IntPtr.Zero)
            {
                _ = NativeMethods.GlobalFree(globalHandle);
            }
        }
    }

    /// <summary>Copies the deterministic native clipboard format name into the provided destination buffer.</summary>
    /// <param name="formatId">The requested format identifier.</param>
    /// <param name="destination">The destination buffer.</param>
    /// <returns>The copied character count, or zero for a missing format.</returns>
    private static int CopyNativeClipboardFormatName(uint formatId, Span<char> destination)
    {
        if (formatId != ClipboardFormatId)
        {
            return 0;
        }

        NativeClipboardFormatName.AsSpan().CopyTo(destination);
        return NativeClipboardFormatName.Length;
    }

    /// <summary>Copies a deterministic native clipboard format name through a pinned pointer buffer.</summary>
    /// <param name="formatId">The requested clipboard format identifier.</param>
    /// <param name="destination">The pinned destination character buffer.</param>
    /// <param name="characterCapacity">The capacity of <paramref name="destination" /> in characters.</param>
    /// <returns>The copied character count, or zero for a missing format.</returns>
    private static unsafe int CopyNativeClipboardFormatNameToPointer(uint formatId, char* destination, int characterCapacity)
    {
        if (formatId != ClipboardFormatId)
        {
            return 0;
        }

        NativeClipboardFormatName.AsSpan().CopyTo(new(destination, characterCapacity));
        return NativeClipboardFormatName.Length;
    }

    /// <summary>Creates a deterministic native clipboard-format-name override in an unsafe pointer context.</summary>
    /// <returns>The disposable scope that restores the native clipboard format-name operation.</returns>
    private static IDisposable CreateNativeClipboardFormatNameScope()
    {
        unsafe
        {
            return ClipboardFormatExtensions.OverrideNativeClipboardFormatNameForTesting(CopyNativeClipboardFormatNameToPointer);
        }
    }

    /// <summary>Represents a deterministic disposable message-window lifetime.</summary>
    private sealed class NoopDisposable : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
