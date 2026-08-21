// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Lifecycle;

/// <summary>Describes an ICA file parsed by the Citrix client.</summary>
public sealed class CitrixICAFileInfo
{
    /// <summary>Initializes a new instance of the <see cref="CitrixICAFileInfo" /> class.</summary>
    /// <param name="filePath">The ICA file path, when known.</param>
    /// <param name="properties">The parsed ICA properties.</param>
    public CitrixICAFileInfo(string filePath, IReadOnlyDictionary<string, string> properties)
    {
        FilePath = filePath;
        Properties = CopyProperties(properties);
    }

    /// <summary>Gets an empty ICA-file payload.</summary>
    public static CitrixICAFileInfo Empty { get; } = new(null, null);

    /// <summary>Gets the ICA file path, when known.</summary>
    public string FilePath { get; }

    /// <summary>Gets the parsed ICA properties.</summary>
    public IReadOnlyDictionary<string, string> Properties { get; }

    /// <summary>Copies parsed properties into an immutable dictionary wrapper.</summary>
    /// <param name="properties">The source properties.</param>
    /// <returns>The copied property set.</returns>
    private static ReadOnlyDictionary<string, string> CopyProperties(IReadOnlyDictionary<string, string> properties)
    {
        if (properties is null || properties.Count == 0)
        {
            return new(new Dictionary<string, string>());
        }

        Dictionary<string, string> copy = new();
        foreach (var property in properties)
        {
            copy[property.Key] = property.Value;
        }

        return new(copy);
    }
}
