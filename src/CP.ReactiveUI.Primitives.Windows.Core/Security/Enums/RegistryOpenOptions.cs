// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

/// <summary>Specifies the option to apply when opening the key.</summary>
[Flags]
public enum RegistryOpenOptions
{
    /// <summary>No options.</summary>
    None = 0,
    /// <summary>The key is a symbolic link. Registry symbolic links should only be used when absolutely necessary.</summary>
    OpenLink = 8
}
