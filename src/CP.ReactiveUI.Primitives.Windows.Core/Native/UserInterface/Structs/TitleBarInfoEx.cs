// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs;

/// <summary>
///     The structure for the TITLEBARINFOEX
///     See
///     <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969233(v=vs.85).aspx">TITLEBARINFOEX struct</a>
/// </summary>
[Serializable]
public readonly struct TitleBarInfoEx : IEquatable<TitleBarInfoEx>
{
    /// <summary>The size of the structure, in bytes. The caller must set this member to sizeof(TITLEBARINFOEX).</summary>
    private readonly uint _nativeSize;

    /// <summary>Native _nativeTitleBarBounds field.</summary>
    private readonly NativeRect _nativeTitleBarBounds;

    /// <summary>Native first state field.</summary>
    private readonly ObjectStates _nativeState0;

    /// <summary>Native second state field.</summary>
    private readonly ObjectStates _nativeState1;

    /// <summary>Native third state field.</summary>
    private readonly ObjectStates _nativeState2;

    /// <summary>Native fourth state field.</summary>
    private readonly ObjectStates _nativeState3;

    /// <summary>Native fifth state field.</summary>
    private readonly ObjectStates _nativeState4;

    /// <summary>Native sixth state field.</summary>
    private readonly ObjectStates _nativeState5;

    /// <summary>Native first rectangle field.</summary>
    private readonly NativeRect _nativeRectangle0;

    /// <summary>Native second rectangle field.</summary>
    private readonly NativeRect _nativeRectangle1;

    /// <summary>Native third rectangle field.</summary>
    private readonly NativeRect _nativeRectangle2;

    /// <summary>Native fourth rectangle field.</summary>
    private readonly NativeRect _nativeRectangle3;

    /// <summary>Native fifth rectangle field.</summary>
    private readonly NativeRect _nativeRectangle4;

    /// <summary>Native sixth rectangle field.</summary>
    private readonly NativeRect _nativeRectangle5;

    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Structs.TitleBarInfoEx" /> struct.</summary>
    /// <param name="nativeSize">The native structure size.</param>
    private TitleBarInfoEx(uint nativeSize)
    {
        _nativeSize = nativeSize;
        _nativeTitleBarBounds = NativeRect.Empty;
        _nativeState0 = ObjectStates.None;
        _nativeState1 = ObjectStates.None;
        _nativeState2 = ObjectStates.None;
        _nativeState3 = ObjectStates.None;
        _nativeState4 = ObjectStates.None;
        _nativeState5 = ObjectStates.None;
        _nativeRectangle0 = NativeRect.Empty;
        _nativeRectangle1 = NativeRect.Empty;
        _nativeRectangle2 = NativeRect.Empty;
        _nativeRectangle3 = NativeRect.Empty;
        _nativeRectangle4 = NativeRect.Empty;
        _nativeRectangle5 = NativeRect.Empty;
    }

    /// <summary>Gets the coordinates of the title bar. These coordinates include all title-bar elements except the window menu.</summary>
    public NativeRect Bounds => _nativeTitleBarBounds;

    /// <summary>Factory method for a default TitleBarInfoEx.</summary>
    /// <returns>The initialized title bar information.</returns>
    public static TitleBarInfoEx Create() => new(checked((uint)Marshal.SizeOf<TitleBarInfoEx>()));

    /// <summary>Compares two TitleBarInfoEx values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when both values are equal.</returns>
    public static bool operator ==(TitleBarInfoEx left, TitleBarInfoEx right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two TitleBarInfoEx values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>True when the values are not equal.</returns>
    public static bool operator !=(TitleBarInfoEx left, TitleBarInfoEx right)
    {
        return !left.Equals(right);
    }

    /// <summary>Returns the ObjectState of the specified element.</summary>
    /// <param name="titleBarInfoIndex">TitleBarInfoIndexes used to specify the element.</param>
    /// <returns>ObjectStates.</returns>
    public ObjectStates ElementState(TitleBarInfoIndexes titleBarInfoIndex) =>
        titleBarInfoIndex switch
        {
            TitleBarInfoIndexes.TitleBar => _nativeState0,
            TitleBarInfoIndexes.Reserved => _nativeState1,
            TitleBarInfoIndexes.MinimizeButton => _nativeState2,
            TitleBarInfoIndexes.MaximizeButton => _nativeState3,
            TitleBarInfoIndexes.HelpButton => _nativeState4,
            TitleBarInfoIndexes.CloseButton => _nativeState5,
            _ => throw new ArgumentOutOfRangeException(
                nameof(titleBarInfoIndex),
                titleBarInfoIndex,
                null),
        };

    /// <summary>Returns the Bounds of the specified element.</summary>
    /// <param name="titleBarInfoIndex">TitleBarInfoIndexes used to specify the element.</param>
    /// <returns>RECT.</returns>
    public NativeRect ElementBounds(TitleBarInfoIndexes titleBarInfoIndex) =>
        titleBarInfoIndex switch
        {
            TitleBarInfoIndexes.TitleBar => _nativeRectangle0,
            TitleBarInfoIndexes.Reserved => _nativeRectangle1,
            TitleBarInfoIndexes.MinimizeButton => _nativeRectangle2,
            TitleBarInfoIndexes.MaximizeButton => _nativeRectangle3,
            TitleBarInfoIndexes.HelpButton => _nativeRectangle4,
            TitleBarInfoIndexes.CloseButton => _nativeRectangle5,
            _ => throw new ArgumentOutOfRangeException(
                nameof(titleBarInfoIndex),
                titleBarInfoIndex,
                null),
        };

    /// <inheritdoc />
    public bool Equals(TitleBarInfoEx other) =>
        HasSameTitleBarStates(in other) && HasSameTitleBarRectangles(in other);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is TitleBarInfoEx other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <summary>Compares title bar state fields.</summary>
    /// <param name="other">The other value.</param>
    /// <returns>True when state values match.</returns>
    private bool HasSameTitleBarStates(in TitleBarInfoEx other) =>
        _nativeSize == other._nativeSize
        && _nativeTitleBarBounds.Equals(other._nativeTitleBarBounds)
        && _nativeState0 == other._nativeState0
        && _nativeState1 == other._nativeState1
        && _nativeState2 == other._nativeState2
        && _nativeState3 == other._nativeState3
        && _nativeState4 == other._nativeState4
        && _nativeState5 == other._nativeState5;

    /// <summary>Compares title bar rectangle fields.</summary>
    /// <param name="other">The other value.</param>
    /// <returns>True when rectangle values match.</returns>
    private bool HasSameTitleBarRectangles(in TitleBarInfoEx other) =>
        _nativeRectangle0.Equals(other._nativeRectangle0)
        && _nativeRectangle1.Equals(other._nativeRectangle1)
        && _nativeRectangle2.Equals(other._nativeRectangle2)
        && _nativeRectangle3.Equals(other._nativeRectangle3)
        && _nativeRectangle4.Equals(other._nativeRectangle4)
        && _nativeRectangle5.Equals(other._nativeRectangle5);
}
