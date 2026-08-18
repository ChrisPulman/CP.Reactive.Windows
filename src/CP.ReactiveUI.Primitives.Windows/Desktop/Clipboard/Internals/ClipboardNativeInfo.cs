// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard.Internals;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.Internals;
#endif
/// <summary>Contains native information to handle the clipboard contents.</summary>
internal sealed class ClipboardNativeInfo : IDisposable
{
    /// <summary>The global memory unlock operation used by this type.</summary>
    private static Func<IntPtr, bool> _globalUnlock = Kernel32Api.GlobalUnlock;

    /// <summary>Gets or sets the global clipboard memory handle.</summary>
    internal IntPtr GlobalHandle { get; set; }

    /// <summary>Gets or sets a value indicating whether the data must be committed to the clipboard.</summary>
    internal bool NeedsWrite { get; set; }

    /// <summary>Gets or sets the format id which is processed.</summary>
    internal uint FormatId { get; set; }

    /// <summary>Gets or sets the locked clipboard memory pointer.</summary>
    internal IntPtr MemoryPtr { get; set; }

    /// <summary>Gets the size of the clipboard area.</summary>
    internal int Size => Kernel32Api.GlobalSize(GlobalHandle);

    /// <summary>Cleanup this native info by unlocking the global handle.</summary>
    public void Dispose()
    {
        if (GlobalHandle != IntPtr.Zero)
        {
            if (MemoryPtr != IntPtr.Zero)
            {
                _ = _globalUnlock(GlobalHandle);
                MemoryPtr = IntPtr.Zero;
            }

            if (NeedsWrite)
            {
                NativeMethods.SetClipboardDataWithErrorHandling(FormatId, GlobalHandle);
                GlobalHandle = IntPtr.Zero;
            }
        }
    }

    /// <summary>Overrides global memory unlocking for deterministic tests.</summary>
    /// <param name="globalUnlock">The replacement global unlock operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideGlobalUnlockForTesting(Func<IntPtr, bool> globalUnlock)
    {
        Throw.IfNull(globalUnlock);
        var previous = _globalUnlock;
        _globalUnlock = globalUnlock;
        return Scope.Create(previous, static previousOperation => _globalUnlock = previousOperation);
    }
}
