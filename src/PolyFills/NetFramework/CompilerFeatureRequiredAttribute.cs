// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NETFRAMEWORK
namespace System.Runtime.CompilerServices;

/// <summary>Identifies a compiler feature used by emitted metadata.</summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    /// <summary>The feature name used for required members.</summary>
    public const string RequiredMembers = nameof(RequiredMembers);

    /// <summary>The feature name used for ref structs.</summary>
    public const string RefStructs = nameof(RefStructs);

    /// <summary>Initializes a new instance of the <see cref="CompilerFeatureRequiredAttribute"/> class.</summary>
    /// <param name="featureName">The compiler feature name.</param>
    public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;

    /// <summary>Gets the compiler feature name.</summary>
    public string FeatureName { get; }

    /// <summary>Gets or sets a value indicating whether the feature is optional.</summary>
    public bool IsOptional { get; set; }
}
#endif
