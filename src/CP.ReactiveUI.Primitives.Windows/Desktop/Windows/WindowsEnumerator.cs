// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>A managed EnumWindows wrapper, offering both as IObservable as an IEnumerable.</summary>
public static class WindowsEnumerator
{
    /// <summary>Composable native enumeration operations.</summary>
    private static WindowsEnumeratorOperations _operations = new();

    /// <summary>Enumerate the windows / child windows (this is NOT lazy, unless you add functions).</summary>
    /// <param name="parent">IInteropWindow with the windowHandle of the parent, or null for all.</param>
    /// <returns>IEnumerable with IntPtr.</returns>
    public static IEnumerable<IntPtr> EnumerateWindowHandles(IInteropWindow parent) => EnumerateWindowHandles(parent, null, null);

    /// <summary>Enumerate the windows / child windows (this is NOT lazy, unless you add functions).</summary>
    /// <param name="parent">IInteropWindow with the windowHandle of the parent, or null for all.</param>
    /// <param name="wherePredicate">Func for the where.</param>
    /// <returns>IEnumerable with IntPtr.</returns>
    public static IEnumerable<IntPtr> EnumerateWindowHandles(IInteropWindow parent, Func<IntPtr, bool> wherePredicate) => EnumerateWindowHandles(parent, wherePredicate, null);

    /// <summary>Enumerate the windows / child windows (this is NOT lazy, unless you add functions).</summary>
    /// <param name="parent">IInteropWindow with the windowHandle of the parent, or null for all.</param>
    /// <param name="wherePredicate">Func for the where.</param>
    /// <param name="takeWhileFunc">Func which can decide to stop enumerating, the second argument is the current count.</param>
    /// <returns>IEnumerable with IntPtr.</returns>
    public static IEnumerable<IntPtr> EnumerateWindowHandles(IInteropWindow parent, Func<IntPtr, bool> wherePredicate, Func<IntPtr, int, bool> takeWhileFunc)
    {
        List<IntPtr> result = new();
        _ = Volatile.Read(ref _operations).EnumChildWindows(parent?.Handle ?? IntPtr.Zero, windowHandle => EnumWindowsProc(windowHandle, IntPtr.Zero));
        return result;
        bool EnumWindowsProc(IntPtr windowHandle, IntPtr _)
        {
            if (wherePredicate is null || wherePredicate(windowHandle))
            {
                result.Add(windowHandle);
            }

            return takeWhileFunc is null || takeWhileFunc(windowHandle, result.Count);
        }
    }

    /// <summary>Enumerate the windows and child handles (IntPtr) via an Observable.</summary>
    /// <returns>IObservable with IntPtr.</returns>
    public static IObservable<IntPtr> ObserveWindowHandles() => ObserveWindowHandles(null);

    /// <summary>Enumerate the windows and child handles (IntPtr) via an Observable.</summary>
    /// <param name="parentWindowHandle">IntPtr with the windowHandle of the parent, or null for all.</param>
    /// <returns>IObservable with IntPtr.</returns>
    public static IObservable<IntPtr> ObserveWindowHandles(IntPtr? parentWindowHandle) => ReactiveSignal.CreateSafe((IObserver<IntPtr> observer) =>
        {
            CancellationTokenSource cancellationTokenSource = new();
            _ = Task.Run(
                () =>
                {
                    _ = Volatile.Read(ref _operations).EnumChildWindows(parentWindowHandle ?? IntPtr.Zero, windowHandle => EnumWindowsProc(windowHandle, IntPtr.Zero));
                    observer.OnCompleted();
                },
                cancellationTokenSource.Token);
            return new CancellationDisposable(cancellationTokenSource);
            bool EnumWindowsProc(IntPtr windowHandle, IntPtr _)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                observer.OnNext(windowHandle);
                return !cancellationTokenSource.IsCancellationRequested;
            }
        });

    /// <summary>Enumerate the windows / child windows (this is NOT lazy).</summary>
    /// <param name="parent">IInteropWindow with the windowHandle of the parent, or null for all.</param>
    /// <returns>IEnumerable with InteropWindow.</returns>
    public static IEnumerable<IInteropWindow> EnumerateWindows(IInteropWindow parent) => EnumerateWindows(parent, null, null);

    /// <summary>Enumerate the windows / child windows (this is NOT lazy).</summary>
    /// <param name="parent">IInteropWindow with the windowHandle of the parent, or null for all.</param>
    /// <param name="wherePredicate">Func for the where.</param>
    /// <returns>IEnumerable with InteropWindow.</returns>
    public static IEnumerable<IInteropWindow> EnumerateWindows(IInteropWindow parent, Func<IInteropWindow, bool> wherePredicate) => EnumerateWindows(parent, wherePredicate, null);

    /// <summary>Enumerate the windows / child windows (this is NOT lazy).</summary>
    /// <param name="parent">IInteropWindow with the windowHandle of the parent, or null for all.</param>
    /// <param name="wherePredicate">Func for the where.</param>
    /// <param name="takeWhileFunc">Func which can decide to stop enumerating, the second argument is the current count.</param>
    /// <returns>IEnumerable with InteropWindow.</returns>
    public static IEnumerable<IInteropWindow> EnumerateWindows(IInteropWindow parent, Func<IInteropWindow, bool> wherePredicate, Func<IInteropWindow, int, bool> takeWhileFunc)
    {
        List<IInteropWindow> result = new();
        _ = Volatile.Read(ref _operations).EnumChildWindows(parent?.Handle ?? IntPtr.Zero, windowHandle => EnumWindowsProc(windowHandle, IntPtr.Zero));
        return result;
        bool EnumWindowsProc(IntPtr windowHandle, IntPtr _)
        {
            InteropWindow interopWindow = InteropWindowFactory.CreateFor(windowHandle);
            if (wherePredicate is null || wherePredicate(interopWindow))
            {
                result.Add(interopWindow);
            }

            return takeWhileFunc is null || takeWhileFunc(interopWindow, result.Count);
        }
    }

    /// <summary>Enumerate the windows / child windows via an Observable.</summary>
    /// <returns>IObservable with IInteropWindow.</returns>
    public static IObservable<IInteropWindow> ObserveWindows() => ObserveWindows(null);

    /// <summary>Enumerate the windows / child windows via an Observable.</summary>
    /// <param name="parentWindowHandle">IntPtr with the windowHandle of the parent, or null for all.</param>
    /// <returns>IObservable with IInteropWindow.</returns>
    public static IObservable<IInteropWindow> ObserveWindows(IntPtr? parentWindowHandle) => ReactiveSignal.CreateSafe((IObserver<IInteropWindow> observer) =>
        {
            CancellationTokenSource cancellationTokenSource = new();
            _ = Task.Run(
                () =>
                {
                    _ = Volatile.Read(ref _operations).EnumChildWindows(parentWindowHandle ?? IntPtr.Zero, windowHandle => EnumWindowsProc(windowHandle, IntPtr.Zero));
                    observer.OnCompleted();
                },
                cancellationTokenSource.Token);
            return new CancellationDisposable(cancellationTokenSource);
            bool EnumWindowsProc(IntPtr windowHandle, IntPtr _)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                InteropWindow interopWindow = InteropWindowFactory.CreateFor(windowHandle);
                observer.OnNext(interopWindow);
                return !cancellationTokenSource.IsCancellationRequested;
            }
        });

    /// <summary>Replaces enumeration operations for a bounded deterministic test scope.</summary>
    /// <param name="operations">The operations to use while the returned scope is alive.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static OperationsOverride OverrideOperationsForTesting(WindowsEnumeratorOperations operations)
    {
        Throw.IfNull(operations);
        return new(Interlocked.Exchange(ref _operations, operations));
    }

    /// <summary>Restores previous operations when disposed.</summary>
    internal sealed class OperationsOverride : IDisposable
    {
        /// <summary>The operation set to restore.</summary>
        private readonly WindowsEnumeratorOperations _previous;

        /// <summary>Tracks whether restoration has already occurred.</summary>
        private int _disposed;

        /// <summary>Initializes a new instance of the <see cref="OperationsOverride"/> class.</summary>
        /// <param name="previous">The previous operations.</param>
        internal OperationsOverride(WindowsEnumeratorOperations previous) => _previous = previous;

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _ = Interlocked.Exchange(ref _operations, _previous);
            }
        }
    }

    /// <summary>Native operations used by <see cref="WindowsEnumerator"/>.</summary>
    internal class WindowsEnumeratorOperations
    {
        /// <summary>Enumerates child windows using a managed callback.</summary>
        /// <param name="parentWindowHandle">The parent window handle.</param>
        /// <param name="callback">Receives each enumerated handle and returns whether enumeration continues.</param>
        /// <returns>true when enumeration completes.</returns>
        internal virtual bool EnumChildWindows(IntPtr parentWindowHandle, Func<IntPtr, bool> callback)
        {
            Throw.IfNull(callback);
            return User32Api.EnumChildWindows(parentWindowHandle, (windowHandle, _) => callback(windowHandle), IntPtr.Zero);
        }
    }
}
