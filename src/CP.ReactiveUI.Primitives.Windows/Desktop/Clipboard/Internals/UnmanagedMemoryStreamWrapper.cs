// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>This wraps an UnmanagedMemoryStream, to also take care or disposing some disposable.</summary>
internal sealed class UnmanagedMemoryStreamWrapper : UnmanagedMemoryStream
{
    /// <summary>The disposable that should be released with this stream.</summary>
    private IDisposable _disposable;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals.UnmanagedMemoryStreamWrapper" /> class.</summary>
    /// <param name="bytes">The unmanaged memory pointer.</param>
    /// <param name="length">The stream length.</param>
    /// <param name="capacity">The stream capacity.</param>
    /// <param name="fileAccess">The stream file access.</param>
    public unsafe UnmanagedMemoryStreamWrapper(byte* bytes, long length, long capacity, FileAccess fileAccess)
        : base(bytes, length, capacity, fileAccess)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals.UnmanagedMemoryStreamWrapper" /> class.</summary>
    /// <param name="bytes">The unmanaged memory pointer.</param>
    /// <param name="length">The stream length.</param>
    /// <param name="capacity">The stream capacity.</param>
    /// <param name="fileAccess">The stream file access.</param>
    internal unsafe UnmanagedMemoryStreamWrapper(IntPtr bytes, long length, long capacity, FileAccess fileAccess)
        : this((byte*)(void*)bytes, length, capacity, fileAccess)
    {
    }

    /// <summary>Sets the disposable that should be released with this stream.</summary>
    /// <param name="disposable">The disposable instance.</param>
    internal void SetDisposable(IDisposable disposable) => _disposable = disposable;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _disposable?.Dispose();
    }
}
