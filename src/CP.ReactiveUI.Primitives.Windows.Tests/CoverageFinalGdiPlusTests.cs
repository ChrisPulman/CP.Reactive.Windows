// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using GdiPlusApi = global::CP.ReactiveUI.Primitives.Windows.Native.Gdi.GdiPlusApi;
using GdiPlusBlurOperations = global::CP.ReactiveUI.Primitives.Windows.Native.Gdi.GdiPlusBlurOperations;
using GdiPlusBlurState = global::CP.ReactiveUI.Primitives.Windows.Native.Gdi.GdiPlusBlurState;
using GdiPlusStatus = global::CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums.GdiPlusStatus;
using GpUnit = global::CP.ReactiveUI.Primitives.Windows.Native.Gdi.Enums.GpUnit;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides final deterministic coverage for GDI+ blur orchestration.</summary>
public sealed class CoverageFinalGdiPlusTests
{
    /// <summary>Defines the minimum supported modern blur radius.</summary>
    private const int MinimumModernBlurRadius = 20;

    /// <summary>Defines an unsupported modern blur radius.</summary>
    private const int UnsupportedModernBlurRadius = 19;

    /// <summary>Defines a deterministic native pointer value.</summary>
    private const int NativePointerValue = 1234;

    /// <summary>Defines the width of the test bitmap.</summary>
    private const int BitmapWidth = 2;

    /// <summary>Defines the height of the test bitmap.</summary>
    private const int BitmapHeight = 2;

    /// <summary>Defines the first GDI+ major Windows version.</summary>
    private const int WindowsVistaMajorVersion = 6;

    /// <summary>Defines the Windows 8 minor version that requires the legacy-radius workaround.</summary>
    private const int WindowsEightMinorVersion = 2;

    /// <summary>Defines the modern Windows major version used by deterministic tests.</summary>
    private const int WindowsTenMajorVersion = 10;

    /// <summary>Defines the minor Windows version used by deterministic tests.</summary>
    private const int DefaultWindowsMinorVersion = 0;

    /// <summary>Defines a GDI+ failure status.</summary>
    private const GdiPlusStatus GenericError = GdiPlusStatus.GenericError;

    /// <summary>Defines the private native-handle field name.</summary>
    private const string PrivateNativeHandleFieldName = "_nativeHandle";

    /// <summary>Defines the expected number of native-handle lookups for drawing.</summary>
    private const int ExpectedDrawNativeHandleLookupCount = 0;

    /// <summary>Tests platform and radius blur-support decisions.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task IsBlurPossible_EvaluatesPlatformStateAndRadiusBranchesAsync()
    {
        await Assert.That(GdiPlusApi.IsBlurPossible(false, new(WindowsTenMajorVersion, DefaultWindowsMinorVersion), MinimumModernBlurRadius)).IsFalse();
        await Assert.That(GdiPlusApi.IsBlurPossible(true, new(5, DefaultWindowsMinorVersion), MinimumModernBlurRadius)).IsFalse();
        await Assert.That(GdiPlusApi.IsBlurPossible(true, new(WindowsVistaMajorVersion, 1), 0)).IsTrue();
        await Assert.That(GdiPlusApi.IsBlurPossible(true, new(WindowsVistaMajorVersion, WindowsEightMinorVersion), MinimumModernBlurRadius)).IsFalse();
        await Assert.That(GdiPlusApi.IsBlurPossible(true, new(WindowsTenMajorVersion, DefaultWindowsMinorVersion), UnsupportedModernBlurRadius)).IsFalse();
        await Assert.That(GdiPlusApi.IsBlurPossible(true, new(WindowsTenMajorVersion, DefaultWindowsMinorVersion), MinimumModernBlurRadius)).IsTrue();
        await Assert.That(static () => GdiPlusApi.IsBlurPossible(true, null!, MinimumModernBlurRadius)).Throws<ArgumentNullException>();
    }

    /// <summary>Tests deterministic state replacement and restoration.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ReplaceStateForTesting_OverridesAndRestoresOperationsAsync()
    {
        var recording = new RecordingOperations();
        var previous = ReplaceState(recording, true);

        try
        {
            await Assert.That(GdiPlusApi.IsBlurPossible(MinimumModernBlurRadius)).IsTrue();
        }
        finally
        {
            GdiPlusApi.RestoreStateForTesting(previous);
        }

        await Assert.That(static () => GdiPlusApi.ReplaceStateForTesting(null!, true)).Throws<ArgumentNullException>();
    }

    /// <summary>Tests native operation factory and native handle reflection helpers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeFactoryAndHandleReflection_ReturnExpectedValuesAsync()
    {
        var nativeOperations = GdiPlusBlurOperations.CreateNative();
        var owner = new NativeHandleOwner((nint)NativePointerValue);

        await Assert.That(nativeOperations.GetOperatingSystemVersion()).IsEqualTo(GdiPlusApi.GetOperatingSystemVersion());
        await Assert.That(owner.HasNativeHandle).IsTrue();
        await Assert.That(GdiPlusApi.GetNativeHandleForTesting(null!, PrivateNativeHandleFieldName)).IsEqualTo(IntPtr.Zero);
        await Assert.That(GdiPlusApi.GetNativeHandleForTesting(owner, PrivateNativeHandleFieldName)).IsEqualTo((nint)NativePointerValue);
        await Assert.That(nativeOperations.GetNativeHandle(owner, PrivateNativeHandleFieldName)).IsEqualTo((nint)NativePointerValue);
        await Assert.That(GdiPlusApi.GetNativeHandleForTesting(owner, "missing")).IsEqualTo(IntPtr.Zero);
    }

    /// <summary>Tests writing blur parameters to unmanaged memory.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WriteBlurParameters_RoundTripsThroughUnmanagedMemoryAsync()
    {
        var expected = BlurParams.Create(MinimumModernBlurRadius, true);
        var memory = Marshal.AllocHGlobal(Marshal.SizeOf<BlurParams>());

        try
        {
            GdiPlusApi.WriteBlurParameters(expected, memory);
            var actual = Marshal.PtrToStructure<BlurParams>(memory);

            await Assert.That(actual).IsEqualTo(expected);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    /// <summary>Tests ApplyBlur successful and failed native status paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ApplyBlur_UsesOperationsAndReportsNativeStatusesAsync()
    {
        var success = new RecordingOperations();
        var successResult = RunApplyBlur(success);
        var failure = new RecordingOperations { ApplyStatus = GenericError };
        var failureResult = RunApplyBlur(failure);

        await Assert.That(successResult).IsTrue();
        await Assert.That(success.ApplyCount).IsEqualTo(1);
        await Assert.That(success.CreateCount).IsEqualTo(1);
        await Assert.That(success.SetCount).IsEqualTo(1);
        await Assert.That(success.DeleteCount).IsEqualTo(1);
        await Assert.That(success.FreeCount).IsEqualTo(1);
        await Assert.That(success.LastUseAuxData).IsFalse();
        await Assert.That(success.LastSourceUnit).IsEqualTo(GpUnit.UnitWorld);
        await Assert.That(failureResult).IsFalse();
        await Assert.That(failure.ApplyCount).IsEqualTo(1);
    }

    /// <summary>Tests DrawWithBlur successful and failed native status paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task DrawWithBlur_UsesOperationsAndReportsNativeStatusesAsync()
    {
        var success = new RecordingOperations();
        var successResult = RunDrawWithBlur(success);
        var failure = new RecordingOperations { DrawStatus = GenericError };
        var failureResult = RunDrawWithBlur(failure);

        await Assert.That(successResult).IsTrue();
        await Assert.That(success.DrawCount).IsEqualTo(1);
        await Assert.That(success.NativeHandleFieldNames.Count).IsEqualTo(ExpectedDrawNativeHandleLookupCount);
        await Assert.That(success.LastSourceUnit).IsEqualTo(GpUnit.UnitPixel);
        await Assert.That(failureResult).IsFalse();
        await Assert.That(failure.DrawCount).IsEqualTo(1);
    }

    /// <summary>Tests setup failures, cleanup failures, and disabled blur short-circuit behavior.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BlurOperations_HandleSetupCleanupAndDisabledPathsAsync()
    {
        var disabled = new RecordingOperations();
        var disabledResult = RunApplyBlur(disabled, false);
        var createFailure = new RecordingOperations { CreateStatus = GenericError };
        var createFailureResult = RunApplyBlur(createFailure);
        var setFailure = new RecordingOperations { SetStatus = GenericError };
        var setFailureResult = RunApplyBlur(setFailure);
        var structureFailure = new RecordingOperations { ThrowOnStructure = true };
        var structureFailureResult = RunApplyBlur(structureFailure);
        var cleanupFailure = new RecordingOperations { ThrowOnFree = true };
        var cleanupFailureResult = RunApplyBlur(cleanupFailure, true, out var cleanupFailureAvailability);

        await Assert.That(disabledResult).IsFalse();
        await Assert.That(disabled.CreateCount).IsEqualTo(0);
        await Assert.That(createFailureResult).IsFalse();
        await Assert.That(createFailure.FreeCount).IsEqualTo(1);
        await Assert.That(createFailure.DeleteCount).IsEqualTo(0);
        await Assert.That(setFailureResult).IsFalse();
        await Assert.That(setFailure.DeleteCount).IsEqualTo(1);
        await Assert.That(structureFailureResult).IsFalse();
        await Assert.That(structureFailure.FreeCount).IsEqualTo(1);
        await Assert.That(cleanupFailureResult).IsTrue();
        await Assert.That(cleanupFailureAvailability).IsFalse();
    }

    /// <summary>Tests native exceptions disable later blur attempts.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task BlurOperations_DisableBlurAfterNativeExceptionsAsync()
    {
        var applyException = new RecordingOperations { ThrowOnApply = true };
        var applyResult = RunApplyBlur(applyException, true, out var applyExceptionAvailability);
        var drawException = new RecordingOperations { ThrowOnDraw = true };
        var drawResult = RunDrawWithBlur(drawException);
        var deleteFailure = new RecordingOperations { DeleteStatus = GenericError };
        var deleteFailureResult = RunApplyBlur(deleteFailure);

        await Assert.That(applyResult).IsFalse();
        await Assert.That(applyExceptionAvailability).IsFalse();
        await Assert.That(drawResult).IsFalse();
        await Assert.That(deleteFailureResult).IsTrue();
        await Assert.That(deleteFailure.DeleteCount).IsEqualTo(1);
    }

    /// <summary>Runs ApplyBlur with deterministic operations.</summary>
    /// <param name="recording">The recording operations.</param>
    /// <param name="isBlurEnabled">Whether blur should start enabled.</param>
    /// <returns>The ApplyBlur result.</returns>
    private static bool RunApplyBlur(RecordingOperations recording, bool isBlurEnabled = true) =>
        RunApplyBlur(recording, isBlurEnabled, out _);

    /// <summary>Runs ApplyBlur with deterministic operations and returns resulting availability.</summary>
    /// <param name="recording">The recording operations.</param>
    /// <param name="isBlurEnabled">Whether blur should start enabled.</param>
    /// <param name="isBlurPossible">Receives whether blur remains available.</param>
    /// <returns>The ApplyBlur result.</returns>
    private static bool RunApplyBlur(RecordingOperations recording, bool isBlurEnabled, out bool isBlurPossible)
    {
        var previous = ReplaceState(recording, isBlurEnabled);

        try
        {
            using var bitmap = new Bitmap(BitmapWidth, BitmapHeight);
            var result = GdiPlusApi.ApplyBlur(bitmap, new(0, 0, BitmapWidth, BitmapHeight), MinimumModernBlurRadius, true);
            isBlurPossible = GdiPlusApi.IsBlurPossible(MinimumModernBlurRadius);
            return result;
        }
        finally
        {
            GdiPlusApi.RestoreStateForTesting(previous);
        }
    }

    /// <summary>Runs DrawWithBlur with deterministic operations.</summary>
    /// <param name="recording">The recording operations.</param>
    /// <returns>The DrawWithBlur result.</returns>
    private static bool RunDrawWithBlur(RecordingOperations recording)
    {
        var previous = ReplaceState(recording, true);

        try
        {
            using var source = new Bitmap(BitmapWidth, BitmapHeight);
            using var target = new Bitmap(BitmapWidth, BitmapHeight);
            using var graphics = Graphics.FromImage(target);
            using var transform = new System.Drawing.Drawing2D.Matrix();
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            return GdiPlusApi.DrawWithBlur(graphics, source, new(0, 0, BitmapWidth, BitmapHeight), transform, attributes, MinimumModernBlurRadius, true);
        }
        finally
        {
            GdiPlusApi.RestoreStateForTesting(previous);
        }
    }

    /// <summary>Replaces the GDI+ deterministic test state.</summary>
    /// <param name="recording">The recording operations.</param>
    /// <param name="isBlurEnabled">Whether blur should start enabled.</param>
    /// <returns>The previous state.</returns>
    private static GdiPlusBlurState ReplaceState(RecordingOperations recording, bool isBlurEnabled) =>
        GdiPlusApi.ReplaceStateForTesting(recording.ToOperations(), isBlurEnabled);

    /// <summary>Provides an object with a private native-handle field.</summary>
    /// <param name="nativeHandle">The native handle value.</param>
    private sealed class NativeHandleOwner(IntPtr nativeHandle)
    {
        /// <summary>Stores the private native handle.</summary>
        private readonly IntPtr _nativeHandle = nativeHandle;

        /// <summary>Gets a value indicating whether the native handle is populated.</summary>
        public bool HasNativeHandle => _nativeHandle != IntPtr.Zero;
    }

    /// <summary>Records deterministic GDI+ blur operations.</summary>
    private sealed class RecordingOperations
    {
        /// <summary>Defines a GDI+ success status.</summary>
        private const GdiPlusStatus Ok = GdiPlusStatus.Ok;

        /// <summary>Gets or sets the create-effect status.</summary>
        public GdiPlusStatus CreateStatus { get; set; } = Ok;

        /// <summary>Gets or sets the set-parameters status.</summary>
        public GdiPlusStatus SetStatus { get; set; } = Ok;

        /// <summary>Gets or sets the apply-effect status.</summary>
        public GdiPlusStatus ApplyStatus { get; set; } = Ok;

        /// <summary>Gets or sets the draw-image status.</summary>
        public GdiPlusStatus DrawStatus { get; set; } = Ok;

        /// <summary>Gets or sets the delete-effect status.</summary>
        public GdiPlusStatus DeleteStatus { get; set; } = Ok;

        /// <summary>Gets or sets a value indicating whether parameter writing throws.</summary>
        public bool ThrowOnStructure { get; set; }

        /// <summary>Gets or sets a value indicating whether apply throws.</summary>
        public bool ThrowOnApply { get; set; }

        /// <summary>Gets or sets a value indicating whether draw throws.</summary>
        public bool ThrowOnDraw { get; set; }

        /// <summary>Gets or sets a value indicating whether free throws.</summary>
        public bool ThrowOnFree { get; set; }

        /// <summary>Gets the create-effect call count.</summary>
        public int CreateCount { get; private set; }

        /// <summary>Gets the set-parameters call count.</summary>
        public int SetCount { get; private set; }

        /// <summary>Gets the delete-effect call count.</summary>
        public int DeleteCount { get; private set; }

        /// <summary>Gets the apply-effect call count.</summary>
        public int ApplyCount { get; private set; }

        /// <summary>Gets the draw-image call count.</summary>
        public int DrawCount { get; private set; }

        /// <summary>Gets the free-memory call count.</summary>
        public int FreeCount { get; private set; }

        /// <summary>Gets the last source unit.</summary>
        public GpUnit LastSourceUnit { get; private set; }

        /// <summary>Gets a value indicating whether the last apply call used auxiliary data.</summary>
        public bool LastUseAuxData { get; private set; }

        /// <summary>Gets requested native-handle field names.</summary>
        public List<string> NativeHandleFieldNames { get; } = [];

        /// <summary>Converts the recorder to a GDI+ operation group.</summary>
        /// <returns>The operation group.</returns>
        public GdiPlusBlurOperations ToOperations() =>
            new()
            {
                AllocateHGlobal = AllocateHGlobal,
                StructureToPtr = StructureToPtr,
                FreeHGlobal = FreeHGlobal,
                CreateEffect = CreateEffect,
                SetEffectParameters = SetEffectParameters,
                DeleteEffect = DeleteEffect,
                ApplyEffect = ApplyEffect,
                DrawImageFx = DrawImageFx,
                GetNativeHandle = GetNativeHandle,
                GetOperatingSystemVersion = static () => new(WindowsTenMajorVersion, DefaultWindowsMinorVersion),
            };

        /// <summary>Allocates deterministic unmanaged memory.</summary>
        /// <param name="size">The allocation size.</param>
        /// <returns>The deterministic pointer.</returns>
        private static IntPtr AllocateHGlobal(int size) => new(NativePointerValue + size);

        /// <summary>Writes deterministic blur parameters.</summary>
        /// <param name="parameters">The parameters to write.</param>
        /// <param name="memory">The target pointer.</param>
        private void StructureToPtr(BlurParams parameters, IntPtr memory)
        {
            _ = parameters;
            _ = memory;

            if (ThrowOnStructure)
            {
                throw new InvalidOperationException(nameof(ThrowOnStructure));
            }
        }

        /// <summary>Frees deterministic unmanaged memory.</summary>
        /// <param name="memory">The target pointer.</param>
        private void FreeHGlobal(IntPtr memory)
        {
            _ = memory;
            FreeCount++;
            if (ThrowOnFree)
            {
                throw new InvalidOperationException(nameof(ThrowOnFree));
            }
        }

        /// <summary>Creates a deterministic effect.</summary>
        /// <param name="guid">The effect identifier.</param>
        /// <param name="effect">Receives the effect pointer.</param>
        /// <returns>The configured status.</returns>
        private GdiPlusStatus CreateEffect(ref Guid guid, out IntPtr effect)
        {
            _ = guid;
            CreateCount++;
            effect = CreateStatus == Ok ? new(NativePointerValue) : IntPtr.Zero;
            return CreateStatus;
        }

        /// <summary>Sets deterministic effect parameters.</summary>
        /// <param name="effect">The effect pointer.</param>
        /// <param name="parameters">The parameter pointer.</param>
        /// <param name="size">The parameter size.</param>
        /// <returns>The configured status.</returns>
        private GdiPlusStatus SetEffectParameters(IntPtr effect, IntPtr parameters, uint size)
        {
            _ = effect;
            _ = parameters;
            _ = size;
            SetCount++;
            return SetStatus;
        }

        /// <summary>Deletes a deterministic effect.</summary>
        /// <param name="effect">The effect pointer.</param>
        /// <returns>The configured status.</returns>
        private GdiPlusStatus DeleteEffect(IntPtr effect)
        {
            _ = effect;
            DeleteCount++;
            return DeleteStatus;
        }

        /// <summary>Applies a deterministic effect.</summary>
        /// <param name="bitmap">The bitmap pointer.</param>
        /// <param name="effect">The effect pointer.</param>
        /// <param name="rectOfInterest">The rectangle of interest.</param>
        /// <param name="useAuxData">Whether auxiliary data should be used.</param>
        /// <param name="auxData">The auxiliary data pointer.</param>
        /// <param name="auxDataSize">The auxiliary data size.</param>
        /// <returns>The configured status.</returns>
        private GdiPlusStatus ApplyEffect(
            IntPtr bitmap,
            IntPtr effect,
            ref NativeRect rectOfInterest,
            bool useAuxData,
            IntPtr auxData,
            int auxDataSize)
        {
            _ = bitmap;
            _ = effect;
            _ = rectOfInterest;
            _ = auxData;
            _ = auxDataSize;
            ApplyCount++;
            LastUseAuxData = useAuxData;
            if (ThrowOnApply)
            {
                throw new InvalidOperationException(nameof(ThrowOnApply));
            }

            return ApplyStatus;
        }

        /// <summary>Draws an image through a deterministic effect.</summary>
        /// <param name="graphics">The graphics pointer.</param>
        /// <param name="bitmap">The bitmap pointer.</param>
        /// <param name="source">The source rectangle.</param>
        /// <param name="matrix">The matrix pointer.</param>
        /// <param name="effect">The effect pointer.</param>
        /// <param name="imageAttributes">The image-attributes pointer.</param>
        /// <param name="sourceUnit">The source unit.</param>
        /// <returns>The configured status.</returns>
        private GdiPlusStatus DrawImageFx(
            IntPtr graphics,
            IntPtr bitmap,
            ref NativeRectFloat source,
            IntPtr matrix,
            IntPtr effect,
            IntPtr imageAttributes,
            GpUnit sourceUnit)
        {
            _ = graphics;
            _ = bitmap;
            _ = source;
            _ = matrix;
            _ = effect;
            _ = imageAttributes;
            DrawCount++;
            LastSourceUnit = sourceUnit;
            if (ThrowOnDraw)
            {
                throw new InvalidOperationException(nameof(ThrowOnDraw));
            }

            return DrawStatus;
        }

        /// <summary>Gets a deterministic native handle.</summary>
        /// <param name="instance">The managed instance.</param>
        /// <param name="fieldName">The requested field name.</param>
        /// <returns>The deterministic pointer.</returns>
        private IntPtr GetNativeHandle(object instance, string fieldName)
        {
            _ = instance;
            NativeHandleFieldNames.Add(fieldName);
            return new(NativePointerValue + NativeHandleFieldNames.Count);
        }
    }
}
