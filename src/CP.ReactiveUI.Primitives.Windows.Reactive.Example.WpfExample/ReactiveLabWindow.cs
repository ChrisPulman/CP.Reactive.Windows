// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Reactive.Example.WpfExample;

/// <summary>Disambiguates the ReactiveUI namespace from the application's CP.ReactiveUI namespace for XAML.</summary>
/// <typeparam name="TViewModel">The reactive view-model type.</typeparam>
public class ReactiveLabWindow<TViewModel> : global::ReactiveUI.Reactive.ReactiveWindow<TViewModel>
    where TViewModel : class
{
}
