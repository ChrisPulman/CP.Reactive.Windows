// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>A simple com wrapper which helps with "using".</summary>
/// <typeparam name="T">Type to wrap.</typeparam>
public interface IDisposableCom<out T> : IDisposable
{
    /// <summary>Gets the actual COM object.</summary>
    T ComObject { get; }
}
