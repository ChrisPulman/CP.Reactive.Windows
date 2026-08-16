// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Interop.Com;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Security;
using CP.ReactiveUI.Primitives.Windows.Native.Security.Enums;
using Microsoft.Win32.SafeHandles;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Final deterministic coverage for kernel, registry, and COM native wrappers.</summary>
public sealed class CoverageFinalKernelTests
{
    /// <summary>Defines a successful Win32 result.</summary>
    private const int Success = 0;

    /// <summary>Defines a synthetic Win32 failure result.</summary>
    private const int Failure = 5;

    /// <summary>Defines the AppModel no-package identity result.</summary>
    private const int AppModelErrorNoPackage = 15_700;

    /// <summary>Defines a fake registry handle value.</summary>
    private const int RegistryHandleValue = 0x1234;

    /// <summary>Defines a fake active COM object pointer value.</summary>
    private const int ActiveObjectPointerValue = 0x5678;

    /// <summary>Defines the expected one-item count.</summary>
    private const int SingleCount = 1;

    /// <summary>Defines the expected two-item count.</summary>
    private const int TwoCount = 2;

    /// <summary>Defines a deterministic module path.</summary>
    private const string ModulePath = @"C:\Windows\System32\module.dll";

    /// <summary>Defines a deterministic image path.</summary>
    private const string ImagePath = @"\Device\HarddiskVolume1\process.exe";

    /// <summary>Defines a deterministic package full name.</summary>
    private const string PackageFullName = "CP.Reactive.Package_1.0.0.0_x64__coverage";

    /// <summary>Defines a deterministic registry subkey.</summary>
    private const string RegistrySubKey = @"Software\CP.Reactive.Windows.Tests";

    /// <summary>Defines a deterministic program identifier.</summary>
    private const string ProgramId = "Coverage.Application";

    /// <summary>Defines a deterministic class identifier.</summary>
    private static readonly Guid ClassId = new("12345678-1234-4321-9876-1234567890AB");

    /// <summary>Covers PSAPI operation forwarding and null-return branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PsApi_OperationsCoverSuccessAndFailureBranchesAsync()
    {
        var emptyWorkingSetCalls = 0;
        using var successScope = OverridePsApiForSuccess(() => emptyWorkingSetCalls++);

        PsApi.EmptyWorkingSet();
        var modulePath = PsApi.GetModuleFilename(IntPtr.Zero, IntPtr.Zero);
        var imagePath = PsApi.GetProcessImageFileName(IntPtr.Zero);
        var copied = CopyImagePathToStackBuffer();

        await Assert.That(copied).IsEqualTo(ImagePath.Length);
        await Assert.That(emptyWorkingSetCalls).IsEqualTo(SingleCount);
        await Assert.That(modulePath).IsEqualTo(ModulePath);
        await Assert.That(imagePath).IsEqualTo(ImagePath);

        using var failureScope = OverridePsApiForFailure();

        await Assert.That(PsApi.GetModuleFilename(IntPtr.Zero, IntPtr.Zero)).IsNull();
        await Assert.That(PsApi.GetProcessImageFileName(IntPtr.Zero)).IsNull();
    }

    /// <summary>Covers package identity version and package-name branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PackageInfo_OperationsCoverVersionNoPackageAndPackageNameBranchesAsync()
    {
        using (OverridePackageInfoForNoPackage())
        {
            await Assert.That(PackageInfo.CurrentPackageFullName).IsNull();
            await Assert.That(PackageInfo.IsRunningOnUwp).IsFalse();
        }

        var packageNameCalls = 0;
        using (OverridePackageInfoForSuccess(() => packageNameCalls++))
        {
            await Assert.That(PackageInfo.CurrentPackageFullName).IsEqualTo(PackageFullName);
            await Assert.That(PackageInfo.IsRunningOnUwp).IsTrue();
            await Assert.That(packageNameCalls).IsEqualTo(TwoCount * TwoCount);
        }

        using (OverridePackageInfoForFetchFailure())
        {
            await Assert.That(PackageInfo.GetCurrentPackageFullNameCore()).IsNull();
        }
    }

    /// <summary>Covers Advapi token failure and registry monitor error paths.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task AdvapiAndRegistryMonitor_OperationsCoverFailureBranchesAsync()
    {
        var tokenCalls = 0;
        using (Advapi32Api.OverrideOperationsForTesting(
            ConvertSidToStringSidFailure,
            static (_, _, _, _, _) => Success,
            OpenRegistryKeySuccess,
            (IntPtr tokenHandle, TokenInformationClasses tokenInformationClasses, IntPtr tokenInformation, int tokenInformationLength, out int returnLength) =>
            {
                tokenCalls++;
                returnLength = sizeof(int);
                return tokenInformation == IntPtr.Zero;
            }))
        {
            await Assert.That(Advapi32Api.GetCurrentSessionId(IntPtr.Zero)).IsEqualTo(string.Empty);
            await Assert.That(tokenCalls).IsEqualTo(TwoCount);
        }

        using (Advapi32Api.OverrideOperationsForTesting(
            ConvertSidToStringSidFailure,
            static (_, _, _, _, _) => Success,
            OpenRegistryKeyFailure,
            GetTokenInformationFailure))
        {
            var openFailureObserver = new CollectingObserver<RxVoid>();
            using var subscription = RegistryMonitor
                .ObserveChanges(RegistryHive.CurrentUser, RegistrySubKey, Sequencer.CurrentThread)
                .Subscribe(openFailureObserver);

            await Assert.That(openFailureObserver.Error).IsNotNull();
        }

        using (Advapi32Api.OverrideOperationsForTesting(
            ConvertSidToStringSidFailure,
            static (_, _, _, _, _) => Failure,
            OpenRegistryKeySuccess,
            GetTokenInformationFailure))
        {
            var notifyFailureObserver = new CollectingObserver<RxVoid>();
            using var subscription = RegistryMonitor
                .ObserveChanges(RegistryHive.CurrentUser, RegistrySubKey, RegistryNotifyFilter.ChangeName)
                .Subscribe(notifyFailureObserver);

            await Assert.That(notifyFailureObserver.Error).IsNotNull();
        }
    }

    /// <summary>Covers OLE Automation active-object overloads and COM disposal branches.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task OleAut32AndDisposableCom_OperationsCoverNullAndDisposeBranchesAsync()
    {
        using var scope = OleAut32Api.OverrideOperationsForTesting(GetActiveObjectFailure, static _ => ClassId);

        var classId = ClassId;
        await Assert.That(OleAut32Api.GetActiveObject(ref classId)).IsNull();
        await Assert.That(OleAut32Api.GetActiveObject(ProgramId)).IsNull();
        await Assert.That(OleAut32Api.GetActiveObject(ProgramId, static activeObject => activeObject)).IsNull();
        await Assert.That(() => OleAut32Api.GetActiveObject<object>(ref classId, null)).Throws<ArgumentNullException>();

        using (OleAut32Api.OverrideOperationsForTesting(
            GetActiveObjectWithoutPointer,
            static _ => ClassId,
            static _ => new object(),
            static _ => Success))
        {
            await Assert.That(OleAut32Api.GetActiveObject(ProgramId)).IsNull();
        }

        var comObject = new object();
        var releasedPointer = IntPtr.Zero;
        using (OleAut32Api.OverrideOperationsForTesting(
            GetActiveObjectSuccess,
            static _ => ClassId,
            _ => comObject,
            pointer =>
            {
                releasedPointer = pointer;
                return Success;
            }))
        {
            using var activeObject = OleAut32Api.GetActiveObject(ProgramId);
            await Assert.That(activeObject.ComObject).IsEqualTo(comObject);
        }

        var releaseCalls = 0;
        var disposable = new DisposableComImplementation<object>(
            comObject,
            static _ => true,
            _ =>
            {
                releaseCalls++;
                return Success;
            });

        disposable.Dispose(disposing: false);
        await Assert.That(disposable.ComObject).IsEqualTo(comObject);

        disposable.Dispose();
        await Assert.That(disposable.ComObject).IsNull();
        await Assert.That(releasedPointer).IsEqualTo(new(ActiveObjectPointerValue));
        await Assert.That(releaseCalls).IsEqualTo(SingleCount);
    }

    /// <summary>Creates successful PSAPI operations.</summary>
    /// <param name="onEmptyWorkingSet">Called when the working set is emptied.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    private static IDisposable OverridePsApiForSuccess(Action onEmptyWorkingSet)
    {
        unsafe
        {
            return PsApi.OverrideOperationsForTesting(
                _ =>
                {
                    onEmptyWorkingSet();
                    return Success;
                },
                WriteModulePath,
                WriteImagePath);
        }
    }

    /// <summary>Creates failing PSAPI operations.</summary>
    /// <returns>A scope that restores the previous operations.</returns>
    private static IDisposable OverridePsApiForFailure()
    {
        unsafe
        {
            return PsApi.OverrideOperationsForTesting(
                static _ => Failure,
                static (_, _, _, _) => Success,
                static (_, _, _) => Success);
        }
    }

    /// <summary>Copies the configured image path through the pointer-based PSAPI overload.</summary>
    /// <returns>The copied character count.</returns>
    private static unsafe int CopyImagePathToStackBuffer()
    {
        var imageBuffer = new char[ImagePath.Length];
        fixed (char* imageBufferPointer = imageBuffer)
        {
            return PsApi.GetProcessImageFileName(IntPtr.Zero, imageBufferPointer, imageBuffer.Length);
        }
    }

    /// <summary>Creates package operations for a process without package identity.</summary>
    /// <returns>A scope that restores the previous operations.</returns>
    private static unsafe IDisposable OverridePackageInfoForNoPackage() =>
        PackageInfo.OverrideOperationsForTesting(
            static (_, _, _, _) => false,
            static (ref int length, char* packageFullName) =>
            {
                length = Success;
                return Success;
            });

    /// <summary>Creates successful package identity operations.</summary>
    /// <param name="onPackageNameCall">Called for each package-name operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    private static unsafe IDisposable OverridePackageInfoForSuccess(Action onPackageNameCall) =>
        PackageInfo.OverrideOperationsForTesting(
            static (_, _, _, _) => true,
            (ref int length, char* packageFullName) =>
            {
                onPackageNameCall();
                if (packageFullName is null)
                {
                    length = PackageFullName.Length;
                    return Success;
                }

                _ = WriteText(packageFullName, length, PackageFullName);
                return Success;
            });

    /// <summary>Creates package operations whose second package-name call reports no identity.</summary>
    /// <returns>A scope that restores the previous operations.</returns>
    private static unsafe IDisposable OverridePackageInfoForFetchFailure() =>
        PackageInfo.OverrideOperationsForTesting(
            static (_, _, _, _) => true,
            static (ref int length, char* packageFullName) =>
            {
                if (packageFullName is null)
                {
                    length = SingleCount;
                    return Success;
                }

                return AppModelErrorNoPackage;
            });

    /// <summary>Writes the deterministic module path.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="moduleHandle">The module handle.</param>
    /// <param name="filename">The output filename buffer.</param>
    /// <param name="size">The output buffer size.</param>
    /// <returns>The copied character count.</returns>
    private static unsafe int WriteModulePath(IntPtr processHandle, IntPtr moduleHandle, char* filename, int size)
    {
        GC.KeepAlive(processHandle);
        GC.KeepAlive(moduleHandle);
        return WriteText(filename, size, ModulePath);
    }

    /// <summary>Writes the deterministic image path.</summary>
    /// <param name="processHandle">The process handle.</param>
    /// <param name="imageFileName">The output image filename buffer.</param>
    /// <param name="size">The output buffer size.</param>
    /// <returns>The copied character count.</returns>
    private static unsafe int WriteImagePath(IntPtr processHandle, char* imageFileName, int size)
    {
        GC.KeepAlive(processHandle);
        return WriteText(imageFileName, size, ImagePath);
    }

    /// <summary>Writes text into a native character buffer.</summary>
    /// <param name="destination">The destination buffer.</param>
    /// <param name="capacity">The destination capacity.</param>
    /// <param name="value">The value to copy.</param>
    /// <returns>The copied character count.</returns>
    private static unsafe int WriteText(char* destination, int capacity, string value)
    {
        var length = Math.Min(capacity, value.Length);
        for (var i = 0; i < length; i++)
        {
            destination[i] = value[i];
        }

        return length;
    }

    /// <summary>Simulates a failed SID conversion.</summary>
    /// <param name="sid">The SID pointer.</param>
    /// <param name="sidString">The SID string pointer.</param>
    /// <returns><see langword="false"/>.</returns>
    private static bool ConvertSidToStringSidFailure(IntPtr sid, out IntPtr sidString)
    {
        GC.KeepAlive(sid);
        sidString = IntPtr.Zero;
        return false;
    }

    /// <summary>Simulates a successful registry key open.</summary>
    /// <param name="key">The parent registry key.</param>
    /// <param name="subKey">The subkey name.</param>
    /// <param name="options">The open options.</param>
    /// <param name="desiredAccess">The desired access rights.</param>
    /// <param name="openedKey">The opened key handle.</param>
    /// <returns>A successful Win32 result.</returns>
    private static int OpenRegistryKeySuccess(
        IntPtr key,
        string subKey,
        RegistryOpenOptions options,
        RegistryKeySecurityAccessRights desiredAccess,
        out SafeRegistryHandle openedKey)
    {
        GC.KeepAlive(key);
        GC.KeepAlive(subKey);
        GC.KeepAlive(options);
        GC.KeepAlive(desiredAccess);
        openedKey = new(new(RegistryHandleValue), ownsHandle: false);
        return Success;
    }

    /// <summary>Simulates a failed registry key open.</summary>
    /// <param name="key">The parent registry key.</param>
    /// <param name="subKey">The subkey name.</param>
    /// <param name="options">The open options.</param>
    /// <param name="desiredAccess">The desired access rights.</param>
    /// <param name="openedKey">The opened key handle.</param>
    /// <returns>A failing Win32 result.</returns>
    private static int OpenRegistryKeyFailure(
        IntPtr key,
        string subKey,
        RegistryOpenOptions options,
        RegistryKeySecurityAccessRights desiredAccess,
        out SafeRegistryHandle openedKey)
    {
        GC.KeepAlive(key);
        GC.KeepAlive(subKey);
        GC.KeepAlive(options);
        GC.KeepAlive(desiredAccess);
        openedKey = null;
        return Failure;
    }

    /// <summary>Simulates a failed token information operation.</summary>
    /// <param name="tokenHandle">The token handle.</param>
    /// <param name="tokenInformationClasses">The token information class.</param>
    /// <param name="tokenInformation">The token information buffer.</param>
    /// <param name="tokenInformationLength">The token information buffer length.</param>
    /// <param name="returnLength">The required buffer length.</param>
    /// <returns><see langword="false"/>.</returns>
    private static bool GetTokenInformationFailure(
        IntPtr tokenHandle,
        TokenInformationClasses tokenInformationClasses,
        IntPtr tokenInformation,
        int tokenInformationLength,
        out int returnLength)
    {
        GC.KeepAlive(tokenHandle);
        GC.KeepAlive(tokenInformationClasses);
        GC.KeepAlive(tokenInformation);
        GC.KeepAlive(tokenInformationLength);
        returnLength = Success;
        return false;
    }

    /// <summary>Simulates a failed active-object lookup.</summary>
    /// <param name="classId">The class identifier.</param>
    /// <param name="reserved">The reserved pointer.</param>
    /// <param name="activeObject">The active object pointer.</param>
    /// <returns>A failing HRESULT.</returns>
    private static HResult GetActiveObjectFailure(ref Guid classId, IntPtr reserved, out IntPtr activeObject)
    {
        GC.KeepAlive(reserved);
        activeObject = IntPtr.Zero;
        return HResult.Fail;
    }

    /// <summary>Simulates a successful active-object lookup without returning an object.</summary>
    /// <param name="classId">The class identifier.</param>
    /// <param name="reserved">The reserved pointer.</param>
    /// <param name="activeObject">The active object pointer.</param>
    /// <returns>A successful HRESULT.</returns>
    private static HResult GetActiveObjectWithoutPointer(
        ref Guid classId,
        IntPtr reserved,
        out IntPtr activeObject)
    {
        GC.KeepAlive(reserved);
        activeObject = IntPtr.Zero;
        return HResult.Ok;
    }

    /// <summary>Simulates a successful active-object lookup.</summary>
    /// <param name="classId">The class identifier.</param>
    /// <param name="reserved">The reserved pointer.</param>
    /// <param name="activeObject">The active object pointer.</param>
    /// <returns>A successful HRESULT.</returns>
    private static HResult GetActiveObjectSuccess(
        ref Guid classId,
        IntPtr reserved,
        out IntPtr activeObject)
    {
        GC.KeepAlive(reserved);
        activeObject = new(ActiveObjectPointerValue);
        return HResult.Ok;
    }

    /// <summary>Collects observable notifications for assertion.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    private sealed class CollectingObserver<T> : IObserver<T>
    {
        /// <summary>Gets the observed terminal error.</summary>
        public Exception Error { get; private set; }

        /// <inheritdoc/>
        public void OnCompleted()
        {
        }

        /// <inheritdoc/>
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc/>
        public void OnNext(T value)
        {
        }
    }
}
