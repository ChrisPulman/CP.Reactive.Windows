// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>A factory for IDisposableCom.</summary>
public static class DisposableCom
{
    /// <summary>Creates a ComDisposable for the supplied typed object.</summary>
    /// <typeparam name="T">Type for the COM object.</typeparam>
    /// <param name="comObject">The COM object itself.</param>
    /// <returns>The disposable COM wrapper.</returns>
    public static IDisposableCom<T> Create<T>(T comObject) =>
        !EqualityComparer<T>.Default.Equals(comObject, default(T))
            ? new DisposableComImplementation<T>(comObject)
            : null;
}
