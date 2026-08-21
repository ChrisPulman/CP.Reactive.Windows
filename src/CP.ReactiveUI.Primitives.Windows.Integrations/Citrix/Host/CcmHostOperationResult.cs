// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Represents the result of a Citrix CCM host-session operation.</summary>
/// <param name="SessionId">The CCM session identifier supplied to the operation.</param>
/// <param name="ResultCode">The CCM result code returned by the SDK operation.</param>
public readonly record struct CcmHostOperationResult(
    int SessionId,
    int ResultCode)
{
    /// <summary>Gets a value indicating whether the CCM operation succeeded.</summary>
    public bool Succeeded => ResultCode == CcmHostResultCodes.Success;
}
