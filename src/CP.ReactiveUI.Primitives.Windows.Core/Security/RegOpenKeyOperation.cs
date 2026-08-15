// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;
using Microsoft.Win32.SafeHandles;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security;

/// <summary>Opens a registry key.</summary>
/// <param name="key">Parent registry key handle.</param>
/// <param name="subKey">Subkey name.</param>
/// <param name="options">Open options.</param>
/// <param name="desiredAccess">Requested access rights.</param>
/// <param name="openedKey">Opened registry key handle.</param>
/// <returns>Win32 result code.</returns>
internal delegate int RegOpenKeyOperation(nint key, string subKey, RegistryOpenOptions options, RegistryKeySecurityAccessRights desiredAccess, out SafeRegistryHandle openedKey);
