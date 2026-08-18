// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Messaging;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
#endif
/// <summary>Registers a window for session notifications.</summary>
/// <param name="windowHandle">The window handle.</param>
/// <param name="flags">The registration flags.</param>
/// <returns><c>true</c> when registration succeeds; otherwise <c>false</c>.</returns>
internal delegate bool SessionRegistrationOperation(IntPtr windowHandle, int flags);
