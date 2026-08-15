// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Some enum extensions used throughout the code.</summary>
public static class EnumExtensions
{
    /// <summary>Provides extension members for the target value.</summary>
    /// <param name="enumVal">The target value.</param>
    extension(Enum enumVal)
    {
        /// <summary>Get an attribute of a certain type, placed upon an enum value.</summary>
        /// <param name="attributeType">The attribute type to retrieve.</param>
        /// <returns>The matching attribute, or null when the enum value does not have it.</returns>
        public Attribute GetAttributeOfType(Type attributeType)
        {
            object[] attributes = enumVal
                .GetType()
                .GetMember(enumVal.ToString())[0]
                .GetCustomAttributes(attributeType, inherit: false);
            return attributes.Length == 0 ? null : (Attribute)attributes[0];
        }

        /// <summary>Get the description of an enum.</summary>
        /// <returns>The enum description, or null when no description attribute exists.</returns>
        public string GetEnumDescription() =>
            (
                (DescriptionAttribute)enumVal.GetAttributeOfType(typeof(DescriptionAttribute)))?.Description;
    }
}
