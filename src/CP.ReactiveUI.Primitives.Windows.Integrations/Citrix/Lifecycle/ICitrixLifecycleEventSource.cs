// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Provides Citrix client lifecycle events without requiring callers to depend on a concrete Citrix client implementation.</summary>
public interface ICitrixLifecycleEventSource
{
    /// <summary>Observes Citrix lifecycle events.</summary>
    /// <returns>The lifecycle event stream.</returns>
    IObservable<CitrixLifecycleEvent> ObserveLifecycleEvents();
}
