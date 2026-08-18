// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs.Interop;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop;
#endif
/// <summary>Owns unmanaged COMDLG_FILTERSPEC storage for a file dialog call.</summary>
internal sealed class NativeFilterSpecs : IDisposable
{
    /// <summary>The number of native pointers in one COMDLG_FILTERSPEC.</summary>
    private const int PointersPerFilter = 2;

    /// <summary>The owned string pointers.</summary>
    private readonly SafeHGlobalHandle[] _strings;

    /// <summary>The native filter-spec pointer.</summary>
    private readonly SafeHGlobalHandle _handle;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.NativeFilterSpecs" /> class.</summary>
    /// <param name="filters">The managed filters.</param>
    internal NativeFilterSpecs(FilterSpec[] filters)
    {
        Count = filters.Length;
        checked
        {
            _strings = new SafeHGlobalHandle[filters.Length * PointersPerFilter];
            _handle = new(IntPtr.Size * _strings.Length);
            for (var index = 0; index < filters.Length; index++)
            {
                var nameIndex = index * PointersPerFilter;
                var specIndex = nameIndex + 1;
                _strings[nameIndex] = SafeHGlobalHandle.FromString(filters[index].Name);
                _strings[specIndex] = SafeHGlobalHandle.FromString(filters[index].Spec);
                _handle.WritePointer(nameIndex * IntPtr.Size, _strings[nameIndex]);
                _handle.WritePointer(specIndex * IntPtr.Size, _strings[specIndex]);
            }
        }
    }

    /// <summary>Gets the number of filters.</summary>
    internal int Count { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        var strings = _strings;
        for (var i = 0; i < strings.Length; i++)
        {
            strings[i].Dispose();
        }

        _handle.Dispose();
    }

    /// <summary>Calls IFileDialog.SetFileTypes with the owned native buffer.</summary>
    /// <param name="method">The native SetFileTypes method.</param>
    /// <param name="dialogHandle">The dialog interface pointer.</param>
    /// <returns>The native HRESULT.</returns>
    internal unsafe int SetFileTypes(
        delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr, int> method,
        IntPtr dialogHandle) =>
        SetFileTypes((count, buffer) => method(dialogHandle, count, buffer));

    /// <summary>Calls a managed SetFileTypes shim with the owned native buffer.</summary>
    /// <param name="method">The deterministic method to invoke.</param>
    /// <returns>The method result.</returns>
    internal int SetFileTypes(Func<uint, IntPtr, int> method) =>
        _handle.UseHandle(buffer => method(checked((uint)Count), buffer));

    /// <summary>Owns memory allocated by <see cref="M:System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Int32)" />.</summary>
    private sealed class SafeHGlobalHandle : SafeHandle
    {
        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.NativeFilterSpecs.SafeHGlobalHandle" /> class.</summary>
        /// <param name="bytes">The number of bytes to allocate.</param>
        internal SafeHGlobalHandle(int bytes)
            : base(IntPtr.Zero, ownsHandle: true)
        {
            SetHandle(Marshal.AllocHGlobal(bytes));
        }

        /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs.Interop.NativeFilterSpecs.SafeHGlobalHandle" /> class.</summary>
        /// <param name="nativeHandle">The owned native memory handle.</param>
        private SafeHGlobalHandle(IntPtr nativeHandle)
            : base(IntPtr.Zero, ownsHandle: true)
        {
            SetHandle(nativeHandle);
        }

        /// <inheritdoc />
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>Creates an owned handle for a UTF-16 string buffer.</summary>
        /// <param name="value">The managed string value.</param>
        /// <returns>The owned native string handle.</returns>
        internal static SafeHGlobalHandle FromString(string value) => new(Marshal.StringToHGlobalUni(value));

        /// <summary>Invokes a function while holding a reference count on the native handle.</summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="action">The function to invoke.</param>
        /// <returns>The function result.</returns>
        internal T UseHandle<T>(Func<IntPtr, T> action)
        {
            var addedReference = false;
            try
            {
                DangerousAddRef(ref addedReference);
                return action(handle);
            }
            finally
            {
                if (addedReference)
                {
                    DangerousRelease();
                }
            }
        }

        /// <summary>Writes a native pointer while both handles are reference-counted.</summary>
        /// <param name="offset">The byte offset to write to.</param>
        /// <param name="value">The native pointer owner to write.</param>
        internal void WritePointer(int offset, SafeHGlobalHandle value)
        {
            var thisReference = false;
            var valueReference = false;
            try
            {
                DangerousAddRef(ref thisReference);
                value.DangerousAddRef(ref valueReference);
                Marshal.WriteIntPtr(handle, offset, value.handle);
            }
            finally
            {
                if (valueReference)
                {
                    value.DangerousRelease();
                }

                if (thisReference)
                {
                    DangerousRelease();
                }
            }
        }

        /// <inheritdoc />
        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }
}
