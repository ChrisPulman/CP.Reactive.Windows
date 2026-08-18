// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons.Structs;
#endif
/// <summary>Contains extended information about an icon or a cursor.</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public readonly struct IconInfoEx : IEquatable<IconInfoEx>, IDisposable
{
    /// <summary>Stores the structure size.</summary>
    private readonly uint _structureSize;

    /// <summary>Stores whether this structure describes an icon.</summary>
    private readonly int _isIcon;

    /// <summary>Stores the hotspot x-coordinate.</summary>
    private readonly int _hotspotX;

    /// <summary>Stores the hotspot y-coordinate.</summary>
    private readonly int _hotspotY;

    /// <summary>Stores the bitmask bitmap handle.</summary>
    private readonly IntPtr _maskBitmapHandle;

    /// <summary>Stores the color bitmap handle.</summary>
    private readonly IntPtr _colorBitmapHandle;

    /// <summary>Stores the resource identifier.</summary>
    private readonly ushort _resourceId;

    /// <summary>Stores the module name.</summary>
    private readonly IconNameBuffer _moduleName;

    /// <summary>Stores the resource name.</summary>
    private readonly IconNameBuffer _resourceName;

    /// <summary>Gets the structure size.</summary>
    public uint StructureSize => _structureSize;

    /// <summary>Gets or sets a value indicating whether this structure defines an icon or a cursor.</summary>
    public bool IsIcon
    {
        get => _isIcon != 0;
        set => Unsafe.AsRef(in _isIcon) = (value ? 1 : 0);
    }

    /// <summary>Gets or sets the coordinates of a cursor hot spot.</summary>
    public NativePoint Hotspot
    {
        get => new(_hotspotX, _hotspotY);
        set
        {
            Unsafe.AsRef(in _hotspotX) = value.X;
            Unsafe.AsRef(in _hotspotY) = value.Y;
        }
    }

    /// <summary>Gets the icon bitmask bitmap handle.</summary>
    public SafeHBitmapHandle BitmaskBitmapHandle => new(_maskBitmapHandle);

    /// <summary>Gets the icon color bitmap handle.</summary>
    public SafeHBitmapHandle ColorBitmapHandle => new(_colorBitmapHandle);

    /// <summary>Gets the resource identifier of the resource in the module name.</summary>
    public ushort ResourceId => _resourceId;

    /// <summary>Gets the name of the module from which an icon or a cursor was loaded.</summary>
    public string ModuleName => _moduleName.GetText();

    /// <summary>Gets the resource name of the resource in the module name.</summary>
    public string ResourceName => _resourceName.GetText();

    /// <summary>Creates icon information with native defaults.</summary>
    /// <returns>The initialized extended icon information.</returns>
    public static IconInfoEx Create()
    {
        IconInfoEx iconInfo = default;
        Unsafe.AsRef(in iconInfo._structureSize) = checked((uint)Marshal.SizeOf<IconInfoEx>());
        return iconInfo;
    }

    /// <summary>Determines whether two extended icon information values are equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A value indicating whether the values are equal.</returns>
    public static bool operator ==(IconInfoEx left, IconInfoEx right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two extended icon information values are not equal.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>A value indicating whether the values are not equal.</returns>
    public static bool operator !=(IconInfoEx left, IconInfoEx right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public bool Equals(IconInfoEx other) =>
        _structureSize == other._structureSize
        && _isIcon == other._isIcon
        && _hotspotX == other._hotspotX
        && _hotspotY == other._hotspotY
        && _maskBitmapHandle == other._maskBitmapHandle
        && _colorBitmapHandle == other._colorBitmapHandle
        && _resourceId == other._resourceId
        && ModuleName == other.ModuleName
        && ResourceName == other.ResourceName;

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is IconInfoEx other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_structureSize, _isIcon, _hotspotX, _hotspotY, _maskBitmapHandle, _colorBitmapHandle, _resourceId);

    /// <inheritdoc />
    public void Dispose()
    {
        ColorBitmapHandle.Dispose();
        BitmaskBitmapHandle.Dispose();
        Unsafe.AsRef(in _resourceId) = 0;
        _moduleName.Clear();
        _resourceName.Clear();
    }

    /// <summary>Creates icon information with a module-name buffer that has no null terminator.</summary>
    /// <param name="value">The value to write into the module-name buffer.</param>
    /// <returns>The initialized extended icon information.</returns>
    internal static IconInfoEx CreateWithUnterminatedModuleNameForTesting(char value)
    {
        var iconInfo = Create();
        iconInfo._moduleName.Fill(value);
        return iconInfo;
    }

    /// <summary>Stores the fixed-size native UTF-16 icon-name buffer.</summary>
    [StructLayout(LayoutKind.Sequential, Size = 520)]
    private readonly struct IconNameBuffer
    {
        /// <summary>The number of UTF-16 characters reserved for each native name.</summary>
        private const int IconNameLength = 260;

        /// <summary>The size of one UTF-16 character in bytes.</summary>
        private const nint WideCharacterSize = sizeof(char);

        /// <summary>Anchors the explicitly sized unmanaged buffer.</summary>
        private readonly byte _firstByte;

        /// <summary>Reads the null-terminated buffer contents.</summary>
        /// <returns>The buffer text.</returns>
        internal unsafe string GetText()
        {
            fixed (byte* ptr = &Unsafe.AsRef(in _firstByte))
            {
                var name = (char*)ptr;
                var length = 0;
                while (length < IconNameLength
                    && *(ushort*)((byte*)name + checked(unchecked((nint)length) * WideCharacterSize)) != 0)
                {
                    length++;
                }

                return new(name, 0, length);
            }
        }

        /// <summary>Clears the first character in the buffer.</summary>
        internal unsafe void Clear()
        {
            fixed (byte* ptr = &Unsafe.AsRef(in _firstByte))
            {
                *(short*)ptr = 0;
            }
        }

        /// <summary>Fills the complete buffer with a character.</summary>
        /// <param name="value">The character value.</param>
        internal unsafe void Fill(char value)
        {
            fixed (byte* ptr = &Unsafe.AsRef(in _firstByte))
            {
                var characters = (char*)ptr;
                for (var index = 0; index < IconNameLength; index = checked(index + 1))
                {
                    *(char*)((byte*)characters + checked(unchecked((nint)index) * WideCharacterSize)) = value;
                }
            }
        }
    }
}
