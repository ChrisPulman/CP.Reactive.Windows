// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Runtime.InteropServices;

/// <summary>Specifies how strings should be marshalled by generated interop stubs.</summary>
internal enum StringMarshalling
{
    /// <summary>Use the platform default marshalling behavior.</summary>
    Custom = 0,

    /// <summary>Use UTF-8 string marshalling.</summary>
    Utf8 = 1,

    /// <summary>Use UTF-16 string marshalling.</summary>
    Utf16 = 2,
}
