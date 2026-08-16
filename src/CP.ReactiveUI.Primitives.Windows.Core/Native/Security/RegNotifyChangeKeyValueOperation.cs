// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security;

/// <summary>Registers for registry key change notifications.</summary>
/// <param name="key">Registry key handle.</param>
/// <param name="watchSubtree">Whether subkeys are watched.</param>
/// <param name="notifyFilter">Notification filter.</param>
/// <param name="eventHandle">Event handle.</param>
/// <param name="asynchronous">Whether notification is asynchronous.</param>
/// <returns>Win32 result code.</returns>
internal delegate int RegNotifyChangeKeyValueOperation(
    SafeRegistryHandle key,
    bool watchSubtree,
    RegistryNotifyFilter notifyFilter,
    SafeWaitHandle eventHandle,
    bool asynchronous);
