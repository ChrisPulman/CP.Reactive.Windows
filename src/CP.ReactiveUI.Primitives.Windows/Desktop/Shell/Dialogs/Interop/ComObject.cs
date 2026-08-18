// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Base wrapper for an owned COM interface pointer.</summary>
internal class ComObject : IDisposable
{
    /// <summary>Initializes a new instance of the <see cref="T:ComObject" /> class.</summary>
    /// <param name="handle">The owned COM interface pointer.</param>
    protected ComObject(IntPtr handle)
    {
        Handle = handle;
    }

    /// <summary>Gets the owned COM interface pointer.</summary>
    internal IntPtr Handle { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Throws when an HRESULT indicates failure.</summary>
    /// <param name="resultCode">The HRESULT value.</param>
    protected static void ThrowIfFailed(int resultCode)
    {
        if (resultCode < 0)
        {
            Marshal.ThrowExceptionForHR(resultCode);
        }
    }

    /// <summary>Releases managed and unmanaged resources.</summary>
    /// <param name="disposing">A value indicating whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Handle != IntPtr.Zero)
        {
            _ = Marshal.Release(Handle);
            Handle = IntPtr.Zero;
        }
    }

    /// <summary>Gets a vtable slot as a native function pointer address.</summary>
    /// <param name="slot">The vtable slot index.</param>
    /// <returns>The native function pointer address.</returns>
    protected virtual unsafe IntPtr GetMethod(int slot)
    {
        var slotOffset = checked((nint)slot * (nint)sizeof(IntPtr));
        return *(IntPtr*)((*(IntPtr*)(void*)Handle) + slotOffset);
    }
}
