// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;

/// <summary>The flags that specify the file information to retrieve when calling SHGetFileInfo. This parameter can be a combination of the following values.</summary>
[Flags]
public enum ShellGetFileInfoFlags
{
    /// <summary>
    /// Retrieve the handle to the icon that represents the file and the index of the icon within the system image list.
    /// The handle is copied to the hIcon member of the structure specified by psfi, and the index is copied to the iIcon member.
    /// </summary>
    Icon = 0x100,

    /// <summary>
    /// Retrieve the display name for the file, which is the name as it appears in Windows Explorer.
    /// The name is copied to the szDisplayName member of the structure specified in psfi.
    /// The returned display name uses the long file name, if there is one, rather than the 8.3 form of the file name.
    /// Note that the display name can be affected by settings such as whether extensions are shown.
    /// </summary>
    DisplayName = 0x200,

    /// <summary>Retrieve the string that describes the file's type. The string is copied to the szTypeName member of the structure specified in psfi.</summary>
    TypeName = 0x400,

    /// <summary>
    /// Retrieve the item attributes.
    /// The attributes are copied to the dwAttributes member of the structure specified in the psfi parameter.
    /// These are the same attributes that are obtained from IShellFolder::GetAttributesOf.
    /// </summary>
    Attributes = 0x800,

    /// <summary>
    /// Retrieve the name of the file that contains the icon representing the file specified by pszPath, as returned by the IExtractIcon::GetIconLocation method of the file's icon handler.
    /// Also retrieve the icon index within that file.
    /// The name of the file containing the icon is copied to the szDisplayName member of the structure specified by psfi.
    /// The icon's index is copied to that structure's iIcon member.
    /// </summary>
    IconLocation = 0x1000,

    /// <summary>
    /// Retrieve the type of the executable file if pszPath identifies an executable file. The information is packed into the return value.
    /// This flag cannot be specified with any other flags.
    /// </summary>
    ExeType = 0x2000,

    /// <summary>
    /// Retrieve the index of a system image list icon.
    /// If successful, the index is copied to the iIcon member of psfi.
    /// The return value is a handle to the system image list.
    /// Only those images whose indices are successfully copied to iIcon are valid.
    /// Attempting to access other images in the system image list will result in undefined behavior.
    /// </summary>
    SysIconIndex = 0x4000,

    /// <summary>Modify SHGFI_ICON, causing the function to add the link overlay to the file's icon. The SHGFI_ICON flag must also be set.</summary>
    LinkOverlay = 0x8000,

    /// <summary>Modify SHGFI_ICON, causing the function to blend the file's icon with the system highlight color. The SHGFI_ICON flag must also be set.</summary>
    Selected = 0x10000,

    /// <summary>
    /// Modify SHGFI_ATTRIBUTES to indicate that the dwAttributes member of the SHFILEINFO structure at psfi contains the specific attributes that are desired.
    /// These attributes are passed to IShellFolder::GetAttributesOf.
    /// If this flag is not specified, 0xFFFFFFFF is passed to IShellFolder::GetAttributesOf, requesting all attributes.
    /// This flag cannot be specified with the SHGFI_ICON flag.
    /// </summary>
    AttributeSpecified = 0x20000,

    /// <summary>
    /// Specifies no modifier flags. When combined with <see cref="F:CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums.ShellGetFileInfoFlags.Icon" />,
    /// the shell returns a large icon.
    /// </summary>
    None = 0,

    /// <summary>
    /// Modify SHGFI_ICON, causing the function to retrieve the file's small icon.
    /// Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains small icon images.
    /// The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
    /// </summary>
    SmallIcon = 1,

    /// <summary>
    /// Modify SHGFI_ICON, causing the function to retrieve the file's open icon.
    /// Also used to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system image list that contains the file's small open icon.
    /// A container object displays an open icon to indicate that the container is open.
    /// The SHGFI_ICON and/or SHGFI_SYSICONINDEX flag must also be set.
    /// </summary>
    OpenIcon = 2,

    /// <summary>
    /// Modify SHGFI_ICON, causing the function to retrieve a Shell-sized icon.
    /// If this flag is not specified the function sizes the icon according to the system metric values.
    /// The SHGFI_ICON flag must also be set.
    /// </summary>
    ShellIconSize = 4,

    /// <summary>Indicate that pszPath is the address of an ITEMIDLIST structure rather than a path name.</summary>
    PointToItemIdList = 8,

    /// <summary>
    /// Indicates that the function should not attempt to access the file specified by pszPath.
    /// Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes.
    /// This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
    /// </summary>
    UseFileAttributes = 0x10,

    /// <summary>Apply the appropriate overlays to the file's icon. The SHGFI_ICON flag must also be set.</summary>
    AddOverlays = 0x20,

    /// <summary>
    /// Return the index of the overlay icon.
    /// The value of the overlay index is returned in the upper eight bits of the iIcon member of the structure specified by psfi.
    /// This flag requires that the SHGFI_ICON be set as well.
    /// </summary>
    OverlayIndex = 0x40,
}
