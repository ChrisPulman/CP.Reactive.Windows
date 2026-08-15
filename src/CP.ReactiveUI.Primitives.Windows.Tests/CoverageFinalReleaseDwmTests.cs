// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic coverage for optional Desktop Window Manager exports.</summary>
public sealed class CoverageFinalReleaseDwmTests
{
    /// <summary>Defines the requested DWM composition action.</summary>
    private const uint CompositionAction = 7U;

    /// <summary>Defines the initial shared-surface format.</summary>
    private const uint InitialSharedSurfaceFormat = 9U;

    /// <summary>Defines the shared-surface window handle.</summary>
    private const int SharedSurfaceWindowHandle = 1;

    /// <summary>Defines the shared-surface adapter LUID.</summary>
    private const long SharedSurfaceAdapterLuid = 2L;

    /// <summary>Defines the first shared-surface option.</summary>
    private const uint SharedSurfaceFirstOption = 3U;

    /// <summary>Defines the second shared-surface option.</summary>
    private const uint SharedSurfaceSecondOption = 4U;

    /// <summary>Defines the shared-surface unknown option.</summary>
    private const ulong SharedSurfaceUnknownOption = 5UL;

    /// <summary>Defines the update-window handle.</summary>
    private const int UpdateWindowHandle = 6;

    /// <summary>Defines the first update-window option.</summary>
    private const int UpdateWindowFirstOption = 7;

    /// <summary>Defines the second update-window option.</summary>
    private const int UpdateWindowSecondOption = 8;

    /// <summary>Defines the third update-window option.</summary>
    private const int UpdateWindowThirdOption = 9;

    /// <summary>Defines the update-window monitor handle.</summary>
    private const int UpdateWindowMonitorHandle = 10;

    /// <summary>Defines the update-window unknown handle.</summary>
    private const int UpdateWindowUnknownHandle = 11;

    /// <summary>Defines the live-preview active value.</summary>
    private const uint LivePreviewActiveValue = 12U;

    /// <summary>Defines the live-preview window handle.</summary>
    private const int LivePreviewWindowHandle = 13;

    /// <summary>Defines the live-preview top window handle.</summary>
    private const int LivePreviewTopWindowHandle = 14;

    /// <summary>Defines the live-preview unknown value.</summary>
    private const uint LivePreviewUnknownValue = 15U;

    /// <summary>Defines the updated shared-surface format.</summary>
    private const uint UpdatedSharedSurfaceFormat = 16U;

    /// <summary>Defines the returned shared-surface handle.</summary>
    private const int ReturnedSharedSurfaceHandle = 17;

    /// <summary>Defines the returned shared-surface result.</summary>
    private const int SharedSurfaceResult = 18;

    /// <summary>Defines the returned update-window result.</summary>
    private const int UpdateWindowResult = 19;

    /// <summary>Verifies optional DWM exports are invoked through managed test stubs and restored after use.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OptionalDwmExportsInvokeManagedStubsAndHandleUnavailableExportsAsync()
    {
        _ = DwmApi.ColorizationSystemDrawingColor;
        var stubs = new OptionalDwmExportStubs();
        using (stubs.OverrideExports())
        {
            var format = InitialSharedSurfaceFormat;
            var compositionResult = DwmApi.DwmEnableComposition(CompositionAction);
            var flip3DResult = DwmApi.DwmpStartOrStopFlip3D();
            var sharedSurfaceResult = DwmApi.GetSharedSurface(
                new(SharedSurfaceWindowHandle),
                SharedSurfaceAdapterLuid,
                SharedSurfaceFirstOption,
                SharedSurfaceSecondOption,
                ref format,
                out var sharedHandle,
                SharedSurfaceUnknownOption);
            var updateWindowResult = DwmApi.UpdateWindowShared(
                new(UpdateWindowHandle),
                UpdateWindowFirstOption,
                UpdateWindowSecondOption,
                UpdateWindowThirdOption,
                new(UpdateWindowMonitorHandle),
                new(UpdateWindowUnknownHandle));
            var livePreviewResult = DwmApi.DwmpActivateLivePreview(
                LivePreviewActiveValue,
                new(LivePreviewWindowHandle),
                new(LivePreviewTopWindowHandle),
                LivePreviewUnknownValue);

            await Assert.That(compositionResult).IsEqualTo(HResult.Ok);
            await Assert.That(flip3DResult).IsTrue();
            await Assert.That(format).IsEqualTo(UpdatedSharedSurfaceFormat);
            await Assert.That(sharedHandle).IsEqualTo(new(ReturnedSharedSurfaceHandle));
            await Assert.That(sharedSurfaceResult).IsEqualTo(SharedSurfaceResult);
            await Assert.That(updateWindowResult).IsEqualTo(UpdateWindowResult);
            await Assert.That(livePreviewResult).IsEqualTo(HResult.Ok);
            await Assert.That(stubs.CapturedCompositionAction).IsEqualTo(CompositionAction);
            await Assert.That(stubs.LivePreviewActive).IsEqualTo(LivePreviewActiveValue);
        }

        using (DwmApi.OverrideOptionalExportsForTesting(
                   IntPtr.Zero,
                   IntPtr.Zero,
                   IntPtr.Zero,
                   IntPtr.Zero,
                   IntPtr.Zero))
        {
            var format = 0U;

            await Assert.That(DwmApi.DwmpStartOrStopFlip3D()).IsFalse();
            await Assert.That(DwmApi.GetSharedSurface(IntPtr.Zero, 0L, 0U, 0U, ref format, out var sharedHandle, 0UL)).IsLessThan(0);
            await Assert.That(sharedHandle).IsEqualTo(IntPtr.Zero);
            await Assert.That(DwmApi.UpdateWindowShared(IntPtr.Zero, 0, 0, 0, IntPtr.Zero, IntPtr.Zero)).IsLessThan(0);
            await Assert.That(DwmApi.DwmpActivateLivePreview(0U, IntPtr.Zero, IntPtr.Zero, 0U)).IsEqualTo(HResult.NotSupported);
        }
    }

    /// <summary>Verifies managed DWM environment and attribute operations cover platform-gated paths without changing the desktop.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmEnvironmentAndCornerPreferenceOperationsUseManagedProvidersAsync()
    {
        using (DwmApi.OverrideEnvironmentForTesting(
                   static () => null,
                   static () => false,
                   static () => false,
                   static () => true,
                   static () => true))
        using (DwmApi.OverrideUIntWindowAttributeOperationForTesting(GetRoundCornerPreference))
        {
            await Assert.That(DwmApi.ColorizationSystemDrawingColor).IsEqualTo(Color.White);
            await Assert.That(DwmApi.GetWindowCornerPreference(IntPtr.Zero)).IsEqualTo(DwmWindowCornerPreference.Round);
        }

        using (DwmApi.OverrideEnvironmentForTesting(
                   static () => 0,
                   static () => false,
                   static () => false,
                   static () => true,
                   static () => false))
        {
            await Assert.That(DwmApi.SetWindowCornerPreference(IntPtr.Zero, DwmWindowCornerPreference.Round)).IsFalse();
        }
    }

    /// <summary>Returns a successful round-corner preference value.</summary>
    /// <param name="windowHandle">The target window handle.</param>
    /// <param name="attribute">The requested attribute.</param>
    /// <param name="value">The retrieved preference value.</param>
    /// <param name="size">The requested value size.</param>
    /// <returns>A successful result.</returns>
    private static HResult GetRoundCornerPreference(
        IntPtr windowHandle,
        DwmWindowAttributes attribute,
        out uint value,
        int size)
    {
        _ = windowHandle;
        _ = attribute;
        _ = size;
        value = (uint)DwmWindowCornerPreference.Round;
        return HResult.Ok;
    }

    /// <summary>Provides managed delegates exposed as synthetic optional DWM exports.</summary>
    private sealed class OptionalDwmExportStubs
    {
        /// <summary>Provides the DwmEnableComposition delegate.</summary>
        private readonly EnableCompositionStub _enableComposition;

        /// <summary>Provides the DwmpStartOrStopFlip3D delegate.</summary>
        private readonly Flip3DStub _flip3D;

        /// <summary>Provides the GetSharedSurface delegate.</summary>
        private readonly GetSharedSurfaceStub _getSharedSurface;

        /// <summary>Provides the UpdateWindowShared delegate.</summary>
        private readonly UpdateWindowSharedStub _updateWindowShared;

        /// <summary>Provides the DwmpActivateLivePreview delegate.</summary>
        private readonly ActivateLivePreviewStub _activateLivePreview;

        /// <summary>Tracks Flip3D delegate invocations.</summary>
        private int _flip3DRequests;

        /// <summary>Initializes a new instance of the <see cref="OptionalDwmExportStubs"/> class.</summary>
        internal OptionalDwmExportStubs()
        {
            _enableComposition = EnableComposition;
            _flip3D = StartOrStopFlip3D;
            _getSharedSurface = GetSharedSurface;
            _updateWindowShared = UpdateWindowShared;
            _activateLivePreview = ActivateLivePreview;
        }

        /// <summary>Gets the requested DWM composition action.</summary>
        internal uint CapturedCompositionAction { get; private set; }

        /// <summary>Gets the requested live-preview active state.</summary>
        internal uint LivePreviewActive { get; private set; }

        /// <summary>Overrides optional native exports with the managed delegates.</summary>
        /// <returns>A scope that restores the prior export addresses.</returns>
        internal IDisposable OverrideExports() => DwmApi.OverrideOptionalExportsForTesting(
            Marshal.GetFunctionPointerForDelegate(_enableComposition),
            Marshal.GetFunctionPointerForDelegate(_flip3D),
            Marshal.GetFunctionPointerForDelegate(_getSharedSurface),
            Marshal.GetFunctionPointerForDelegate(_updateWindowShared),
            Marshal.GetFunctionPointerForDelegate(_activateLivePreview));

        /// <summary>Writes deterministic shared-surface results.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="adapterLuid">The adapter LUID.</param>
        /// <param name="one">The first undocumented option.</param>
        /// <param name="two">The second undocumented option.</param>
        /// <param name="format">The Direct3D format pointer.</param>
        /// <param name="sharedHandle">The shared-handle pointer.</param>
        /// <param name="unknown">The undocumented option.</param>
        /// <returns>A deterministic result.</returns>
        private static int GetSharedSurface(
            IntPtr windowHandle,
            long adapterLuid,
            uint one,
            uint two,
            IntPtr format,
            IntPtr sharedHandle,
            ulong unknown)
        {
            _ = windowHandle;
            _ = adapterLuid;
            _ = one;
            _ = two;
            _ = unknown;
            Marshal.WriteInt32(format, (int)UpdatedSharedSurfaceFormat);
            Marshal.WriteIntPtr(sharedHandle, new(ReturnedSharedSurfaceHandle));
            return SharedSurfaceResult;
        }

        /// <summary>Reports deterministic shared-window update success.</summary>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="one">The first undocumented option.</param>
        /// <param name="two">The second undocumented option.</param>
        /// <param name="three">The third undocumented option.</param>
        /// <param name="monitorHandle">The monitor handle.</param>
        /// <param name="unknown">The undocumented option.</param>
        /// <returns>A deterministic result.</returns>
        private static int UpdateWindowShared(
            IntPtr windowHandle,
            int one,
            int two,
            int three,
            IntPtr monitorHandle,
            IntPtr unknown)
        {
            _ = windowHandle;
            _ = one;
            _ = two;
            _ = three;
            _ = monitorHandle;
            _ = unknown;
            return UpdateWindowResult;
        }

        /// <summary>Records the DWM composition action.</summary>
        /// <param name="compositionAction">The requested composition action.</param>
        /// <returns>A successful result.</returns>
        private HResult EnableComposition(uint compositionAction)
        {
            CapturedCompositionAction = compositionAction;
            return HResult.Ok;
        }

        /// <summary>Reports successful Flip3D execution.</summary>
        /// <returns>A non-zero native success value.</returns>
        private int StartOrStopFlip3D()
        {
            _flip3DRequests++;
            return _flip3DRequests;
        }

        /// <summary>Records the live-preview active state.</summary>
        /// <param name="active">The requested active state.</param>
        /// <param name="windowHandle">The target window handle.</param>
        /// <param name="onTopHandle">The topmost window handle.</param>
        /// <param name="unknown">The undocumented option.</param>
        /// <returns>A successful result.</returns>
        private HResult ActivateLivePreview(uint active, IntPtr windowHandle, IntPtr onTopHandle, uint unknown)
        {
            _ = windowHandle;
            _ = onTopHandle;
            _ = unknown;
            LivePreviewActive = active;
            return HResult.Ok;
        }
    }
}
