// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Provides deterministic release coverage for managed Kernel32 wrapper behavior.</summary>
public sealed class CoverageReleaseKernel32Tests
{
    /// <summary>Defines a successful native result.</summary>
    private const int Success = 0;

    /// <summary>Defines a failed native result.</summary>
    private const int Failure = 87;

    /// <summary>Defines a managed process path returned from a synthetic native operation.</summary>
    private const string FullProcessPath = @"C:\Program Files\CP Reactive\process.exe";

    /// <summary>Defines a DOS device prefix returned from a synthetic native operation.</summary>
    private const string DosDevicePrefix = @"\Device\HarddiskVolumeCoverage";

    /// <summary>Defines a DOS process image returned from a synthetic native operation.</summary>
    private const string DosProcessPath = DosDevicePrefix + @"\process.exe";

    /// <summary>Defines a package full name returned from a synthetic native operation.</summary>
    private const string PackageFullName = "CP.Reactive.Windows_1.0.0.0_x64__coverage";

    /// <summary>Defines a package name builder capacity that selects the heap buffer path.</summary>
    private const int LargePackageBuilderCapacity = 513;

    /// <summary>Defines a synthetic first process handle.</summary>
    private static readonly IntPtr FirstProcessHandle = new(1);

    /// <summary>Defines a synthetic second process handle.</summary>
    private static readonly IntPtr SecondProcessHandle = new(2);

    /// <summary>Exercises DLL search and console forwarding without changing process state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Kernel32_ManagedForwardersUseConfiguredOperationsAsync()
    {
        var directories = new List<string>();
        var flags = new List<DefaultDllDirectories>();
        var attachedProcessIds = new List<uint>();

        using var scope = Kernel32Api.OverrideOperationsForTesting(CreateOperations(operations =>
        {
            operations.SetDefaultDllDirectories = value =>
            {
                flags.Add(value);
                return true;
            };
            operations.SetDllDirectory = value =>
            {
                directories.Add(value);
                return true;
            };
            operations.AllocConsole = static () => true;
            operations.AttachConsole = value =>
            {
                attachedProcessIds.Add(value);
                return true;
            };
        }));

        Kernel32Api.PreventDllHijacking();
        Kernel32Api.PreventDllHijacking("C:\\CP-Reactive");

        await Assert.That(Kernel32Api.SetDefaultDllDirectories(DefaultDllDirectories.SearchApplicationDirectory)).IsTrue();
        await Assert.That(Kernel32Api.SetDllDirectory("C:\\CP-Reactive-Direct")).IsTrue();
        await Assert.That(Kernel32Api.AllocConsole()).IsTrue();
        await Assert.That(Kernel32Api.AttachConsole()).IsTrue();
        await Assert.That(Kernel32Api.AttachConsole(FortyTwo)).IsTrue();
        await Assert.That(directories).IsEquivalentTo([string.Empty, "C:\\CP-Reactive", "C:\\CP-Reactive-Direct"]);
        await Assert.That(flags).IsEquivalentTo(
            [
                DefaultDllDirectories.SearchSystem32Directory,
                DefaultDllDirectories.SearchUserDirectories | DefaultDllDirectories.SearchSystem32Directory,
                DefaultDllDirectories.SearchApplicationDirectory,
            ]);
        await Assert.That(attachedProcessIds).IsEquivalentTo([uint.MaxValue, (uint)FortyTwo]);
    }

    /// <summary>Exercises process path retrieval through synthetic process and PSAPI operations.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Kernel32_ProcessPathUsesImageAndDosFallbacksAsync()
    {
        using (var missingProcessScope = Kernel32Api.OverrideOperationsForTesting(CreateOperations()))
        {
            await Assert.That(Kernel32Api.GetProcessPath(NinetyNine)).IsNull();
        }

        using (var processScope = Kernel32Api.OverrideOperationsForTesting(CreateOperations(ConfigureFullProcessPathOperations)))
        using (OverridePsApiForFullProcessPath())
        {
            await Assert.That(Kernel32Api.GetProcessPath(Hundred)).IsEqualTo(FullProcessPath);
        }

        using (var processScope = Kernel32Api.OverrideOperationsForTesting(CreateOperations(ConfigureDosProcessPathOperations)))
        using (OverridePsApiForDosProcessPath())
        {
            string processPath = Kernel32Api.GetProcessPath(Hundred + One);
            await Assert.That(processPath).EndsWith(@"\process.exe");
        }
    }

    /// <summary>Exercises version and package-name result branches without invoking Kernel32.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Kernel32_VersionAndPackageQueriesUseConfiguredOperationsAsync()
    {
        using var scope = Kernel32Api.OverrideOperationsForTesting(CreateOperations(ConfigureVersionAndPackageOperations));

        var version = default(OsVersionInfoEx);
        var failedPackageName = new StringBuilder(PackageFullName.Length);
        var failedPackageLength = failedPackageName.Capacity;
        var smallPackageName = new StringBuilder(PackageFullName.Length);
        var smallPackageLength = smallPackageName.Capacity;
        var largePackageName = new StringBuilder(LargePackageBuilderCapacity);
        var largePackageLength = largePackageName.Capacity;

        await Assert.That(Kernel32Api.GetVersionEx(ref version)).IsFalse();
        await Assert.That(Kernel32Api.GetPackageFullName(IntPtr.Zero, ref failedPackageLength, failedPackageName)).IsEqualTo(Failure);
        await Assert.That(Kernel32Api.GetPackageFullName(IntPtr.Zero, ref smallPackageLength, smallPackageName)).IsEqualTo(Success);
        await Assert.That(Kernel32Api.GetPackageFullName(IntPtr.Zero, ref largePackageLength, largePackageName)).IsEqualTo(Success);
        await Assert.That(smallPackageName.ToString()).IsEqualTo(PackageFullName);
        await Assert.That(largePackageName.ToString()).IsEqualTo(PackageFullName);
    }

    /// <summary>Exercises the heap-backed current-package-name buffer path without a package identity lookup.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task PackageInfo_CurrentPackageFullNameUsesHeapBufferForLargeNamesAsync()
    {
        string expectedPackageName = new('P', LargePackageBuilderCapacity);
        string actualPackageName;
        unsafe
        {
            using var scope = PackageInfo.OverrideOperationsForTesting(
                static (_, _, _, _) => true,
                (ref int length, char* packageName) =>
                {
                    length = expectedPackageName.Length;
                    if (packageName is not null)
                    {
                        _ = WriteText(packageName, length, expectedPackageName);
                    }

                    return Success;
                });

            actualPackageName = PackageInfo.CurrentPackageFullName;
        }

        await Assert.That(actualPackageName).IsEqualTo(expectedPackageName);
    }

    /// <summary>Creates a deterministic Kernel32 operation group.</summary>
    /// <param name="customize">An optional operation-group customizer.</param>
    /// <returns>A deterministic operation group.</returns>
    private static Kernel32Operations CreateOperations(
        Action<Kernel32Operations> customize = null)
    {
        Kernel32Operations operations;
        unsafe
        {
            operations = new Kernel32Operations
            {
                SetDefaultDllDirectories = static _ => true,
                SetDllDirectory = static _ => true,
                AllocConsole = static () => true,
                AttachConsole = static _ => true,
                CloseHandle = static _ => true,
                OpenProcess = static (_, _, _) => IntPtr.Zero,
                QueryDosDevice = static (_, _, _) => 0,
                QueryFullProcessImageName = QueryFullProcessImageNameFailure,
                GetVersionEx = static _ => false,
                GetPackageFullName = GetPackageFullNameFailure,
            };
        }

        customize?.Invoke(operations);
        return operations;
    }

    /// <summary>Configures operations that provide a full process image path.</summary>
    /// <param name="operations">The operations to configure.</param>
    private static void ConfigureFullProcessPathOperations(Kernel32Operations operations)
    {
        operations.OpenProcess = CreateSequentialProcessOpenOperation();
        operations.CloseHandle = static _ => true;
        unsafe
        {
            operations.QueryFullProcessImageName = WriteFullProcessPath;
        }
    }

    /// <summary>Configures operations that provide a DOS process image path.</summary>
    /// <param name="operations">The operations to configure.</param>
    private static void ConfigureDosProcessPathOperations(Kernel32Operations operations)
    {
        operations.OpenProcess = CreateSequentialProcessOpenOperation();
        operations.CloseHandle = static _ => true;
        unsafe
        {
            operations.QueryDosDevice = WriteDosDevicePath;
            operations.QueryFullProcessImageName = QueryFullProcessImageNameFailure;
        }
    }

    /// <summary>Configures operations that provide version and package query results.</summary>
    /// <param name="operations">The operations to configure.</param>
    private static void ConfigureVersionAndPackageOperations(Kernel32Operations operations)
    {
        unsafe
        {
            operations.GetVersionEx = static _ => false;
            operations.GetPackageFullName = CreatePackageNameOperation();
        }
    }

    /// <summary>Overrides PSAPI operations for the full-process-path branch.</summary>
    /// <returns>A scope that restores the PSAPI operations.</returns>
    private static IDisposable OverridePsApiForFullProcessPath()
    {
        unsafe
        {
            return PsApi.OverrideOperationsForTesting(
                static _ => Success,
                static (_, _, _, _) => Success,
                static (_, _, _) => Success);
        }
    }

    /// <summary>Overrides PSAPI operations for the DOS-process-path branch.</summary>
    /// <returns>A scope that restores the PSAPI operations.</returns>
    private static IDisposable OverridePsApiForDosProcessPath()
    {
        unsafe
        {
            return PsApi.OverrideOperationsForTesting(
                static _ => Success,
                static (_, _, _, _) => Success,
                WriteDosProcessPath);
        }
    }

    /// <summary>Creates a package query that fails once and then writes the configured package name.</summary>
    /// <returns>The package query operation.</returns>
    private static unsafe GetPackageFullNameOperation CreatePackageNameOperation()
    {
        var packageCallCount = 0;
        return (IntPtr _, ref int packageNameLength, char* packageName) =>
        {
            packageCallCount++;
            if (packageCallCount == 1)
            {
                return Failure;
            }

            packageNameLength = PackageFullName.Length;
            WriteNullTerminatedText(packageName, PackageFullName);
            return Success;
        };
    }

    /// <summary>Creates an operation that returns a first then second synthetic process handle.</summary>
    /// <returns>The synthetic process-opening operation.</returns>
    private static Func<ProcessAccessRights, bool, int, IntPtr> CreateSequentialProcessOpenOperation()
    {
        var callCount = 0;
        return (_, _, _) =>
        {
            callCount++;
            return callCount == One ? FirstProcessHandle : SecondProcessHandle;
        };
    }

    /// <summary>Writes a full process path into a native buffer.</summary>
    /// <param name="processHandle">The synthetic process handle.</param>
    /// <param name="flags">The query flags.</param>
    /// <param name="executableName">The output buffer.</param>
    /// <param name="size">The buffer size on entry and characters written on success.</param>
    /// <returns><see langword="true"/>.</returns>
    private static unsafe bool WriteFullProcessPath(IntPtr processHandle, uint flags, char* executableName, ref int size)
    {
        GC.KeepAlive(processHandle);
        GC.KeepAlive(flags);
        size = WriteText(executableName, size, FullProcessPath);
        return true;
    }

    /// <summary>Returns a synthetic process-image-name-query failure.</summary>
    /// <param name="processHandle">The synthetic process handle.</param>
    /// <param name="flags">The query flags.</param>
    /// <param name="executableName">The output buffer.</param>
    /// <param name="size">The buffer size on entry and zero on return.</param>
    /// <returns><see langword="false"/>.</returns>
    private static unsafe bool QueryFullProcessImageNameFailure(
        IntPtr processHandle,
        uint flags,
        char* executableName,
        ref int size)
    {
        GC.KeepAlive(processHandle);
        GC.KeepAlive(flags);
        GC.KeepAlive((IntPtr)executableName);
        size = 0;
        return false;
    }

    /// <summary>Writes a synthetic DOS device path into a native buffer.</summary>
    /// <param name="deviceName">The queried device name.</param>
    /// <param name="targetPath">The output buffer.</param>
    /// <param name="maximumLength">The output buffer capacity.</param>
    /// <returns>The character count written.</returns>
    private static unsafe int WriteDosDevicePath(string deviceName, char* targetPath, int maximumLength)
    {
        GC.KeepAlive(deviceName);
        return WriteText(targetPath, maximumLength, DosDevicePrefix);
    }

    /// <summary>Writes a synthetic DOS process path into a native buffer.</summary>
    /// <param name="processHandle">The synthetic process handle.</param>
    /// <param name="imageFileName">The output buffer.</param>
    /// <param name="size">The output buffer capacity.</param>
    /// <returns>The character count written.</returns>
    private static unsafe int WriteDosProcessPath(IntPtr processHandle, char* imageFileName, int size)
    {
        GC.KeepAlive(processHandle);
        return WriteText(imageFileName, size, DosProcessPath);
    }

    /// <summary>Writes text into a native buffer.</summary>
    /// <param name="destination">The output buffer.</param>
    /// <param name="capacity">The output buffer capacity.</param>
    /// <param name="value">The text to write.</param>
    /// <returns>The number of characters written.</returns>
    private static unsafe int WriteText(char* destination, int capacity, string value)
    {
        int length = Math.Min(capacity, value.Length);
        value.AsSpan(0, length).CopyTo(new(destination, length));
        return length;
    }

    /// <summary>Writes null-terminated text into a native buffer.</summary>
    /// <param name="destination">The output buffer.</param>
    /// <param name="value">The text to write.</param>
    private static unsafe void WriteNullTerminatedText(char* destination, string value)
    {
        _ = WriteText(destination, value.Length, value);
        destination[value.Length] = '\0';
    }

    /// <summary>Returns a synthetic package-name-query failure.</summary>
    /// <param name="processHandle">The synthetic process handle.</param>
    /// <param name="packageNameLength">The package-name buffer length.</param>
    /// <param name="packageName">The package-name output buffer.</param>
    /// <returns>The synthetic failure code.</returns>
    private static unsafe int GetPackageFullNameFailure(
        IntPtr processHandle,
        ref int packageNameLength,
        char* packageName)
    {
        GC.KeepAlive(processHandle);
        GC.KeepAlive(packageNameLength);
        GC.KeepAlive((IntPtr)packageName);
        return Failure;
    }
}
