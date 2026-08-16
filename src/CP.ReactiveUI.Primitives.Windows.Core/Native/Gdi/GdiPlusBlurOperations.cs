// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Gdi;

/// <summary>Groups GDI+ blur operations behind a composable boundary.</summary>
internal sealed class GdiPlusBlurOperations
{
    /// <summary>Gets the unmanaged memory allocator.</summary>
    public GdiPlusAllocateHGlobalOperation AllocateHGlobal { get; init; } = Marshal.AllocHGlobal;

    /// <summary>Gets the native effect applicator.</summary>
    public GdiPlusApplyEffectOperation ApplyEffect { get; init; } =
        GdiPlusApi.NativeMethods.GdipBitmapApplyEffect;

    /// <summary>Gets the native effect creator.</summary>
    public GdiPlusCreateEffectOperation CreateEffect { get; init; } =
        GdiPlusApi.NativeMethods.GdipCreateEffect;

    /// <summary>Gets the native effect deleter.</summary>
    public GdiPlusDeleteEffectOperation DeleteEffect { get; init; } =
        GdiPlusApi.NativeMethods.GdipDeleteEffect;

    /// <summary>Gets the native effect drawing operation.</summary>
    public GdiPlusDrawImageFxOperation DrawImageFx { get; init; } =
        GdiPlusApi.NativeMethods.GdipDrawImageFX;

    /// <summary>Gets the unmanaged memory releaser.</summary>
    public GdiPlusFreeHGlobalOperation FreeHGlobal { get; init; } = Marshal.FreeHGlobal;

    /// <summary>Gets the managed-native handle reader.</summary>
    public GdiPlusGetNativeHandleOperation GetNativeHandle { get; init; } =
        GdiPlusApi.GetNativeHandleForTesting;

    /// <summary>Gets the operating-system version reader.</summary>
    public Func<Version> GetOperatingSystemVersion { get; init; } =
        GdiPlusApi.GetOperatingSystemVersion;

    /// <summary>Gets the native effect-parameter setter.</summary>
    public GdiPlusSetEffectParametersOperation SetEffectParameters { get; init; } =
        GdiPlusApi.NativeMethods.GdipSetEffectParameters;

    /// <summary>Gets the blur-parameter writer.</summary>
    public GdiPlusStructureToPtrOperation StructureToPtr { get; init; } =
        GdiPlusApi.WriteBlurParameters;

    /// <summary>Creates the native operation group.</summary>
    /// <returns>The native operation group.</returns>
    internal static GdiPlusBlurOperations CreateNative() => new();
}
