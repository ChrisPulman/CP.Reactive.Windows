// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Composes window icon lookup operations.</summary>
internal sealed class IconWindowOperations
{
    /// <summary>Gets or initializes the window factory.</summary>
    internal required Func<IntPtr, IInteropWindow> CreateWindow { get; init; }

    /// <summary>Gets or initializes the class-long lookup operation.</summary>
    internal required Func<IntPtr, ClassLongIndex, IntPtr> GetClassLong { get; init; }

    /// <summary>Gets or initializes the process-by-id lookup operation.</summary>
    internal required Func<int, Process> GetProcessById { get; init; }

    /// <summary>Gets or initializes the process-id lookup operation.</summary>
    internal required Func<IInteropWindow, int> GetProcessId { get; init; }

    /// <summary>Gets or initializes the executable path lookup operation.</summary>
    internal required Func<int, string> GetProcessPath { get; init; }

    /// <summary>Gets or initializes the process-name lookup operation.</summary>
    internal required Func<string, Process[]> GetProcessesByName { get; init; }

    /// <summary>Gets or initializes the top-window lookup operation.</summary>
    internal required Func<IEnumerable<IInteropWindow>> GetTopWindows { get; init; }

    /// <summary>Gets or initializes the app-window predicate.</summary>
    internal required Func<IInteropWindow, bool> IsApp { get; init; }

    /// <summary>Gets or initializes the SendMessage operation.</summary>
    internal required TrySendIconMessage TrySendMessage { get; init; }

    /// <summary>Creates the native operations implementation.</summary>
    /// <returns>The native operations implementation.</returns>
    internal static IconWindowOperations CreateNative() => new IconWindowOperations
    {
        CreateWindow = InteropWindowFactory.CreateFor,
        GetClassLong = User32Api.GetClassLongWrapper,
        GetProcessById = Process.GetProcessById,
        GetProcessId = static (window) => window.GetProcessId(),
        GetProcessPath = Kernel32Api.GetProcessPath,
        GetProcessesByName = Process.GetProcessesByName,
        GetTopWindows = InteropWindowQueryExtensions.GetTopWindows,
        IsApp = static (window) => window.IsApp(),
        TrySendMessage = User32Api.TrySendMessage,
    };
}
