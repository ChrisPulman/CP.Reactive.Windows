// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif

/// <summary>Invokes native COM activation without coupling dialog logic to the platform entry point.</summary>
/// <param name="classId">The COM class identifier.</param>
/// <param name="outerUnknown">The optional controlling unknown.</param>
/// <param name="classContext">The COM activation context.</param>
/// <param name="interfaceId">The requested interface identifier.</param>
/// <param name="instance">The activated interface pointer.</param>
/// <returns>The native HRESULT.</returns>
internal delegate int CoCreateInstanceOperation(ref Guid classId, IntPtr outerUnknown, uint classContext, ref Guid interfaceId, out IntPtr instance);
