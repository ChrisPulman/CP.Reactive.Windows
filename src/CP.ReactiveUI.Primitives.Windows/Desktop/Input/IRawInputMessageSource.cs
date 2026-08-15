// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Input;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Input;
#endif
/// <summary>Provides the window-message source used by raw-input monitors.</summary>
internal interface IRawInputMessageSource
{
    /// <summary>Gets raw window messages.</summary>
    IObservable<WindowMessage> Messages { get; }

    /// <summary>Gets message-window handle changes.</summary>
    /// <returns>The handle change stream.</returns>
    IObservable<long> ObserveHandleChanges();
}
