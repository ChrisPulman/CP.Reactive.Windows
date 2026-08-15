// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Defines region result values returned by GetWindowRgn.</summary>
public enum RegionResults
{
    /// <summary>The specified window does not have a region, or an error occurred while attempting to return the region.</summary>
    Error,
    /// <summary>The region is empty.</summary>
    NullRegion,
    /// <summary>The region is a single rectangle.</summary>
    SimpleRegion,
    /// <summary>The region is more than one rectangle.</summary>
    ComplexRegion
}
