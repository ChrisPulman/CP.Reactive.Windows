// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Tries to send an icon lookup message.</summary>
/// <param name="windowHandle">The target window handle.</param>
/// <param name="message">The window message.</param>
/// <param name="wordParameter">The message parameter.</param>
/// <param name="result">The returned native handle.</param>
/// <returns><see langword="true" /> when the call succeeded.</returns>
internal delegate bool TrySendIconMessage(IntPtr windowHandle, WindowsMessages message, IntPtr wordParameter, out IntPtr result);
