// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic tail coverage for core GDI values, geometry, and safe handles.</summary>
public sealed class CoverageFinalCoreGdiValuesTests
{
    /// <summary>The first synthetic integer value.</summary>
    private const int FirstValue = 3;

    /// <summary>The second synthetic integer value.</summary>
    private const int SecondValue = 4;

    /// <summary>The third synthetic integer value.</summary>
    private const int ThirdValue = 5;

    /// <summary>A synthetic bitmap width.</summary>
    private const int BitmapWidth = 7;

    /// <summary>A synthetic bitmap height.</summary>
    private const int BitmapHeight = 9;

    /// <summary>A synthetic bitmap bit count.</summary>
    private const ushort BitmapBitCount = 32;

    /// <summary>A synthetic window or native handle.</summary>
    private const int SyntheticHandle = 42;

    /// <summary>A second synthetic native handle.</summary>
    private const int PreviousHandle = 84;

    /// <summary>The total bytes occupied by three bitfield color masks.</summary>
    private const uint BitfieldMaskBytes = 12U;

    /// <summary>The second unsigned test value.</summary>
    private const uint UIntTwo = 2U;

    /// <summary>The third unsigned test value.</summary>
    private const uint UIntThree = 3U;

    /// <summary>The required test plane count.</summary>
    private const ushort PlaneCount = 2;

    /// <summary>A floating-point rectangle origin.</summary>
    private const float RectangleOrigin = 10F;

    /// <summary>A floating-point rectangle extent.</summary>
    private const float RectangleExtent = 20F;

    /// <summary>A point inside the test rectangle.</summary>
    private const int InsideCoordinate = 15;

    /// <summary>A point immediately before the test rectangle.</summary>
    private const int BeforeRectangleCoordinate = 9;

    /// <summary>A point immediately after the test rectangle.</summary>
    private const int AfterRectangleCoordinate = 31;

    /// <summary>The expected X coordinate after offsetting.</summary>
    private const float OffsetExpectedX = 11F;

    /// <summary>The expected Y coordinate after offsetting.</summary>
    private const float OffsetExpectedY = 12F;

    /// <summary>The second floating-point test value.</summary>
    private const float FloatTwo = 2F;

    /// <summary>The edge coordinate immediately beside the rectangle.</summary>
    private const float DockedEdgeCoordinate = 9F;

    /// <summary>A vertical coordinate outside the rectangle.</summary>
    private const float OutsideVerticalCoordinate = 40F;

    /// <summary>A short rectangle extent.</summary>
    private const float ShortExtent = 5F;

    /// <summary>A left edge that is not docked.</summary>
    private const float NotDockedLeftEdge = 8F;

    /// <summary>The origin of a right-docked rectangle.</summary>
    private const float RightDockedOrigin = 31F;

    /// <summary>The origin of a non-docked rectangle to the right.</summary>
    private const float NotDockedRightOrigin = 32F;

    /// <summary>The expected number of two operations.</summary>
    private const int ExpectedTwoCalls = 2;

    /// <summary>The expected number of three operations.</summary>
    private const int ExpectedThreeCalls = 3;

    /// <summary>Exercises all mutable BITMAPV4HEADER fields and equality short-circuit paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BitmapV4Header_MutableFieldsAndEqualityBranches_ReturnExpectedValuesAsync()
    {
        var endpoints = CieXyzTriple.Create(CieXyz.Create(1U), CieXyz.Create(UIntTwo), CieXyz.Create(UIntThree));
        var header = BitmapV4Header.Create(BitmapWidth, BitmapHeight, BitmapBitCount);
        header.Planes = PlaneCount;
        header.Compression = GdiEnums.BitmapCompressionMethods.BI_BITFIELDS;
        header.XPelsPerMeter = FirstValue;
        header.YPelsPerMeter = SecondValue;
        header.ColorsUsed = ThirdValue;
        header.ColorsImportant = FirstValue;
        header.Endpoints = endpoints;
        header.GammaRed = 1U;
        header.GammaGreen = UIntTwo;
        header.GammaBlue = UIntThree;

        await Assert.That(header.Width).IsEqualTo(BitmapWidth);
        await Assert.That(header.Height).IsEqualTo(BitmapHeight);
        await Assert.That(header.Planes).IsEqualTo(PlaneCount);
        await Assert.That(header.BitCount).IsEqualTo(BitmapBitCount);
        await Assert.That(header.Compression).IsEqualTo(GdiEnums.BitmapCompressionMethods.BI_BITFIELDS);
        await Assert.That(header.XPelsPerMeter).IsEqualTo(FirstValue);
        await Assert.That(header.YPelsPerMeter).IsEqualTo(SecondValue);
        await Assert.That(header.ColorsUsed).IsEqualTo((uint)ThirdValue);
        await Assert.That(header.ColorsImportant).IsEqualTo((uint)FirstValue);
        await Assert.That(header.Endpoints).IsEqualTo(endpoints);
        await Assert.That(header.GammaRed).IsEqualTo(1U);
        await Assert.That(header.GammaGreen).IsEqualTo(UIntTwo);
        await Assert.That(header.GammaBlue).IsEqualTo(UIntThree);
        await Assert.That(header.OffsetToPixels).IsEqualTo(header.Size + BitfieldMaskBytes);

        var firstTupleDifference = header;
        firstTupleDifference.Width++;
        var secondTupleDifference = header;
        secondTupleDifference.SizeImage++;
        var thirdTupleDifference = header;
        thirdTupleDifference.GammaBlue++;

        var sameHeader = header;
        await Assert.That(header.Equals(sameHeader)).IsTrue();
        await Assert.That(header.Equals(firstTupleDifference)).IsFalse();
        await Assert.That(header.Equals(secondTupleDifference)).IsFalse();
        await Assert.That(header.Equals(thirdTupleDifference)).IsFalse();
    }

    /// <summary>Exercises independent CIE XYZ component setters and the corrected triple type.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task CieXyz_ComponentSettersAndTriple_ReturnExpectedValuesAsync()
    {
        var xyz = new CieXyz { X = 1U, Y = UIntTwo, Z = UIntThree };
        var triple = CieXyzTriple.Create(xyz, CieXyz.Create(SecondValue), CieXyz.Create(ThirdValue));

        await Assert.That(xyz.X).IsEqualTo(1U);
        await Assert.That(xyz.Y).IsEqualTo(UIntTwo);
        await Assert.That(xyz.Z).IsEqualTo(UIntThree);
        await Assert.That(triple.Red).IsEqualTo(xyz);
        await Assert.That(triple.Green).IsEqualTo(CieXyz.Create(SecondValue));
        await Assert.That(triple.Blue).IsEqualTo(CieXyz.Create(ThirdValue));
    }

    /// <summary>Exercises object equality alternatives for floating-point native points.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativePointFloat_ObjectEqualityAlternatives_ReturnExpectedValuesAsync()
    {
        var point = new NativePointFloat(FirstValue, SecondValue);

        await Assert.That(point.Equals((object)new NativePoint(FirstValue, SecondValue))).IsTrue();
        await Assert.That(point.Equals((object)new System.Windows.Point(FirstValue, SecondValue))).IsTrue();
        await Assert.That(point.Equals((object)string.Empty)).IsFalse();
    }

    /// <summary>Exercises floating-point rectangle constructors, corners, extensions, and contains branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeRectFloat_GeometryTailBranches_ReturnExpectedValuesAsync()
    {
        var rect = new NativeRectFloat(RectangleOrigin, RectangleOrigin, new NativeSizeFloat(RectangleExtent, RectangleExtent));

        await Assert.That(rect.BottomLeft).IsEqualTo(new(RectangleOrigin, RectangleOrigin + RectangleExtent));
        await Assert.That(rect.TopLeft).IsEqualTo(new(RectangleOrigin, RectangleOrigin));
        await Assert.That(rect.BottomRight).IsEqualTo(new(RectangleOrigin + RectangleExtent, RectangleOrigin + RectangleExtent));
        await Assert.That(rect.TopRight).IsEqualTo(new(RectangleOrigin + RectangleExtent, RectangleOrigin));
        await Assert.That(rect.Contains(new(InsideCoordinate, InsideCoordinate))).IsTrue();
        await Assert.That(rect.Contains(new(BeforeRectangleCoordinate, InsideCoordinate))).IsFalse();
        await Assert.That(rect.Contains(new(AfterRectangleCoordinate, InsideCoordinate))).IsFalse();
        await Assert.That(rect.Contains(new(InsideCoordinate, BeforeRectangleCoordinate))).IsFalse();
        await Assert.That(rect.Contains(new(InsideCoordinate, AfterRectangleCoordinate))).IsFalse();
        await Assert.That(rect.Contains(new NativePointFloat(InsideCoordinate, InsideCoordinate))).IsTrue();
        await Assert.That(rect.Offset(new NativePointFloat(1F, FloatTwo))).IsEqualTo(new(OffsetExpectedX, OffsetExpectedY, RectangleExtent, RectangleExtent));
        await Assert.That(rect.MoveTo(new NativePointFloat(1F, FloatTwo))).IsEqualTo(new(1F, FloatTwo, RectangleExtent, RectangleExtent));

        var dockedLeftTopOutside = new NativeRectFloat(0F, 0F, DockedEdgeCoordinate, InsideCoordinate);
        var dockedLeftOutside = new NativeRectFloat(0F, OutsideVerticalCoordinate, DockedEdgeCoordinate, ShortExtent);
        var notDockedLeft = new NativeRectFloat(0F, RectangleOrigin, NotDockedLeftEdge, RectangleExtent);
        var dockedRightTopOutside = new NativeRectFloat(RightDockedOrigin, 0F, DockedEdgeCoordinate, InsideCoordinate);
        var dockedRightOutside = new NativeRectFloat(RightDockedOrigin, OutsideVerticalCoordinate, DockedEdgeCoordinate, ShortExtent);
        var notDockedRight = new NativeRectFloat(NotDockedRightOrigin, RectangleOrigin, DockedEdgeCoordinate, RectangleExtent);

        await Assert.That(dockedLeftTopOutside.IsDockedToLeftOf(rect)).IsTrue();
        await Assert.That(dockedLeftOutside.IsDockedToLeftOf(rect)).IsFalse();
        await Assert.That(notDockedLeft.IsDockedToLeftOf(rect)).IsFalse();
        await Assert.That(dockedRightTopOutside.IsDockedToRightOf(rect)).IsTrue();
        await Assert.That(dockedRightOutside.IsDockedToRightOf(rect)).IsFalse();
        await Assert.That(notDockedRight.IsDockedToRightOf(rect)).IsFalse();
    }

    /// <summary>Exercises window device-context safe handles through a deterministic composed API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeWindowDcHandle_ComposedApi_CoversFactoriesAndReleaseAsync()
    {
        var api = new WindowDcHandleApiProbe();
        var operations = new WindowDcHandleOperations(api.GetWindowDc, api.GetDc, api.GetDesktopWindow, api.ReleaseDc);
        var previousApi = SafeWindowDcHandle.SwapApi(operations);
        try
        {
            using var invalid = new SafeWindowDcHandle();
            await Assert.That(invalid.IsInvalid).IsTrue();
            await Assert.That(SafeWindowDcHandle.FromWindow(IntPtr.Zero)).IsNull();

            var windowHandle = SafeWindowDcHandle.FromWindow(new(SyntheticHandle));
            var clientHandle = SafeWindowDcHandle.FromWindowClientArea(new(SyntheticHandle));
            var desktopHandle = SafeWindowDcHandle.FromDesktop();
            windowHandle.Dispose();
            clientHandle.Dispose();
            desktopHandle.Dispose();

            await Assert.That(api.GetWindowDcCalls).IsEqualTo(ExpectedTwoCalls);
            await Assert.That(api.GetDcCalls).IsEqualTo(1);
            await Assert.That(api.GetDesktopWindowCalls).IsEqualTo(1);
            await Assert.That(api.ReleaseDcCalls).IsEqualTo(ExpectedThreeCalls);
            await Assert.That(api.LastReleasedDeviceContext).IsEqualTo(new(PreviousHandle));
            await Assert.That(static () => SafeWindowDcHandle.SwapApi(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = SafeWindowDcHandle.SwapApi(previousApi);
        }
    }

    /// <summary>Exercises object and selection safe-handle release behavior without native GDI mutation.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GdiObjectSafeHandles_ComposedApi_CoversReleaseBranchesAsync()
    {
        var api = new GdiSafeHandleApiProbe();
        var operations = new GdiSafeHandleOperations(api.DeleteObject, api.SelectObject, api.RestoreObject);
        var previousApi = GdiSafeHandleApi.Swap(operations);
        try
        {
            using var deviceContext = new SafeNonDisposableObjectHandle(new(SyntheticHandle));
            using var selectedObject = new SafeNonDisposableObjectHandle(new(PreviousHandle));
            using (var selected = new SafeSelectObjectHandle(deviceContext, selectedObject))
            {
                await Assert.That(selected.IsInvalid).IsFalse();
            }

            using (var noDeviceContext = new SafeSelectObjectHandle(null, selectedObject))
            {
                await Assert.That(noDeviceContext.IsInvalid).IsFalse();
            }

            using (var deletedObject = new SafeHBitmapHandle(new IntPtr(SyntheticHandle)))
            {
                await Assert.That(deletedObject.IsInvalid).IsFalse();
            }

            api.DeleteResult = false;
            using (var retainedObject = new SafeDibSectionHandle(new(PreviousHandle)))
            {
                await Assert.That(retainedObject.IsInvalid).IsFalse();
            }

            await Assert.That(api.SelectCalls).IsEqualTo(ExpectedTwoCalls);
            await Assert.That(api.RestoreCalls).IsEqualTo(1);
            await Assert.That(api.DeleteCalls).IsEqualTo(ExpectedTwoCalls);
            await Assert.That(static () => GdiSafeHandleApi.Swap(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = GdiSafeHandleApi.Swap(previousApi);
        }
    }

    /// <summary>Exercises graphics safe-handle release and optional disposal through composed actions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task SafeGraphicsDcHandle_ComposedActions_CoverDisposalBranchesAsync()
    {
        await Assert.That(static () => SafeGraphicsDcHandle.FromGraphics(null)).Throws<ArgumentNullException>();

        var retainedGraphics = new GraphicsReleaseProbe();
        using (var handle = new SafeGraphicsDcHandle(new(SyntheticHandle), false, retainedGraphics.ReleaseDeviceContext, retainedGraphics.DisposeGraphics))
        {
            await Assert.That(handle.IsInvalid).IsFalse();
        }

        var disposedGraphics = new GraphicsReleaseProbe();
        using (var handle = new SafeGraphicsDcHandle(new(PreviousHandle), true, disposedGraphics.ReleaseDeviceContext, disposedGraphics.DisposeGraphics))
        {
            await Assert.That(handle.IsInvalid).IsFalse();
        }

        await Assert.That(retainedGraphics.ReleasedHandle).IsEqualTo(new(SyntheticHandle));
        await Assert.That(retainedGraphics.DisposeCalls).IsEqualTo(0);
        await Assert.That(disposedGraphics.ReleasedHandle).IsEqualTo(new(PreviousHandle));
        await Assert.That(disposedGraphics.DisposeCalls).IsEqualTo(1);
    }

    /// <summary>Records deterministic GDI safe-handle operations.</summary>
    private sealed class GdiSafeHandleApiProbe
    {
        /// <summary>Gets the number of delete calls.</summary>
        internal int DeleteCalls { get; private set; }

        /// <summary>Gets the number of restore calls.</summary>
        internal int RestoreCalls { get; private set; }

        /// <summary>Gets the number of select calls.</summary>
        internal int SelectCalls { get; private set; }

        /// <summary>Gets or sets the delete result.</summary>
        internal bool DeleteResult { get; set; } = true;

        /// <summary>Records a delete operation.</summary>
        /// <param name="objectHandle">The deleted object handle.</param>
        /// <returns>The configured delete result.</returns>
        internal bool DeleteObject(IntPtr objectHandle)
        {
            _ = objectHandle;
            DeleteCalls++;
            return DeleteResult;
        }

        /// <summary>Records a select operation.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="objectHandle">The selected object.</param>
        /// <returns>The synthetic previous object handle.</returns>
        internal IntPtr SelectObject(SafeHandle deviceContext, SafeHandle objectHandle)
        {
            _ = deviceContext;
            _ = objectHandle;
            SelectCalls++;
            return new(PreviousHandle);
        }

        /// <summary>Records a restore operation.</summary>
        /// <param name="deviceContext">The device context.</param>
        /// <param name="objectHandle">The restored object.</param>
        /// <returns>The restored object handle.</returns>
        internal IntPtr RestoreObject(SafeHandle deviceContext, IntPtr objectHandle)
        {
            _ = deviceContext;
            RestoreCalls++;
            return objectHandle;
        }
    }

    /// <summary>Records deterministic window device-context operations.</summary>
    private sealed class WindowDcHandleApiProbe
    {
        /// <summary>Gets the number of client-area device-context calls.</summary>
        internal int GetDcCalls { get; private set; }

        /// <summary>Gets the number of desktop-window calls.</summary>
        internal int GetDesktopWindowCalls { get; private set; }

        /// <summary>Gets the number of whole-window device-context calls.</summary>
        internal int GetWindowDcCalls { get; private set; }

        /// <summary>Gets the last released device-context handle.</summary>
        internal IntPtr LastReleasedDeviceContext { get; private set; }

        /// <summary>Gets the number of release calls.</summary>
        internal int ReleaseDcCalls { get; private set; }

        /// <summary>Records a whole-window device-context request.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The synthetic device-context handle.</returns>
        internal IntPtr GetWindowDc(IntPtr windowHandle)
        {
            _ = windowHandle;
            GetWindowDcCalls++;
            return new(PreviousHandle);
        }

        /// <summary>Records a client-area device-context request.</summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The synthetic device-context handle.</returns>
        internal IntPtr GetDc(IntPtr windowHandle)
        {
            _ = windowHandle;
            GetDcCalls++;
            return new(PreviousHandle);
        }

        /// <summary>Records a desktop-window request.</summary>
        /// <returns>The synthetic desktop-window handle.</returns>
        internal IntPtr GetDesktopWindow()
        {
            GetDesktopWindowCalls++;
            return new(SyntheticHandle);
        }

        /// <summary>Records a device-context release.</summary>
        /// <param name="windowHandle">The owning window.</param>
        /// <param name="deviceContextHandle">The released device context.</param>
        /// <returns><see langword="true"/>.</returns>
        internal bool ReleaseDc(IntPtr windowHandle, IntPtr deviceContextHandle)
        {
            _ = windowHandle;
            ReleaseDcCalls++;
            LastReleasedDeviceContext = deviceContextHandle;
            return true;
        }
    }

    /// <summary>Records composed graphics release operations.</summary>
    private sealed class GraphicsReleaseProbe
    {
        /// <summary>Gets the number of graphics dispose calls.</summary>
        internal int DisposeCalls { get; private set; }

        /// <summary>Gets the released device-context handle.</summary>
        internal IntPtr ReleasedHandle { get; private set; }

        /// <summary>Records a graphics disposal.</summary>
        internal void DisposeGraphics() => DisposeCalls++;

        /// <summary>Records a released graphics device context.</summary>
        /// <param name="deviceContextHandle">The released device-context handle.</param>
        internal void ReleaseDeviceContext(IntPtr deviceContextHandle) => ReleasedHandle = deviceContextHandle;
    }
}
