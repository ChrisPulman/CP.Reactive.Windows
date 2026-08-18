// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Display.Dpi;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Display.Dpi;
#endif
/// <summary>Default native DPI API implementation.</summary>
internal sealed class WindowsNativeDpiApi : INativeDpiApi
{
    /// <summary>The active native operation implementation.</summary>
    private static INativeDpiOperations _operations = NativeDpiOperations.Shared;

    /// <summary>Gets the singleton native DPI API.</summary>
    internal static WindowsNativeDpiApi Instance { get; } = new();

    /// <inheritdoc />
    public IDisposable DefaultScopedThreadDpiAwarenessContext() => _operations.DefaultScopedThreadDpiAwarenessContext();

    /// <inheritdoc />
    public HResult EnableNonClientDpiScaling(IntPtr windowHandle) => _operations.EnableNonClientDpiScaling(windowHandle);

    /// <inheritdoc />
    public int GetDpi(IntPtr windowHandle) => _operations.GetDpi(windowHandle);

    /// <inheritdoc />
    public int GetDpi(NativePoint location) => _operations.GetDpi(location);

    /// <inheritdoc />
    public uint GetDpiForSystem() => _operations.GetDpiForSystem();

    /// <inheritdoc />
    public int GetSystemMetricsForDpi(SystemMetric index, uint dpi) => _operations.GetSystemMetricsForDpi(index, dpi);

    /// <inheritdoc />
    public bool AdjustWindowRectExForDpi(
        ref NativeRect rect,
        WindowStyleFlags style,
        bool hasMenu,
        ExtendedWindowStyleFlags extendedStyle,
        uint dpi) =>
        _operations.AdjustWindowRectExForDpi(ref rect, style, hasMenu, extendedStyle, dpi);

    /// <inheritdoc />
    public bool SystemParametersInfoForDpi(
        SystemParametersInfoActions action,
        uint uiParameter,
        IntPtr parameter,
        SystemParametersInfoBehaviors updateProfileFlags,
        uint dpi) =>
        _operations.SystemParametersInfoForDpi(action, uiParameter, parameter, updateProfileFlags, dpi);

    /// <summary>Exchanges native operation delegates for deterministic testing.</summary>
    /// <param name="operations">The replacement operations.</param>
    /// <returns>The previous operations.</returns>
    internal static INativeDpiOperations ExchangeOperations(INativeDpiOperations operations)
    {
        Throw.IfNull(operations);
        var operations2 = _operations;
        _operations = operations;
        return operations2;
    }

    /// <summary>Default native DPI operation implementation.</summary>
    private sealed class NativeDpiOperations : INativeDpiOperations
    {
        /// <summary>Gets the singleton native operation implementation.</summary>
        internal static NativeDpiOperations Shared { get; } = new();

        /// <inheritdoc />
        public IDisposable DefaultScopedThreadDpiAwarenessContext() =>
            NativeDpiMethods.DefaultScopedThreadDpiAwarenessContextCore();

        /// <inheritdoc />
        public HResult EnableNonClientDpiScaling(IntPtr windowHandle) =>
            NativeDpiMethods.EnableNonClientDpiScalingCore(windowHandle);

        /// <inheritdoc />
        public int GetDpi(IntPtr windowHandle) => NativeDpiMethods.GetDpiCore(windowHandle);

        /// <inheritdoc />
        public int GetDpi(NativePoint location) => NativeDpiMethods.GetDpiCore(location);

        /// <inheritdoc />
        public uint GetDpiForSystem() => NativeDpiMethods.NativeMethods.GetDpiForSystem();

        /// <inheritdoc />
        public int GetSystemMetricsForDpi(SystemMetric index, uint dpi) =>
            NativeDpiMethods.NativeMethods.GetSystemMetricsForDpi(index, dpi);

        /// <inheritdoc />
        public bool AdjustWindowRectExForDpi(
            ref NativeRect rect,
            WindowStyleFlags style,
            bool hasMenu,
            ExtendedWindowStyleFlags extendedStyle,
            uint dpi) =>
            NativeDpiMethods.AdjustWindowRectExForDpiCore(ref rect, style, hasMenu, extendedStyle, dpi);

        /// <inheritdoc />
        public bool SystemParametersInfoForDpi(
            SystemParametersInfoActions action,
            uint uiParameter,
            IntPtr parameter,
            SystemParametersInfoBehaviors updateProfileFlags,
            uint dpi) =>
            NativeDpiMethods.NativeMethods.SystemParametersInfoForDpi(
                action,
                uiParameter,
                parameter,
                updateProfileFlags,
                dpi);
    }
}
