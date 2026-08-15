// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.PolyFills;

/// <summary>Compatibility throw helpers for target frameworks without modern BCL throw APIs.</summary>
internal static class Throw
{
    /// <summary>Throws when a value is null.</summary>
    /// <param name="value">The value to test.</param>
    /// <param name="parameterName">The parameter name.</param>
#if NETFRAMEWORK
    internal static void IfNull(object value, string parameterName = null)
#else
    internal static void IfNull(object value, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string parameterName = null)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    /// <summary>Throws when a string is null or empty.</summary>
    /// <param name="value">The string to test.</param>
    /// <param name="parameterName">The parameter name.</param>
#if NETFRAMEWORK
    internal static void IfNullOrEmpty(string value, string parameterName = null)
#else
    internal static void IfNullOrEmpty(string value, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string parameterName = null)
#endif
    {
#if NETFRAMEWORK
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("The value cannot be null or empty.", parameterName);
        }
#else
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
#endif
    }

    /// <summary>Throws when a string is null, empty, or consists only of white-space characters.</summary>
    /// <param name="value">The string to test.</param>
    /// <param name="parameterName">The parameter name.</param>
#if NETFRAMEWORK
    internal static void IfNullOrWhiteSpace(string value, string parameterName = null)
#else
    internal static void IfNullOrWhiteSpace(string value, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(value))] string parameterName = null)
#endif
    {
#if NETFRAMEWORK
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The value cannot be null, empty, or white space.", parameterName);
        }
#else
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
#endif
    }

    /// <summary>Throws when an object has been disposed.</summary>
    /// <param name="condition">A value indicating whether the object is disposed.</param>
    /// <param name="instance">The disposed instance.</param>
    internal static void IfDisposed(bool condition, object instance)
    {
#if NETFRAMEWORK
        if (condition)
        {
            throw new ObjectDisposedException(instance.GetType().FullName);
        }
#else
        ObjectDisposedException.ThrowIf(condition, instance);
#endif
    }
}
