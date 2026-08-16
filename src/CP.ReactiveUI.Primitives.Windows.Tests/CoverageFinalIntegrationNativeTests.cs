// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Covers Citrix native method composition without requiring Citrix WFAPI.</summary>
public sealed class CoverageFinalIntegrationNativeTests
{
    /// <summary>A Windows module that is present on supported test hosts.</summary>
    private const string Kernel32LibraryName = "kernel32";

    /// <summary>A stable Kernel32 export used to cover the NativeLibrary adapter.</summary>
    private const string CurrentProcessExportName = "GetCurrentProcess";

    /// <summary>The WFAPI module name requested by the production adapter.</summary>
    private const string WfApiLibraryName = "WFAPI";

    /// <summary>The WFAPI memory-free export requested by the production adapter.</summary>
    private const string FreeMemoryExportName = "WFFreeMemory";

    /// <summary>Represents the fake WFAPI WFFreeMemory export.</summary>
    /// <param name="memory">The memory pointer to release.</param>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void WfFreeMemoryDelegate(IntPtr memory);

    /// <summary>Verifies WFFreeMemory loads its export through the injected adapter and caches it.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task WFFreeMemory_UsesInjectedNativeLibraryAndCachesExportAsync()
    {
        var nativeLibrary = new NativeLibraryProbe();
        var originalNativeLibrary = CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.ExchangeNativeLibrary(nativeLibrary);

        try
        {
            CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.WFFreeMemory(new(FortyTwo));
            CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.WFFreeMemory(new(FortyThree));

            await Assert.That(nativeLibrary.LoadCalls).IsEqualTo(One);
            await Assert.That(nativeLibrary.ExportCalls).IsEqualTo(One);
            await Assert.That(nativeLibrary.LoadedLibraryName).IsEqualTo(WfApiLibraryName);
            await Assert.That(nativeLibrary.RequestedExportName).IsEqualTo(FreeMemoryExportName);
        }
        finally
        {
            _ = CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.ExchangeNativeLibrary(originalNativeLibrary);
        }
    }

    /// <summary>Verifies replacing the native adapter clears cached module and export handles.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ExchangeNativeLibrary_RejectsNullAndResetsCachedExportAsync()
    {
        var firstNativeLibrary = new NativeLibraryProbe();
        var secondNativeLibrary = new NativeLibraryProbe();
        var originalNativeLibrary = CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.ExchangeNativeLibrary(firstNativeLibrary);

        try
        {
            CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.WFFreeMemory(new(FortyTwo));
            _ = CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.ExchangeNativeLibrary(secondNativeLibrary);
            CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.WFFreeMemory(new(FortyThree));

            await Assert.That(firstNativeLibrary.LoadCalls).IsEqualTo(One);
            await Assert.That(firstNativeLibrary.ExportCalls).IsEqualTo(One);
            await Assert.That(secondNativeLibrary.LoadCalls).IsEqualTo(One);
            await Assert.That(secondNativeLibrary.ExportCalls).IsEqualTo(One);
            await Assert.That(static () => CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.ExchangeNativeLibrary(null)).Throws<ArgumentNullException>();
        }
        finally
        {
            _ = CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.ExchangeNativeLibrary(originalNativeLibrary);
        }
    }

    /// <summary>Verifies the production NativeLibrary adapter can load a known Windows export.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task NativeLibraryAdapter_LoadsKnownWindowsExportAsync()
    {
        var nativeLibrary = CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.NativeLibraryAdapter.Instance;
        var module = nativeLibrary.Load(Kernel32LibraryName);
        var export = nativeLibrary.GetExport(module, CurrentProcessExportName);

        await Assert.That(module).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(export).IsNotEqualTo(IntPtr.Zero);
    }

    /// <summary>Captures native-library load and export requests.</summary>
    private sealed class NativeLibraryProbe : CP.ReactiveUI.Primitives.Windows.Integrations.Citrix.NativeMethods.INativeLibrary
    {
        /// <summary>Keeps the callback delegate alive while its unmanaged thunk is in use.</summary>
        private readonly WfFreeMemoryDelegate _freeMemoryCallback;

        /// <summary>The fake WFFreeMemory export pointer.</summary>
        private readonly IntPtr _freeMemoryExport;

        /// <summary>Initializes a new instance of the <see cref="NativeLibraryProbe"/> class.</summary>
        public NativeLibraryProbe()
        {
            _freeMemoryCallback = CaptureFreeMemory;
            _freeMemoryExport = Marshal.GetFunctionPointerForDelegate(_freeMemoryCallback);
        }

        /// <summary>Gets the number of native library load calls.</summary>
        public int LoadCalls { get; private set; }

        /// <summary>Gets the number of native export lookup calls.</summary>
        public int ExportCalls { get; private set; }

        /// <summary>Gets the last requested native library name.</summary>
        public string LoadedLibraryName { get; private set; } = string.Empty;

        /// <summary>Gets the last requested native export name.</summary>
        public string RequestedExportName { get; private set; } = string.Empty;

        /// <inheritdoc />
        public IntPtr Load(string libraryName)
        {
            LoadCalls++;
            LoadedLibraryName = libraryName;
            return new(TwentyOne);
        }

        /// <inheritdoc />
        public IntPtr GetExport(IntPtr module, string exportName)
        {
            ExportCalls++;
            RequestedExportName = exportName;
            return module == new IntPtr(TwentyOne) ? _freeMemoryExport : IntPtr.Zero;
        }

        /// <summary>Captures fake WFFreeMemory calls.</summary>
        /// <param name="memory">The pointer passed to WFFreeMemory.</param>
        private static void CaptureFreeMemory(IntPtr memory) => GC.KeepAlive(memory);
    }
}
