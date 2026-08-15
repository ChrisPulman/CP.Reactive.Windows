// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Stores deterministic GDI+ blur state.</summary>
/// <param name="Operations">The operation group.</param>
/// <param name="IsBlurEnabled">Whether blur is enabled.</param>
internal readonly record struct GdiPlusBlurState(GdiPlusBlurOperations Operations, bool IsBlurEnabled);
