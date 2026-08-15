// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Represents unmanaged memory release.</summary>
/// <param name="memory">The memory to release.</param>
internal delegate void GdiPlusFreeHGlobalOperation(nint memory);
