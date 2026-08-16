// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NETFRAMEWORK
namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that an output is not null when the method returns the specified value.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class.</summary>
    /// <param name="returnValue">The return value for which the annotated value is not null.</param>
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>Gets the return value condition.</summary>
    public bool ReturnValue { get; }
}
#endif
