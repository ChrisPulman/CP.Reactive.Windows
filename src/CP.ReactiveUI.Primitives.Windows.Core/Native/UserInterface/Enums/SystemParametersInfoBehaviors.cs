// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>
/// If a system parameter is being set, specifies whether the user profile is to be updated, and if so, whether the WM_SETTINGCHANGE message is to be broadcast to
/// all top-level windows to notify them of the change. This parameter can be zero if you do not want to update the user profile or broadcast the WM_SETTINGCHANGE
/// message, or it can be one or more of the following values.
/// </summary>
public enum SystemParametersInfoBehaviors : uint
{
    /// <summary>Do nothing.</summary>
    None = 0U,

    /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
    UpdateIniFile = 1U,

    /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
    SendChange = 2U,

    /// <summary>Same as SPIF_SENDCHANGE.</summary>
    SendWinIniChange = SendChange,
}
