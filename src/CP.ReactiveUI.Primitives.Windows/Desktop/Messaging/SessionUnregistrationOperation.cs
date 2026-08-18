// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Unregisters a window from session notifications.</summary>
/// <param name="windowHandle">The window handle.</param>
/// <returns><c>true</c> when unregistration succeeds; otherwise <c>false</c>.</returns>
internal delegate bool SessionUnregistrationOperation(IntPtr windowHandle);
