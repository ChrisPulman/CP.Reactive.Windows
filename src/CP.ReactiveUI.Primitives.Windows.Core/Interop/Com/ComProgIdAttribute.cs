// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Interop.Com;

/// <summary>An attribute to specify the ProgID of the COM class to create.</summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class ComProgIdAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Interop.Com.ComProgIdAttribute" /> class.</summary>
    /// <param name="value">The COM ProgID.</param>
    public ComProgIdAttribute(string value)
    {
        Value = value;
    }

    /// <summary>Gets the COM ProgID.</summary>
    public string Value { get; }

    /// <summary>Extracts the attribute from the specified type.</summary>
    /// <param name="interfaceType">
    /// The interface type.
    /// </param>
    /// <returns>
    /// The <see cref="T:CP.ReactiveUI.Primitives.Windows.Interop.Com.ComProgIdAttribute" />.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="interfaceType" /> is <see langword="null" />.
    /// </exception>
    public static ComProgIdAttribute GetAttribute(Type interfaceType)
    {
        Throw.IfNull(interfaceType);
        Type attributeType = typeof(ComProgIdAttribute);
        object[] attributes = interfaceType.GetCustomAttributes(attributeType, inherit: false);
        if (attributes.Length == 0)
        {
            Type[] interfaces = interfaceType.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                interfaceType = interfaces[i];
                attributes = interfaceType.GetCustomAttributes(attributeType, inherit: false);
                if (attributes.Length != 0)
                {
                    break;
                }
            }
        }

        return attributes.Length != 0 ? (ComProgIdAttribute)attributes[0] : null;
    }
}
