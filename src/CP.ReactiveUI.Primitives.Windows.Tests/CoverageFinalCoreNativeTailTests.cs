// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using NativePointTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativePointTypeConverter;
using NativeRectFloatTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeRectFloatTypeConverter;
using NativeRectTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeRectTypeConverter;
using NativeSizeFloatTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeSizeFloatTypeConverter;
using NativeSizeTypeConverter = CP.ReactiveUI.Primitives.Windows.Native.TypeConverters.NativeSizeTypeConverter;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic tail coverage for Core native value helpers.</summary>
public sealed class CoverageFinalCoreNativeTailTests
{
    /// <summary>Defines the first integer coordinate used by native value tests.</summary>
    private const int CoordinateOne = 1;

    /// <summary>Defines the second integer coordinate used by native value tests.</summary>
    private const int CoordinateTwo = 2;

    /// <summary>Defines the third integer coordinate used by native value tests.</summary>
    private const int CoordinateThree = 3;

    /// <summary>Defines the fourth integer coordinate used by native value tests.</summary>
    private const int CoordinateFour = 4;

    /// <summary>Defines the first floating-point coordinate used by native value tests.</summary>
    private const float CoordinateOneFloat = 1F;

    /// <summary>Defines the one-and-a-half floating-point coordinate used by native value tests.</summary>
    private const float CoordinateOneAndHalfFloat = 1.5F;

    /// <summary>Defines the second floating-point coordinate used by native value tests.</summary>
    private const float CoordinateTwoFloat = 2F;

    /// <summary>Defines the two-and-a-half floating-point coordinate used by native value tests.</summary>
    private const float CoordinateTwoAndHalfFloat = 2.5F;

    /// <summary>Defines the three-and-a-half floating-point coordinate used by native value tests.</summary>
    private const float CoordinateThreeAndHalfFloat = 3.5F;

    /// <summary>Defines the fourth floating-point coordinate used by native value tests.</summary>
    private const float CoordinateFourFloat = 4F;

    /// <summary>Defines the four-and-a-half floating-point coordinate used by native value tests.</summary>
    private const float CoordinateFourAndHalfFloat = 4.5F;

    /// <summary>Defines the eighth floating-point coordinate used by native value tests.</summary>
    private const float CoordinateEightFloat = 8F;

    /// <summary>Defines the invalid text used by converter fallback tests.</summary>
    private const string InvalidText = "invalid";

    /// <summary>Defines an error code which has no system message.</summary>
    private const uint UnknownErrorCode = 2_147_483_647U;

    /// <summary>Defines the expected message for <see cref="UnknownErrorCode" />.</summary>
    private const string UnknownErrorMessage = "Unknown error (0x7fffffff)";

    /// <summary>Exercises rectangle point, overlap, and UTF-16 buffer edge paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectangleAndUtf16_EdgePaths_ReturnExpectedValuesAsync()
    {
        var rectangle = new NativeRectFloat(0F, 0F, CoordinateFourFloat, CoordinateFourFloat);
        var partiallyOverlapping = new NativeRectFloat(CoordinateTwoFloat, CoordinateTwoFloat, CoordinateFourFloat, CoordinateFourFloat);
        var separate = new NativeRectFloat(CoordinateEightFloat, CoordinateEightFloat, CoordinateOneFloat, CoordinateOneFloat);

        await Assert.That(rectangle.Contains(new NativePointFloat(CoordinateOneFloat, CoordinateOneFloat))).IsTrue();
        await Assert.That(rectangle.Contains(CoordinateOneFloat, CoordinateOneFloat)).IsTrue();
        await Assert.That(rectangle.HasOverlap(partiallyOverlapping)).IsTrue();
        await Assert.That(rectangle.HasOverlap(separate)).IsFalse();
        await Assert.That(NativeUtf16String.ReadNullTerminated([])).IsEqualTo(string.Empty);
    }

    /// <summary>Exercises the native Win32 unknown-message fallback without mutating operating-system state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Win32_UnknownMessage_ReturnsStableFallbackAsync()
    {
        await Assert.That(Win32.GetMessage((Win32Error)UnknownErrorCode)).IsEqualTo(UnknownErrorMessage);
        await Assert.That(Win32.GetMessage((Win32Error)UnknownErrorCode, UnknownErrorCode)).IsEqualTo(UnknownErrorMessage);
    }

    /// <summary>Exercises the Windows 11 predicate's future-major-version path.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WindowsVersion_FutureMajorVersion_IsWindows11OrLaterAsync()
    {
        using (WindowsVersion.OverrideVersionProviderForTesting(static () => new(11, 0)))
        {
            await Assert.That(WindowsVersion.IsWindows11OrLater).IsTrue();
        }
    }

    /// <summary>Exercises unsupported point-converter capability queries.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativePointTypeConverter_UnsupportedCapabilities_AreFalseAsync()
    {
        var converter = new NativePointTypeConverter();

        await Assert.That(converter.CanConvertFrom(null, typeof(DateTime))).IsFalse();
        await Assert.That(converter.CanConvertTo(null, typeof(DateTime))).IsFalse();
    }

    /// <summary>Exercises every result path of the native rectangle converters.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectangleTypeConverters_ConvertToAllValueKindsAsync()
    {
        var rectConverter = new NativeRectTypeConverter();
        var rectFloatConverter = new NativeRectFloatTypeConverter();
        var rect = new NativeRect(CoordinateOne, CoordinateTwo, CoordinateThree, CoordinateFour);
        var rectFloat = new NativeRectFloat(CoordinateOneAndHalfFloat, CoordinateTwoAndHalfFloat, CoordinateThreeAndHalfFloat, CoordinateFourAndHalfFloat);

        await Assert.That(() => rectConverter.ConvertTo(null, CultureInfo.InvariantCulture, rect, typeof(object))).Throws<NotSupportedException>();
        await Assert.That(rectConverter.ConvertTo(null, CultureInfo.InvariantCulture, InvalidText, typeof(string))).IsEqualTo(InvalidText);
        await Assert.That(rectConverter.ConvertTo(null, CultureInfo.InvariantCulture, rect, typeof(string))).IsEqualTo("1,2,3,4");
        await Assert.That(() => rectFloatConverter.ConvertTo(null, CultureInfo.InvariantCulture, rectFloat, typeof(object))).Throws<NotSupportedException>();
        await Assert.That(rectFloatConverter.ConvertTo(null, CultureInfo.InvariantCulture, InvalidText, typeof(string))).IsEqualTo(InvalidText);
        await Assert.That(rectFloatConverter.ConvertTo(null, CultureInfo.InvariantCulture, rectFloat, typeof(string))).IsEqualTo("1.5,2.5,3.5,4.5");
    }

    /// <summary>Exercises every result path of the native size converters.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeSizeTypeConverters_ConvertToAllValueKindsAsync()
    {
        var sizeConverter = new NativeSizeTypeConverter();
        var sizeFloatConverter = new NativeSizeFloatTypeConverter();
        var size = new NativeSize(CoordinateThree, CoordinateFour);
        var sizeFloat = new NativeSizeFloat(CoordinateThreeAndHalfFloat, CoordinateFourAndHalfFloat);

        await Assert.That(() => sizeConverter.ConvertTo(null, CultureInfo.InvariantCulture, size, typeof(object))).Throws<NotSupportedException>();
        await Assert.That(sizeConverter.ConvertTo(null, CultureInfo.InvariantCulture, InvalidText, typeof(string))).IsEqualTo(InvalidText);
        await Assert.That(sizeConverter.ConvertTo(null, CultureInfo.InvariantCulture, size, typeof(string))).IsEqualTo("4,3");
        await Assert.That(() => sizeFloatConverter.ConvertTo(null, CultureInfo.InvariantCulture, sizeFloat, typeof(object))).Throws<NotSupportedException>();
        await Assert.That(sizeFloatConverter.ConvertTo(null, CultureInfo.InvariantCulture, InvalidText, typeof(string))).IsEqualTo(InvalidText);
        await Assert.That(sizeFloatConverter.ConvertTo(null, CultureInfo.InvariantCulture, sizeFloat, typeof(string))).IsEqualTo("3.5,4.5");
    }
}
