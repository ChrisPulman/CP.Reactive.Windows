// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace System.Runtime.InteropServices;

/// <summary>Polyfills <see cref="LibraryImportAttribute" /> for target frameworks that do not include it.</summary>
/// <param name="libraryName">The native library name.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class LibraryImportAttribute(string libraryName) : Attribute
{
    /// <summary>Gets the library name.</summary>
    public string LibraryName { get; } = libraryName;

    /// <summary>Gets or sets the native entry point.</summary>
    public string EntryPoint { get; set; }

    /// <summary>Gets or sets a value indicating whether the generated stub preserves the last platform error.</summary>
    public bool SetLastError { get; set; }

    /// <summary>Gets or sets the string marshalling mode.</summary>
    public StringMarshalling StringMarshalling { get; set; }
}
