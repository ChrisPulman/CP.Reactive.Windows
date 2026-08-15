// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>
///     Callback function used by RmShutdown and RmRestart to report status updates.
///     See <a href="https://docs.microsoft.com/en-us/windows/win32/api/restartmanager/nc-restartmanager-rm_write_status_callback">RM_WRITE_STATUS_CALLBACK</a>.
/// </summary>
/// <param name="percentComplete">An integer value between 0 and 100 that indicates the completed percentage.</param>
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
public delegate void RmStatusCallback(uint percentComplete);
