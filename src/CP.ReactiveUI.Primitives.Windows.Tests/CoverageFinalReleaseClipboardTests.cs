// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for clipboard operation composition.</summary>
public sealed class CoverageFinalReleaseClipboardTests
{
    /// <summary>A deterministic native owner-window handle.</summary>
    private const int OwnerWindowHandle = 321;

    /// <summary>A deterministic registered clipboard format identifier.</summary>
    private const uint RegisteredFormatId = 0xC201;

    /// <summary>A failing native last-error value.</summary>
    private const int FailureError = 5;

    /// <summary>Covers default clipboard operation composition without invoking Windows APIs.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DefaultClipboardOperationCompositionIsDeterministicAsync()
    {
        using var nativeOperations = ClipboardSemaphore.UseDefaultOperationsForTesting();
        using var sharedWindowOperations = ClipboardSemaphore.UseDefaultSharedMessageWindowOperationsForTesting();

        await Assert.That(nativeOperations).IsNotNull();
        await Assert.That(sharedWindowOperations).IsNotNull();
    }

    /// <summary>Covers clipboard enumeration failure delegates for all operation slots.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardFormatFailureOperationsAreInvokedDeterministicallyAsync()
    {
        using var operations = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static _ => 0,
            static _ => RegisteredFormatId,
            static _ => null,
            static () => FailureError);
        using var token = new ClipboardAccessToken();

        await Assert.That(() => Materialize(token.AvailableFormatIds())).Throws<Win32Exception>();
        await Assert.That(ClipboardFormatExtensions.MapIdToFormat(RegisteredFormatId)).IsNull();
    }

    /// <summary>Covers the default and zero-handle clipboard update factories without opening the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardUpdateFactoriesUseTheInjectedSharedWindowHandleAsync()
    {
        var requestedHandle = IntPtr.Zero;
        using var updateScope = ClipboardUpdateInformation.OverrideSharedMessageWindowHandleForTesting(
            static () => new(OwnerWindowHandle));
        using var nativeScope = ClipboardNative.OverrideOperationsForTesting(
            static () => IntPtr.Zero,
            static () => 0U,
            static _ => false);
        using var semaphoreScope = ClipboardSemaphore.OverrideOperationsForTesting(
            handle =>
            {
                requestedHandle = handle;
                return true;
            },
            static () => true,
            static (_, _) => new(OwnerWindowHandle));
        using var formatScope = ClipboardFormatExtensions.OverrideOperationsForTesting(
            static _ => 0,
            static _ => RegisteredFormatId,
            static _ => null,
            static () => 0);

        _ = ClipboardUpdateInformation.Create();
        await Assert.That(requestedHandle).IsEqualTo(new(OwnerWindowHandle));

        requestedHandle = IntPtr.Zero;
        _ = ClipboardUpdateInformation.Create(IntPtr.Zero);
        await Assert.That(requestedHandle).IsEqualTo(new(OwnerWindowHandle));
    }

    /// <summary>Covers unsuccessful native clipboard open handling after a valid owner handle was supplied.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardSemaphoreReportsAnOpenTimeoutWhenOpeningFailsAsync()
    {
        using var operations = ClipboardSemaphore.OverrideOperationsForTesting(
            static _ => false,
            static () => true,
            static (_, _) => new(OwnerWindowHandle));
        using var semaphore = new ClipboardSemaphore();
        using var token = semaphore.Lock(new(OwnerWindowHandle), 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

        await Assert.That(token.CanAccess).IsFalse();
        await Assert.That(token.IsOpenTimeout).IsTrue();
    }

    /// <summary>Covers the four-operation clipboard information override overload.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ClipboardInformationFourOperationOverrideCanReadMissingDataAsync()
    {
        using var operations = ClipboardInfoExtensions.OverrideOperationsForTesting(
            static _ => IntPtr.Zero,
            static _ => false,
            static (_, _) => IntPtr.Zero,
            static _ => IntPtr.Zero);
        using var token = new ClipboardAccessToken();

        await Assert.That(token.TryReadInfo(RegisteredFormatId, out var information)).IsFalse();
        await Assert.That(information).IsNull();
    }

    /// <summary>Covers disposal of an unmanaged stream wrapper with no attached disposable.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task UnmanagedMemoryStreamWrapperAllowsDisposalWithoutAttachedDisposableAsync()
    {
        var memory = Marshal.AllocHGlobal(1);
        try
        {
#if NETFRAMEWORK
            using var wrapper = new UnmanagedMemoryStreamWrapper(memory, 1, 1, FileAccess.Read);
#else
            await using var wrapper = new UnmanagedMemoryStreamWrapper(memory, 1, 1, FileAccess.Read);
#endif
            await Assert.That(wrapper.CanRead).IsTrue();
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    /// <summary>Materializes clipboard format identifiers without LINQ allocations.</summary>
    /// <param name="values">The values to materialize.</param>
    /// <returns>The materialized values.</returns>
    private static List<uint> Materialize(IEnumerable<uint> values)
    {
        var result = new List<uint>();
        foreach (uint value in values)
        {
            result.Add(value);
        }

        return result;
    }
}
