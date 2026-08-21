// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Provides composition-first observable Citrix client lifecycle filters.</summary>
public static class CitrixLifecycleObservables
{
    /// <summary>Gets the default Citrix lifecycle source backed by WFAPI.</summary>
    public static ICitrixLifecycleEventSource DefaultSource { get; } = CitrixWinFrameLifecycleEventSource.Instance;

    /// <summary>Observes Citrix client connect events from the default source.</summary>
    /// <returns>The connect event stream.</returns>
    public static IObservable<CitrixConnectEvent> OnConnect() => DefaultSource.OnConnect();

    /// <summary>Observes Citrix client disconnect events from the default source.</summary>
    /// <returns>The disconnect event stream.</returns>
    public static IObservable<CitrixDisconnectEvent> OnDisconnect() => DefaultSource.OnDisconnect();

    /// <summary>Observes Citrix client login events from the default source.</summary>
    /// <returns>The login event stream.</returns>
    public static IObservable<CitrixLoginEvent> OnLogin() => DefaultSource.OnLogin();

    /// <summary>Observes Citrix session state change events from the default source.</summary>
    /// <returns>The session state change event stream.</returns>
    public static IObservable<CitrixSessionStateChangeEvent> OnSessionStateChange() => DefaultSource.OnSessionStateChange();
}
