// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.Host;

/// <summary>Immutable managed representation of a Citrix CCM ICA session.</summary>
/// <param name="SessionId">The CCM session identifier.</param>
/// <param name="ConnectionId">The CCM connection identifier.</param>
/// <param name="FriendlyName">The user-facing session name.</param>
/// <param name="NonSeamlessAppTitle">The non-seamless application title.</param>
/// <param name="IsFullScreen">A value indicating whether the session is full-screen.</param>
/// <param name="Ssl">A value indicating whether SSL is enabled.</param>
/// <param name="EncryptionLevel">The session encryption level.</param>
/// <param name="EngineVersion">The ICA engine version.</param>
/// <param name="ServerName">The server name.</param>
/// <param name="UserName">The user name.</param>
/// <param name="DomainName">The domain name.</param>
/// <param name="RxFrameCount">The received frame count.</param>
/// <param name="TxFrameCount">The transmitted frame count.</param>
/// <param name="RxByteCount">The received byte count.</param>
/// <param name="TxByteCount">The transmitted byte count.</param>
/// <param name="RxFrameErrorCount">The received frame error count.</param>
/// <param name="TxFrameErrorCount">The transmitted frame error count.</param>
/// <param name="SeamlessMode">A value indicating whether seamless mode is enabled.</param>
/// <param name="ZlMode">A value indicating whether zero latency mode is enabled.</param>
/// <param name="Cgp">A value indicating whether Common Gateway Protocol is enabled.</param>
/// <param name="SpeedBrowseEnabled">A value indicating whether SpeedBrowse is enabled.</param>
/// <param name="LastLatency">The last latency value.</param>
/// <param name="AverageLatency">The average latency value.</param>
/// <param name="RoundTripDeviation">The round-trip deviation value.</param>
/// <param name="HorizontalResolution">The horizontal resolution.</param>
/// <param name="VerticalResolution">The vertical resolution.</param>
/// <param name="ColorDepth">The color depth.</param>
/// <param name="AudioEnabled">A value indicating whether audio is enabled.</param>
/// <param name="PdaEnabled">A value indicating whether PDA redirection is enabled.</param>
/// <param name="TwnEnabled">A value indicating whether TWAIN redirection is enabled.</param>
/// <param name="PnpEnabled">A value indicating whether Plug and Play redirection is enabled.</param>
public readonly record struct CcmHostSessionInformation(
    int SessionId,
    int ConnectionId,
    string FriendlyName,
    string NonSeamlessAppTitle,
    bool IsFullScreen,
    bool Ssl,
    string EncryptionLevel,
    string EngineVersion,
    string ServerName,
    string UserName,
    string DomainName,
    uint RxFrameCount,
    uint TxFrameCount,
    uint RxByteCount,
    uint TxByteCount,
    uint RxFrameErrorCount,
    uint TxFrameErrorCount,
    bool SeamlessMode,
    bool ZlMode,
    bool Cgp,
    bool SpeedBrowseEnabled,
    uint LastLatency,
    uint AverageLatency,
    uint RoundTripDeviation,
    uint HorizontalResolution,
    uint VerticalResolution,
    uint ColorDepth,
    bool AudioEnabled,
    bool PdaEnabled,
    bool TwnEnabled,
    bool PnpEnabled);
