// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for clipboard primitives.</summary>
public sealed class CoverageFinalClipboardTests
{
    /// <summary>A successful native last-error value.</summary>
    private const int SuccessError = 0;

    /// <summary>A failing native last-error value.</summary>
    private const int FailureError = 5;

    /// <summary>The expected two-item count.</summary>
    private const int TwoItemCount = 2;

    /// <summary>A fake native owner window handle.</summary>
    private const int OwnerWindowHandle = 123;

    /// <summary>A deterministic clipboard sequence number.</summary>
    private const uint SequenceNumber = 789;

    /// <summary>A deterministic registered clipboard format identifier.</summary>
    private const uint RegisteredFormatId = 0xC101;

    /// <summary>A deterministic named clipboard format identifier.</summary>
    private const uint NamedFormatId = 0xC102;

    /// <summary>A deterministic second clipboard format identifier.</summary>
    private const uint SecondFormatId = 0xC103;

    /// <summary>A missing clipboard format identifier.</summary>
    private const uint MissingFormatId = 0xC104;

    /// <summary>The deterministic cloud clipboard format identifier.</summary>
    private const uint CloudFormatId = 0xC105;

    /// <summary>A fake native global-memory handle.</summary>
    private const int GlobalHandleValue = 0x1001;

    /// <summary>A fake native memory pointer value.</summary>
    private const int MemoryPointerValue = 0x1002;

    /// <summary>The first test byte.</summary>
    private const byte FirstByte = 10;

    /// <summary>The second test byte.</summary>
    private const byte SecondByte = 20;

    /// <summary>The third test byte.</summary>
    private const byte ThirdByte = 30;

    /// <summary>The fourth test byte.</summary>
    private const byte FourthByte = 40;

    /// <summary>The first stream byte copied after seeking.</summary>
    private const byte SeekedFirstByte = 2;

    /// <summary>The second stream byte copied after seeking.</summary>
    private const byte SeekedSecondByte = 3;

    /// <summary>The third stream byte copied after seeking.</summary>
    private const byte SeekedThirdByte = 4;

    /// <summary>The copied stream length after seeking.</summary>
    private const int SeekedLength = 3;

    /// <summary>The non-seekable stream size supplied by the caller.</summary>
    private const int ExplicitNonSeekableSize = 4;

    /// <summary>The buffered non-seekable stream length.</summary>
    private const int BufferedNonSeekableLength = 2;

    /// <summary>The unmanaged wrapper memory length.</summary>
    private const int WrapperLength = 1;

    /// <summary>The native memory buffer size used by wrapper tests.</summary>
    private const int NativeMemoryBufferSize = 256;

    /// <summary>The DROPFILES header size expected by file-list clipboard data.</summary>
    private const int DropFilesHeaderSize = 20;

    /// <summary>The unmanaged wrapper byte value.</summary>
    private const byte WrapperByte = 99;

    /// <summary>The deterministic string payload.</summary>
    private const string ClipboardText = "Final clipboard text";

    /// <summary>The deterministic file path payload.</summary>
    private const string ClipboardFileName = @"C:\Temp\final-clipboard.txt";

    /// <summary>The deterministic registered format name.</summary>
    private const string RegisteredFormatName = "CP_REACTIVE_FINAL_CLIPBOARD_REGISTERED";

    /// <summary>The deterministic format name used only by wrapper overload tests.</summary>
    private const string WrapperFormatName = "CP_REACTIVE_FINAL_CLIPBOARD_WRAPPER";

    /// <summary>The deterministic format name used only by stream overload tests.</summary>
    private const string StreamFormatName = "CP_REACTIVE_FINAL_CLIPBOARD_STREAM";

    /// <summary>The deterministic format name used only by the registration cache test.</summary>
    private const string RegistrationOnlyFormatName = "CP_REACTIVE_FINAL_CLIPBOARD_REGISTRATION_ONLY";

    /// <summary>The deterministic format name used only by the native availability test.</summary>
    private const string NativeAvailabilityFormatName = "CP_REACTIVE_FINAL_CLIPBOARD_NATIVE_AVAILABILITY";

    /// <summary>The deterministic native format name.</summary>
    private const string NativeFormatName = "CP_REACTIVE_FINAL_CLIPBOARD_NATIVE";

    /// <summary>Covers clipboard native owner, sequence, and format availability composition.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardNativeOperationsExposeOwnerSequenceAndMappedAvailabilityAsync()
    {
        uint capturedFormatId = 0;
        using var formatScope = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static _ => 0,
            static _ => RegisteredFormatId,
            static _ => null,
            static () => SuccessError);
        using var nativeScope = ClipboardNative.OverrideOperationsForTesting(
            static () => new(OwnerWindowHandle),
            static () => SequenceNumber,
            formatId =>
            {
                capturedFormatId = formatId;
                return formatId == RegisteredFormatId;
            });

        await Assert.That(ClipboardNative.HasOwner).IsTrue();
        await Assert.That(ClipboardNative.SequenceNumber).IsEqualTo(SequenceNumber);
        await Assert.That(ClipboardNative.HasFormat(RegisteredFormatId)).IsTrue();
        await Assert.That(ClipboardNative.HasFormat(NativeAvailabilityFormatName)).IsTrue();
        await Assert.That(capturedFormatId).IsEqualTo(RegisteredFormatId);
    }

    /// <summary>Covers clipboard native observable rename endpoints and access overload delegation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardNativeAccessOverloadsCoverSynchronousAndAsyncDelegationAsync()
    {
        var openCalls = 0;
        var closeCalls = 0;
        using var semaphoreScope = ClipboardSemaphore.OverrideOperationsForTesting(
            _ =>
            {
                openCalls++;
                return true;
            },
            () =>
            {
                closeCalls++;
                return true;
            },
            static (_, _) => new(OwnerWindowHandle));

        await Assert.That(ClipboardNative.ClipboardUpdateEvents).IsNotNull();
        await Assert.That(ClipboardNative.ClipboardRenderFormatRequests).IsNotNull();

        var ownerHandle = new IntPtr(OwnerWindowHandle);
        await AssertCanAccessAsync(InvokeClipboardNativeAccess(nameof(ClipboardNative.Access), []));
        await AssertCanAccessAsync(InvokeClipboardNativeAccess(nameof(ClipboardNative.Access), [typeof(IntPtr)], ownerHandle));
        await AssertCanAccessAsync(InvokeClipboardNativeAccess(nameof(ClipboardNative.Access), [typeof(IntPtr), typeof(int)], ownerHandle, 0));
        await AssertCanAccessAsync(InvokeClipboardNativeAccess(nameof(ClipboardNative.Access), [typeof(IntPtr), typeof(int), typeof(TimeSpan)], ownerHandle, 0, TimeSpan.Zero));
        await AssertCanAccessAsync(InvokeClipboardNativeAccess(
            nameof(ClipboardNative.Access),
            [typeof(IntPtr), typeof(int), typeof(TimeSpan), typeof(TimeSpan)],
            ownerHandle,
            0,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1)));
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync());
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync(CancellationToken.None));
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync(ownerHandle));
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync(ownerHandle, 0));
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync(ownerHandle, 0, TimeSpan.Zero));
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync(ownerHandle, 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(1)));
        await AssertCanAccessAsync(await ClipboardNative.AccessAsync(ownerHandle, 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(1), CancellationToken.None));

        await Assert.That(openCalls).IsEqualTo(closeCalls);
    }

    /// <summary>Covers format cache misses, registration, enumeration, and enumeration failure.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardFormatOperationsCoverMappingEnumerationAndFailureAsync()
    {
        var registerCalls = 0;
        using var scope = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static formatId => formatId switch
            {
                0 => NamedFormatId,
                NamedFormatId => RegisteredFormatId,
                _ => 0,
            },
            format =>
            {
                registerCalls++;
                return format == RegistrationOnlyFormatName ? RegisteredFormatId : SecondFormatId;
            },
            static formatId => formatId == NamedFormatId ? NativeFormatName : null,
            static () => SuccessError);

        await Assert.That(ClipboardFormatExtensions.MapIdToFormat(NamedFormatId)).IsEqualTo(NativeFormatName);
        await Assert.That(ClipboardFormatExtensions.MapIdToFormat(MissingFormatId)).IsNull();
        await Assert.That(ClipboardFormatExtensions.RegisterFormat(RegistrationOnlyFormatName)).IsEqualTo(RegisteredFormatId);
        await Assert.That(ClipboardFormatExtensions.RegisterFormat(RegistrationOnlyFormatName)).IsEqualTo(RegisteredFormatId);
        await Assert.That(registerCalls).IsEqualTo(1);

        var token = new ClipboardAccessToken();
        var identifiers = ToList(token.AvailableFormatIds());
        var formats = ToList(token.AvailableFormats());
        await Assert.That(identifiers.Count).IsEqualTo(TwoItemCount);
        await Assert.That(identifiers[0]).IsEqualTo(NamedFormatId);
        await Assert.That(formats.Count).IsEqualTo(TwoItemCount);
        await Assert.That(formats[0]).IsEqualTo(NativeFormatName);
        await Assert.That(formats[1]).IsEqualTo(RegistrationOnlyFormatName);

        using var failureScope = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static _ => 0,
            static _ => RegisteredFormatId,
            static _ => null,
            static () => FailureError);
        await Assert.That(() => ToList(token.AvailableFormatIds())).Throws<Win32Exception>();
    }

    /// <summary>Covers clipboard access token state transitions and exception constructors.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardAccessTokenCoversAccessStatesAndExceptionConstructorsAsync()
    {
        var disposeCalls = 0;
        var successful = new ClipboardAccessToken(() => disposeCalls++);
        successful.ThrowWhenNoAccess();
        successful.Dispose();

        await Assert.That(successful.CanAccess).IsFalse();
        await Assert.That(disposeCalls).IsEqualTo(1);
        await Assert.That(() => successful.ThrowWhenNoAccess()).Throws<ClipboardAccessDeniedException>();
        await Assert.That(static () => new ClipboardAccessToken(null)).Throws<ArgumentNullException>();

        var lockTimeout = new ClipboardAccessToken { CanAccess = false, IsLockTimeout = true };
        var openTimeout = new ClipboardAccessToken { CanAccess = false, IsOpenTimeout = true };
        await Assert.That(() => lockTimeout.ThrowWhenNoAccess()).Throws<ClipboardAccessDeniedException>();
        await Assert.That(() => openTimeout.ThrowWhenNoAccess()).Throws<ClipboardAccessDeniedException>();

        var inner = new InvalidOperationException();
        await Assert.That(new ClipboardAccessDeniedException().Message).IsNotNull();
        await Assert.That(new ClipboardAccessDeniedException(RegisteredFormatName).Message).IsEqualTo(RegisteredFormatName);
        await Assert.That(new ClipboardAccessDeniedException(RegisteredFormatName, inner).InnerException).IsEqualTo(inner);
    }

    /// <summary>Covers clipboard semaphore open, retry, timeout, and cancellation branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardSemaphoreCoversOpenRetryTimeoutAndCancellationBranchesAsync()
    {
        var openCalls = 0;
        var closeCalls = 0;
        using var scope = ClipboardSemaphore.OverrideOperationsForTesting(
            _ =>
            {
                openCalls++;
                return openCalls > 1;
            },
            () =>
            {
                closeCalls++;
                return true;
            },
            TryAcquireMessageWindow);
        using var semaphore = new ClipboardSemaphore();

        using (var token = semaphore.Lock(new(OwnerWindowHandle), 1, TimeSpan.Zero, TimeSpan.FromMilliseconds(1)))
        {
            await Assert.That(token.CanAccess).IsTrue();
            await Assert.That(openCalls).IsEqualTo(TwoItemCount);
        }

        await Assert.That(closeCalls).IsEqualTo(1);

        using (var outerToken = semaphore.Lock(new(OwnerWindowHandle), 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(1)))
        {
            using var lockTimeout = semaphore.Lock(new(OwnerWindowHandle), 0, TimeSpan.Zero, TimeSpan.Zero);
            await Assert.That(outerToken.CanAccess).IsTrue();
            await Assert.That(lockTimeout.CanAccess).IsFalse();
            await Assert.That(lockTimeout.IsLockTimeout).IsTrue();
        }

        using var openTimeout = semaphore.Lock(IntPtr.Zero, 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
        await Assert.That(openTimeout.CanAccess).IsFalse();
        await Assert.That(openTimeout.IsOpenTimeout).IsTrue();

        using var cancellation = new CancellationTokenSource();
#if NETFRAMEWORK
        cancellation.Cancel();
#else
        await cancellation.CancelAsync();
#endif
        OperationCanceledException canceledException = null;
        try
        {
            _ = await semaphore.LockAsync(
                new(OwnerWindowHandle),
                0,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(1),
                cancellation.Token);
        }
        catch (OperationCanceledException exception)
        {
            canceledException = exception;
        }

        await Assert.That(canceledException).IsNotNull();
    }

    /// <summary>Covers clipboard semaphore direct overloads and shared message-window polling.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardSemaphoreDefaultOverloadsAndSharedWindowPollingAreCoveredAsync()
    {
        var openCalls = 0;
        var closeCalls = 0;
        using var scope = ClipboardSemaphore.OverrideOperationsForTesting(
            _ =>
            {
                openCalls++;
                return true;
            },
            () =>
            {
                closeCalls++;
                return true;
            },
            static (_, _) => new(OwnerWindowHandle));
        using var semaphore = new ClipboardSemaphore();
        var ownerHandle = new IntPtr(OwnerWindowHandle);

        await AssertCanAccessAsync(semaphore.Lock());
        await AssertCanAccessAsync(semaphore.Lock(ownerHandle));
        await AssertCanAccessAsync(semaphore.Lock(ownerHandle, 0));
        await AssertCanAccessAsync(semaphore.Lock(ownerHandle, 0, TimeSpan.Zero));
        await AssertCanAccessAsync(await semaphore.LockAsync());
        await AssertCanAccessAsync(await semaphore.LockAsync(CancellationToken.None));
        await AssertCanAccessAsync(await semaphore.LockAsync(ownerHandle));
        await AssertCanAccessAsync(await semaphore.LockAsync(ownerHandle, 0));
        await AssertCanAccessAsync(await semaphore.LockAsync(ownerHandle, 0, TimeSpan.Zero));
        await AssertCanAccessAsync(await semaphore.LockAsync(ownerHandle, 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(1)));

        await Assert.That(openCalls).IsEqualTo(closeCalls);

        var keepAliveCalls = 0;
        var handleCalls = 0;
        using (ClipboardSemaphore.OverrideSharedMessageWindowOperationsForTesting(
            () =>
            {
                keepAliveCalls++;
                return new RecordingDisposable();
            },
            () =>
            {
                handleCalls++;
                return handleCalls == 1 ? IntPtr.Zero : new(OwnerWindowHandle);
            }))
        {
            var acquiredHandle = ClipboardSemaphore.TryAcquireSharedMessageWindowForTesting(1, TimeSpan.Zero);
            await Assert.That(acquiredHandle).IsEqualTo(new IntPtr(OwnerWindowHandle));
            await Assert.That(keepAliveCalls).IsEqualTo(1);
            await Assert.That(handleCalls).IsEqualTo(TwoItemCount);
        }

        using (ClipboardSemaphore.OverrideSharedMessageWindowOperationsForTesting(
            static () => new RecordingDisposable(),
            static () => IntPtr.Zero))
        {
            var missingHandle = ClipboardSemaphore.TryAcquireSharedMessageWindowForTesting(0, TimeSpan.Zero);
            await Assert.That(missingHandle).IsNull();
        }
    }

    /// <summary>Covers clipboard native info read and write success and failure branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardInfoOperationsCoverReadAndWriteBranchesAsync()
    {
        var globalHandle = new IntPtr(GlobalHandleValue);
        var memoryPointer = new IntPtr(MemoryPointerValue);
        var allocatedHandle = globalHandle;
        var lockedPointer = memoryPointer;
        using var scope = ClipboardInfoExtensions.OverrideOperationsForTesting(
            formatId => formatId == RegisteredFormatId ? globalHandle : IntPtr.Zero,
            static formatId => formatId == RegisteredFormatId,
            (_, _) => allocatedHandle,
            _ => lockedPointer);
        var token = new ClipboardAccessToken();

        var readSucceeded = token.TryReadInfo(RegisteredFormatId, out var readInfo);
        await Assert.That(readSucceeded).IsTrue();
        await Assert.That(readInfo.GlobalHandle).IsEqualTo(globalHandle);
        await Assert.That(readInfo.MemoryPtr).IsEqualTo(memoryPointer);
        readInfo.GlobalHandle = IntPtr.Zero;
        readInfo.Dispose();

        await Assert.That(token.TryReadInfo(MissingFormatId, out _)).IsFalse();
        await Assert.That(() => token.ReadInfo(MissingFormatId)).Throws<Win32Exception>();

        lockedPointer = IntPtr.Zero;
        await Assert.That(token.TryReadInfo(RegisteredFormatId, out _)).IsFalse();
        await Assert.That(() => token.ReadInfo(RegisteredFormatId)).Throws<Win32Exception>();

        allocatedHandle = IntPtr.Zero;
        await Assert.That(() => token.WriteInfo(RegisteredFormatId, ExplicitNonSeekableSize)).Throws<Win32Exception>();

        allocatedHandle = globalHandle;
        await Assert.That(() => token.WriteInfo(RegisteredFormatId, ExplicitNonSeekableSize)).Throws<Win32Exception>();

        lockedPointer = memoryPointer;
        var writeInfo = token.WriteInfo(RegisteredFormatId, ExplicitNonSeekableSize);
        await Assert.That(writeInfo.NeedsWrite).IsTrue();
        await Assert.That(writeInfo.FormatId).IsEqualTo(RegisteredFormatId);
        writeInfo.NeedsWrite = false;
        writeInfo.GlobalHandle = IntPtr.Zero;
        writeInfo.Dispose();

        var denied = new ClipboardAccessToken { CanAccess = false };
        await Assert.That(denied.TryReadInfo(RegisteredFormatId, out _)).IsFalse();
        await Assert.That(() => denied.ReadInfo(RegisteredFormatId)).Throws<ClipboardAccessDeniedException>();
        await Assert.That(() => denied.WriteInfo(RegisteredFormatId, ExplicitNonSeekableSize)).Throws<ClipboardAccessDeniedException>();

        using var unavailableScope = ClipboardInfoExtensions.OverrideOperationsForTesting(
            static _ => IntPtr.Zero,
            static _ => true,
            static (_, _) => new(GlobalHandleValue),
            static _ => new(MemoryPointerValue));
        await Assert.That(() => token.ReadInfo(MissingFormatId)).Throws<Win32Exception>();
    }

    /// <summary>Covers native clipboard set-data failure handling.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeClipboardSetDataFailureBranchesAreCoveredAsync()
    {
        using var scope = NativeMethods.OverrideSetClipboardDataForTesting(static (_, _) => IntPtr.Zero);

        await Assert.That(static () => NativeMethods.SetClipboardDataWithErrorHandling(RegisteredFormatId, new(GlobalHandleValue))).Throws<Win32Exception>();
        NativeMethods.SetClipboardDataWithErrorHandling(RegisteredFormatId, IntPtr.Zero);
    }

    /// <summary>Covers byte, string, cloud, delayed-rendering, and file wrapper overloads.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardWriteWrapperOverloadsCoverRemainingBranchesAsync()
    {
        var memory = Marshal.AllocHGlobal(NativeMemoryBufferSize);
        var setDataCalls = 0;
        var lastSetFormat = 0U;
        var lastSetMemory = IntPtr.Zero;
        try
        {
            using var formatScope = ClipboardFormatExtensions.OverrideOperationsForTesting(
                static _ => 0,
                static format => format == WrapperFormatName ? RegisteredFormatId : CloudFormatId,
                static _ => null,
                static () => SuccessError);
            using var infoScope = ClipboardInfoExtensions.OverrideOperationsForTesting(
                static _ => new(GlobalHandleValue),
                static _ => true,
                static (_, _) => new(GlobalHandleValue),
                _ => memory);
            using var setDataScope = NativeMethods.OverrideSetClipboardDataForTesting((format, clipboardMemory) =>
            {
                setDataCalls++;
                lastSetFormat = format;
                lastSetMemory = clipboardMemory;
                return clipboardMemory;
            });
            using var token = new ClipboardAccessToken();

            token.SetAsBytes([FirstByte, SecondByte], StandardClipboardFormats.UnicodeText);
            await Assert.That(Marshal.ReadByte(memory)).IsEqualTo(FirstByte);

            token.SetAsBytes([ThirdByte, FourthByte], WrapperFormatName);
            await Assert.That(Marshal.ReadByte(memory)).IsEqualTo(ThirdByte);

            token.SetAsUnicodeString(ClipboardText, StandardClipboardFormats.UnicodeText);
            await Assert.That(Marshal.PtrToStringUni(memory)).IsEqualTo(ClipboardText);

            token.SetAsUnicodeString(ClipboardText, WrapperFormatName);
            await Assert.That(Marshal.PtrToStringUni(memory)).IsEqualTo(ClipboardText);

            token.SetCanIncludeInClipboardHistory();
            await Assert.That(Marshal.ReadInt32(memory)).IsEqualTo(1);

            token.SetCloudClipboardOptions(canIncludeInHistory: false, canUploadToCloud: false, excludeFromMonitoring: true);
            await Assert.That(Marshal.ReadInt32(memory)).IsEqualTo(1);

            token.SetDelayedRenderedContent(StandardClipboardFormats.UnicodeText);
            await Assert.That(lastSetMemory).IsEqualTo(IntPtr.Zero);

            token.SetDelayedRenderedContent(WrapperFormatName);
            await Assert.That(lastSetFormat).IsEqualTo(RegisteredFormatId);
            await Assert.That(lastSetMemory).IsEqualTo(IntPtr.Zero);

            var fileNames = ToList(token.GetFileNames());
            await Assert.That(fileNames.Count).IsEqualTo(0);

            token.SetFileNames([ClipboardFileName]);
            await Assert.That(Marshal.ReadInt32(memory)).IsEqualTo(DropFilesHeaderSize);
            await Assert.That(setDataCalls).IsGreaterThan(0);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    /// <summary>Covers stream write validation, seekable and non-seekable copy paths, and read stream creation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardStreamOperationsCoverReadWriteAndValidationBranchesAsync()
    {
        var capturedFormat = 0U;
        var capturedLength = 0L;
        var capturedBytes = Array.Empty<byte>();
        using var formatScope = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static _ => 0,
            static _ => RegisteredFormatId,
            static _ => null,
            static () => SuccessError);
        using var streamScope = ClipboardStreamExtensions.OverrideOperationsForTesting(
            (_, formatId, stream, length) =>
            {
                capturedFormat = formatId;
                capturedLength = length;
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);
                capturedBytes = buffer.ToArray();
            },
            static _ => ExplicitNonSeekableSize);

        var token = new ClipboardAccessToken();
#if NETFRAMEWORK
        using var seekable = new MemoryStream([1, SeekedFirstByte, SeekedSecondByte, SeekedThirdByte]);
#else
        await using var seekable = new MemoryStream([1, SeekedFirstByte, SeekedSecondByte, SeekedThirdByte]);
#endif
        seekable.Position = 1;
        token.SetAsStream(RegisteredFormatId, seekable);
        await Assert.That(capturedFormat).IsEqualTo(RegisteredFormatId);
        await Assert.That(capturedLength).IsEqualTo(SeekedLength);
        await Assert.That(capturedBytes[0]).IsEqualTo(SeekedFirstByte);

        seekable.Position = 0;
        token.SetAsStream(StandardClipboardFormats.UnicodeText, seekable, ExplicitNonSeekableSize);
        await Assert.That(capturedLength).IsEqualTo(ExplicitNonSeekableSize);

#if NETFRAMEWORK
        using var nonSeekableWithSize = new NonSeekableReadStream([FirstByte, SecondByte, ThirdByte, FourthByte]);
#else
        await using var nonSeekableWithSize = new NonSeekableReadStream([FirstByte, SecondByte, ThirdByte, FourthByte]);
#endif
        token.SetAsStream(StreamFormatName, nonSeekableWithSize, ExplicitNonSeekableSize);
        await Assert.That(capturedFormat).IsEqualTo(RegisteredFormatId);
        await Assert.That(capturedLength).IsEqualTo(ExplicitNonSeekableSize);
        await Assert.That(capturedBytes[3]).IsEqualTo(FourthByte);

#if NETFRAMEWORK
        using var nonSeekableWithoutSize = new NonSeekableReadStream([FirstByte, SecondByte]);
#else
        await using var nonSeekableWithoutSize = new NonSeekableReadStream([FirstByte, SecondByte]);
#endif
        token.SetAsStream(StandardClipboardFormats.UnicodeText, nonSeekableWithoutSize);
        await Assert.That(capturedLength).IsEqualTo(BufferedNonSeekableLength);
        await Assert.That(capturedBytes[1]).IsEqualTo(SecondByte);

#if NETFRAMEWORK
        using var empty = new MemoryStream();
        using var nonReadable = new NonReadableStream();
#else
        await using var empty = new MemoryStream();
        await using var nonReadable = new NonReadableStream();
#endif
        await Assert.That(() => token.SetAsStream(RegisteredFormatId, empty)).Throws<NotSupportedException>();
        await Assert.That(() => token.SetAsStream(RegisteredFormatId, nonReadable)).Throws<NotSupportedException>();

        var denied = new ClipboardAccessToken { CanAccess = false };
        await Assert.That(() => denied.SetAsStream(RegisteredFormatId, new MemoryStream([FirstByte]))).Throws<ClipboardAccessDeniedException>();
        await Assert.That(denied.TryGetAsStream(RegisteredFormatId, out var deniedStream)).IsFalse();
        await Assert.That(deniedStream).IsNull();
    }

    /// <summary>Covers stream read success using deterministic clipboard native information.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardStreamReadCopiesNativeMemoryIntoManagedStreamsAsync()
    {
        var bytes = new[] { FirstByte, SecondByte, ThirdByte, FourthByte };
        var memory = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, memory, bytes.Length);
            using var infoScope = ClipboardInfoExtensions.OverrideOperationsForTesting(
                static _ => new(GlobalHandleValue),
                static _ => true,
                static (_, _) => new(GlobalHandleValue),
                _ => memory);
            using var streamScope = ClipboardStreamExtensions.OverrideOperationsForTesting(
                static (_, _, _, _) => { },
                _ => bytes.Length);
            var token = new ClipboardAccessToken();

            var trySucceeded = token.TryGetAsStream(RegisteredFormatId, out var tryStream);
#if NETFRAMEWORK
            using var readStream = token.GetAsStream(RegisteredFormatId);
            using var namedReadStream = token.GetAsStream(RegisteredFormatName);
#else
            await using var readStream = token.GetAsStream(RegisteredFormatId);
            await using var namedReadStream = token.GetAsStream(RegisteredFormatName);
#endif
            await Assert.That(trySucceeded).IsTrue();
            await Assert.That(ReadFirstByte(tryStream)).IsEqualTo(FirstByte);
            await Assert.That(ReadFirstByte(readStream)).IsEqualTo(FirstByte);
            await Assert.That(ReadFirstByte(namedReadStream)).IsEqualTo(FirstByte);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    /// <summary>Covers clipboard update information creation and format projection.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardUpdateInformationCreateCoversZeroAndExplicitWindowHandlesAsync()
    {
        using var nativeScope = ClipboardNative.OverrideOperationsForTesting(
            static () => new(OwnerWindowHandle),
            static () => SequenceNumber,
            static _ => true);
        using var semaphoreScope = ClipboardSemaphore.OverrideOperationsForTesting(
            static _ => true,
            static () => true,
            static (_, _) => new(OwnerWindowHandle));
        using var formatScope = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static formatId => formatId == 0 ? (uint)StandardClipboardFormats.UnicodeText : 0,
            static _ => RegisteredFormatId,
            static _ => null,
            static () => SuccessError);

        var zeroWindowInformation = ClipboardUpdateInformation.Create(IntPtr.Zero);
        var explicitWindowInformation = ClipboardUpdateInformation.Create(new(OwnerWindowHandle));
        var zeroWindowFormatIds = ToList(zeroWindowInformation.FormatIds);
        var explicitWindowFormats = ToList(explicitWindowInformation.Formats);

        await Assert.That(zeroWindowInformation.Id).IsEqualTo(SequenceNumber);
        await Assert.That(zeroWindowInformation.HasOwner).IsTrue();
        await Assert.That(zeroWindowFormatIds.Count).IsEqualTo(1);
        await Assert.That(explicitWindowFormats.Count).IsEqualTo(1);
    }

    /// <summary>Covers unmanaged stream wrapper disposal of its attached disposable.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task UnmanagedMemoryStreamWrapperDisposesAttachedDisposableAsync()
    {
        var memory = Marshal.AllocHGlobal(WrapperLength);
        try
        {
            Marshal.WriteByte(memory, WrapperByte);
            var disposable = new RecordingDisposable();
#if NETFRAMEWORK
            using (var wrapper = new UnmanagedMemoryStreamWrapper(memory, WrapperLength, WrapperLength, FileAccess.Read))
#else
            await using (var wrapper = new UnmanagedMemoryStreamWrapper(memory, WrapperLength, WrapperLength, FileAccess.Read))
#endif
            {
                wrapper.SetDisposable(disposable);
                await Assert.That(wrapper.ReadByte()).IsEqualTo(WrapperByte);
            }

            await Assert.That(disposable.DisposeCalls).IsEqualTo(1);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    /// <summary>Asserts that a clipboard access token can access the clipboard, then disposes it.</summary>
    /// <param name="token">The clipboard access token.</param>
    /// <returns>A task representing the asynchronous assertion.</returns>
    private static async Task AssertCanAccessAsync(IClipboardAccessToken token)
    {
        using (token)
        {
            await Assert.That(token.CanAccess).IsTrue();
        }
    }

    /// <summary>Invokes a synchronous <see cref="ClipboardNative.Access()"/> overload for coverage without analyzer-blocking direct calls.</summary>
    /// <param name="methodName">The method name.</param>
    /// <param name="parameterTypes">The overload parameter types.</param>
    /// <param name="arguments">The invocation arguments.</param>
    /// <returns>The clipboard access token returned by the invoked overload.</returns>
    private static IClipboardAccessToken InvokeClipboardNativeAccess(string methodName, Type[] parameterTypes, params object[] arguments)
    {
        var method = typeof(ClipboardNative).GetMethod(methodName, parameterTypes) ?? throw new MissingMethodException(nameof(ClipboardNative), methodName);
        return method.Invoke(null, arguments) as IClipboardAccessToken ?? throw new InvalidOperationException(methodName);
    }

    /// <summary>Materializes unsigned integer values as a list.</summary>
    /// <param name="values">The values to materialize.</param>
    /// <returns>The materialized values.</returns>
    private static List<uint> ToList(IEnumerable<uint> values)
    {
        var result = new List<uint>();
        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }

    /// <summary>Materializes string values as a list.</summary>
    /// <param name="values">The values to materialize.</param>
    /// <returns>The materialized values.</returns>
    private static List<string> ToList(IEnumerable<string> values)
    {
        var result = new List<string>();
        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }

    /// <summary>Reads the first byte from a stream.</summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The first byte.</returns>
    private static int ReadFirstByte(Stream stream)
    {
        using (stream)
        {
            return stream.ReadByte();
        }
    }

    /// <summary>Acquires the fake message-window handle.</summary>
    /// <param name="retries">The number of retries requested.</param>
    /// <param name="retryInterval">The retry interval requested.</param>
    /// <returns><see langword="null"/> to force the open-timeout branch.</returns>
    private static IntPtr? TryAcquireMessageWindow(int retries, TimeSpan retryInterval)
    {
        _ = retries;
        _ = retryInterval;
        return null;
    }

    /// <summary>A stream that can be read but cannot seek.</summary>
    /// <param name="bytes">The stream bytes.</param>
    private sealed class NonSeekableReadStream(byte[] bytes) : Stream
    {
        /// <summary>The inner stream.</summary>
        private readonly MemoryStream _inner = new(bytes);

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Flush()
        {
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>A stream that cannot be read.</summary>
    private sealed class NonReadableStream : Stream
    {
        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Flush()
        {
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>Records disposal calls.</summary>
    private sealed class RecordingDisposable : IDisposable
    {
        /// <summary>Gets the number of dispose calls.</summary>
        public int DisposeCalls { get; private set; }

        /// <inheritdoc />
        public void Dispose() => DisposeCalls++;
    }
}
