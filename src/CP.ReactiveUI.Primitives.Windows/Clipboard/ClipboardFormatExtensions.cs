// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Clipboard;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard;
#endif
/// <summary>These are extensions to work with the clipboard.</summary>
public static class ClipboardFormatExtensions
{
    /// <summary>The clipboard format name buffer capacity.</summary>
    private const int FormatNameCapacity = 256;

    /// <summary>Used for internal cache locking.</summary>
    private static readonly object Lock;

    /// <summary>Cache for all known clipboard format names keyed by format identifier.</summary>
    private static readonly Dictionary<uint, string> Id2Format;

    /// <summary>Cache for all known clipboard format identifiers keyed by format name.</summary>
    private static readonly Dictionary<string, uint> Format2Id;

    /// <summary>Native clipboard format operations used by this type.</summary>
    private static ClipboardFormatOperations _operations;

    /// <summary>Native clipboard format-name operation used by this type.</summary>
    private static NativeFormatNameOperation _nativeFormatNameOperation = GetClipboardFormatNameNative;

    /// <summary>Native clipboard format-name operation used after pinning a managed destination buffer.</summary>
    private static unsafe NativeClipboardFormatNamePointerOperation _nativeClipboardFormatNameOperation = NativeMethods.GetClipboardFormatName;

    /// <summary>Initializes static data of the class.</summary>
    static ClipboardFormatExtensions()
    {
        Lock = new();
        Id2Format = new();
        Format2Id = new();
        _operations = new(NativeMethods.EnumClipboardFormats, NativeMethods.RegisterClipboardFormat, GetNativeFormatName, Marshal.GetLastWin32Error);
        StandardClipboardFormats[] array = CP.ReactiveUI.Primitives.Windows.PolyFills.EnumValues.Get<StandardClipboardFormats>();
        foreach (StandardClipboardFormats enumValue in array)
        {
            string formatName = enumValue.AsString();
            if (!string.IsNullOrEmpty(formatName))
            {
                uint id = (uint)enumValue;
                Format2Id[formatName] = id;
                Id2Format[id] = formatName;
            }
        }
    }

    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="clipboardAccessToken">The extended instance.</param>
    extension(IClipboardAccessToken clipboardAccessToken)
    {
        /// <summary>Enumerates all formats on the clipboard, assuming the clipboard was already locked.</summary>
        /// <returns>The available clipboard format names.</returns>
        public IEnumerable<string> AvailableFormats()
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            foreach (uint item in clipboardAccessToken.AvailableFormatIds())
            {
                string format = MapIdToFormat(item);
                if (!string.IsNullOrEmpty(format))
                {
                    yield return format;
                }
            }
        }

        /// <summary>Enumerates all format identifiers on the clipboard, assuming the clipboard was already locked.</summary>
        /// <returns>The available clipboard format identifiers.</returns>
        public IEnumerable<uint> AvailableFormatIds()
        {
            clipboardAccessToken.ThrowWhenNoAccess();
            uint clipboardFormatId = 0U;
            while (true)
            {
                clipboardFormatId = _operations.EnumFormats(clipboardFormatId);
                if (clipboardFormatId == 0)
                {
                    break;
                }

                yield return clipboardFormatId;
            }

            if (_operations.GetLastError() == 0)
            {
                yield break;
            }

            throw new Win32Exception();
        }
    }

    /// <summary>Provides extension members for the target instance.</summary>
    /// <param name="format">The extended instance.</param>
    extension(StandardClipboardFormats format)
    {
        /// <summary>Gets the format string for the <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Clipboard.StandardClipboardFormats" /> value.</summary>
        /// <returns>The clipboard format string.</returns>
        public string AsString()
        {
            MemberInfo[] member = typeof(StandardClipboardFormats).GetMember(format.ToString());
            return member.Length != 0 ? member[0].GetCustomAttribute<DisplayAttribute>()?.Name : null;
        }
    }

    /// <summary>Maps a clipboard format name to an identifier.</summary>
    /// <param name="format">The clipboard format.</param>
    /// <returns>The clipboard format identifier.</returns>
    public static uint MapFormatToId(string format) =>
        Format2Id.TryGetValue(format, out var formatId) ? formatId : RegisterFormat(format);

    /// <summary>Maps a clipboard format identifier to a format name.</summary>
    /// <param name="formatId">The clipboard format identifier.</param>
    /// <returns>The clipboard format name.</returns>
    public static string MapIdToFormat(uint formatId)
    {
        if (Id2Format.TryGetValue(formatId, out var format))
        {
            return format;
        }

        format = _operations.GetFormatName(formatId);
        if (string.IsNullOrEmpty(format))
        {
            return null;
        }

        Id2Format[formatId] = format;
        Format2Id[format] = formatId;
        return format;
    }

    /// <summary>Registers the clipboard format so it can be used.</summary>
    /// <param name="format">The format to register.</param>
    /// <returns>The registered clipboard format identifier.</returns>
    public static uint RegisterFormat(string format)
    {
        lock (Lock)
        {
            if (Format2Id.TryGetValue(format, out var clipboardFormatId))
            {
                return clipboardFormatId;
            }

            clipboardFormatId = _operations.RegisterFormat(format);
            Id2Format[clipboardFormatId] = format;
            Format2Id[format] = clipboardFormatId;
            return clipboardFormatId;
        }
    }

    /// <summary>Overrides native clipboard format operations for deterministic tests.</summary>
    /// <param name="enumFormats">The replacement format enumeration operation.</param>
    /// <param name="registerFormat">The replacement format registration operation.</param>
    /// <param name="getFormatName">The replacement format-name query operation.</param>
    /// <param name="getLastError">The replacement last-error query operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<uint, uint> enumFormats, Func<string, uint> registerFormat, Func<uint, string> getFormatName, Func<int> getLastError)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(enumFormats);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(registerFormat);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(getFormatName);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(getLastError);
        ClipboardFormatOperations operations = _operations;
        _operations = new(enumFormats, registerFormat, getFormatName, getLastError);
        return Scope.Create(operations, static previous => _operations = previous);
    }

    /// <summary>Registers a deterministic format mapping for tests that replace native clipboard storage.</summary>
    /// <param name="format">The format name.</param>
    /// <param name="formatId">The deterministic format identifier.</param>
    internal static void RegisterCachedFormatForTesting(string format, uint formatId)
    {
        lock (Lock)
        {
            Format2Id[format] = formatId;
            Id2Format[formatId] = format;
        }
    }

    /// <summary>Gets a native clipboard format name for deterministic coverage tests.</summary>
    /// <param name="formatId">The clipboard format identifier.</param>
    /// <returns>The native clipboard format name, or <see langword="null" />.</returns>
    internal static string GetNativeFormatNameForTesting(uint formatId) => GetNativeFormatName(formatId);

    /// <summary>Overrides native clipboard format-name lookup for deterministic tests.</summary>
    /// <param name="getFormatName">The replacement native format-name operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideNativeFormatNameForTesting(NativeFormatNameOperation getFormatName)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(getFormatName);
        NativeFormatNameOperation previous = _nativeFormatNameOperation;
        _nativeFormatNameOperation = getFormatName;
        return Scope.Create(previous, static previousOperation => _nativeFormatNameOperation = previousOperation);
    }

    /// <summary>Overrides the pinned-buffer native format-name operation for deterministic tests.</summary>
    /// <param name="getFormatName">The replacement pinned-buffer format-name operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideNativeClipboardFormatNameForTesting(NativeClipboardFormatNamePointerOperation getFormatName)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(getFormatName);
        NativeClipboardFormatNamePointerOperation previous = _nativeClipboardFormatNameOperation;
        _nativeClipboardFormatNameOperation = getFormatName;
        return Scope.Create(previous, static previousOperation => _nativeClipboardFormatNameOperation = previousOperation);
    }

    /// <summary>Gets a clipboard format name through the native API.</summary>
    /// <param name="formatId">The clipboard format identifier.</param>
    /// <returns>The format name, or <see langword="null" /> when the format has no registered name.</returns>
    private static string GetNativeFormatName(uint formatId)
    {
        Span<char> clipboardFormatName = stackalloc char[FormatNameCapacity];
        int characterCount = _nativeFormatNameOperation(formatId, clipboardFormatName);
        return characterCount > 0
            ? CP.ReactiveUI.Primitives.Windows.PolyFills.SpanText.Create(
                clipboardFormatName.Slice(0, characterCount))
            : null;
    }

    /// <summary>Gets a clipboard format name through the native API.</summary>
    /// <param name="formatId">The clipboard format identifier.</param>
    /// <param name="clipboardFormatName">The destination buffer.</param>
    /// <returns>The copied character count, or zero when no registered name is available.</returns>
    private static unsafe int GetClipboardFormatNameNative(uint formatId, Span<char> clipboardFormatName)
    {
        fixed (char* formatName = clipboardFormatName)
        {
            return _nativeClipboardFormatNameOperation(formatId, formatName, FormatNameCapacity);
        }
    }

    /// <summary>Composes clipboard format operations without invoking them during construction.</summary>
    /// <param name="enumFormats">The format enumeration operation.</param>
    /// <param name="registerFormat">The format registration operation.</param>
    /// <param name="getFormatName">The format-name query operation.</param>
    /// <param name="getLastError">The last-error query operation.</param>
    private sealed class ClipboardFormatOperations(
        Func<uint, uint> enumFormats,
        Func<string, uint> registerFormat,
        Func<uint, string> getFormatName,
        Func<int> getLastError)
    {
        /// <summary>Enumerates clipboard format identifiers.</summary>
        /// <param name="formatId">The previous format identifier.</param>
        /// <returns>The next format identifier, or zero.</returns>
        public uint EnumFormats(uint formatId) => enumFormats(formatId);

        /// <summary>Gets a registered format name.</summary>
        /// <param name="formatId">The clipboard format identifier.</param>
        /// <returns>The format name.</returns>
        public string GetFormatName(uint formatId) => getFormatName(formatId);

        /// <summary>Gets the last native error code.</summary>
        /// <returns>The last error code.</returns>
        public int GetLastError() => getLastError();

        /// <summary>Registers a clipboard format name.</summary>
        /// <param name="format">The format name.</param>
        /// <returns>The registered format identifier.</returns>
        public uint RegisterFormat(string format) => registerFormat(format);
    }
}
