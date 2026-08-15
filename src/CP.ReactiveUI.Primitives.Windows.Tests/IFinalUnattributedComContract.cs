// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Defines an inherited unattributed COM contract.</summary>
internal interface IFinalUnattributedComContract
{
    /// <summary>Gets a marker member to keep the contract non-empty.</summary>
    int Marker { get; }
}
