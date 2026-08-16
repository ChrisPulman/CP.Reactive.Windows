// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging;
using CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Tests.Benchmarks;

/// <summary>Benchmarks clipboard access.</summary>
[MinColumn]
[MaxColumn]
[MemoryDiagnoser]
public class ClipboardBenchmarks
{
    /// <summary>Holds the message-loop subscription for the benchmark lifetime.</summary>
    private readonly IDisposable _subscription;

    /// <summary>Initializes a new instance of the <see cref="ClipboardBenchmarks"/> class.</summary>
    public ClipboardBenchmarks()
    {
        _subscription = SharedMessageWindow.ObserveWindowMessages().Subscribe(MessageObserver.Instance);
    }

    /// <summary>Releases the message-loop subscription after the benchmark completes.</summary>
    [GlobalCleanup]
    public void Cleanup() => _subscription.Dispose();

    /// <summary>Measures acquisition and release of the clipboard lock.</summary>
    /// <returns>A task that represents the asynchronous benchmark operation.</returns>
    [Benchmark]
    [STAThread]
    public async Task LockClipboardAsync()
    {
        using var access = await ClipboardNative.AccessAsync();
    }

    /// <summary>Absorbs message notifications while preserving reactive error propagation.</summary>
    private sealed class MessageObserver : IObserver<WindowMessage>
    {
        /// <summary>Provides the singleton observer instance.</summary>
        internal static readonly MessageObserver Instance = new();

        /// <summary>Handles the completion notification.</summary>
        public void OnCompleted() => GC.KeepAlive(this);

        /// <summary>Rethrows an error notification.</summary>
        /// <param name="error">The error received from the message stream.</param>
        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error ?? throw new ArgumentNullException(nameof(error))).Throw();

        /// <summary>Handles a message notification.</summary>
        /// <param name="value">The message received from the message stream.</param>
        public void OnNext(WindowMessage value) => GC.KeepAlive(value);
    }
}
