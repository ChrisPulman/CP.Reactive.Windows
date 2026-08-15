// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>Thread access rights.</summary>
[Flags]
public enum ThreadAccess
{
    /// <summary>The TERMINATE value.</summary>
    None = 0,
    /// <summary>The TERMINATE value.</summary>
    TERMINATE = 1,
    /// <summary>SUSPEND RESUME.</summary>
    SUSPEND_RESUME = 2,
    /// <summary>GET CONTEXT.</summary>
    GET_CONTEXT = 8,
    /// <summary>SET CONTEXT.</summary>
    SET_CONTEXT = 0x10,
    /// <summary>SET INFORMATION.</summary>
    SET_INFORMATION = 0x20,
    /// <summary>QUERY INFORMATION.</summary>
    QUERY_INFORMATION = 0x40,
    /// <summary>SET THREAD TOKEN.</summary>
    SET_THREAD_TOKEN = 0x80,
    /// <summary>The IMPERSONATE value.</summary>
    IMPERSONATE = 0x100,
    /// <summary>DIRECT IMPERSONATION.</summary>
    DIRECT_IMPERSONATION = 0x200
}
