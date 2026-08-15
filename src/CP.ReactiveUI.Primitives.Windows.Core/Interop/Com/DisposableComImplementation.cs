// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>Implementation of IDisposableCom for internal COM lifetime management.</summary>
/// <typeparam name="T">Type of the COM object.</typeparam>
internal sealed class DisposableComImplementation<T> : IDisposableCom<T>
{
    /// <summary>Determines whether the wrapped value is a COM object.</summary>
    private readonly Func<T, bool> _isComObject;

    /// <summary>Releases the wrapped COM object.</summary>
    private readonly Func<T, int> _releaseComObject;

    /// <summary>Initializes a new instance of the <see cref="DisposableComImplementation{T}" /> class.</summary>
    /// <param name="obj">The object to wrap.</param>
    internal DisposableComImplementation(T obj)
        : this(
            obj,
            static value => Marshal.IsComObject(value),
            static value => Marshal.ReleaseComObject(value))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DisposableComImplementation{T}" /> class.</summary>
    /// <param name="obj">The object to wrap.</param>
    /// <param name="isComObject">Determines whether the wrapped value is a COM object.</param>
    /// <param name="releaseComObject">Releases the wrapped COM object.</param>
    internal DisposableComImplementation(
        T obj,
        Func<T, bool> isComObject,
        Func<T, int> releaseComObject)
    {
        Throw.IfNull(isComObject);
        Throw.IfNull(releaseComObject);
        ComObject = obj;
        _isComObject = isComObject;
        _releaseComObject = releaseComObject;
    }

    /// <inheritdoc />
    public T ComObject { get; private set; }

    /// <summary>Cleans up the COM object.</summary>
    public void Dispose() => Dispose(disposing: true);

    /// <summary>Releases the COM reference.</summary>
    /// <param name="disposing"><see langword="true" /> if this was called from the<see cref="T:System.IDisposable" /> interface.</param>
    internal void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (
                !EqualityComparer<T>.Default.Equals(ComObject, default(T))
                && _isComObject(ComObject))
            {
                _ = _releaseComObject(ComObject);
            }

            ComObject = default;
        }
    }
}
