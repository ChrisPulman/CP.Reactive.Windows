// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Represents native-handle discovery.</summary>
/// <param name="instance">The managed instance.</param>
/// <param name="fieldName">The field name.</param>
/// <returns>The native handle.</returns>
internal delegate nint GdiPlusGetNativeHandleOperation(object instance, string fieldName);
