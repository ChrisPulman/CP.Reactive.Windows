// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Dialogs;
#endif

/// <summary>Invokes native shell-item creation without coupling dialog logic to the platform entry point.</summary>
/// <param name="path">The parsing path.</param>
/// <param name="bindContext">The optional bind context.</param>
/// <param name="interfaceId">The requested interface identifier.</param>
/// <param name="shellItem">The created shell-item interface pointer.</param>
/// <returns>The native HRESULT.</returns>
internal delegate int CreateShellItemOperation(string path, IntPtr bindContext, ref Guid interfaceId, out IntPtr shellItem);
