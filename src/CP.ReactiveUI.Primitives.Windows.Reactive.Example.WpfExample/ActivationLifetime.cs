// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.WpfExample;

/// <summary>Provides explicit activation-lifetime tracking without mixing disposable implementations.</summary>
internal static class ActivationLifetime
{
    /// <summary>Adds a resource to the current ReactiveUI activation lifetime.</summary>
    /// <param name="resource">The resource to dispose when the view deactivates.</param>
    /// <param name="track">The activation resource collector.</param>
    internal static void TrackWith(this IDisposable resource, Action<IDisposable> track) => track(resource);
}
