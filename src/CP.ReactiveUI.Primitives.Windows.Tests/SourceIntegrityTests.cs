// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Verifies repository source files have not been truncated or left with merge markers.</summary>
public sealed class SourceIntegrityTests
{
    /// <summary>The minimum byte count expected for a real tracked source file.</summary>
    private const long MinimumSourceBytes = 64;

    /// <summary>The source and project file patterns protected by the integrity check.</summary>
    private static readonly string[] FilePatterns = ["*.cs", "*.csproj", "*.props", "*.targets", "*.sln", "*.slnx"];

    /// <summary>The repository directories excluded from the integrity check.</summary>
    private static readonly string[] ExcludedDirectoryNames =
    [
        ".codex-diagnostics",
        ".codex-recovery",
        ".codex-tools",
        ".git",
        ".vs",
        "artifacts",
        "bin",
        "obj",
        "TestResults",
    ];

    /// <summary>Validates every tracked C# source file in the repository.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task TrackedSourcesArePresentAndNonEmptyAsync()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourcePaths = GetSourcePaths(repositoryRoot);
        var failures = new List<string>();

        foreach (var relativePath in sourcePaths)
        {
            var fullPath = Path.Combine(repositoryRoot, relativePath);
            if (!File.Exists(fullPath))
            {
                failures.Add($"Missing tracked source: {relativePath}");
                continue;
            }

            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Length < MinimumSourceBytes)
            {
                failures.Add(FormattableString.Invariant($"Suspiciously small source file ({fileInfo.Length} bytes): {relativePath}"));
                continue;
            }

            var content = await ReadAllTextAsync(fullPath);
            if (ContainsMergeConflictMarker(content))
            {
                failures.Add($"Merge conflict marker found in source: {relativePath}");
            }
        }

        await Assert.That(failures).IsEmpty();
    }

    /// <summary>Finds the repository root by walking up from the test assembly output directory.</summary>
    /// <returns>The repository root directory path.</returns>
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    }

    /// <summary>Returns repository source and build-definition paths.</summary>
    /// <param name="repositoryRoot">The repository root directory.</param>
    /// <returns>The repository source paths.</returns>
    private static string[] GetSourcePaths(string repositoryRoot)
    {
        var files = new List<string>();
        foreach (var pattern in FilePatterns)
        {
            foreach (var filePath in Directory.EnumerateFiles(repositoryRoot, pattern, SearchOption.AllDirectories))
            {
                if (IsExcluded(filePath))
                {
                    continue;
                }

                files.Add(GetRelativePath(repositoryRoot, filePath));
            }
        }

        files.Sort(StringComparer.OrdinalIgnoreCase);
        return files.ToArray();
    }

    /// <summary>Reads all text from a file without blocking the caller thread.</summary>
    /// <param name="filePath">The file path to read.</param>
    /// <returns>The file contents.</returns>
    private static async Task<string> ReadAllTextAsync(string filePath)
    {
        using var reader = File.OpenText(filePath);
        return await reader.ReadToEndAsync();
    }

    /// <summary>Determines whether content contains a Git merge-conflict marker line.</summary>
    /// <param name="content">The source text to inspect.</param>
    /// <returns><see langword="true" /> when a merge-conflict marker line is present.</returns>
    private static bool ContainsMergeConflictMarker(string content)
    {
        using var reader = new StringReader(content);
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
        {
            if (line.StartsWith("<<<<<<< ", StringComparison.Ordinal)
                || line.StartsWith("=======", StringComparison.Ordinal)
                || line.StartsWith(">>>>>>> ", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Returns a repository-relative path using APIs available on every target framework.</summary>
    /// <param name="repositoryRoot">The repository root directory.</param>
    /// <param name="filePath">The file path to relativize.</param>
    /// <returns>The repository-relative path.</returns>
    private static string GetRelativePath(string repositoryRoot, string filePath)
    {
        var root = repositoryRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var prefix = root + Path.DirectorySeparatorChar;
        return filePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? filePath.Substring(prefix.Length) : filePath;
    }

    /// <summary>Determines whether a file path is below an excluded repository directory.</summary>
    /// <param name="filePath">The file path to inspect.</param>
    /// <returns><see langword="true" /> when the file is excluded; otherwise, <see langword="false" />.</returns>
    private static bool IsExcluded(string filePath)
    {
        var directory = new FileInfo(filePath).Directory;
        while (directory is not null)
        {
            if (Array.IndexOf(ExcludedDirectoryNames, directory.Name) >= 0)
            {
                return true;
            }

            directory = directory.Parent;
        }

        return false;
    }
}
