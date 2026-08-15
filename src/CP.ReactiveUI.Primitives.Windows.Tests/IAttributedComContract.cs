// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Interop.Com;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>COM contract with a direct ProgID attribute.</summary>
[ComProgId(CoreInteropCoverageTestConstants.ComProgIdValue)]
internal interface IAttributedComContract
{
    /// <summary>Marker member to keep the test contract non-empty.</summary>
    void Marker();
}
