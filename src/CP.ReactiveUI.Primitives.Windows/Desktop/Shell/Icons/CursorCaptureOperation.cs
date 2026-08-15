// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif

/// <summary>Captures cursor layers from native cursor information.</summary>
/// <param name="result">The cursor capture to populate.</param>
/// <param name="cursorHandle">The cursor handle.</param>
/// <param name="iconInfo">The native icon information.</param>
internal delegate void CursorCaptureOperation(CapturedCursor result, IntPtr cursorHandle, in IconInfoEx iconInfo);
