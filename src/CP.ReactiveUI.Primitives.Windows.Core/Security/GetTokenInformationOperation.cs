// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Security;

/// <summary>Retrieves information about an access token.</summary>
/// <param name="tokenHandle">Access token handle.</param>
/// <param name="tokenInformationClasses">Token information class.</param>
/// <param name="tokenInformation">Output token information buffer.</param>
/// <param name="tokenInformationLength">Output token information buffer length.</param>
/// <param name="returnLength">Required or written buffer length.</param>
/// <returns>True on success; otherwise, false.</returns>
internal delegate bool GetTokenInformationOperation(
    nint tokenHandle,
    TokenInformationClasses tokenInformationClasses,
    nint tokenInformation,
    int tokenInformationLength,
    out int returnLength);
