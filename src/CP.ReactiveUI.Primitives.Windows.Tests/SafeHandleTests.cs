// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Gdi;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Safe Handle Tests behavior.</summary>
public class SafeHandleTests
{
    /// <summary>Defines the Safe Window Dc Handle Type Name test value.</summary>
    private const string SafeWindowDcHandleTypeName = "CP.ReactiveUI.Primitives.Windows.Native.Gdi.SafeHandles.SafeWindowDcHandle";

    /// <summary>Tests Create Compatible Dc.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCreateCompatibleDcAsync()
    {
        using var desktopDcHandle = FromDesktop();
        await Assert.That(desktopDcHandle.IsInvalid).IsFalse();

        using var safeCompatibleDcHandle = Gdi32Api.CreateCompatibleDC(desktopDcHandle);
        await Assert.That(safeCompatibleDcHandle.IsInvalid).IsFalse();
    }

    /// <summary>Tests Safe Window Dc Handle Int Ptr Zero.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestSafeWindowDcHandle_IntPtrZeroAsync()
    {
        using var safeDctHandle = FromWindow(IntPtr.Zero);
        await Assert.That(safeDctHandle).IsNull();
    }

    /// <summary>Creates a desktop device-context safe handle through the internal factory.</summary>
    /// <returns>The safe handle returned by the internal desktop factory.</returns>
    private static SafeHandle FromDesktop() =>
        (SafeHandle)GetFactory(nameof(FromDesktop)).Invoke(null, null)!;

    /// <summary>Creates a window device-context safe handle through the internal factory.</summary>
    /// <param name="windowHandle">The source window handle.</param>
    /// <returns>The safe handle returned by the internal window factory.</returns>
    private static SafeHandle FromWindow(IntPtr windowHandle) =>
        (SafeHandle)GetFactory(nameof(FromWindow)).Invoke(null, [windowHandle]);

    /// <summary>Gets a safe-handle factory method by name.</summary>
    /// <param name="methodName">The factory method name.</param>
    /// <returns>The matching factory method.</returns>
    private static MethodInfo GetFactory(string methodName) =>
        GetSafeWindowDcHandleType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>Gets the internal safe window device-context handle type.</summary>
    /// <returns>The internal safe handle type.</returns>
    private static Type GetSafeWindowDcHandleType() =>
        typeof(Gdi32Api).Assembly.GetType(SafeWindowDcHandleTypeName, throwOnError: true)!;
}
