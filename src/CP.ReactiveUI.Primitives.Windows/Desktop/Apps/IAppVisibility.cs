// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Apps;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Apps;
#endif
/// <summary>COM contract used on Windows 8 and later to inspect Windows Store app visibility.</summary>
#if NET
[GeneratedComInterface]
[Guid("2246EA2D-CAEA-4444-A3C4-6DE827E44313")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IAppVisibility
#else
[ComImport]
[Guid("2246EA2D-CAEA-4444-A3C4-6DE827E44313")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAppVisibility
#endif
{
    /// <summary>Gets app visibility on the supplied monitor.</summary>
    /// <param name="monitorHandle">Monitor handle to inspect.</param>
    /// <returns>The monitor app visibility state.</returns>
    [PreserveSig]
    MonitorAppVisibility GetAppVisibilityOnMonitor(IntPtr monitorHandle);

    /// <summary>Gets a value indicating whether the app launcher is visible.</summary>
    /// <returns>true when the launcher is visible.</returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool IsLauncherVisible();
}
