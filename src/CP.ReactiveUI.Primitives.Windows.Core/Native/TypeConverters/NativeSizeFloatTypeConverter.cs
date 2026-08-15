// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Globalization;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;

/// <summary>This implements a TypeConverter for the NativeSizeFloat structur.</summary>
public class NativeSizeFloatTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string sizeStringValue)
        {
            string[] hw = sizeStringValue.Split(',');
            if (hw.Length == 2 && float.TryParse(hw[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var h) && float.TryParse(hw[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var w))
            {
                return new NativeSizeFloat(h, w);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => destinationType != typeof(string) || !(value is NativeSizeFloat nativeSizeFloat) ? base.ConvertTo(context, culture, value, destinationType) : string.Join(",", nativeSizeFloat.Width.ToString(CultureInfo.InvariantCulture), nativeSizeFloat.Height.ToString(CultureInfo.InvariantCulture));
}
