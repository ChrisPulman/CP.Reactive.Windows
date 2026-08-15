// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Security;

/// <summary>Converts a security identifier to string form.</summary>
/// <param name="sid">Security identifier pointer.</param>
/// <param name="sidString">Allocated string pointer.</param>
/// <returns>True on success; otherwise, false.</returns>
internal delegate bool ConvertSidToStringSidOperation(nint sid, out nint sidString);
