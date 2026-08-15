// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Gdi.Structs;
using CP.ReactiveUI.Primitives.Windows.Native.Structs;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using log4net;

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Provides GDI+ blur-effect helpers.</summary>
public static class GdiPlusApi
{
    /// <summary>Specifies the earliest Windows version with GDI+ blur.</summary>
    private const int MinimumBlurWindowsMajorVersion = 6;

    /// <summary>Specifies the Windows minor version for the blur workaround.</summary>
    private const int MinimumBlurWindowsMinorVersion = 2;

    /// <summary>Contains native GDI+ entry points.</summary>
    internal static class NativeMethods
    {
        /// <summary>Specifies the GDI+ library.</summary>
        private const string GdiPlusDll = "gdiplus.dll";

        /// <summary>Invokes the native <c>GdipBitmapApplyEffect</c> entry point.</summary>
        /// <param name="bitmap">The native <paramref name="bitmap" /> value.</param>
        /// <param name="effect">The native <paramref name="effect" /> value.</param>
        /// <param name="rectOfInterest">The native <paramref name="rectOfInterest" /> value.</param>
        /// <param name="useAuxData">The native <paramref name="useAuxData" /> value.</param>
        /// <param name="auxData">The native <paramref name="auxData" /> value.</param>
        /// <param name="auxDataSize">The native <paramref name="auxDataSize" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdiplus.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern GdiPlusStatus GdipBitmapApplyEffect(IntPtr bitmap, IntPtr effect, ref NativeRect rectOfInterest, [MarshalAs(UnmanagedType.Bool)] bool useAuxData, IntPtr auxData, int auxDataSize);

        /// <summary>Invokes the native <c>GdipCreateEffect</c> entry point.</summary>
        /// <param name="guid">The native <paramref name="guid" /> value.</param>
        /// <param name="effect">The native <paramref name="effect" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdiplus.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern GdiPlusStatus GdipCreateEffect(ref Guid guid, out IntPtr effect);

        /// <summary>Invokes the native <c>GdipDeleteEffect</c> entry point.</summary>
        /// <param name="effect">The native <paramref name="effect" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdiplus.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern GdiPlusStatus GdipDeleteEffect(IntPtr effect);

        /// <summary>Invokes the native <c>GdipDrawImageFX</c> entry point.</summary>
        /// <param name="graphics">The native <paramref name="graphics" /> value.</param>
        /// <param name="bitmap">The native <paramref name="bitmap" /> value.</param>
        /// <param name="source">The native <paramref name="source" /> value.</param>
        /// <param name="matrix">The native <paramref name="matrix" /> value.</param>
        /// <param name="effect">The native <paramref name="effect" /> value.</param>
        /// <param name="imageAttributes">The native <paramref name="imageAttributes" /> value.</param>
        /// <param name="srcUnit">The native <paramref name="srcUnit" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdiplus.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern GdiPlusStatus GdipDrawImageFX(IntPtr graphics, IntPtr bitmap, ref NativeRectFloat source, IntPtr matrix, IntPtr effect, IntPtr imageAttributes, GpUnit srcUnit);

        /// <summary>Invokes the native <c>GdipSetEffectParameters</c> entry point.</summary>
        /// <param name="effect">The native <paramref name="effect" /> value.</param>
        /// <param name="parameters">The native <paramref name="parameters" /> value.</param>
        /// <param name="size">The native <paramref name="size" /> value.</param>
        /// <returns>The native result.</returns>
        [DllImport("gdiplus.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern GdiPlusStatus GdipSetEffectParameters(IntPtr effect, IntPtr parameters, uint size);
    }

    /// <summary>Specifies the smallest blur radius outside the workaround.</summary>
    private const int MinimumBlurRadius = 20;

    /// <summary>Provides logging for blur failures.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(GdiPlusApi));

    /// <summary>Identifies the GDI+ blur effect.</summary>
    private static readonly Guid BlurEffectGuid = new("{633C80A4-1843-482B-9EF2-BE2834C5FDD4}");

    /// <summary>Provides native and memory operations for GDI+ blur.</summary>
    private static GdiPlusBlurOperations _operations = GdiPlusBlurOperations.CreateNative();

    /// <summary>Indicates whether GDI+ blur remains available.</summary>
    private static bool _isBlurEnabled = _operations.GetOperatingSystemVersion().Major >= 6;

    /// <summary>Applies a GDI+ blur effect to a bitmap.</summary>
    /// <param name="destinationBitmap">The bitmap to modify.</param>
    /// <param name="area">The bitmap area to modify.</param>
    /// <param name="radius">The blur radius.</param>
    /// <param name="expandEdges">Whether the blur effect expands image edges.</param>
    /// <returns><see langword="true" /> when the effect is applied; otherwise, <see langword="false" />.</returns>
    public static bool ApplyBlur(Bitmap destinationBitmap, Rectangle area, int radius, bool expandEdges)
    {
        if (!IsBlurPossible(radius) || !TryCreateBlurEffect(radius, expandEdges, out var parametersMemory, out var effect))
        {
            return false;
        }

        try
        {
            NativeRect rectangle = area;
            return LogResult(_operations.ApplyEffect(GetNativeImage(destinationBitmap), effect, ref rectangle, useAuxData: false, IntPtr.Zero, 0), "Couldn't apply effect: ");
        }
        catch (Exception exception)
        {
            DisableBlur(exception, "Problem using GdipBitmapApplyEffect: ");
            return false;
        }
        finally
        {
            ReleaseBlurEffect(effect, parametersMemory, "Problem cleaning up ApplyBlur: ");
        }
    }

    /// <summary>Draws an image through a GDI+ blur effect.</summary>
    /// <param name="graphics">The graphics target.</param>
    /// <param name="image">The source image.</param>
    /// <param name="source">The image area to draw.</param>
    /// <param name="transform">The image transform.</param>
    /// <param name="imageAttributes">The image attributes.</param>
    /// <param name="radius">The blur radius.</param>
    /// <param name="expandEdges">Whether the blur effect expands image edges.</param>
    /// <returns><see langword="true" /> when the image is drawn; otherwise, <see langword="false" />.</returns>
    public static bool DrawWithBlur(Graphics graphics, Bitmap image, Rectangle source, Matrix transform, ImageAttributes imageAttributes, int radius, bool expandEdges)
    {
        if (!IsBlurPossible(radius) || !TryCreateBlurEffect(radius, expandEdges, out var parametersMemory, out var effect))
        {
            return false;
        }

        try
        {
            NativeRectFloat sourceRectangle = source;
            return LogResult(_operations.DrawImageFx(GetNativeGraphics(graphics), GetNativeImage(image), ref sourceRectangle, GetNativeMatrix(transform), effect, GetNativeImageAttributes(imageAttributes), GpUnit.UnitPixel), "Couldn't draw image: ");
        }
        catch (Exception exception)
        {
            DisableBlur(exception, "Problem using GdipDrawImageFX: ");
            return false;
        }
        finally
        {
            ReleaseBlurEffect(effect, parametersMemory, "Problem cleaning up DrawWithBlur: ");
        }
    }

    /// <summary>Returns whether GDI+ can create a blur effect for the supplied radius.</summary>
    /// <param name="radius">The blur radius.</param>
    /// <returns><see langword="true" /> when blur is available; otherwise, <see langword="false" />.</returns>
    public static bool IsBlurPossible(int radius) => IsBlurPossible(_isBlurEnabled, _operations.GetOperatingSystemVersion(), radius);

    /// <summary>Returns whether the supplied platform state supports a blur effect.</summary>
    /// <param name="isBlurEnabled">Whether blur has not been disabled after a native failure.</param>
    /// <param name="operatingSystemVersion">The Windows version to evaluate.</param>
    /// <param name="radius">The requested blur radius.</param>
    /// <returns><see langword="true" /> when the supplied state supports blur; otherwise, <see langword="false" />.</returns>
    internal static bool IsBlurPossible(bool isBlurEnabled, Version operatingSystemVersion, int radius)
    {
        Throw.IfNull(operatingSystemVersion);
        if (isBlurEnabled)
        {
            if (operatingSystemVersion.Major != 6)
            {
                return operatingSystemVersion.Major > 6 && radius >= 20;
            }

            return operatingSystemVersion.Minor < 2;
        }

        return false;
    }

    /// <summary>Replaces the current blur state for deterministic tests.</summary>
    /// <param name="operations">The operations to use.</param>
    /// <param name="isBlurEnabled">Whether blur should start enabled.</param>
    /// <returns>The previous state.</returns>
    internal static GdiPlusBlurState ReplaceStateForTesting(GdiPlusBlurOperations operations, bool isBlurEnabled)
    {
        Throw.IfNull(operations);
        GdiPlusBlurState result = new(_operations, _isBlurEnabled);
        _operations = operations;
        _isBlurEnabled = isBlurEnabled;
        return result;
    }

    /// <summary>Restores a previous deterministic-test blur state.</summary>
    /// <param name="state">The state to restore.</param>
    internal static void RestoreStateForTesting(GdiPlusBlurState state)
    {
        _operations = state.Operations;
        _isBlurEnabled = state.IsBlurEnabled;
    }

    /// <summary>Gets a native handle from an object for deterministic tests.</summary>
    /// <param name="instance">The object that may contain the handle field.</param>
    /// <param name="fieldName">The field name to look for.</param>
    /// <returns>The reflected native handle, or zero.</returns>
    internal static IntPtr GetNativeHandleForTesting(object instance, string fieldName) => GetNativeHandle(instance, fieldName);

    /// <summary>Writes blur parameters into unmanaged memory.</summary>
    /// <param name="parameters">The parameters to write.</param>
    /// <param name="parametersMemory">The target memory.</param>
    internal static void WriteBlurParameters(BlurParams parameters, IntPtr parametersMemory) => Marshal.StructureToPtr(parameters, parametersMemory, fDeleteOld: false);

    /// <summary>Gets the current operating-system version.</summary>
    /// <returns>The current operating-system version.</returns>
    internal static Version GetOperatingSystemVersion() => Environment.OSVersion.Version;

    /// <summary>Creates a configured native GDI+ blur effect.</summary>
    /// <param name="radius">The blur radius.</param>
    /// <param name="expandEdges">Whether the blur effect expands image edges.</param>
    /// <param name="parametersMemory">Receives allocated unmanaged blur-parameter memory.</param>
    /// <param name="effect">Receives the created native effect.</param>
    /// <returns><see langword="true" /> when the effect is created; otherwise, <see langword="false" />.</returns>
    private static bool TryCreateBlurEffect(int radius, bool expandEdges, out IntPtr parametersMemory, out IntPtr effect)
    {
        parametersMemory = IntPtr.Zero;
        effect = IntPtr.Zero;
        try
        {
            BlurParams parameters = BlurParams.Create(radius, expandEdges);
            parametersMemory = _operations.AllocateHGlobal(Marshal.SizeOf<BlurParams>());
            _operations.StructureToPtr(parameters, parametersMemory);
            Guid effectIdentifier = BlurEffectGuid;
            GdiPlusStatus status = _operations.CreateEffect(ref effectIdentifier, out effect);
            if (status == GdiPlusStatus.Ok)
            {
                status = _operations.SetEffectParameters(effect, parametersMemory, checked((uint)Marshal.SizeOf<BlurParams>()));
            }

            if (status == GdiPlusStatus.Ok)
            {
                return true;
            }

            Log.ErrorFormat("Couldn't configure blur effect: {0}", status);
        }
        catch (Exception exception)
        {
            DisableBlur(exception, "Problem configuring GDI+ blur: ");
        }

        ReleaseBlurEffect(effect, parametersMemory, "Problem cleaning up failed blur setup: ");
        effect = IntPtr.Zero;
        parametersMemory = IntPtr.Zero;
        return false;
    }

    /// <summary>Releases native resources created for a blur effect.</summary>
    /// <param name="effect">The native GDI+ effect.</param>
    /// <param name="parametersMemory">The allocated blur-parameter memory.</param>
    /// <param name="failureMessage">The message to log when cleanup fails.</param>
    private static void ReleaseBlurEffect(IntPtr effect, IntPtr parametersMemory, string failureMessage)
    {
        try
        {
            if (effect != IntPtr.Zero && _operations.DeleteEffect(effect) != GdiPlusStatus.Ok)
            {
                Log.Error("Couldn't delete effect.");
            }

            if (parametersMemory != IntPtr.Zero)
            {
                _operations.FreeHGlobal(parametersMemory);
            }
        }
        catch (Exception exception)
        {
            DisableBlur(exception, failureMessage);
        }
    }

    /// <summary>Records a native status and reports whether it represents success.</summary>
    /// <param name="status">The native status.</param>
    /// <param name="failureMessage">The message to log for a failure.</param>
    /// <returns><see langword="true" /> for a successful status; otherwise, <see langword="false" />.</returns>
    private static bool LogResult(GdiPlusStatus status, string failureMessage)
    {
        if (status == GdiPlusStatus.Ok)
        {
            return true;
        }

        Log.ErrorFormat("{0}{1}", failureMessage, status);
        return false;
    }

    /// <summary>Disables blur support and records the exception that made it unavailable.</summary>
    /// <param name="exception">The exception raised by the native operation.</param>
    /// <param name="message">The operation-specific log message.</param>
    private static void DisableBlur(Exception exception, string message)
    {
        _isBlurEnabled = false;
        Log.Error(message, exception);
    }

    /// <summary>Gets the GDI+ graphics handle held by a <see cref="T:System.Drawing.Graphics" /> instance.</summary>
    /// <param name="graphics">The graphics instance.</param>
    /// <returns>The native graphics handle, or <see cref="F:System.IntPtr.Zero" /> when unavailable.</returns>
    private static IntPtr GetNativeGraphics(Graphics graphics) => GetNativeHandle(graphics, "nativeGraphics");

    /// <summary>Gets the GDI+ image handle held by a <see cref="T:System.Drawing.Bitmap" /> instance.</summary>
    /// <param name="bitmap">The bitmap instance.</param>
    /// <returns>The native image handle, or <see cref="F:System.IntPtr.Zero" /> when unavailable.</returns>
    private static IntPtr GetNativeImage(Bitmap bitmap) => GetNativeHandle(bitmap, "nativeImage");

    /// <summary>Gets the GDI+ image-attributes handle held by an <see cref="T:System.Drawing.Imaging.ImageAttributes" /> instance.</summary>
    /// <param name="imageAttributes">The image attributes instance.</param>
    /// <returns>The native image-attributes handle, or <see cref="F:System.IntPtr.Zero" /> when unavailable.</returns>
    private static IntPtr GetNativeImageAttributes(ImageAttributes imageAttributes) => GetNativeHandle(imageAttributes, "nativeImageAttributes");

    /// <summary>Gets the GDI+ matrix handle held by a <see cref="T:System.Drawing.Drawing2D.Matrix" /> instance.</summary>
    /// <param name="matrix">The matrix instance.</param>
    /// <returns>The native matrix handle, or <see cref="F:System.IntPtr.Zero" /> when unavailable.</returns>
    private static IntPtr GetNativeMatrix(Matrix matrix) => GetNativeHandle(matrix, "nativeMatrix");

    /// <summary>Gets a System.Drawing native handle while retaining legacy runtime compatibility.</summary>
    /// <param name="instance">The System.Drawing instance that owns the handle.</param>
    /// <param name="fieldName">The runtime-private handle field name.</param>
    /// <returns>The native handle, or <see cref="F:System.IntPtr.Zero" /> when the field is unavailable.</returns>
    private static IntPtr GetNativeHandle(object instance, string fieldName)
    {
        if (instance is null)
        {
            return IntPtr.Zero;
        }

        foreach (FieldInfo field in instance.GetType().GetRuntimeFields())
        {
            if (string.Equals(field.Name, fieldName, StringComparison.Ordinal))
            {
                object value = field.GetValue(instance);
                if (value is IntPtr)
                {
                    return (IntPtr)value;
                }
            }
        }

        return IntPtr.Zero;
    }
}
