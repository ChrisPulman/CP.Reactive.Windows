// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Ipc;

/// <summary>Describes a VdRegisterFeature result.</summary>
public sealed class CitrixVirtualChannelFeatureRegistration
{
    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelFeatureRegistration"/> class.</summary>
    /// <param name="featureName">The registered feature name.</param>
    /// <param name="status">The operation status.</param>
    public CitrixVirtualChannelFeatureRegistration(string featureName, CitrixVirtualChannelStatus status)
        : this(featureName, status, 0, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CitrixVirtualChannelFeatureRegistration"/> class.</summary>
    /// <param name="featureName">The registered feature name.</param>
    /// <param name="status">The operation status.</param>
    /// <param name="nativeStatus">The optional host status code.</param>
    /// <param name="message">The optional host status message.</param>
    public CitrixVirtualChannelFeatureRegistration(string featureName, CitrixVirtualChannelStatus status, int nativeStatus, string message)
    {
        Throw.IfNull(featureName);
        FeatureName = featureName;
        Status = status;
        NativeStatus = nativeStatus;
        Message = message;
    }

    /// <summary>Gets the registered feature name.</summary>
    public string FeatureName { get; }

    /// <summary>Gets the operation status.</summary>
    public CitrixVirtualChannelStatus Status { get; }

    /// <summary>Gets the optional host status code.</summary>
    public int NativeStatus { get; }

    /// <summary>Gets the optional host status message.</summary>
    public string Message { get; }
}
