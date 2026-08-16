// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa366574(v=vs.85).aspx">GlobalAlloc function</a>.</summary>
[Flags]
public enum GlobalMemorySettings : uint
{
    /// <summary>Allocates fixed memory. The return value is a pointer.</summary>
    None = 0U,

    /// <summary>
    /// Allocates movable memory. Memory blocks are never moved in physical memory, but they can be moved within the default heap.
    /// The return value is a handle to the memory object. To translate the handle into a pointer, use the GlobalLock function.
    /// This value cannot be combined with Fixed.
    /// </summary>
    Movable = 2U,

    /// <summary>Initializes memory contents to zero.</summary>
    ZeroInit = 0x40U,
}
