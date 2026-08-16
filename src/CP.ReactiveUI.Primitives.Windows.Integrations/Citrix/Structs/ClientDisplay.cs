// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Structs;

/// <summary>This structure is returned when WFQuerySessionInformation is called with WFInfoClasses.ClientDisplay.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ClientDisplay : IEquatable<ClientDisplay>
{
    /// <summary>The four-bit color-depth code.</summary>
    private const uint FourBitColorDepthCode = 1;

    /// <summary>The eight-bit color-depth code.</summary>
    private const uint EightBitColorDepthCode = 2;

    /// <summary>The sixteen-bit color-depth code.</summary>
    private const uint SixteenBitColorDepthCode = 4;

    /// <summary>The twenty-four-bit color-depth code.</summary>
    private const uint TwentyFourBitColorDepthCode = 8;

    /// <summary>The thirty-two-bit color-depth code.</summary>
    private const uint ThirtyTwoBitColorDepthCode = 16;

    /// <summary>The four-bit color depth.</summary>
    private const uint FourBitColorDepth = 4;

    /// <summary>The eight-bit color depth.</summary>
    private const uint EightBitColorDepth = 8;

    /// <summary>The sixteen-bit color depth.</summary>
    private const uint SixteenBitColorDepth = 16;

    /// <summary>The twenty-four-bit color depth.</summary>
    private const uint TwentyFourBitColorDepth = 24;

    /// <summary>The thirty-two-bit color depth.</summary>
    private const uint ThirtyTwoBitColorDepth = 32;

    /// <summary>The horizontal resolution.</summary>
    private readonly uint _horizontalResolution;

    /// <summary>The vertical resolution.</summary>
    private readonly uint _verticalResolution;

    /// <summary>The color depth.</summary>
    private readonly uint _colorDepth;

    /// <summary>Initializes a new instance of the <see cref="ClientDisplay"/> struct.</summary>
    /// <param name="horizontalResolution">The horizontal resolution.</param>
    /// <param name="verticalResolution">The vertical resolution.</param>
    /// <param name="colorDepth">The color-depth code.</param>
    internal ClientDisplay(uint horizontalResolution, uint verticalResolution, uint colorDepth)
    {
        _horizontalResolution = horizontalResolution;
        _verticalResolution = verticalResolution;
        _colorDepth = colorDepth;
    }

    /// <summary>Gets the client's display size.</summary>
    public NativeSize ClientSize => new((int)_horizontalResolution, (int)_verticalResolution);

    /// <summary>Gets the number of colors the client can display.</summary>
    public uint ColorDepth
    {
        get => _colorDepth switch
        {
            FourBitColorDepthCode => FourBitColorDepth,
            EightBitColorDepthCode => EightBitColorDepth,
            SixteenBitColorDepthCode => SixteenBitColorDepth,
            TwentyFourBitColorDepthCode => TwentyFourBitColorDepth,
            ThirtyTwoBitColorDepthCode => ThirtyTwoBitColorDepth,
            _ => _colorDepth
        };
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ClientDisplay left, ClientDisplay right) => left.Equals(right);

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ClientDisplay left, ClientDisplay right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ClientDisplay other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ClientDisplay other) =>
        _horizontalResolution == other._horizontalResolution
        && _verticalResolution == other._verticalResolution
        && _colorDepth == other._colorDepth;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        _horizontalResolution,
        _verticalResolution,
        _colorDepth);

    /// <inheritdoc/>
    public override string ToString() => $"{ClientSize}|{ColorDepth}";
}
