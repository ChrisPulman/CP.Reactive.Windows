// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>All clipboard related tests.</summary>
public class ClipboardTests : IDisposable
{
    /// <summary>Defines the TestValue200 test value.</summary>
    private const int TestValue200 = 200;

    /// <summary>Defines the TestValue400 test value.</summary>
    private const int TestValue400 = 400;

    /// <summary>Defines the TestValue2000 test value.</summary>
    private const int TestValue2000 = 2000;

    /// <summary>Defines the TestValue100 test value.</summary>
    private const int TestValue100 = 100;

    /// <summary>Defines the custom test clipboard format.</summary>
    private const string TestFormat = "TEST_FORMAT";

    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(ClipboardTests));

    /// <summary>Tracks shared message-window subscriptions for clipboard tests.</summary>
    private readonly IDisposable _subscription;

    /// <summary>Initializes a new instance of the <see cref="ClipboardTests"/> class.</summary>
    public ClipboardTests()
    {
        TestLogging.UseConsoleLogger();
        _subscription = SharedMessageWindow.ObserveWindowMessages().Subscribe();
    }

    /// <summary>Test monitoring the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardMonitor_WaitForCopyAsync()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = ClipboardNative.ClipboardUpdateEvents
            .Where(static clipboardUpdateInformation => ContainsFormat(clipboardUpdateInformation.Formats, TestFormat))
            .Subscribe(clipboardUpdateInformation =>
        {
            Log.DebugFormat("Formats {0}", string.Join(",", clipboardUpdateInformation.Formats));
            Log.DebugFormat("Owner {0}", clipboardUpdateInformation.HasOwner);
            Log.DebugFormat("Sequence {0}", clipboardUpdateInformation.Id);
            _ = tcs.TrySetResult(true);
        });

        bool canAccess;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            canAccess = clipboardAccessToken.CanAccess;
            if (canAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsUnicodeString("Clipboard update test", TestFormat);
            }
        }

        await Assert.That(canAccess).IsTrue();
        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TestValue2000));
        await Assert.That(ReferenceEquals(completedTask, tcs.Task)).IsTrue();
    }

    /// <summary>Test delayed rendering of the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardMonitor_DelayedRenderAsync()
    {
        const string testString = "Hi";
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = ClipboardNative.ClipboardRenderFormatRequests.Subscribe(request => HandleRenderFormatRequest(request, tcs, testString));

        var formatToTestWith = $"{TestFormat}_{Guid.NewGuid():N}";
        var formatId = ClipboardFormatExtensions.MapFormatToId(formatToTestWith);
        Log.DebugFormat("Registered clipboard format {0} as {1}", formatToTestWith, formatId);
        var clipboardOwner = await WaitForMessageWindowHandleAsync();
        await Assert.That(clipboardOwner).IsNotEqualTo(IntPtr.Zero);

        // Make the clipboard ready for testing
        (bool CanAccess, bool LockTimedOut) delayedWriteState;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync(clipboardOwner))
        {
            delayedWriteState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout);
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();

                // Set delayed rendered content
                clipboardAccessToken.SetDelayedRenderedContent(formatId);
            }
        }

        await Assert.That(delayedWriteState.CanAccess).IsTrue();
        await Assert.That(delayedWriteState.LockTimedOut).IsFalse();

        await Task.Delay(TestValue200);

        (bool CanAccess, bool LockTimedOut, bool ContainsFormat) delayedReadState;
        string resultString;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync(clipboardOwner))
        {
            Log.DebugFormat("Test if the clipboard has our format {0} as {1}", formatToTestWith, formatId);
            delayedReadState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout, ClipboardNative.HasFormat(formatToTestWith));

            // Request the missing content, this should trigger the rendering
            Log.DebugFormat("Request the clipboard for our format {0} as {1}", formatToTestWith, formatId);
            resultString = clipboardAccessToken.CanAccess
                ? clipboardAccessToken.GetAsUnicodeString(formatToTestWith)
                : string.Empty;
        }

        await Assert.That(delayedReadState.CanAccess).IsTrue();
        await Assert.That(delayedReadState.LockTimedOut).IsFalse();
        await Assert.That(delayedReadState.ContainsFormat).IsTrue();
        await Assert.That(resultString).IsEqualTo(testString);

        await Assert.That(await tcs.Task).IsTrue();
    }

    /// <summary>Test the format mappers.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboard_FormatsAsync()
    {
        var formatId = ClipboardFormatExtensions.MapFormatToId(StandardClipboardFormats.DisplayBitmap.AsString());
        await Assert.That(formatId).IsEqualTo((uint)StandardClipboardFormats.DisplayBitmap);
    }

    /// <summary>Test registering a clipboard format for the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboard_RegisterFormatAsync()
    {
        var format = $"DAPPLO.DOPY{ClipboardNative.SequenceNumber}";

        // Register the format
        var id1 = ClipboardFormatExtensions.RegisterFormat(format);

        // Register the format again
        var id2 = ClipboardFormatExtensions.RegisterFormat(format);

        await Assert.That(id2).IsEqualTo(id1);

        // Make sure it works
        (bool CanAccess, bool LockTimedOut) writeState;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            writeState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout);
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsUnicodeString("Blub", format);
            }
        }

        await Assert.That(writeState.CanAccess).IsTrue();
        await Assert.That(writeState.LockTimedOut).IsFalse();
    }

    /// <summary>Test monitoring the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardMonitor_TextAsync()
    {
        const string testString = "CP.ReactiveUI.Primitives.Windows.Tests.ClipboardTests";
        var hasNewContent = false;
        var subscription = ClipboardNative.ClipboardUpdateEvents.Where(static clipboard => ContainsFormat(clipboard.Formats, TestFormat)).Subscribe(clipboard =>
        {
            Log.DebugFormat("Detected change {0}", string.Join(",", clipboard.Formats));
            Log.DebugFormat("Owner {0}", clipboard.HasOwner);
            Log.DebugFormat("Sequence {0}", clipboard.Id);

            hasNewContent = true;
        });
        (bool CanAccess, bool LockTimedOut) writeState;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            writeState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout);
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsUnicodeString(testString, TestFormat);
            }
        }

        await Assert.That(writeState.CanAccess).IsTrue();
        await Assert.That(writeState.LockTimedOut).IsFalse();
        await Task.Delay(TestValue400);
        subscription.Dispose();

        // Doesn't work on AppVeyor!!
        await Assert.That(hasNewContent).IsTrue();
    }

    /// <summary>Test monitoring the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardStore_StringAsync()
    {
        const string testString = "CP.ReactiveUI.Primitives.Windows.Tests.ClipboardTests";
        using (var writeAccessToken = await ClipboardNative.AccessAsync())
        {
            writeAccessToken.ClearContents();
            writeAccessToken.SetAsUnicodeString(testString);
        }

        await Task.Delay(TestValue100);
        string readText;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            readText = clipboardAccessToken.GetAsUnicodeString();
        }

        await Assert.That(readText).IsEqualTo(testString);
    }

    /// <summary>Test if the clipboard contains files.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboard_FileNamesAsync()
    {
        var expectedFiles = new[] { @"C:\path\to\existing-file-test.txt" };
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            clipboardAccessToken.ClearContents();
            clipboardAccessToken.SetFileNames(expectedFiles);
        }

        List<string> fileNames;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            fileNames = ToList(readAccessToken.GetFileNames());
        }

        await Assert.That(fileNames.Count).IsEqualTo(1);
        await Assert.That(fileNames[0]).IsEqualTo(expectedFiles[0]);
    }

    /// <summary>Test setting file names on the clipboard and reading them back.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboard_SetFileNamesAsync()
    {
        // Note: DROPFILES stores file name strings in the clipboard buffer without
        // needing the files to actually exist on disk. The paths are stored as-is.
        var testFiles = new List<string> { @"C:\path\to\file1.txt", @"C:\path\to\file2.txt", };

        (bool CanAccess, bool LockTimedOut) writeState;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            writeState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout);
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetFileNames(testFiles);
            }
        }

        await Assert.That(writeState.CanAccess).IsTrue();
        await Assert.That(writeState.LockTimedOut).IsFalse();
        await Task.Delay(TestValue100);

        (bool CanAccess, bool LockTimedOut) readState;
        List<string> result;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            readState = (readAccessToken.CanAccess, readAccessToken.IsLockTimeout);
            result = ToList(readAccessToken.GetFileNames());
        }

        await Assert.That(readState.CanAccess).IsTrue();
        await Assert.That(readState.LockTimedOut).IsFalse();
        await Assert.That(result.Count).IsEqualTo(testFiles.Count);
        await Assert.That(result[0]).IsEqualTo(testFiles[0]);
        await Assert.That(result[1]).IsEqualTo(testFiles[1]);
    }

    /// <summary>Test monitoring the clipboard.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardStore_MemoryStreamAsync()
    {
        const string testString = "CP.ReactiveUI.Primitives.Windows.Tests.ClipboardTests";
        var testStream = new MemoryStream();
        var bytes = Encoding.Unicode.GetBytes($"{testString}\u0000");
        await Assert.That(Encoding.Unicode.GetString(bytes).TrimEnd('\0')).IsEqualTo(testString);
        testStream.Write(bytes, 0, bytes.Length);

        _ = testStream.Seek(0, SeekOrigin.Begin);
        await Assert.That(Encoding.Unicode.GetString(testStream.GetBuffer(), 0, (int)testStream.Length).TrimEnd('\0')).IsEqualTo(testString);

        (bool CanAccess, bool LockTimedOut) writeState;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            writeState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout);
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsStream(StandardClipboardFormats.UnicodeText, testStream);
            }
        }

        await Assert.That(writeState.CanAccess).IsTrue();
        await Assert.That(writeState.LockTimedOut).IsFalse();
        var (readState, unicodeText, unicodeBytes, unicodeStream) = await ReadClipboardStreamDataAsync();

        await Assert.That(readState.CanAccess).IsTrue();
        await Assert.That(readState.LockTimedOut).IsFalse();
        await Assert.That(unicodeText).IsEqualTo(testString);
        await Assert.That(Encoding.Unicode.GetString(unicodeBytes, 0, unicodeBytes.Length).TrimEnd('\0')).IsEqualTo(testString);

#if NETFRAMEWORK
        using var memoryStream = new MemoryStream();
#else
        await using var memoryStream = new MemoryStream();
#endif
        await unicodeStream.CopyToAsync(memoryStream);
        await Assert.That(Encoding.Unicode.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length).TrimEnd('\0')).IsEqualTo(testString);
    }

    /// <summary>Test AccessAsync.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ClipboardAccess_LockTimeoutAsync()
    {
        bool outerCanAccess;
        bool innerLockTimedOut;
        using (var outerClipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            outerCanAccess = outerClipboardAccessToken.CanAccess;
            using var clipboardAccessToken = await ClipboardNative.AccessAsync();
            innerLockTimedOut = clipboardAccessToken.IsLockTimeout;
        }

        await Assert.That(outerCanAccess).IsTrue();
        await Assert.That(innerLockTimedOut).IsTrue();
    }

    /// <summary>Test AccessAsync.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ClipboardAccess_LockTimeout_ExceptionAsync()
    {
        IClipboardAccessToken clipboardAccessToken;
        using (var outerClipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            clipboardAccessToken = await ClipboardNative.AccessAsync();
        }

        using (clipboardAccessToken)
        {
            await Assert.That(() => clipboardAccessToken.ThrowWhenNoAccess()).Throws<ClipboardAccessDeniedException>();
        }
    }

    /// <summary>Test setting cloud clipboard options.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCloudClipboard_SetOptionsAsync()
    {
        const string testString = "Cloud clipboard test";
        (bool CanAccess, bool LockTimedOut) writeState;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            writeState = (clipboardAccessToken.CanAccess, clipboardAccessToken.IsLockTimeout);
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsUnicodeString(testString);

                // Set cloud clipboard options
                clipboardAccessToken.SetCloudClipboardOptions(
                    canIncludeInHistory: false,
                    canUploadToCloud: false,
                    excludeFromMonitoring: true);
            }
        }

        await Assert.That(writeState.CanAccess).IsTrue();
        await Assert.That(writeState.LockTimedOut).IsFalse();
        var (readState, formats, readText) = await ReadClipboardFormatsAndTextAsync();

        await Assert.That(readState.CanAccess).IsTrue();
        await Assert.That(readState.LockTimedOut).IsFalse();
        await Assert.That(formats).Contains(ClipboardCloudExtensions.CanIncludeInClipboardHistoryFormat);
        await Assert.That(formats).Contains(ClipboardCloudExtensions.CanUploadToCloudClipboardFormat);
        await Assert.That(formats).Contains(ClipboardCloudExtensions.ExcludeClipboardContentFromMonitorProcessingFormat);

        // Verify the text is still there
        await Assert.That(readText).IsEqualTo(testString);
    }

    /// <summary>Test setting individual cloud clipboard options.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCloudClipboard_SetIndividualOptionsAsync()
    {
        const string testString = "Individual cloud clipboard test";
        bool canAccess;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            canAccess = clipboardAccessToken.CanAccess;
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsUnicodeString(testString);

                // Set individual options
                clipboardAccessToken.SetCanIncludeInClipboardHistory(false);
                clipboardAccessToken.SetCanUploadToCloudClipboard();
                clipboardAccessToken.SetExcludeClipboardContentFromMonitorProcessing();
            }
        }

        await Assert.That(canAccess).IsTrue();
        await Task.Delay(TestValue100);

        // Verify the formats were set
        List<string> formats;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            formats = ToList(readAccessToken.AvailableFormats());
        }

        await Assert.That(formats).Contains(ClipboardCloudExtensions.CanIncludeInClipboardHistoryFormat);
        await Assert.That(formats).Contains(ClipboardCloudExtensions.CanUploadToCloudClipboardFormat);
        await Assert.That(formats).Contains(ClipboardCloudExtensions.ExcludeClipboardContentFromMonitorProcessingFormat);
    }

    /// <summary>Test setting cloud clipboard options with default values.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestCloudClipboard_DefaultOptionsAsync()
    {
        const string testString = "Default cloud clipboard test";
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            clipboardAccessToken.ClearContents();
            clipboardAccessToken.SetAsUnicodeString(testString);

            // Use default values (should allow history and cloud, not exclude monitoring)
            clipboardAccessToken.SetCloudClipboardOptions();
        }

        await Task.Delay(TestValue100);

        // Verify the formats were set
        List<string> formats;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            formats = ToList(readAccessToken.AvailableFormats());
        }

        await Assert.That(formats).Contains(ClipboardCloudExtensions.CanIncludeInClipboardHistoryFormat);
        await Assert.That(formats).Contains(ClipboardCloudExtensions.CanUploadToCloudClipboardFormat);
        await Assert.That(formats).Contains(ClipboardCloudExtensions.ExcludeClipboardContentFromMonitorProcessingFormat);
    }

    /// <summary>Test TryGetAsStream with available format.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardTryGetAsStream_SuccessAsync()
    {
        const string testString = "CP.ReactiveUI.Primitives.Windows.Tests.TryGetAsStream";
        var testStream = new MemoryStream();
        var bytes = Encoding.Unicode.GetBytes($"{testString}\u0000");
        testStream.Write(bytes, 0, bytes.Length);
        _ = testStream.Seek(0, SeekOrigin.Begin);

        bool canWrite;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            canWrite = clipboardAccessToken.CanAccess;
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsStream(StandardClipboardFormats.UnicodeText, testStream);
            }
        }

        await Assert.That(canWrite).IsTrue();
        await Task.Delay(TestValue100);

        bool canRead;
        bool success;
        Stream stream;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            canRead = readAccessToken.CanAccess;

            // Try to get the stream - should succeed
            success = readAccessToken.TryGetAsStream(StandardClipboardFormats.UnicodeText, out stream);
        }

        await Assert.That(canRead).IsTrue();
        await Assert.That(success).IsTrue();
        await Assert.That(stream).IsNotNull();

#if NETFRAMEWORK
        using var memoryStream = new MemoryStream();
#else
        await using var memoryStream = new MemoryStream();
#endif
        await stream.CopyToAsync(memoryStream);
        var resultString = Encoding.Unicode.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length).TrimEnd('\0');
        await Assert.That(resultString).IsEqualTo(testString);
    }

    /// <summary>Test TryGetAsStream with unavailable format.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardTryGetAsStream_FailureAsync()
    {
        bool canWrite;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            canWrite = clipboardAccessToken.CanAccess;
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
            }
        }

        await Assert.That(canWrite).IsTrue();
        await Task.Delay(TestValue100);

        bool canRead;
        bool success;
        Stream stream;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            canRead = readAccessToken.CanAccess;

            // Try to get a non-existent format - should fail gracefully
            success = readAccessToken.TryGetAsStream(StandardClipboardFormats.UnicodeText, out stream);
        }

        await Assert.That(canRead).IsTrue();
        await Assert.That(success).IsFalse();
        await Assert.That(stream).IsNull();
    }

    /// <summary>Test TryGetAsStream with custom format.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestClipboardTryGetAsStream_CustomFormatAsync()
    {
        var customFormat = $"CUSTOM_TEST_FORMAT_{Guid.NewGuid():N}";
        const string testString = "Custom format test data";
        var testStream = new MemoryStream();
        var bytes = "Custom format test data"u8.ToArray();
        testStream.Write(bytes, 0, bytes.Length);
        _ = testStream.Seek(0, SeekOrigin.Begin);

        bool canWrite;
        using (var clipboardAccessToken = await ClipboardNative.AccessAsync())
        {
            canWrite = clipboardAccessToken.CanAccess;
            if (clipboardAccessToken.CanAccess)
            {
                clipboardAccessToken.ClearContents();
                clipboardAccessToken.SetAsStream(customFormat, testStream);
            }
        }

        await Assert.That(canWrite).IsTrue();

        await Task.Delay(TestValue100);

        bool canRead;
        bool success;
        bool missingSuccess;
        Stream stream;
        Stream missingStream;
        using (var readAccessToken = await ClipboardNative.AccessAsync())
        {
            canRead = readAccessToken.CanAccess;

            // Try to get with correct format - should succeed
            success = readAccessToken.TryGetAsStream(customFormat, out stream);

            // Try to get with wrong format - should fail
            missingSuccess = readAccessToken.TryGetAsStream("WRONG_FORMAT", out missingStream);
        }

        await Assert.That(canRead).IsTrue();
        await Assert.That(success).IsTrue();
        await Assert.That(stream).IsNotNull();

#if NETFRAMEWORK
        using var memoryStream = new MemoryStream();
#else
        await using var memoryStream = new MemoryStream();
#endif
        await stream.CopyToAsync(memoryStream);
        var resultString = Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        await Assert.That(resultString).IsEqualTo(testString);

        await Assert.That(missingSuccess).IsFalse();
        await Assert.That(missingStream).IsNull();
    }

    /// <summary>Provides Dispose for tests.</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases managed resources used by this test fixture.</summary>
    /// <param name="disposing">A value indicating whether managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscription.Dispose();
        }
    }

    /// <summary>Handles delayed clipboard render requests.</summary>
    /// <param name="request">The render request.</param>
    /// <param name="completion">The render completion signal.</param>
    /// <param name="testString">The string to render.</param>
    private static void HandleRenderFormatRequest(ClipboardRenderFormatRequest request, TaskCompletionSource<bool> completion, string testString)
    {
        if (request.IsDestroyClipboard || request.RenderAllFormats)
        {
            Log.Debug(request.IsDestroyClipboard ? "Destroy clipboard" : "Render all formats");
            return;
        }

        Log.DebugFormat("Got request render {0} ({1}) to the clipboard", request.RequestedFormat, request.RequestedFormatId);
        request.AccessToken.SetAsUnicodeString(testString, request.RequestedFormat);
        _ = completion.TrySetResult(true);
    }

    /// <summary>Waits for the shared message window to expose a native owner handle.</summary>
    /// <returns>The native owner handle, or <see cref="IntPtr.Zero"/> when the message window is unavailable.</returns>
    private static async Task<IntPtr> WaitForMessageWindowHandleAsync()
    {
        if (SharedMessageWindow.Handle != 0)
        {
            return new(SharedMessageWindow.Handle);
        }

        var completion = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = SharedMessageWindow.ObserveHandleChanges()
            .Where(static handle => handle != 0)
            .Subscribe(handle => _ = completion.TrySetResult(handle));
        var completedTask = await Task.WhenAny(completion.Task, Task.Delay(TestValue2000));
        return ReferenceEquals(completedTask, completion.Task)
            ? new(await completion.Task)
            : new(SharedMessageWindow.Handle);
    }

    /// <summary>Determines whether the supplied formats contain the expected format.</summary>
    /// <param name="formats">The formats to inspect.</param>
    /// <param name="expectedFormat">The expected format.</param>
    /// <returns><see langword="true"/> when the format is present; otherwise, <see langword="false"/>.</returns>
    private static bool ContainsFormat(IEnumerable<string> formats, string expectedFormat)
    {
        foreach (var format in formats)
        {
            if (format == expectedFormat)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads clipboard stream data, retrying until delayed native formats are readable.</summary>
    /// <returns>The access state and clipboard data.</returns>
    private static async Task<((bool CanAccess, bool LockTimedOut) ReadState, string UnicodeText, byte[] UnicodeBytes, Stream UnicodeStream)> ReadClipboardStreamDataAsync()
    {
        Exception lastException = null;
        for (var attempt = 0; attempt < TestValue2000 / TestValue100; attempt++)
        {
            try
            {
                using var readAccessToken = await ClipboardNative.AccessAsync();
                var readState = (readAccessToken.CanAccess, readAccessToken.IsLockTimeout);
                var unicodeText = readAccessToken.GetAsUnicodeString();
                var unicodeBytes = readAccessToken.GetAsBytes(StandardClipboardFormats.UnicodeText);
                var unicodeStream = readAccessToken.GetAsStream(StandardClipboardFormats.UnicodeText);
                return (readState, unicodeText, unicodeBytes, unicodeStream);
            }
            catch (Win32Exception exception)
            {
                lastException = exception;
                await Task.Delay(TestValue100);
            }
        }

        throw lastException ?? new InvalidOperationException("Clipboard stream data could not be read.");
    }

    /// <summary>Reads clipboard formats and text, retrying until delayed native formats are readable.</summary>
    /// <returns>The access state, available format names, and text.</returns>
    private static async Task<((bool CanAccess, bool LockTimedOut) ReadState, List<string> Formats, string Text)> ReadClipboardFormatsAndTextAsync()
    {
        for (var attempt = 0; attempt < TestValue2000 / TestValue100; attempt++)
        {
            using var readAccessToken = await ClipboardNative.AccessAsync();
            var readState = (readAccessToken.CanAccess, readAccessToken.IsLockTimeout);
            var formats = ToList(readAccessToken.AvailableFormats());
            var hasCloudFormats = ContainsFormat(formats, ClipboardCloudExtensions.CanIncludeInClipboardHistoryFormat)
                && ContainsFormat(formats, ClipboardCloudExtensions.CanUploadToCloudClipboardFormat)
                && ContainsFormat(formats, ClipboardCloudExtensions.ExcludeClipboardContentFromMonitorProcessingFormat);

            if (hasCloudFormats)
            {
                return (readState, formats, readAccessToken.GetAsUnicodeString());
            }

            await Task.Delay(TestValue100);
        }

        using var finalReadAccessToken = await ClipboardNative.AccessAsync();
        return (
            (finalReadAccessToken.CanAccess, finalReadAccessToken.IsLockTimeout),
            ToList(finalReadAccessToken.AvailableFormats()),
            finalReadAccessToken.GetAsUnicodeString());
    }

    /// <summary>Materializes the supplied values as a list without LINQ.</summary>
    /// <param name="values">The values to materialize.</param>
    /// <returns>The materialized list.</returns>
    private static List<string> ToList(IEnumerable<string> values)
    {
        var result = new List<string>();
        foreach (var value in values)
        {
            result.Add(value);
        }

        return result;
    }
}
