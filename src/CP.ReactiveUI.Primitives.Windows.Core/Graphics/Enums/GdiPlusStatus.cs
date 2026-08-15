// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;

/// <summary>Specifies the status result returned by a GDI+ call.</summary>
public enum GdiPlusStatus
{
    /// <summary>Specifies success.</summary>
    Ok,
    /// <summary>Specifies an unspecified error.</summary>
    GenericError,
    /// <summary>Specifies an invalid parameter.</summary>
    InvalidParameter,
    /// <summary>Specifies insufficient memory.</summary>
    OutOfMemory,
    /// <summary>Specifies a busy object.</summary>
    ObjectBusy,
    /// <summary>Specifies an insufficient buffer.</summary>
    InsufficientBuffer,
    /// <summary>Specifies an unimplemented operation.</summary>
    NotImplemented,
    /// <summary>Specifies a Win32 error.</summary>
    Win32Error,
    /// <summary>Specifies an invalid object state.</summary>
    WrongState,
    /// <summary>Specifies an aborted operation.</summary>
    Aborted,
    /// <summary>Specifies a missing file.</summary>
    FileNotFound,
    /// <summary>Specifies a value overflow.</summary>
    ValueOverflow,
    /// <summary>Specifies access denial.</summary>
    AccessDenied,
    /// <summary>Specifies an unknown image format.</summary>
    UnknownImageFormat,
    /// <summary>Specifies a missing font family.</summary>
    FontFamilyNotFound,
    /// <summary>Specifies a missing font style.</summary>
    FontStyleNotFound,
    /// <summary>Specifies a non-TrueType font.</summary>
    NotTrueTypeFont,
    /// <summary>Specifies an unsupported GDI+ version.</summary>
    UnsupportedGdiplusVersion,
    /// <summary>Specifies an uninitialized GDI+ runtime.</summary>
    GdiplusNotInitialized,
    /// <summary>Specifies a missing property.</summary>
    PropertyNotFound,
    /// <summary>Specifies an unsupported property.</summary>
    PropertyNotSupported,
    /// <summary>Specifies a missing color profile.</summary>
    ProfileNotFound
}
