// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Win32;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Composition;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Composition;
#endif
/// <summary>DwmApi Utils class.</summary>
public static partial class DwmApi
{
    /// <summary>The composition action value that disables composition.</summary>
    private const uint DwmEcDisableComposition = 0U;

    /// <summary>The composition action value that enables composition.</summary>
    private const uint DwmEcEnableComposition = 1U;

    /// <summary>The size, in bytes, of a DWORD DWM window attribute value.</summary>
    private const int DwordSize = sizeof(uint);

    /// <summary>The registry key containing the DWM colorization color.</summary>
    private const string ColorizationColorKey = "SOFTWARE\\Microsoft\\Windows\\DWM";

    /// <summary>Desktop Window Manager native methods.</summary>
    private static class DwmNativeMethods
    {
        /// <summary>The Desktop Window Manager native library name.</summary>
        private const string DwmApiDll = "dwmapi.dll";

        /// <summary>The HRESULT returned when an optional DWM export is unavailable.</summary>
        private const int HResultNotSupported = unchecked((int)0x80070032);

        /// <summary>The loaded Desktop Window Manager module.</summary>
        private static readonly IntPtr DwmApiModule = NativeLibrary.Load(Path.Combine(Environment.SystemDirectory, DwmApiDll));

        /// <summary>The DwmEnableBlurBehindWindow export.</summary>
        private static readonly IntPtr DwmEnableBlurBehindWindowExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmEnableBlurBehindWindow));

        /// <summary>The DwmGetWindowAttribute export.</summary>
        private static readonly IntPtr DwmGetWindowAttributeExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmGetWindowAttribute));

        /// <summary>The DwmQueryThumbnailSourceSize export.</summary>
        private static readonly IntPtr DwmQueryThumbnailSourceSizeExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmQueryThumbnailSourceSize));

        /// <summary>The DwmSetIconicLivePreviewBitmap export.</summary>
        private static readonly IntPtr DwmSetIconicLivePreviewBitmapExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmSetIconicLivePreviewBitmap));

        /// <summary>The DwmUpdateThumbnailProperties export.</summary>
        private static readonly IntPtr DwmUpdateThumbnailPropertiesExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmUpdateThumbnailProperties));

        /// <summary>The DwmIsCompositionEnabled export.</summary>
        private static readonly IntPtr DwmIsCompositionEnabledExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmIsCompositionEnabled));

        /// <summary>The DwmRegisterThumbnail export.</summary>
        private static readonly IntPtr DwmRegisterThumbnailExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmRegisterThumbnail));

        /// <summary>The DwmSetWindowAttribute export.</summary>
        private static readonly IntPtr DwmSetWindowAttributeExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmSetWindowAttribute));

        /// <summary>The DwmUnregisterThumbnail export.</summary>
        private static readonly IntPtr DwmUnregisterThumbnailExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmUnregisterThumbnail));

        /// <summary>The DwmpActivateLivePreview export.</summary>
        private static IntPtr _dwmpActivateLivePreviewExport = TryGetExport("#113");

        /// <summary>The DwmEnableComposition export.</summary>
        private static IntPtr _dwmEnableCompositionExport = NativeLibrary.GetExport(DwmApiModule, nameof(DwmEnableComposition));

        /// <summary>The DwmpStartOrStopFlip3D export.</summary>
        private static IntPtr _dwmpStartOrStopFlip3DExport = TryGetExport("#105");

        /// <summary>The shared surface export.</summary>
        private static IntPtr _getSharedSurfaceExport = TryGetExport("#100");

        /// <summary>The shared window update export.</summary>
        private static IntPtr _updateWindowSharedExport = TryGetExport("#101");

        /// <summary>Overrides optional DWM exports while the returned scope is alive.</summary>
        /// <param name="enableCompositionExport">The DwmEnableComposition export.</param>
        /// <param name="flip3DExport">The DwmpStartOrStopFlip3D export.</param>
        /// <param name="getSharedSurfaceExport">The GetSharedSurface export.</param>
        /// <param name="updateWindowSharedExport">The UpdateWindowShared export.</param>
        /// <param name="activateLivePreviewExport">The DwmpActivateLivePreview export.</param>
        /// <returns>A scope that restores the original exports.</returns>
        internal static IDisposable OverrideOptionalExportsForTesting(
            IntPtr enableCompositionExport,
            IntPtr flip3DExport,
            IntPtr getSharedSurfaceExport,
            IntPtr updateWindowSharedExport,
            IntPtr activateLivePreviewExport)
        {
            var previous = new OptionalExports(
                _dwmEnableCompositionExport,
                _dwmpStartOrStopFlip3DExport,
                _getSharedSurfaceExport,
                _updateWindowSharedExport,
                _dwmpActivateLivePreviewExport);
            _dwmEnableCompositionExport = enableCompositionExport;
            _dwmpStartOrStopFlip3DExport = flip3DExport;
            _getSharedSurfaceExport = getSharedSurfaceExport;
            _updateWindowSharedExport = updateWindowSharedExport;
            _dwmpActivateLivePreviewExport = activateLivePreviewExport;
            return new OptionalExportsOverride(previous);
        }

        /// <summary>Enables the blur effect on a specified window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="blurBehind">The blur-behind configuration.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmEnableBlurBehindWindow(IntPtr windowHandle, ref DwmBlurBehind blurBehind)
        {
            var nativeBlurBehind = blurBehind.ToNative();
            return ((delegate* unmanaged[Stdcall]<IntPtr, NativeDwmBlurBehind*, HResult>)(void*)DwmEnableBlurBehindWindowExport)(windowHandle, &nativeBlurBehind);
        }

        /// <summary>Enables or disables DWM composition.</summary>
        /// <param name="compositionAction">The composition action.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmEnableComposition(uint compositionAction) =>
            ((delegate* unmanaged[Stdcall]<uint, HResult>)(void*)_dwmEnableCompositionExport)(
                compositionAction);

        /// <summary>Retrieves a window rectangle attribute.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="attribute">The window attribute.</param>
        /// <param name="rectangle">The retrieved rectangle.</param>
        /// <param name="size">The attribute size.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmGetWindowAttribute(IntPtr windowHandle, DwmWindowAttributes attribute, out NativeRect rectangle, int size)
        {
            delegate* unmanaged[Stdcall]<IntPtr, DwmWindowAttributes, void*, int, HResult>
                getWindowAttribute =
                    (delegate* unmanaged[Stdcall]<IntPtr, DwmWindowAttributes, void*, int, HResult>)
                    (void*)DwmGetWindowAttributeExport;
            fixed (NativeRect* rectanglePointer = &rectangle)
            {
                return getWindowAttribute(windowHandle, attribute, rectanglePointer, size);
            }
        }

        /// <summary>Retrieves a window Boolean attribute.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="attribute">The window attribute.</param>
        /// <param name="value">The retrieved value.</param>
        /// <param name="size">The attribute size.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmGetWindowAttribute(IntPtr windowHandle, DwmWindowAttributes attribute, out bool value, int size)
        {
            Unsafe.SkipInit<int>(out var nativeValue);
            var result =
                ((delegate* unmanaged[Stdcall]<IntPtr, DwmWindowAttributes, int*, int, HResult>)
                    (void*)DwmGetWindowAttributeExport)(windowHandle, attribute, &nativeValue, size);
            value = nativeValue != 0;
            return result;
        }

        /// <summary>Retrieves a window unsigned integer attribute.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="attribute">The window attribute.</param>
        /// <param name="value">The retrieved value.</param>
        /// <param name="size">The attribute size.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmGetWindowAttribute(IntPtr windowHandle, DwmWindowAttributes attribute, out uint value, int size)
        {
            value = 0U;
            fixed (uint* valuePointer = &value)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, DwmWindowAttributes, uint*, int, HResult>)
                    (void*)DwmGetWindowAttributeExport)(windowHandle, attribute, valuePointer, size);
            }
        }

        /// <summary>Gets whether DWM composition is enabled.</summary>
        /// <param name="enabled">The enabled state.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmIsCompositionEnabled(out bool enabled)
        {
            Unsafe.SkipInit<int>(out var nativeEnabled);
            var result = ((delegate* unmanaged[Stdcall]<int*, HResult>)(void*)DwmIsCompositionEnabledExport)(&nativeEnabled);
            enabled = nativeEnabled != 0;
            return result;
        }

        /// <summary>Activates Aero Peek live preview.</summary>
        /// <param name="active">The active state.</param>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="onTopHandle">The topmost window handle.</param>
        /// <param name="unknown">The undocumented option value.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmpActivateLivePreview(
            uint active,
            IntPtr windowHandle,
            IntPtr onTopHandle,
            uint unknown) =>
            _dwmpActivateLivePreviewExport == IntPtr.Zero
                ? HResult.NotSupported
                : ((delegate* unmanaged[Stdcall]<uint, IntPtr, IntPtr, uint, HResult>)
                    (void*)_dwmpActivateLivePreviewExport)(active, windowHandle, onTopHandle, unknown);

        /// <summary>Starts or stops Flip3D.</summary>
        /// <returns><see langword="true" /> when the native call succeeds.</returns>
        internal static unsafe bool DwmpStartOrStopFlip3D() =>
            _dwmpStartOrStopFlip3DExport != IntPtr.Zero
                && ((delegate* unmanaged[Stdcall]<int>)(void*)_dwmpStartOrStopFlip3DExport)() != 0;

        /// <summary>Retrieves the source size of a DWM thumbnail.</summary>
        /// <param name="thumbnailHandle">The thumbnail handle.</param>
        /// <param name="size">The retrieved size.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmQueryThumbnailSourceSize(IntPtr thumbnailHandle, out NativeSize size)
        {
            delegate* unmanaged[Stdcall]<IntPtr, NativeSize*, HResult>
                queryThumbnailSourceSize =
                    (delegate* unmanaged[Stdcall]<IntPtr, NativeSize*, HResult>)
                    (void*)DwmQueryThumbnailSourceSizeExport;
            fixed (NativeSize* sizePointer = &size)
            {
                return queryThumbnailSourceSize(thumbnailHandle, sizePointer);
            }
        }

        /// <summary>Registers a DWM thumbnail relationship.</summary>
        /// <param name="destinationWindowHandle">The destination window handle.</param>
        /// <param name="sourceWindowHandle">The source window handle.</param>
        /// <param name="thumbnailId">The thumbnail identifier.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmRegisterThumbnail(IntPtr destinationWindowHandle, IntPtr sourceWindowHandle, out IntPtr thumbnailId)
        {
            thumbnailId = default;
            fixed (IntPtr* thumbnailIdPointer = &thumbnailId)
            {
                return ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr*, HResult>)
                    (void*)DwmRegisterThumbnailExport)(
                    destinationWindowHandle,
                    sourceWindowHandle,
                    thumbnailIdPointer);
            }
        }

        /// <summary>Sets an iconic live preview bitmap.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="bitmapHandle">The bitmap handle.</param>
        /// <param name="clientPoint">The client point.</param>
        /// <param name="setIconicLivePreviewFlags">The live preview flags.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmSetIconicLivePreviewBitmap(
            IntPtr windowHandle,
            IntPtr bitmapHandle,
            ref NativePoint clientPoint,
            DwmSetIconicLivePreviewFlags setIconicLivePreviewFlags)
        {
            delegate* unmanaged[Stdcall]<IntPtr, IntPtr, NativePoint*, DwmSetIconicLivePreviewFlags, HResult>
                setIconicLivePreviewBitmap =
                    (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, NativePoint*, DwmSetIconicLivePreviewFlags, HResult>)
                    (void*)DwmSetIconicLivePreviewBitmapExport;
            fixed (NativePoint* clientPointPointer = &clientPoint)
            {
                return setIconicLivePreviewBitmap(windowHandle, bitmapHandle, clientPointPointer, setIconicLivePreviewFlags);
            }
        }

        /// <summary>Sets a DWM window attribute.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="attribute">The window attribute.</param>
        /// <param name="attributeValue">The attribute value pointer.</param>
        /// <param name="attributeSize">The attribute size.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmSetWindowAttribute(
            IntPtr windowHandle,
            DwmWindowAttributes attribute,
            IntPtr attributeValue,
            int attributeSize) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, DwmWindowAttributes, IntPtr, int, HResult>)
                (void*)DwmSetWindowAttributeExport)(windowHandle, attribute, attributeValue, attributeSize);

        /// <summary>Unregisters a DWM thumbnail relationship.</summary>
        /// <param name="thumbnailId">The thumbnail identifier.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmUnregisterThumbnail(IntPtr thumbnailId) =>
            ((delegate* unmanaged[Stdcall]<IntPtr, HResult>)(void*)DwmUnregisterThumbnailExport)(thumbnailId);

        /// <summary>Updates the properties for a DWM thumbnail.</summary>
        /// <param name="thumbnailId">The thumbnail identifier.</param>
        /// <param name="props">The thumbnail properties.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe HResult DwmUpdateThumbnailProperties(IntPtr thumbnailId, ref DwmThumbnailProperties props)
        {
            var nativeProperties = props.ToNative();
            return ((delegate* unmanaged[Stdcall]<IntPtr, NativeDwmThumbnailProperties*, HResult>)
                (void*)DwmUpdateThumbnailPropertiesExport)(thumbnailId, &nativeProperties);
        }

        /// <summary>Gets a shared surface for the specified window.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="adapterLuid">The adapter LUID.</param>
        /// <param name="one">The first undocumented value.</param>
        /// <param name="two">The second undocumented value.</param>
        /// <param name="d3DFormat">The Direct3D format.</param>
        /// <param name="sharedHandle">The shared handle.</param>
        /// <param name="unknown">The undocumented option value.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe int GetSharedSurface(IntPtr windowHandle, long adapterLuid, uint one, uint two, ref uint d3DFormat, out IntPtr sharedHandle, ulong unknown)
        {
            sharedHandle = default;
            if (_getSharedSurfaceExport == IntPtr.Zero)
            {
                return HResultNotSupported;
            }

            fixed (uint* d3DFormatPointer = &d3DFormat)
            {
                fixed (IntPtr* sharedHandlePointer = &sharedHandle)
                {
                    return ((delegate* unmanaged[Stdcall]<IntPtr, long, uint, uint, uint*, IntPtr*, ulong, int>)
                        (void*)_getSharedSurfaceExport)(
                        windowHandle,
                        adapterLuid,
                        one,
                        two,
                        d3DFormatPointer,
                        sharedHandlePointer,
                        unknown);
                }
            }
        }

        /// <summary>Updates a shared window surface.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="one">The first undocumented value.</param>
        /// <param name="two">The second undocumented value.</param>
        /// <param name="three">The third undocumented value.</param>
        /// <param name="monitorHandle">The monitor handle.</param>
        /// <param name="unknown">The undocumented option value.</param>
        /// <returns>The operation result.</returns>
        internal static unsafe int UpdateWindowShared(
            IntPtr windowHandle,
            int one,
            int two,
            int three,
            IntPtr monitorHandle,
            IntPtr unknown) =>
            _updateWindowSharedExport == IntPtr.Zero
                ? HResultNotSupported
                : ((delegate* unmanaged[Stdcall]<IntPtr, int, int, int, IntPtr, IntPtr, int>)
                    (void*)_updateWindowSharedExport)(windowHandle, one, two, three, monitorHandle, unknown);

        /// <summary>Gets a DWM export when it exists on the current operating system.</summary>
        /// <param name="exportName">The export name or ordinal.</param>
        /// <returns>The export address, or <see cref="F:System.IntPtr.Zero" /> when unavailable.</returns>
        private static IntPtr TryGetExport(string exportName)
        {
            var found = NativeLibrary.TryGetExport(DwmApiModule, exportName, out var exportAddress);
            return ResolveOptionalExport(found, exportAddress);
        }

        /// <summary>Represents optional DWM export addresses.</summary>
        private readonly struct OptionalExports
        {
            /// <summary>The DwmEnableComposition export.</summary>
            private readonly IntPtr _enableComposition;

            /// <summary>The DwmpStartOrStopFlip3D export.</summary>
            private readonly IntPtr _flip3D;

            /// <summary>The GetSharedSurface export.</summary>
            private readonly IntPtr _getSharedSurface;

            /// <summary>The UpdateWindowShared export.</summary>
            private readonly IntPtr _updateWindowShared;

            /// <summary>The DwmpActivateLivePreview export.</summary>
            private readonly IntPtr _activateLivePreview;

            /// <summary>Initializes a new instance of the <see cref="OptionalExports"/> struct.</summary>
            /// <param name="enableComposition">The DwmEnableComposition export.</param>
            /// <param name="flip3D">The DwmpStartOrStopFlip3D export.</param>
            /// <param name="getSharedSurface">The GetSharedSurface export.</param>
            /// <param name="updateWindowShared">The UpdateWindowShared export.</param>
            /// <param name="activateLivePreview">The DwmpActivateLivePreview export.</param>
            internal OptionalExports(
                IntPtr enableComposition,
                IntPtr flip3D,
                IntPtr getSharedSurface,
                IntPtr updateWindowShared,
                IntPtr activateLivePreview)
            {
                _enableComposition = enableComposition;
                _flip3D = flip3D;
                _getSharedSurface = getSharedSurface;
                _updateWindowShared = updateWindowShared;
                _activateLivePreview = activateLivePreview;
            }

            /// <summary>Restores the captured export addresses.</summary>
            internal void Restore()
            {
                _dwmEnableCompositionExport = _enableComposition;
                _dwmpStartOrStopFlip3DExport = _flip3D;
                _getSharedSurfaceExport = _getSharedSurface;
                _updateWindowSharedExport = _updateWindowShared;
                _dwmpActivateLivePreviewExport = _activateLivePreview;
            }
        }

        /// <summary>Restores optional DWM export addresses.</summary>
        /// <param name="previous">The original optional exports.</param>
        private sealed class OptionalExportsOverride(OptionalExports previous) : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose() => previous.Restore();
        }
    }
}

/// <summary>Provides the public Desktop Window Manager API.</summary>
public static partial class DwmApi
{
    /// <summary>Provides the current DWM colorization value.</summary>
    private static Func<object> _colorizationValueProvider = ReadColorizationValue;

    /// <summary>Provides the Windows 8.x platform test.</summary>
    private static Func<bool> _isWindows8XProvider = static () => WindowsVersion.IsWindows8X;

    /// <summary>Provides the pre-Vista platform test.</summary>
    private static Func<bool> _isWindowsBeforeVistaProvider = static () => WindowsVersion.IsWindowsBeforeVista;

    /// <summary>Provides the Windows 8-or-later platform test.</summary>
    private static Func<bool> _isWindows8OrLaterProvider = static () => WindowsVersion.IsWindows8OrLater;

    /// <summary>Provides the Windows 11-or-later platform test.</summary>
    private static Func<bool> _isWindows11OrLaterProvider = static () => WindowsVersion.IsWindows11OrLater;

    /// <summary>Provides the DwmEnableComposition operation.</summary>
    private static Func<uint, HResult> _enableCompositionOperation = DwmNativeMethods.DwmEnableComposition;

    /// <summary>Provides the DwmpStartOrStopFlip3D operation.</summary>
    private static Func<bool> _startOrStopFlip3DOperation = DwmNativeMethods.DwmpStartOrStopFlip3D;

    /// <summary>Provides the GetSharedSurface operation.</summary>
    private static DwmGetSharedSurfaceOperation _getSharedSurfaceOperation = DwmNativeMethods.GetSharedSurface;

    /// <summary>Provides the UpdateWindowShared operation.</summary>
    private static DwmUpdateWindowSharedOperation _updateWindowSharedOperation = DwmNativeMethods.UpdateWindowShared;

    /// <summary>Provides the unsigned-integer DWM window-attribute operation.</summary>
    private static DwmGetUIntWindowAttributeOperation _getUIntWindowAttributeOperation = DwmNativeMethods.DwmGetWindowAttribute;

    /// <summary>Gets the Aero color.</summary>
    public static MediaColor ColorizationColor => ToMediaColor(ColorizationSystemDrawingColor);

    /// <summary>Gets the Aero drawing color.</summary>
    public static DrawingColor ColorizationDrawingColor => ColorizationSystemDrawingColor;

    /// <summary>Gets the Aero system drawing color.</summary>
    public static DrawingColor ColorizationSystemDrawingColor
    {
        get
        {
            var dwordValue = _colorizationValueProvider();
            return dwordValue is null ? DrawingColor.White : DrawingColor.FromArgb((int)dwordValue);
        }
    }

    /// <summary>Gets a value indicating whether DWM is available and active.</summary>
    public static bool IsDwmEnabled
    {
        get
        {
            if (_isWindows8XProvider())
            {
                return true;
            }

            if (_isWindowsBeforeVistaProvider())
            {
                return false;
            }

            _ = DwmIsCompositionEnabled(out var dwmEnabled);
            return dwmEnabled;
        }
    }

    /// <summary>Disables DWM composition.</summary>
    /// <returns><see langword="true" /> when composition is disabled.</returns>
    public static bool DisableComposition() => DwmEnableComposition(0U).Succeeded();

    /// <summary>
    /// See
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969508(v=vs.85).aspx">
    /// DwmEnableBlurBehindWindow
    /// function
    /// </a>
    /// Enables the blur effect on a specified window.
    /// </summary>
    /// <param name="windowHandle">The handle to the window on which the blur behind data is applied.</param>
    /// <param name="blurBehind">The blur-behind configuration.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmEnableBlurBehindWindow(
        IntPtr windowHandle,
        ref DwmBlurBehind blurBehind) =>
        DwmNativeMethods.DwmEnableBlurBehindWindow(windowHandle, ref blurBehind);

    /// <summary>
    /// See
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969510(v=vs.85).aspx">
    /// DwmEnableComposition
    /// function
    /// </a>
    /// As of Windows 8, calling this function with DWM_EC_DISABLECOMPOSITION has no effect. However, the function will
    /// still return a success code.
    /// </summary>
    /// <param name="compositionAction">
    /// DWM_EC_ENABLECOMPOSITION to enable DWM composition; DWM_EC_DISABLECOMPOSITION to
    /// disable composition.
    /// </param>
    /// <returns>The operation result.</returns>
    public static HResult DwmEnableComposition(uint compositionAction) => _enableCompositionOperation(compositionAction);

    /// <summary>
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969515(v=vs.85).aspx">DwmGetWindowAttribute function</a>
    /// Retrieves the current value of a specified attribute applied to a window.
    /// TODO: Currently only DWMWA_EXTENDED_FRAME_BOUNDS is supported, due to the type of lpRect.
    /// </summary>
    /// <param name="windowHandle">The handle to the window from which the attribute data is retrieved.</param>
    /// <param name="attribute">The attribute to retrieve, specified as a DwmWindowAttributes value.</param>
    /// <param name="rectangle">
    /// A pointer to a value that, when this function returns successfully, receives the current value of
    /// the attribute. The type of the retrieved value depends on the value of the dwAttribute parameter.
    /// </param>
    /// <param name="size">The size value.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmGetWindowAttribute(
        IntPtr windowHandle,
        DwmWindowAttributes attribute,
        out NativeRect rectangle,
        int size) =>
        DwmNativeMethods.DwmGetWindowAttribute(windowHandle, attribute, out rectangle, size);

    /// <summary>
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969515(v=vs.85).aspx">DwmGetWindowAttribute function</a>
    /// Retrieves the current value of a specified attribute applied to a window.
    /// </summary>
    /// <param name="windowHandle">The handle to the window from which the attribute data is retrieved.</param>
    /// <param name="attribute">The attribute to retrieve, specified as a DwmWindowAttributes value.</param>
    /// <param name="value">A pointer to a value that, when this function returns successfully, receives the current value of
    /// the attribute. The type of the retrieved value depends on the value of the dwAttribute parameter.
    /// </param>
    /// <param name="size">The size value.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmGetWindowAttribute(
        IntPtr windowHandle,
        DwmWindowAttributes attribute,
        out bool value,
        int size) =>
        DwmNativeMethods.DwmGetWindowAttribute(windowHandle, attribute, out value, size);

    /// <summary>
    /// See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969515(v=vs.85).aspx">DwmGetWindowAttribute function</a>
    /// Retrieves the current value of a specified attribute applied to a window.
    /// </summary>
    /// <param name="windowHandle">The handle to the window from which the attribute data is retrieved.</param>
    /// <param name="attribute">The attribute to retrieve, specified as a DwmWindowAttributes value.</param>
    /// <param name="value">A pointer to a value that, when this function returns successfully, receives the current value of
    /// the attribute. The type of the retrieved value depends on the value of the dwAttribute parameter.
    /// </param>
    /// <param name="size">The size value.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmGetWindowAttribute(
        IntPtr windowHandle,
        DwmWindowAttributes attribute,
        out uint value,
        int size) =>
        DwmNativeMethods.DwmGetWindowAttribute(windowHandle, attribute, out value, size);

    /// <summary>Activates the Windows+Tab effect.</summary>
    /// <returns><see langword="true" /> when the native call succeeds.</returns>
    public static bool DwmpStartOrStopFlip3D() => _startOrStopFlip3DOperation();

    /// <summary>Retrieves the source size of the Desktop Window Manager (DWM) thumbnail.</summary>
    /// <param name="thumbnailHandle">A handle to the thumbnail to retrieve the source window size from.</param>
    /// <param name="size">A NativeSize structure that receives the size of the source thumbnail.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmQueryThumbnailSourceSize(IntPtr thumbnailHandle, out NativeSize size) => DwmNativeMethods.DwmQueryThumbnailSourceSize(thumbnailHandle, out size);

    /// <summary>Creates a Desktop Window Manager (DWM) thumbnail relationship between the destination and source windows.</summary>
    /// <param name="destinationWindowHandle">
    /// The handle to the window that will use the DWM thumbnail. Setting the destination window handle to anything other
    /// than a top-level window type will result in a return value of E_INVALIDARG.
    /// </param>
    /// <param name="sourceWindowHandle">
    /// The handle to the window to use as the thumbnail source. Setting the source window handle to anything other than a
    /// top-level window type will result in a return value of E_INVALIDARG.
    /// </param>
    /// <param name="thumbnailId">A handle representing the DWM thumbnail registration.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmRegisterThumbnail(
        IntPtr destinationWindowHandle,
        IntPtr sourceWindowHandle,
        out IntPtr thumbnailId) =>
        DwmNativeMethods.DwmRegisterThumbnail(destinationWindowHandle, sourceWindowHandle, out thumbnailId);

    /// <summary>
    /// Sets the value of non-client rendering attributes for a window.
    /// See
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969524(v=vs.85).aspx">
    /// DwmSetWindowAttribute
    /// function
    /// </a>
    /// </summary>
    /// <param name="windowHandle">The window handle that will receive the attributes.</param>
    /// <param name="attributeToSet">
    /// A single DWMWINDOWATTRIBUTE flag to apply to the window. This parameter specifies the
    /// attribute and the pvAttribute parameter points to the value of that attribute.
    /// </param>
    /// <param name="attributeValue">
    /// A pointer to the value of the attribute specified in the dwAttribute parameter.
    /// Different DWMWINDOWATTRIBUTE flags require different value types.
    /// </param>
    /// <param name="attributeSize">The size, in bytes, of the value type pointed to by the attribute value parameter.</param>
    /// <returns>The result.</returns>
    public static HResult DwmSetWindowAttribute(
        IntPtr windowHandle,
        DwmWindowAttributes attributeToSet,
        IntPtr attributeValue,
        int attributeSize) =>
        DwmNativeMethods.DwmSetWindowAttribute(windowHandle, attributeToSet, attributeValue, attributeSize);

    /// <summary>Removes a Desktop Window Manager (DWM) thumbnail relationship created by the DwmRegisterThumbnail function.</summary>
    /// <param name="thumbnailId">
    /// The handle to the thumbnail relationship to be removed. Null or non-existent handles will result in a return value
    /// of E_INVALIDARG.
    /// </param>
    /// <returns>The operation result.</returns>
    public static HResult DwmUnregisterThumbnail(IntPtr thumbnailId) => DwmNativeMethods.DwmUnregisterThumbnail(thumbnailId);

    /// <summary>Updates the properties for a Desktop Window Manager (DWM) thumbnail.</summary>
    /// <param name="thumbnailId">
    /// The handle to the DWM thumbnail to be updated. Null or invalid thumbnails, as well as thumbnails owned by other
    /// processes, will result in a return value of E_INVALIDARG.
    /// </param>
    /// <param name="props">A pointer to a DwmThumbnailProperties structure that contains the new thumbnail properties.</param>
    /// <returns>The operation result.</returns>
    public static HResult DwmUpdateThumbnailProperties(IntPtr thumbnailId, ref DwmThumbnailProperties props) => DwmNativeMethods.DwmUpdateThumbnailProperties(thumbnailId, ref props);

    /// <summary>Enables DWM composition.</summary>
    /// <returns><see langword="true" /> when composition is enabled.</returns>
    public static bool EnableComposition() => DwmEnableComposition(1U).Succeeded();

    /// <summary>Gets the window size for DWM windows.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="rectangle">The extended frame bounds rectangle.</param>
    /// <returns><see langword="true" /> when the bounds were retrieved.</returns>
    public static bool GetExtendedFrameBounds(IntPtr windowHandle, out NativeRect rectangle)
    {
        if (DwmGetWindowAttribute(windowHandle, DwmWindowAttributes.ExtendedFrameBounds, out rectangle, NativeRect.SizeOf).Succeeded())
        {
            return true;
        }

        rectangle = NativeRect.Empty;
        return false;
    }

    /// <summary>Checks whether the specified window is cloaked, such as on a different virtual desktop.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns><see langword="true" /> when the window is cloaked.</returns>
    public static bool IsWindowCloaked(IntPtr windowHandle)
    {
        if (!_isWindows8OrLaterProvider())
        {
            return false;
        }

        _ = DwmGetWindowAttribute(windowHandle, DwmWindowAttributes.Cloaked, out bool isCloaked, Marshal.SizeOf<bool>());
        return isCloaked;
    }

    /// <summary>Retrieves the window corner preference for the specified window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <returns>The window corner preference.</returns>
    public static DwmWindowCornerPreference GetWindowCornerPreference(IntPtr windowHandle)
    {
        if (!_isWindows11OrLaterProvider())
        {
            return DwmWindowCornerPreference.Default;
        }

        return _getUIntWindowAttributeOperation(
                windowHandle,
                DwmWindowAttributes.WindowCornerPreference,
                out var cornerPreference,
                DwordSize).Succeeded()
            ? (DwmWindowCornerPreference)cornerPreference
            : DwmWindowCornerPreference.Default;
    }

    /// <summary>Sets the window corner preference for the specified window.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="windowCornerPreference">The window corner preference.</param>
    /// <returns><see langword="true" /> when the preference was set.</returns>
    public static bool SetWindowCornerPreference(IntPtr windowHandle, DwmWindowCornerPreference windowCornerPreference)
    {
        if (!_isWindows11OrLaterProvider())
        {
            return false;
        }

        var cornerPreference = (uint)windowCornerPreference;
        var attributeValue = Marshal.AllocHGlobal(DwordSize);
        try
        {
            Marshal.WriteInt32(attributeValue, unchecked((int)cornerPreference));
            return DwmSetWindowAttribute(
                windowHandle,
                DwmWindowAttributes.WindowCornerPreference,
                attributeValue,
                DwordSize).Succeeded();
        }
        finally
        {
            Marshal.FreeHGlobal(attributeValue);
        }
    }

    /// <summary>
    /// Retrieves the shared surface of the specified windowHandle, maybe https://github.com/notr1ch/DWMCapture can help on the usage.
    /// http://undoc.airesoft.co.uk/user32.dll/DwmGetDxSharedSurface.php?
    /// </summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="adapterLuid">The adapter LUID.</param>
    /// <param name="one">The one value.</param>
    /// <param name="two">The two value.</param>
    /// <param name="d3DFormat">The Direct3D format.</param>
    /// <param name="sharedHandle">The shared handle.</param>
    /// <param name="unknown">The unknown value.</param>
    /// <returns>The result.</returns>
    public static int GetSharedSurface(
        IntPtr windowHandle,
        long adapterLuid,
        uint one,
        uint two,
        [In][Out] ref uint d3DFormat,
        out IntPtr sharedHandle,
        ulong unknown) =>
        _getSharedSurfaceOperation(
            windowHandle,
            adapterLuid,
            one,
            two,
            ref d3DFormat,
            out sharedHandle,
            unknown);

    /// <summary>Updates the shared window surface.</summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="one">The one value.</param>
    /// <param name="two">The two value.</param>
    /// <param name="three">The three value.</param>
    /// <param name="monitorHandle">The monitor handle.</param>
    /// <param name="unknown">The unknown value.</param>
    /// <returns>The result.</returns>
    public static int UpdateWindowShared(
        IntPtr windowHandle,
        int one,
        int two,
        int three,
        IntPtr monitorHandle,
        IntPtr unknown) =>
        _updateWindowSharedOperation(windowHandle, one, two, three, monitorHandle, unknown);

    /// <summary>Activates Aero Peek.</summary>
    /// <param name="active">The active state.</param>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="onTopHandle">The topmost window handle.</param>
    /// <param name="unknown">The undocumented option value.</param>
    /// <returns>The operation result.</returns>
    internal static HResult DwmpActivateLivePreview(
        uint active,
        IntPtr windowHandle,
        IntPtr onTopHandle,
        uint unknown) =>
        DwmNativeMethods.DwmpActivateLivePreview(active, windowHandle, onTopHandle, unknown);

    /// <summary>
    /// Sets a static, iconic bitmap to display a live preview (also known as a Peek preview) of a window or tab. The
    /// taskbar can use this bitmap to show a full-sized preview of a window or tab.
    /// See
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dd389410(v=vs.85).aspx">
    /// DwmSetIconicLivePreviewBitmap
    /// function
    /// </a>
    /// </summary>
    /// <param name="windowHandle">The window handle.</param>
    /// <param name="bitmapHandle">The bitmap handle.</param>
    /// <param name="clientPoint">
    /// The offset of a tab window's client region (the content area inside the client window frame)
    /// from the host window's frame. This offset enables the tab window's contents to be drawn correctly in a live preview
    /// when it is drawn without its frame.
    /// </param>
    /// <param name="setIconicLivePreviewFlags">The display options for the live preview.</param>
    /// <returns>The operation result.</returns>
    internal static HResult DwmSetIconicLivePreviewBitmap(
        IntPtr windowHandle,
        IntPtr bitmapHandle,
        ref NativePoint clientPoint,
        DwmSetIconicLivePreviewFlags setIconicLivePreviewFlags) =>
        DwmNativeMethods.DwmSetIconicLivePreviewBitmap(
            windowHandle,
            bitmapHandle,
            ref clientPoint,
            setIconicLivePreviewFlags);

    /// <summary>Overrides DWM environment values while the returned scope is alive.</summary>
    /// <param name="colorizationValueProvider">Provides the current colorization registry value.</param>
    /// <param name="isWindows8XProvider">Provides the Windows 8.x test.</param>
    /// <param name="isWindowsBeforeVistaProvider">Provides the pre-Vista test.</param>
    /// <param name="isWindows8OrLaterProvider">Provides the Windows 8-or-later test.</param>
    /// <param name="isWindows11OrLaterProvider">Provides the Windows 11-or-later test.</param>
    /// <returns>A scope that restores the original providers.</returns>
    internal static IDisposable OverrideEnvironmentForTesting(
        Func<object> colorizationValueProvider,
        Func<bool> isWindows8XProvider,
        Func<bool> isWindowsBeforeVistaProvider,
        Func<bool> isWindows8OrLaterProvider,
        Func<bool> isWindows11OrLaterProvider)
    {
        var previous = new EnvironmentProviders(
            _colorizationValueProvider,
            _isWindows8XProvider,
            _isWindowsBeforeVistaProvider,
            _isWindows8OrLaterProvider,
            _isWindows11OrLaterProvider);
        _colorizationValueProvider = colorizationValueProvider;
        _isWindows8XProvider = isWindows8XProvider;
        _isWindowsBeforeVistaProvider = isWindowsBeforeVistaProvider;
        _isWindows8OrLaterProvider = isWindows8OrLaterProvider;
        _isWindows11OrLaterProvider = isWindows11OrLaterProvider;
        return new EnvironmentProvidersOverride(previous);
    }

    /// <summary>Overrides optional DWM exports while the returned scope is alive.</summary>
    /// <param name="enableCompositionExport">The DwmEnableComposition export.</param>
    /// <param name="flip3DExport">The DwmpStartOrStopFlip3D export.</param>
    /// <param name="getSharedSurfaceExport">The GetSharedSurface export.</param>
    /// <param name="updateWindowSharedExport">The UpdateWindowShared export.</param>
    /// <param name="activateLivePreviewExport">The DwmpActivateLivePreview export.</param>
    /// <returns>A scope that restores the original exports.</returns>
    internal static IDisposable OverrideOptionalExportsForTesting(
        IntPtr enableCompositionExport,
        IntPtr flip3DExport,
        IntPtr getSharedSurfaceExport,
        IntPtr updateWindowSharedExport,
        IntPtr activateLivePreviewExport) =>
        DwmNativeMethods.OverrideOptionalExportsForTesting(
            enableCompositionExport,
            flip3DExport,
            getSharedSurfaceExport,
            updateWindowSharedExport,
            activateLivePreviewExport);

    /// <summary>Resolves an optional DWM export lookup result.</summary>
    /// <param name="found">A value indicating whether the export was found.</param>
    /// <param name="exportAddress">The discovered export address.</param>
    /// <returns>The export address when found; otherwise <see cref="F:System.IntPtr.Zero" />.</returns>
    internal static IntPtr ResolveOptionalExport(bool found, IntPtr exportAddress) =>
        found ? exportAddress : IntPtr.Zero;

    /// <summary>Gets the colorization value from an optional DWM registry key.</summary>
    /// <param name="key">The optional DWM registry key.</param>
    /// <returns>The colorization value, or <see langword="null" /> when the key is unavailable.</returns>
    internal static object GetColorizationValue(RegistryKey key) => key?.GetValue(nameof(ColorizationColor));

    /// <summary>Overrides optional DWM operations while the returned scope is alive.</summary>
    /// <param name="enableComposition">The DwmEnableComposition operation.</param>
    /// <param name="startOrStopFlip3D">The DwmpStartOrStopFlip3D operation.</param>
    /// <param name="getSharedSurface">The GetSharedSurface operation.</param>
    /// <param name="updateWindowShared">The UpdateWindowShared operation.</param>
    /// <returns>A scope that restores the original operations.</returns>
    internal static IDisposable OverrideOptionalOperationsForTesting(
        Func<uint, HResult> enableComposition,
        Func<bool> startOrStopFlip3D,
        DwmGetSharedSurfaceOperation getSharedSurface,
        DwmUpdateWindowSharedOperation updateWindowShared)
    {
        Throw.IfNull(enableComposition);
        Throw.IfNull(startOrStopFlip3D);
        Throw.IfNull(getSharedSurface);
        Throw.IfNull(updateWindowShared);
        var previous = new OptionalOperations(
            _enableCompositionOperation,
            _startOrStopFlip3DOperation,
            _getSharedSurfaceOperation,
            _updateWindowSharedOperation);
        _enableCompositionOperation = enableComposition;
        _startOrStopFlip3DOperation = startOrStopFlip3D;
        _getSharedSurfaceOperation = getSharedSurface;
        _updateWindowSharedOperation = updateWindowShared;
        return new OptionalOperationsOverride(previous);
    }

    /// <summary>Overrides the unsigned-integer window-attribute operation while the returned scope is alive.</summary>
    /// <param name="getWindowAttribute">The replacement window-attribute operation.</param>
    /// <returns>A scope that restores the original operation.</returns>
    internal static IDisposable OverrideUIntWindowAttributeOperationForTesting(
        DwmGetUIntWindowAttributeOperation getWindowAttribute)
    {
        Throw.IfNull(getWindowAttribute);
        var previous = _getUIntWindowAttributeOperation;
        _getUIntWindowAttributeOperation = getWindowAttribute;
        return new UIntWindowAttributeOperationOverride(previous);
    }

    /// <summary>
    /// See
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa969518(v=vs.85).aspx">
    /// DwmIsCompositionEnabled
    /// function
    /// </a>
    /// Obtains a value that indicates whether Desktop Window Manager (DWM) composition is enabled.
    /// Applications on machines running Windows 7 or earlier can listen for composition state changes by handling the
    /// WM_DWMCOMPOSITIONCHANGED notification.
    /// Note: As of Windows 8, DWM composition is always enabled.
    /// If an app declares Windows 8 compatibility in their manifest, this function will receive a value of TRUE through
    /// pfEnabled.
    /// If no such manifest entry is found, Windows 8 compatibility is not assumed and this function receives a value of
    /// FALSE through pfEnabled.
    /// This is done so that older programs that interpret a value of TRUE to imply that high contrast mode is off can
    /// continue to make the correct decisions about rendering their images.
    /// (Note that this is a bad practice—you should use the SystemParametersInfo function with the SPI_GETHIGHCONTRAST
    /// flag to determine the state of high contrast mode.)
    /// </summary>
    /// <param name="enabled">The current composition state.</param>
    /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
    private static HResult DwmIsCompositionEnabled(out bool enabled) => DwmNativeMethods.DwmIsCompositionEnabled(out enabled);

    /// <summary>Reads the colorization value from the current-user DWM registry key.</summary>
    /// <returns>The colorization value, or <see langword="null" /> when unavailable.</returns>
    private static object ReadColorizationValue()
    {
        using var key = Registry.CurrentUser.OpenSubKey(ColorizationColorKey, writable: false);
        return GetColorizationValue(key);
    }

    /// <summary>Converts a drawing color to a WPF media color.</summary>
    /// <param name="color">The drawing color to convert.</param>
    /// <returns>The equivalent media color.</returns>
    private static MediaColor ToMediaColor(DrawingColor color) =>
        MediaColor.FromArgb(color.A, color.R, color.G, color.B);

    /// <summary>Represents the DWM environment value providers.</summary>
    /// <param name="ColorizationValue">Provides the current colorization registry value.</param>
    /// <param name="IsWindows8X">Provides the Windows 8.x test.</param>
    /// <param name="IsWindowsBeforeVista">Provides the pre-Vista test.</param>
    /// <param name="IsWindows8OrLater">Provides the Windows 8-or-later test.</param>
    /// <param name="IsWindows11OrLater">Provides the Windows 11-or-later test.</param>
    private readonly record struct EnvironmentProviders(
        Func<object> ColorizationValue,
        Func<bool> IsWindows8X,
        Func<bool> IsWindowsBeforeVista,
        Func<bool> IsWindows8OrLater,
        Func<bool> IsWindows11OrLater);

    /// <summary>Represents the optional DWM operations.</summary>
    /// <param name="EnableComposition">Provides DwmEnableComposition.</param>
    /// <param name="StartOrStopFlip3D">Provides DwmpStartOrStopFlip3D.</param>
    /// <param name="GetSharedSurface">Provides GetSharedSurface.</param>
    /// <param name="UpdateWindowShared">Provides UpdateWindowShared.</param>
    private readonly record struct OptionalOperations(
        Func<uint, HResult> EnableComposition,
        Func<bool> StartOrStopFlip3D,
        DwmGetSharedSurfaceOperation GetSharedSurface,
        DwmUpdateWindowSharedOperation UpdateWindowShared);

    /// <summary>Restores DWM environment value providers.</summary>
    /// <param name="previous">The previous environment value providers.</param>
    private sealed class EnvironmentProvidersOverride(EnvironmentProviders previous) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            _colorizationValueProvider = previous.ColorizationValue;
            _isWindows8XProvider = previous.IsWindows8X;
            _isWindowsBeforeVistaProvider = previous.IsWindowsBeforeVista;
            _isWindows8OrLaterProvider = previous.IsWindows8OrLater;
            _isWindows11OrLaterProvider = previous.IsWindows11OrLater;
        }
    }

    /// <summary>Restores optional DWM operations.</summary>
    /// <param name="previous">The previous optional operations.</param>
    private sealed class OptionalOperationsOverride(OptionalOperations previous) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            _enableCompositionOperation = previous.EnableComposition;
            _startOrStopFlip3DOperation = previous.StartOrStopFlip3D;
            _getSharedSurfaceOperation = previous.GetSharedSurface;
            _updateWindowSharedOperation = previous.UpdateWindowShared;
        }
    }

    /// <summary>Restores the unsigned-integer window-attribute operation.</summary>
    /// <param name="previous">The original window-attribute operation.</param>
    private sealed class UIntWindowAttributeOperationOverride(DwmGetUIntWindowAttributeOperation previous) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => _getUIntWindowAttributeOperation = previous;
    }
}
