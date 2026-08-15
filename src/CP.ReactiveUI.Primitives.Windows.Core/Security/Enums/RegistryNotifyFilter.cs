// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

/// <summary>A value that indicates the changes that should be reported.</summary>
[Flags]
public enum RegistryNotifyFilter
{
    /// <summary>Notify the caller if a subkey is added or deleted.</summary>
    None = 0,
    /// <summary>Notify the caller if a subkey is added or deleted.</summary>
    ChangeName = 1,
    /// <summary>Notify the caller of changes to the attributes of the key.</summary>
    ChangeAttributes = 2,
    /// <summary>Notify the caller of changes to a value of the key.</summary>
    ChangeLastSet = 4,
    /// <summary>Notify the caller of changes to the security descriptor of the key.</summary>
    ChangeSecurity = 8,
    /// <summary>Indicates that the registration lifetime must not be tied to the issuing thread lifetime.</summary>
    ThreadAgnostic = 0x10000000
}
