// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Globalization;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.TypeConverters;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.TypeConverters;

/// <summary>This implements a TypeConverter for the WindowPlacement structur.</summary>
public class WindowPlacementTypeConverter : TypeConverter
{
    /// <summary>Stores the native point type converter value.</summary>
    private readonly NativePointTypeConverter _nativePointTypeConverter = TypeDescriptor.GetConverter(typeof(NativePoint)) as NativePointTypeConverter;

    /// <summary>Stores the native rect type converter value.</summary>
    private readonly NativeRectTypeConverter _nativeRectTypeConverter = TypeDescriptor.GetConverter(typeof(NativeRect)) as NativeRectTypeConverter;

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc />
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string windowPlacementString)
        {
            string[] cmdMinMaxNormal = windowPlacementString.Split('|');
            if (cmdMinMaxNormal.Length == 4 && Enum.TryParse<ShowWindowCommands>(cmdMinMaxNormal[0], ignoreCase: true, out var showCommand))
            {
                WindowPlacement windowPlacement = WindowPlacement.Create();
                windowPlacement.ShowCmd = showCommand;
                windowPlacement.MinPosition = ((NativePoint?)_nativePointTypeConverter.ConvertFromInvariantString(cmdMinMaxNormal[1])) ?? NativePoint.Empty;
                windowPlacement.MaxPosition = ((NativePoint?)_nativePointTypeConverter.ConvertFromInvariantString(cmdMinMaxNormal[2])) ?? NativePoint.Empty;
                windowPlacement.NormalPosition = ((NativeRect?)_nativeRectTypeConverter.ConvertFromInvariantString(cmdMinMaxNormal[3])) ?? NativeRect.Empty;
                return windowPlacement;
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is WindowPlacement windowPlacement)
        {
            string minimum = _nativePointTypeConverter.ConvertToInvariantString(windowPlacement.MinPosition);
            string maximum = _nativePointTypeConverter.ConvertToInvariantString(windowPlacement.MaxPosition);
            string normal = _nativeRectTypeConverter.ConvertToInvariantString(windowPlacement.NormalPosition);
            return $"{windowPlacement.ShowCmd}|{minimum}|{maximum}|{normal}";
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
