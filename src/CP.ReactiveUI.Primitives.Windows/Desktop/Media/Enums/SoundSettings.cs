// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Media.Enums;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Media.Enums;
#endif
/// <summary>See <a href="http://msdn.microsoft.com/en-us/library/aa909766.aspx">PlaySound</a>.</summary>
[Flags]
public enum SoundSettings : uint
{
    /// <summary>Default: Synchronous playback of a sound event. PlaySound returns after the sound event completes.</summary>
    None = 0U,
    /// <summary>The sound is played asynchronously and PlaySound returns immediately after beginning the sound.</summary>
    Async = 1U,
    /// <summary>No default sound event is used.</summary>
    NoDefault = 2U,
    /// <summary>The pszSound parameter points to a sound loaded in memory.</summary>
    Memory = 4U,
    /// <summary>The sound plays repeatedly until PlaySound is called again with the pszSound parameter set to NULL.</summary>
    Loop = 8U,
    /// <summary>The specified sound event will yield to another sound event that is already playing.</summary>
    NoStop = 0x10U,
    /// <summary>Not supported.</summary>
    NoWait = 0x2000U,
    /// <summary>Not supported.</summary>
    Purge = 0x40U,
    /// <summary>The pszSound parameter is a system-event alias in the registry or the WIN.INI file.</summary>
    Alias = 0x10000U,
    /// <summary>The pszSound parameter is a file name.</summary>
    Filename = 0x20000U,
    /// <summary>The resource type selector bit used by SND_RESOURCE.</summary>
    ResourceType = 0x40000U,
    /// <summary>If this flag is set, the function triggers a SoundSentry event when the sound is played.</summary>
    Sentry = 0x80000U,
    /// <summary>The predefined identifier selector bit used by SND_ALIAS_ID.</summary>
    AliasIdentifier = 0x100000U,
    /// <summary>If this flag is set, the sound is assigned to the audio session for system notification sounds.</summary>
    System = 0x200000U,
    /// <summary>The pszSound parameter is a predefined id.</summary>
    AliasId = Alias | AliasIdentifier,
    /// <summary>The pszSound parameter is a resource identifier.</summary>
    Resource = Memory | ResourceType,
}
