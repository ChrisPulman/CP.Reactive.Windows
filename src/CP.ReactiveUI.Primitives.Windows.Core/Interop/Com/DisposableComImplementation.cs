// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>Implementation of IDisposableCom for internal COM lifetime management.</summary>
/// <typeparam name="T">Type of the COM object.</typeparam>
/// <param name="obj">The COM object to release.</param>
internal sealed class DisposableComImplementation<T>(T obj) : IDisposableCom<T>
{
    /// <inheritdoc />
    public T ComObject { get; private set; } = obj;

    /// <summary>Cleans up the COM object.</summary>
    public void Dispose() => Dispose(disposing: true);

    /// <summary>Releases the COM reference.</summary>
    /// <param name="disposing"><see langword="true" /> if this was called from the<see cref="T:System.IDisposable" /> interface.</param>
    internal void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!EqualityComparer<T>.Default.Equals(ComObject, default(T)) && Marshal.IsComObject(ComObject))
            {
                _ = Marshal.ReleaseComObject(ComObject);
            }

            ComObject = default;
        }
    }
}
