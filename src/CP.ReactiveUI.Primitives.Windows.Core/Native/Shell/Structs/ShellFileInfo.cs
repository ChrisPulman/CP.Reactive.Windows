// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Shell.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs;

/// <summary>A structure which describes shell32 info on a file.</summary>
public readonly struct ShellFileInfo : IEquatable<ShellFileInfo>
{
    /// <summary>Initializes a new instance of the <see cref="T:CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs.ShellFileInfo" /> struct.</summary>
    /// <param name="iconHandle">The shell icon handle.</param>
    /// <param name="iconIndex">The icon index.</param>
    /// <param name="attributes">The shell file attributes.</param>
    /// <param name="displayName">The shell display name.</param>
    /// <param name="typeName">The shell type name.</param>
    internal ShellFileInfo(
        IntPtr iconHandle,
        int iconIndex,
        uint attributes,
        string displayName,
        string typeName)
    {
        IconHandle = new(iconHandle);
        IconIndex = iconIndex;
        Attributes = attributes;
        DisplayName = displayName;
        TypeName = typeName;
    }

    /// <summary>Gets a safe handle to the icon that represents the file.</summary>
    public SafeIconHandle IconHandle { get; }

    /// <summary>Gets the index of the icon image within the system image list.</summary>
    public int IconIndex { get; }

    /// <summary>Gets an array of values that indicates the attributes of the file object. For information about these values, see the IShellFolder::GetAttributesOf method.</summary>
    public uint Attributes { get; }

    /// <summary>Gets the name of the file as it appears in the Windows Shell.</summary>
    public string DisplayName { get; }

    /// <summary>Gets a string that describes the type of file.</summary>
    public string TypeName { get; }

    /// <summary>Compares two values for equality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the values are equal; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(ShellFileInfo left, ShellFileInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two values for inequality.</summary>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns><see langword="true" /> when the values are not equal; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(ShellFileInfo left, ShellFileInfo right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is ShellFileInfo other && Equals(other);

    /// <inheritdoc />
    public bool Equals(ShellFileInfo other)
    {
        SafeIconHandle otherIconHandle = other.IconHandle;
        return IconHandle.UseNativeHandle(
                (leftHandle) =>
                    otherIconHandle.UseNativeHandle((rightHandle) => leftHandle == rightHandle))
            && IconIndex == other.IconIndex
            && Attributes == other.Attributes
            && string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int iconIndex = IconIndex;
        uint attributes = Attributes;
        string displayName = DisplayName;
        string typeName = TypeName;
        return IconHandle.UseNativeHandle(
            (iconHandle) =>
                HashCode.Combine(iconHandle, iconIndex, attributes, displayName, typeName));
    }
}
