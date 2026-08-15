// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Defines behavior flags for the SendMessageTimeout function.</summary>
[Flags]
public enum SendMessageTimeoutFlags : uint
{
    /// <summary>The calling thread is not prevented from processing other requests while waiting for the function to return.</summary>
    None = 0U,

    /// <summary>Prevents the calling thread from processing any other requests until the function returns.</summary>
    Block = 1U,

    /// <summary>The function returns without waiting for the time-out period to elapse if the receiving thread appears to not respond or "hangs.".</summary>
    AbortIfHung = 2U,

    /// <summary>The function does not enforce the time-out period as long as the receiving thread is processing messages.</summary>
    NoTimeoutIfNotHung = 8U,

    /// <summary>The function should return 0 if the receiving window is destroyed or its owning thread dies while the message is being processed.</summary>
    ErrorOnExit = 0x20U,
}
