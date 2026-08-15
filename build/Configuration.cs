// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using Nuke.Common.Tooling;

namespace CP.ReactiveUI.Primitives.Windows.Building;

/// <summary>Represents a supported build configuration.</summary>
[TypeConverter(typeof(TypeConverter<Configuration>))]
public sealed class Configuration : Enumeration
{
    /// <summary>The debug build configuration.</summary>
    public static readonly Configuration Debug = new() { Value = nameof(Debug) };

    /// <summary>The release build configuration.</summary>
    public static readonly Configuration Release = new() { Value = nameof(Release) };

    /// <summary>Converts a configuration to its MSBuild value.</summary>
    /// <param name="configuration">The configuration to convert.</param>
    /// <returns>The MSBuild configuration value.</returns>
    public static implicit operator string(Configuration configuration) => configuration.Value;

    /// <summary>Returns the MSBuild configuration value.</summary>
    /// <returns>The MSBuild configuration value.</returns>
    public override string ToString() => Value;
}
