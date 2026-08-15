// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;

/// <summary>
/// The TOKEN_INFORMATION_CLASS enumeration contains values that specify the type of information being assigned to or retrieved from an access token.
/// The GetTokenInformation function uses these values to indicate the type of token information to retrieve.
/// The SetTokenInformation function uses these values to set the token information.
/// </summary>
public enum TokenInformationClasses
{
    /// <summary>No token information class.</summary>
    None,
    /// <summary>The buffer receives a TOKEN_USER structure that contains the user account of the token.</summary>
    TokenUser,
    /// <summary>The buffer receives a TOKEN_GROUPS structure that contains the group accounts associated with the token.</summary>
    TokenGroups,
    /// <summary>The buffer receives a TOKEN_PRIVILEGES structure that contains the privileges of the token.</summary>
    TokenPrivileges,
    /// <summary>The buffer receives a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects.</summary>
    TokenOwner,
    /// <summary>The buffer receives a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects.</summary>
    TokenPrimaryGroup,
    /// <summary>The buffer receives a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects.</summary>
    TokenDefaultDacl,
    /// <summary>The buffer receives a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information.</summary>
    TokenSource,
    /// <summary>The buffer receives a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token.</summary>
    TokenType,
    /// <summary>The buffer receives a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token.</summary>
    TokenImpersonationLevel,
    /// <summary>The buffer receives a TOKEN_STATISTICS structure that contains various token statistics.</summary>
    TokenStatistics,
    /// <summary>The buffer receives a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token.</summary>
    TokenRestrictedSids,
    /// <summary>The buffer receives the Terminal Services session identifier associated with the token.</summary>
    TokenSessionId,
    /// <summary>The buffer receives a TOKEN_GROUPS_AND_PRIVILEGES structure.</summary>
    TokenGroupsAndPrivileges,
    /// <summary>The Reserved value.</summary>
    TokenSessionReference,
    /// <summary>The buffer receives a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag.</summary>
    TokenSandBoxInert,
    /// <summary>The Reserved value.</summary>
    TokenAuditPolicy,
    /// <summary>The buffer receives a TOKEN_ORIGIN value.</summary>
    TokenOrigin,
    /// <summary>The buffer receives a TOKEN_ELEVATION_TYPE value that specifies the elevation level of the token.</summary>
    TokenElevationType,
    /// <summary>The buffer receives a TOKEN_LINKED_TOKEN structure that contains a handle to another token that is linked to this token.</summary>
    TokenLinkedToken,
    /// <summary>The buffer receives a TOKEN_ELEVATION structure that specifies whether the token is elevated.</summary>
    TokenElevation,
    /// <summary>The buffer receives a DWORD value that is nonzero if the token has ever been filtered.</summary>
    TokenHasRestrictions,
    /// <summary>The buffer receives a TOKEN_ACCESS_INFORMATION structure that specifies security information contained in the token.</summary>
    TokenAccessInformation,
    /// <summary>The buffer receives a DWORD value that is nonzero if virtualization is allowed for the token.</summary>
    TokenVirtualizationAllowed,
    /// <summary>The buffer receives a DWORD value that is nonzero if virtualization is enabled for the token.</summary>
    TokenVirtualizationEnabled,
    /// <summary>The buffer receives a TOKEN_MANDATORY_LABEL structure that specifies the token's integrity level.</summary>
    TokenIntegrityLevel,
    /// <summary>The buffer receives a DWORD value that is nonzero if the token has the UIAccess flag set.</summary>
    TokenUiAccess,
    /// <summary>The buffer receives a TOKEN_MANDATORY_POLICY structure that specifies the token's mandatory integrity policy.</summary>
    TokenMandatoryPolicy,
    /// <summary>The buffer receives a TOKEN_GROUPS structure that specifies the token's logon SID.</summary>
    TokenLogonSid,
    /// <summary>The buffer receives a DWORD value that is nonzero if the token is an app container token.</summary>
    TokenIsAppContainer,
    /// <summary>The buffer receives a TOKEN_GROUPS structure that contains the capabilities associated with the token.</summary>
    TokenCapabilities,
    /// <summary>The buffer receives a TOKEN_APPCONTAINER_INFORMATION structure.</summary>
    TokenAppContainerSid,
    /// <summary>The buffer receives a DWORD value that includes the app container number for the token. For tokens that are not app container tokens, this value is zero.</summary>
    TokenAppContainerNumber,
    /// <summary>The buffer receives a CLAIM_SECURITY_ATTRIBUTES_INFORMATION structure that contains the user claims associated with the token.</summary>
    TokenUserClaimAttributes,
    /// <summary>The buffer receives a CLAIM_SECURITY_ATTRIBUTES_INFORMATION structure that contains the device claims associated with the token.</summary>
    TokenDeviceClaimAttributes,
    /// <summary>This value is reserved.</summary>
    TokenRestrictedUserClaimAttributes,
    /// <summary>This value is reserved.</summary>
    TokenRestrictedDeviceClaimAttributes,
    /// <summary>The buffer receives a TOKEN_GROUPS structure that contains the device groups that are associated with the token.</summary>
    TokenDeviceGroups,
    /// <summary>The buffer receives a TOKEN_GROUPS structure that contains the restricted device groups that are associated with the token.</summary>
    TokenRestrictedDeviceGroups,
    /// <summary>This value is reserved.</summary>
    TokenSecurityAttributes,
    /// <summary>This value is reserved.</summary>
    TokenIsRestricted,
    /// <summary>The maximum value for this enumeration.</summary>
    MaxTokenInfoClass
}
