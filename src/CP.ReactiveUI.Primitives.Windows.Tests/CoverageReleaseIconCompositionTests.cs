// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Exercises deterministic icon composition branches required by release coverage.</summary>
public sealed class CoverageReleaseIconCompositionTests
{
    /// <summary>Defines the in-memory associated-icon combinations.</summary>
    private enum AssociatedIconMode
    {
        /// <summary>No icon is returned.</summary>
        None,

        /// <summary>Only a large icon is returned for the large-icon preference.</summary>
        Large,

        /// <summary>Only a small icon is returned.</summary>
        Small,

        /// <summary>Only a large icon is returned for the small-icon fallback.</summary>
        LargeFallback,
    }

    /// <summary>Verifies every associated-icon selection path without shell state.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task ExtractAssociatedIcon_SelectsLargeSmallAndLargeFallbackHandlesAsync()
    {
        var extractor = new AssociatedIconExtractor();
        var previousExtractor = Icons.IconHelper.SetAssociatedIconExtractorForTesting(extractor.Extract);
        try
        {
            await Assert
                .That(static () => Icons.IconHelper.SetAssociatedIconExtractorForTesting(null))
                .Throws<ArgumentNullException>();

            var sourcePath = typeof(CoverageReleaseIconCompositionTests).Assembly.Location;

            extractor.Mode = AssociatedIconMode.Large;
            using var largeIcon = Icons.IconHelper.ExtractAssociatedIcon(sourcePath, default(Icon), Zero, useLargeIcon: true);

            extractor.Mode = AssociatedIconMode.Small;
            using var smallIcon = Icons.IconHelper.ExtractAssociatedIcon(sourcePath, default(Icon), Zero, useLargeIcon: false);

            extractor.Mode = AssociatedIconMode.LargeFallback;
            using var fallbackIcon = Icons.IconHelper.ExtractAssociatedIcon(sourcePath, default(Icon), Zero, useLargeIcon: false);

            await Assert.That(largeIcon).IsNotNull();
            await Assert.That(smallIcon).IsNotNull();
            await Assert.That(fallbackIcon).IsNotNull();
        }
        finally
        {
            _ = Icons.IconHelper.SetAssociatedIconExtractorForTesting(previousExtractor);
        }
    }

    /// <summary>Verifies app-logo path handling for a manifest logo without a directory component.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GetAppLogoFromProcessPath_ReturnsNullForRootRelativeLogoPathAsync()
    {
        var temporaryDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"cp-reactive-root-logo-{Guid.NewGuid():N}"));
        try
        {
            var executablePath = Path.Combine(temporaryDirectory.FullName, "app.exe");
            var manifestPath = Path.Combine(temporaryDirectory.FullName, "AppxManifest.xml");
            File.WriteAllBytes(executablePath, []);
            const string manifest = "<Package xmlns=\"http://schemas.microsoft.com/appx/manifest/foundation/windows10\"><Properties><Logo>C:</Logo></Properties></Package>";
#if NETFRAMEWORK
            File.WriteAllText(manifestPath, manifest);
#else
            await File.WriteAllTextAsync(manifestPath, manifest);
#endif

            await Assert.That(Icons.IconHelper.GetAppLogoFromProcessPath(executablePath, default(Bitmap), Hundred)).IsNull();
        }
        finally
        {
            Directory.Delete(temporaryDirectory.FullName, recursive: true);
        }
    }

    /// <summary>Verifies process and top-level icon fallbacks when no sibling supplies an icon.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GetIcon_ReturnsNullWhenSiblingProcessAndTopLevelWindowsHaveNoIconAsync()
    {
        var sourceWindow = new InteropWindow(new(One)) { ProcessId = FortyTwo };
        var previousOperations = Icons.IconExtensions.SetOperationsForTesting(CreateNoIconOperations());
        try
        {
            var icon = sourceWindow.GetIcon(default(Icon), useLargeIcons: false);

            await Assert.That(icon).IsNull();
        }
        finally
        {
            _ = Icons.IconExtensions.SetOperationsForTesting(previousOperations);
        }
    }

    /// <summary>Verifies process-path associated-icon lookup for an existing file with no icon.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task GetIcon_UsesTheProcessPathBeforeSiblingFallbacksAsync()
    {
        var sourceWindow = new InteropWindow(new(Two)) { ProcessId = FortyThree };
        var previousOperations = Icons.IconExtensions.SetOperationsForTesting(CreateNoIconOperations(typeof(CoverageReleaseIconCompositionTests).Assembly.Location));
        var previousExtractor = Icons.IconHelper.SetAssociatedIconExtractorForTesting(new AssociatedIconExtractor().Extract);
        try
        {
            using var icon = sourceWindow.GetIcon(default(Icon), useLargeIcons: false);

            await Assert.That(icon).IsNull();
        }
        finally
        {
            _ = Icons.IconHelper.SetAssociatedIconExtractorForTesting(previousExtractor);
            _ = Icons.IconExtensions.SetOperationsForTesting(previousOperations);
        }
    }

    /// <summary>Creates deterministic window operations that return no direct or sibling icon.</summary>
    /// <param name="processPath">The optional process path.</param>
    /// <returns>The composed operations.</returns>
    private static IconWindowOperations CreateNoIconOperations(string processPath = null) => new()
    {
        CreateWindow = static handle => new InteropWindow(handle),
        GetClassLong = static (_, _) => IntPtr.Zero,
        GetProcessById = static _ => Process.GetCurrentProcess(),
        GetProcessId = static window => window.ProcessId ?? Zero,
        GetProcessPath = _ => processPath,
        GetProcessesByName = static _ => [Process.GetCurrentProcess()],
        GetTopWindows = static () => [],
        IsApp = static _ => false,
        TrySendMessage = static (IntPtr _, WindowsMessages _, IntPtr _, out IntPtr result) => SetNoIcon(out result),
    };

    /// <summary>Sets a zero icon result for a message probe.</summary>
    /// <param name="result">The returned icon handle.</param>
    /// <returns><see langword="false" />.</returns>
    private static bool SetNoIcon(out IntPtr result)
    {
        result = IntPtr.Zero;
        return false;
    }

    /// <summary>Supplies controllable in-memory icon handles to the associated-icon seam.</summary>
    private sealed class AssociatedIconExtractor
    {
        /// <summary>Gets or sets the handles produced by the extractor.</summary>
        public AssociatedIconMode Mode { get; set; }

        /// <summary>Extracts the configured in-memory icon handles.</summary>
        /// <param name="filePath">The source file path.</param>
        /// <param name="index">The icon index.</param>
        /// <param name="largeIcon">Receives the large icon handle.</param>
        /// <param name="smallIcon">Receives the small icon handle.</param>
        /// <param name="iconCount">The icon count.</param>
        /// <returns>The number of icon handles returned.</returns>
        public int Extract(string filePath, int index, out IntPtr largeIcon, out IntPtr smallIcon, int iconCount)
        {
            _ = filePath;
            _ = index;
            _ = iconCount;
            largeIcon = IntPtr.Zero;
            smallIcon = IntPtr.Zero;

            if (Mode is AssociatedIconMode.Large or AssociatedIconMode.LargeFallback)
            {
                largeIcon = CreateIconHandle(Color.Red);
            }

            if (Mode == AssociatedIconMode.Small)
            {
                smallIcon = CreateIconHandle(Color.Blue);
            }

            return largeIcon != IntPtr.Zero || smallIcon != IntPtr.Zero ? One : Zero;
        }

        /// <summary>Creates an owned icon handle from an in-memory bitmap.</summary>
        /// <param name="color">The icon color.</param>
        /// <returns>The owned icon handle.</returns>
        private static IntPtr CreateIconHandle(Color color)
        {
            using var bitmap = new Bitmap(Sixteen, Sixteen);
            bitmap.SetPixel(Zero, Zero, color);
            return bitmap.GetHicon();
        }
    }
}
