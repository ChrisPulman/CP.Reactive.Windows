// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Represents the result of the managed CCMGetSessionInformation wrapper.</summary>
/// <param name="SessionId">The CCM session identifier supplied to the operation.</param>
/// <param name="ResultCode">The CCM result code returned by the SDK operation.</param>
/// <param name="SessionInformation">The immutable session information returned by the SDK.</param>
/// <param name="HasSessionInformation">A value indicating whether session information was materialized.</param>
public readonly record struct CcmHostSessionInformationResult(
    int SessionId,
    int ResultCode,
    CcmHostSessionInformation SessionInformation,
    bool HasSessionInformation)
{
    /// <summary>Gets a value indicating whether the CCM operation succeeded.</summary>
    public bool Succeeded => ResultCode == CcmHostResultCodes.Success && HasSessionInformation;
}
