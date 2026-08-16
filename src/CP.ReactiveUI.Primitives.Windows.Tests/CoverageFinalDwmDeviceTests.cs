// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for DWM, device notifications, and display topology.</summary>
public sealed class CoverageFinalDwmDeviceTests
{
    /// <summary>The fake recipient handle.</summary>
    private static readonly IntPtr RecipientHandle = new(0x1234);

    /// <summary>The fake registration handle.</summary>
    private static readonly IntPtr RegistrationHandle = new(0x5678);

    /// <summary>Covers renamed device-notification public factories without native registration.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DeviceNotificationRenamedFactories_ReturnObservablePipelinesAsync()
    {
        await Assert.That(DeviceNotification.DeviceNotificationEvents).IsNotNull();
        await Assert.That(DeviceNotification.ObserveDeviceNotifications()).IsEqualTo(DeviceNotification.DeviceNotificationEvents);
        await Assert.That(DeviceNotification.ObserveDeviceNotifications(DeviceInterfaceClass.Unknown)).IsEqualTo(DeviceNotification.DeviceNotificationEvents);
        await Assert.That(DeviceNotification.ObserveDeviceArrivals()).IsNotNull();
        await Assert.That(DeviceNotification.ObserveDeviceRemovals()).IsNotNull();
        await Assert.That(DeviceNotification.ObserveVolumeChanges()).IsNotNull();
        await Assert.That(DeviceNotification.ObserveVolumeAdditions()).IsNotNull();
        await Assert.That(DeviceNotification.ObserveVolumeRemovals()).IsNotNull();
    }

    /// <summary>Covers registration-filter marshalling without calling the native registration API.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task RegisterDeviceNotificationCore_MarshalsFilterAndFreesPointerAsync()
    {
        var filter = DevBroadcastDeviceInterface.Create();
        filter.DeviceClass = DeviceInterfaceClass.Keyboard;
        var capturedFilter = default(DevBroadcastDeviceInterface);
        var capturedRecipient = IntPtr.Zero;
        var capturedFlags = default(DeviceNotifyFlags);

        var result = DeviceNotification.RegisterDeviceNotificationCore(
            RecipientHandle,
            filter,
            DeviceNotifyFlags.AllInterfaceClasses,
            (recipientHandle, notificationFilterPointer, flags) =>
            {
                capturedRecipient = recipientHandle;
                capturedFlags = flags;
                capturedFilter = Marshal.PtrToStructure<DevBroadcastDeviceInterface>(notificationFilterPointer);
                return RegistrationHandle;
            });

        await Assert.That(result).IsEqualTo(RegistrationHandle);
        await Assert.That(capturedRecipient).IsEqualTo(RecipientHandle);
        await Assert.That(capturedFlags).IsEqualTo(DeviceNotifyFlags.AllInterfaceClasses);
        await Assert.That(capturedFilter.DeviceClassGuid).IsEqualTo(filter.DeviceClassGuid);
    }

    /// <summary>Covers volume-broadcast value equality, flags, and drive decoding.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DevBroadcastVolume_DecodesFlagsDrivesAndEqualityAsync()
    {
        using var firstMemory = DeviceBroadcastMemory.CreateVolume(0b101U, 0x0003);
        using var secondMemory = DeviceBroadcastMemory.CreateVolume(0b101U, 0x0003);
        using var differentMemory = DeviceBroadcastMemory.CreateVolume(0b010U, 0x0000);
        var first = Marshal.PtrToStructure<DevBroadcastVolume>(firstMemory.Pointer);
        var second = Marshal.PtrToStructure<DevBroadcastVolume>(secondMemory.Pointer);
        var different = Marshal.PtrToStructure<DevBroadcastVolume>(differentMemory.Pointer);

        await Assert.That(first.Drives).IsEqualTo("AC");
        await Assert.That(first.IsMediaChange).IsTrue();
        await Assert.That(first.IsNetworkVolume).IsTrue();
        await Assert.That(different.IsMediaChange).IsFalse();
        await Assert.That(different.IsNetworkVolume).IsFalse();
        await Assert.That(first.Equals((object)second)).IsTrue();
        await Assert.That(first.Equals((object)"volume")).IsFalse();
        await Assert.That(first == second).IsTrue();
        await Assert.That(first != different).IsTrue();
        await Assert.That(first.GetHashCode()).IsNotEqualTo(different.GetHashCode());
    }

    /// <summary>Covers device-interface invalid parsing and unknown-class branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DevBroadcastDeviceInterface_InvalidNamesAndUnknownClasses_ReturnFallbacksAsync()
    {
        var empty = DevBroadcastDeviceInterface.Test(string.Empty);
        var malformed = DevBroadcastDeviceInterface.Test("USB#VID_0001");
        var noPrefix = DevBroadcastDeviceInterface.Test("USB#VID_0001&PID_0002#");
        var whitespace = DevBroadcastDeviceInterface.Test(@"\\?\# # #{class}");
        var unknownClass = DevBroadcastDeviceInterface.Test(@"\\?\USB#VID_ABCD&PID_1234#XYZ#{class}");

        await Assert.That(empty.DeviceSetupClassGuid).IsNull();
        await Assert.That(malformed.DeviceSetupClassGuid).IsNull();
        await Assert.That(noPrefix.DeviceSetupClassGuid).IsNull();
        await Assert.That(whitespace.DeviceSetupClassGuid).IsNull();
        await Assert.That(unknownClass.DeviceClass).IsEqualTo(DeviceInterfaceClass.Unknown);
        await Assert.That(unknownClass.IsUsb).IsTrue();
        await Assert.That(unknownClass.IsPci).IsFalse();
        await Assert.That(unknownClass.DeviceId).IsNull();
        await Assert.That(unknownClass.VendorId).IsEqualTo("ABCD");
        await Assert.That(unknownClass.ProductId).IsEqualTo("1234");
        await Assert.That(unknownClass.UsbDeviceInfoUri.AbsoluteUri).Contains("v=0xABCD");
        await Assert.That(unknownClass.Equals((object)"device")).IsFalse();
    }

    /// <summary>Covers DWM managed and native value tail branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmValueTypes_CoverManagedAndNativeEqualityTailsAsync()
    {
        const int blurRegionHandle = 42;
        const int destinationTop = 2;
        const int destinationRight = 3;
        const int destinationBottom = 4;
        const int sourceLeft = 5;
        const int sourceTop = 6;
        const int sourceRight = 7;
        const int sourceBottom = 8;
        const byte thumbnailOpacity = 200;
        var blur = new DwmBlurBehind { Enable = true, TransitionOnMaximized = true };
        blur.SetBlurRegion(new(blurRegionHandle));
        var sameBlur = blur;
        var otherBlur = new DwmBlurBehind { Enable = true };
        var nativeBlur = blur.ToNative();
        var sameNativeBlur = blur.ToNative();
        var differentNativeBlur = otherBlur.ToNative();
        NativeRect destination = new(1, destinationTop, destinationRight, destinationBottom);
        NativeRect source = new(sourceLeft, sourceTop, sourceRight, sourceBottom);
        DwmThumbnailProperties thumbnail = new() { Destination = destination, Source = source, Opacity = thumbnailOpacity, Visible = true, SourceClientAreaOnly = true };
        var sameThumbnail = thumbnail;
        var otherThumbnail = new DwmThumbnailProperties { Visible = true };
        var nativeThumbnail = thumbnail.ToNative();
        var sameNativeThumbnail = thumbnail.ToNative();
        var differentNativeThumbnail = otherThumbnail.ToNative();

        nativeBlur.MarkFieldsAsRead();
        nativeThumbnail.MarkFieldsAsRead();

        await Assert.That(blur.Equals((object)sameBlur)).IsTrue();
        await Assert.That(blur.Equals((object)"blur")).IsFalse();
        await Assert.That(blur == sameBlur).IsTrue();
        await Assert.That(blur != otherBlur).IsTrue();
        await Assert.That(blur.GetHashCode()).IsEqualTo(typeof(DwmBlurBehind).GetHashCode());
        await Assert.That(nativeBlur.Equals((object)sameNativeBlur)).IsTrue();
        await Assert.That(nativeBlur.Equals((object)"native")).IsFalse();
        await Assert.That(nativeBlur == sameNativeBlur).IsTrue();
        await Assert.That(nativeBlur != differentNativeBlur).IsTrue();
        await Assert.That(nativeBlur.GetHashCode()).IsNotEqualTo(differentNativeBlur.GetHashCode());

        await Assert.That(thumbnail.Equals((object)sameThumbnail)).IsTrue();
        await Assert.That(thumbnail.Equals((object)"thumbnail")).IsFalse();
        await Assert.That(thumbnail == sameThumbnail).IsTrue();
        await Assert.That(thumbnail != otherThumbnail).IsTrue();
        await Assert.That(thumbnail.GetHashCode()).IsEqualTo(typeof(DwmThumbnailProperties).GetHashCode());
        await Assert.That(nativeThumbnail.Equals((object)sameNativeThumbnail)).IsTrue();
        await Assert.That(nativeThumbnail.Equals((object)"native")).IsFalse();
        await Assert.That(nativeThumbnail == sameNativeThumbnail).IsTrue();
        await Assert.That(nativeThumbnail != differentNativeThumbnail).IsTrue();
        await Assert.That(nativeThumbnail.GetHashCode()).IsNotEqualTo(differentNativeThumbnail.GetHashCode());
    }

    /// <summary>Covers display topology fallback and observable creation branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DisplayTopology_PublicFallbacks_ReturnUsableValuesAsync()
    {
        const int primaryBoundsLeft = 10;
        const int primaryBoundsTop = 20;
        const int primaryBoundsRight = 30;
        const int primaryBoundsBottom = 40;
        const int secondaryBoundsLeft = -5;
        const int secondaryBoundsTop = 5;
        const int secondaryBoundsRight = 10;
        const int secondaryBoundsBottom = 15;
        const int combinedBoundsRight = 45;
        const int combinedBoundsBottom = 55;
        var impossiblePoint = new NativePoint(int.MinValue, int.MinValue);
        var fallbackBounds = DisplayTopology.GetBounds(impossiblePoint);
        var changes = DisplayTopology.ObserveChanges();
        var emptyBounds = DisplayTopology.CalculateScreenBounds([]);
        var combinedBounds = DisplayTopology.CalculateScreenBounds(
        [
            new() { Bounds = new(primaryBoundsLeft, primaryBoundsTop, primaryBoundsRight, primaryBoundsBottom), IsPrimary = true },
            new() { Bounds = new(secondaryBoundsLeft, secondaryBoundsTop, secondaryBoundsRight, secondaryBoundsBottom) },
        ]);
        NativeRect expectedCombinedBounds = new(secondaryBoundsLeft, secondaryBoundsTop, combinedBoundsRight, combinedBoundsBottom);

        await Assert.That(changes).IsNotNull();
        await Assert.That(fallbackBounds.Width >= 0).IsTrue();
        await Assert.That(fallbackBounds.Height >= 0).IsTrue();
        await Assert.That(emptyBounds).IsEqualTo(NativeRect.Empty);
        await Assert.That(combinedBounds).IsEqualTo(expectedCombinedBounds);
    }

    /// <summary>Covers DWM public wrappers that safely return error values for invalid handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DwmInvalidHandleWrappers_ReturnDeterministicFailureValuesAsync()
    {
        var blur = new DwmBlurBehind { Enable = true };
        var blurResult = DwmApi.DwmEnableBlurBehindWindow(IntPtr.Zero, ref blur);
        var rectangleResult = DwmApi.DwmGetWindowAttribute(IntPtr.Zero, DwmWindowAttributes.ExtendedFrameBounds, out NativeRect rectangle, NativeRect.SizeOf);
        var boolResult = DwmApi.DwmGetWindowAttribute(IntPtr.Zero, DwmWindowAttributes.Cloaked, out bool booleanValue, Marshal.SizeOf<bool>());
        var uintResult = DwmApi.DwmGetWindowAttribute(IntPtr.Zero, DwmWindowAttributes.WindowCornerPreference, out uint uintValue, sizeof(uint));
        var extendedFrameResult = DwmApi.GetExtendedFrameBounds(IntPtr.Zero, out var extendedFrameBounds);
        var cloakedResult = DwmApi.IsWindowCloaked(IntPtr.Zero);
        var cornerPreference = DwmApi.GetWindowCornerPreference(IntPtr.Zero);
        var setCornerPreferenceResult = DwmApi.SetWindowCornerPreference(IntPtr.Zero, DwmWindowCornerPreference.Round);
        var sourceSizeResult = DwmApi.DwmQueryThumbnailSourceSize(IntPtr.Zero, out var sourceSize);
        var registerResult = DwmApi.DwmRegisterThumbnail(IntPtr.Zero, IntPtr.Zero, out var thumbnailId);
        var unregisterResult = DwmApi.DwmUnregisterThumbnail(IntPtr.Zero);
        var updateProperties = new DwmThumbnailProperties { Visible = true };
        var updateResult = DwmApi.DwmUpdateThumbnailProperties(IntPtr.Zero, ref updateProperties);
        var setWindowAttributeResult = DwmApi.DwmSetWindowAttribute(IntPtr.Zero, DwmWindowAttributes.WindowCornerPreference, IntPtr.Zero, 0);
        var previewPoint = default(NativePoint);
        var previewResult = DwmApi.DwmSetIconicLivePreviewBitmap(IntPtr.Zero, IntPtr.Zero, ref previewPoint, DwmSetIconicLivePreviewFlags.None);
        var livePreviewResult = DwmApi.DwmpActivateLivePreview(0, IntPtr.Zero, IntPtr.Zero, 0);
        var sharedFormat = 0U;
        var sharedSurfaceResult = DwmApi.GetSharedSurface(IntPtr.Zero, 0, 0, 0, ref sharedFormat, out var sharedHandle, 0);
        var updateSharedResult = DwmApi.UpdateWindowShared(IntPtr.Zero, 0, 0, 0, IntPtr.Zero, IntPtr.Zero);

        await Assert.That(blurResult.Succeeded()).IsFalse();
        await Assert.That(rectangleResult.Succeeded()).IsFalse();
        await Assert.That(boolResult.Succeeded()).IsFalse();
        await Assert.That(uintResult.Succeeded()).IsFalse();
        await Assert.That(extendedFrameResult).IsFalse();
        await Assert.That(extendedFrameBounds).IsEqualTo(NativeRect.Empty);
        await Assert.That(cloakedResult).IsFalse();
        await Assert.That(cornerPreference).IsEqualTo(DwmWindowCornerPreference.Default);
        await Assert.That(setCornerPreferenceResult).IsFalse();
        await Assert.That(rectangle).IsEqualTo(default);
        await Assert.That(booleanValue).IsFalse();
        await Assert.That(uintValue).IsEqualTo(0U);
        await Assert.That(sourceSizeResult.Succeeded()).IsFalse();
        await Assert.That(sourceSize).IsEqualTo(default);
        await Assert.That(registerResult.Succeeded()).IsFalse();
        await Assert.That(thumbnailId).IsEqualTo(IntPtr.Zero);
        await Assert.That(unregisterResult.Succeeded()).IsFalse();
        await Assert.That(updateResult.Succeeded()).IsFalse();
        await Assert.That(setWindowAttributeResult.Succeeded()).IsFalse();
        await Assert.That(previewResult.Succeeded()).IsFalse();
        await Assert.That(livePreviewResult.Succeeded()).IsFalse();
        await Assert.That(sharedSurfaceResult).IsNotEqualTo(0);
        await Assert.That(sharedHandle).IsEqualTo(IntPtr.Zero);
        await Assert.That(updateSharedResult).IsNotEqualTo(0);
    }
}
