// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>Provides internal clipboard native information helpers.</summary>
internal static class ClipboardInfoExtensions
{
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Tries to create clipboard native information for reading.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <param name="readInfo">The clipboard native information.</param>
        /// <returns><see langword="true" /> if the format can be read.</returns>
        internal bool TryReadInfo(uint formatId, out ClipboardNativeInfo readInfo)
        {
            readInfo = null;
            if (!clipboardAccessToken.CanAccess)
            {
                return false;
            }

            IntPtr globalHandle = _operations.GetClipboardData(formatId);
            if (globalHandle == IntPtr.Zero)
            {
                return false;
            }

            IntPtr memoryPtr = _operations.GlobalLock(globalHandle);
            if (memoryPtr == IntPtr.Zero)
            {
                return false;
            }

            readInfo = new ClipboardNativeInfo { GlobalHandle = globalHandle, MemoryPtr = memoryPtr, FormatId = formatId };
            return true;
        }

        /// <summary>Creates clipboard native information for reading.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <returns>The clipboard native information.</returns>
        internal ClipboardNativeInfo ReadInfo(uint formatId)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            IntPtr globalHandle = _operations.GetClipboardData(formatId);
            if (globalHandle == IntPtr.Zero)
            {
                if (_operations.IsFormatAvailable(formatId))
                {
                    throw new Win32Exception($"Format {formatId} not available.");
                }

                throw new Win32Exception();
            }

            IntPtr memoryPtr = _operations.GlobalLock(globalHandle);
            if (memoryPtr == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return new ClipboardNativeInfo { GlobalHandle = globalHandle, MemoryPtr = memoryPtr, FormatId = formatId };
        }

        /// <summary>Creates clipboard native information for writing.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <param name="size">The clipboard area size.</param>
        /// <returns>The clipboard native information.</returns>
        internal ClipboardNativeInfo WriteInfo(uint formatId, long size)
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            IntPtr globalHandle = _operations.GlobalAlloc(GlobalMemorySettings.Movable | GlobalMemorySettings.ZeroInit, new(checked((ulong)size)));
            if (globalHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            IntPtr memoryPtr = _operations.GlobalLock(globalHandle);
            if (memoryPtr == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return new ClipboardNativeInfo { GlobalHandle = globalHandle, MemoryPtr = memoryPtr, NeedsWrite = true, FormatId = formatId };
        }
    }

    /// <summary>Composes clipboard native memory operations without invoking them during construction.</summary>
    /// <param name="getClipboardData">The clipboard-data query operation.</param>
    /// <param name="isFormatAvailable">The format availability query operation.</param>
    /// <param name="globalAlloc">The global allocation operation.</param>
    /// <param name="globalLock">The global lock operation.</param>
    private sealed class ClipboardInfoOperations(Func<uint, IntPtr> getClipboardData, Func<uint, bool> isFormatAvailable, Func<GlobalMemorySettings, UIntPtr, IntPtr> globalAlloc, Func<IntPtr, IntPtr> globalLock)
    {
        /// <summary>Allocates global memory.</summary>
        /// <param name="settings">The global memory settings.</param>
        /// <param name="size">The allocation size.</param>
        /// <returns>The allocated memory handle.</returns>
        public IntPtr GlobalAlloc(GlobalMemorySettings settings, UIntPtr size) => globalAlloc(settings, size);

        /// <summary>Locks global memory.</summary>
        /// <param name="globalHandle">The global memory handle.</param>
        /// <returns>The locked memory pointer.</returns>
        public IntPtr GlobalLock(IntPtr globalHandle) => globalLock(globalHandle);

        /// <summary>Gets clipboard data for a format.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <returns>The clipboard data handle.</returns>
        public IntPtr GetClipboardData(uint formatId) => getClipboardData(formatId);

        /// <summary>Determines whether a format is available.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <returns><see langword="true" /> when the format is available.</returns>
        public bool IsFormatAvailable(uint formatId) => isFormatAvailable(formatId);
    }

    /// <summary>Native clipboard memory operations used by this type.</summary>
    private static ClipboardInfoOperations _operations = new(NativeMethods.GetClipboardData, NativeMethods.IsClipboardFormatAvailable, Kernel32Api.GlobalAlloc, Kernel32Api.GlobalLock);

    /// <summary>Overrides native clipboard information operations for deterministic tests.</summary>
    /// <param name="getClipboardData">The replacement clipboard-data query operation.</param>
    /// <param name="isFormatAvailable">The replacement format availability query operation.</param>
    /// <param name="globalAlloc">The replacement global allocation operation.</param>
    /// <param name="globalLock">The replacement global lock operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<uint, IntPtr> getClipboardData, Func<uint, bool> isFormatAvailable, Func<GlobalMemorySettings, UIntPtr, IntPtr> globalAlloc, Func<IntPtr, IntPtr> globalLock)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(getClipboardData);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(isFormatAvailable);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(globalAlloc);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(globalLock);
        ClipboardInfoOperations operations = _operations;
        _operations = new(getClipboardData, isFormatAvailable, globalAlloc, globalLock);
        return Scope.Create(operations, delegate(ClipboardInfoOperations previous)
        {
            _operations = previous;
        });
    }
}
