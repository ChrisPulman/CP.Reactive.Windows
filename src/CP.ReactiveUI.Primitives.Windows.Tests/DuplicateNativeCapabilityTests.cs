// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Guards the single-owner rule for unmanaged Windows capabilities.</summary>
public class DuplicateNativeCapabilityTests
{
    /// <summary>The DLL suffix length.</summary>
    private const int DllSuffixLength = 4;

    /// <summary>Ensures an exact unmanaged signature is declared by only one managed owner.</summary>
    /// <returns>A task that completes when the assertion has run.</returns>
    [Test]
    public async Task NativeImportsHaveOneOwnerAsync()
    {
        var sourceRoot = FindSourceRoot();
        var importsByKey = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var filePath in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (IsSkippedPath(filePath))
            {
                continue;
            }

            AddImportsFromFile(sourceRoot, filePath, importsByKey);
        }

        var duplicates = new List<string>();
        foreach (var entry in importsByKey)
        {
            if (entry.Value.Count <= 1)
            {
                continue;
            }

            duplicates.Add($"{entry.Key}: {string.Join(", ", entry.Value)}");
        }

        duplicates.Sort(StringComparer.Ordinal);
        await Assert.That(duplicates).IsEmpty();
    }

    /// <summary>Adds native imports discovered in a source file.</summary>
    /// <param name="sourceRoot">The repository source directory.</param>
    /// <param name="filePath">The file to scan.</param>
    /// <param name="importsByKey">The discovered import owners keyed by normalized import signature.</param>
    private static void AddImportsFromFile(string sourceRoot, string filePath, Dictionary<string, List<string>> importsByKey)
    {
        var lines = File.ReadAllLines(filePath);
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (!TryReadImport(line, out var import))
            {
                continue;
            }

            var methodLine = FindMethodLine(lines, index + 1);
            if (methodLine.Length == 0)
            {
                continue;
            }

            var methodName = GetMethodName(methodLine);
            var entryPoint = import.EntryPoint ?? methodName;
            var key = $"{NormalizeLibraryName(import.Library)}!{entryPoint.ToUpperInvariant()}:{NormalizeMethodSignature(methodLine)}";
#if NETFRAMEWORK
            if (!importsByKey.TryGetValue(key, out var owners))
            {
                owners = [];
                importsByKey.Add(key, owners);
            }

            var owner = GetRelativePath(sourceRoot, filePath);
#else
            ref var owners = ref CollectionsMarshal.GetValueRefOrAddDefault(importsByKey, key, out var exists);
            if (!exists)
            {
                owners = [];
            }

            var owner = Path.GetRelativePath(sourceRoot, filePath);
#endif
            if (owners.IndexOf(owner) < 0)
            {
                owners.Add(owner);
            }
        }
    }

    /// <summary>Finds the next method signature after an import attribute.</summary>
    /// <param name="lines">The source lines.</param>
    /// <param name="startIndex">The line index to start from.</param>
    /// <returns>The method signature line.</returns>
    private static string FindMethodLine(string[] lines, int startIndex)
    {
        var signature = new StringBuilder();
        for (var index = startIndex; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (line.Length == 0
#if NETFRAMEWORK
                || line.StartsWith("[", StringComparison.Ordinal)
                || line.StartsWith("#", StringComparison.Ordinal))
#else
                || line.StartsWith('[')
                || line.StartsWith('#'))
#endif
            {
                continue;
            }

            if (signature.Length > 0)
            {
                _ = signature.Append(' ');
            }

            _ = signature.Append(line);
            if (
#if NETFRAMEWORK
                line.IndexOf(';') >= 0)
#else
                line.Contains(';'))
#endif
            {
                return signature.ToString();
            }
        }

        return string.Empty;
    }

    /// <summary>Finds the repository source directory.</summary>
    /// <returns>The absolute source directory path.</returns>
    private static string FindSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var sourcePath = Path.Combine(directory.FullName, "src");
            var solutionPath = Path.Combine(sourcePath, "CP.ReactiveUI.Primitives.Windows.slnx");
            if (Directory.Exists(sourcePath) && File.Exists(solutionPath))
            {
                return sourcePath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository src directory.");
    }

    /// <summary>Gets the method name from a source signature.</summary>
    /// <param name="methodLine">The method signature line.</param>
    /// <returns>The method name.</returns>
    private static string GetMethodName(string methodLine)
    {
        var parenthesisIndex = methodLine.IndexOf('(');
        if (parenthesisIndex < 0)
        {
            return methodLine;
        }

        var endIndex = parenthesisIndex - 1;
        while (endIndex >= 0 && char.IsWhiteSpace(methodLine[endIndex]))
        {
            endIndex--;
        }

        var startIndex = endIndex;
        while (startIndex >= 0 && IsIdentifierCharacter(methodLine[startIndex]))
        {
            startIndex--;
        }

        return methodLine.Substring(startIndex + 1, endIndex - startIndex);
    }

    /// <summary>Determines whether a character can be part of a C# identifier.</summary>
    /// <param name="value">The character to check.</param>
    /// <returns><see langword="true"/> when the character can be part of an identifier.</returns>
    private static bool IsIdentifierCharacter(char value) => char.IsLetterOrDigit(value) || value == '_';

    /// <summary>Determines whether a file path should be skipped.</summary>
    /// <param name="filePath">The path to inspect.</param>
    /// <returns><see langword="true"/> when the path should not be scanned.</returns>
    private static bool IsSkippedPath(string filePath) =>
#if NETFRAMEWORK
        filePath.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0
        || filePath.IndexOf($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0
        || filePath.IndexOf($"{Path.DirectorySeparatorChar}CP.ReactiveUI.Primitives.Windows.Tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) >= 0;
#else
        filePath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || filePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || filePath.Contains($"{Path.DirectorySeparatorChar}CP.ReactiveUI.Primitives.Windows.Tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
#endif

    /// <summary>Normalizes an unmanaged library name for single-owner comparison.</summary>
    /// <param name="libraryName">The library name from interop metadata.</param>
    /// <returns>The normalized library name without a path or DLL suffix.</returns>
    private static string NormalizeLibraryName(string libraryName)
    {
        var normalizedName = libraryName.Replace('\\', '/');
        normalizedName = normalizedName.Substring(normalizedName.LastIndexOf('/') + 1);
        return normalizedName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? normalizedName.Substring(0, normalizedName.Length - DllSuffixLength).ToUpperInvariant()
            : normalizedName.ToUpperInvariant();
    }

    /// <summary>Normalizes a method signature line for import comparison.</summary>
    /// <param name="methodLine">The method signature line.</param>
    /// <returns>The normalized signature.</returns>
    private static string NormalizeMethodSignature(string methodLine)
    {
        var builder = new StringBuilder(methodLine.Length);
        var normalizedModifiers = methodLine
            .Replace("extern", string.Empty)
            .Replace("partial", string.Empty);
        foreach (var character in normalizedModifiers)
        {
            if (!char.IsWhiteSpace(character))
            {
                _ = builder.Append(character);
            }
        }

        return builder.ToString();
    }

    /// <summary>Reads import metadata from an attribute line.</summary>
    /// <param name="line">The source line.</param>
    /// <param name="import">The discovered import metadata.</param>
    /// <returns><see langword="true"/> when an import was found.</returns>
    private static bool TryReadImport(string line, out ImportInfo import)
    {
        import = default;
        var attributeName = string.Empty;
        if (
#if NETFRAMEWORK
            line.IndexOf("LibraryImport", StringComparison.Ordinal) >= 0)
#else
            line.Contains("LibraryImport", StringComparison.Ordinal))
#endif
        {
            attributeName = "LibraryImport";
        }
        else if (
#if NETFRAMEWORK
            line.IndexOf("DllImport", StringComparison.Ordinal) >= 0)
#else
            line.Contains("DllImport", StringComparison.Ordinal))
#endif
        {
            attributeName = "DllImport";
        }

        if (attributeName.Length == 0)
        {
            return false;
        }

        var library = ReadFirstQuotedValue(line);
        if (library.Length == 0)
        {
            return false;
        }

        var entryPoint = ReadNamedQuotedValue(line, "EntryPoint");
        import = new(library, entryPoint.Length == 0 ? null : entryPoint);
        return true;
    }

    /// <summary>Reads the first quoted value from a source line.</summary>
    /// <param name="line">The source line.</param>
    /// <returns>The quoted value, or an empty string.</returns>
    private static string ReadFirstQuotedValue(string line)
    {
        var startIndex = line.IndexOf('"');
        if (startIndex < 0)
        {
            return string.Empty;
        }

        var endIndex = line.IndexOf('"', startIndex + 1);
        return endIndex < 0 ? string.Empty : line.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    /// <summary>Reads the first quoted value from a source line, beginning at an offset.</summary>
    /// <param name="line">The source line.</param>
    /// <param name="startIndex">The character index at which to begin searching.</param>
    /// <returns>The quoted value, or an empty string.</returns>
    private static string ReadFirstQuotedValue(string line, int startIndex)
    {
        var quoteIndex = line.IndexOf('"', startIndex);
        if (quoteIndex < 0)
        {
            return string.Empty;
        }

        var endIndex = line.IndexOf('"', quoteIndex + 1);
        return endIndex < 0 ? string.Empty : line.Substring(quoteIndex + 1, endIndex - quoteIndex - 1);
    }

    /// <summary>Reads a named quoted attribute value from a source line.</summary>
    /// <param name="line">The source line.</param>
    /// <param name="name">The attribute property name.</param>
    /// <returns>The quoted value, or an empty string.</returns>
    private static string ReadNamedQuotedValue(string line, string name)
    {
        var nameIndex = line.IndexOf(name, StringComparison.Ordinal);
        return nameIndex < 0 ? string.Empty : ReadFirstQuotedValue(line, nameIndex);
    }

#if NETFRAMEWORK
    /// <summary>Builds a relative path without requiring newer runtime APIs.</summary>
    /// <param name="basePath">The source directory path.</param>
    /// <param name="filePath">The file path to make relative.</param>
    /// <returns>The relative file path.</returns>
    private static string GetRelativePath(string basePath, string filePath)
    {
        var baseUri = new Uri(AppendDirectorySeparator(basePath));
        var fileUri = new Uri(filePath);
        var relativeUri = baseUri.MakeRelativeUri(fileUri);
        return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
    }
#endif

#if NETFRAMEWORK
    /// <summary>Ensures a path is interpreted as a directory by <see cref="Uri"/>.</summary>
    /// <param name="path">The directory path.</param>
    /// <returns>The directory path with a trailing separator.</returns>
    private static string AppendDirectorySeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
#endif

    /// <summary>Describes an unmanaged library import.</summary>
    /// <param name="Library">The unmanaged library.</param>
    /// <param name="EntryPoint">The unmanaged entry point.</param>
    private readonly record struct ImportInfo(string Library, string EntryPoint);
}
