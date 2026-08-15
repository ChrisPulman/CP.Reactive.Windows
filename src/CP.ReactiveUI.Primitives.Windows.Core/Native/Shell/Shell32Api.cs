// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Native.Shell.Enums;
using CP.ReactiveUI.Primitives.Windows.Native.Shell.Structs;

namespace CP.ReactiveUI.Primitives.Windows.Native.Shell;

/// <summary>An API for Shell32 functionality.</summary>
#if NET462 || NET472 || NET48 || NET481
public static class Shell32Api
#else
public static partial class Shell32Api
#endif
{
    /// <summary>Invokes a shell-file-information operation over pinned input and output buffers.</summary>
    /// <param name="path">The pinned path pointer.</param>
    /// <param name="fileAttributes">The file attributes.</param>
    /// <param name="fileInfo">The pinned native information buffer.</param>
    /// <param name="fileInfoSize">The native information buffer size.</param>
    /// <param name="flags">The shell information flags.</param>
    /// <returns>The native shell result.</returns>
    internal delegate IntPtr ShellFileInfoOperation(
        IntPtr path,
        ShellFileAttributeFlags fileAttributes,
        IntPtr fileInfo,
        uint fileInfoSize,
        ShellGetFileInfoFlags flags);

    /// <summary>Gets an AppBarData struct which describes the taskbar bounds.</summary>
    /// <returns>AppBarData.</returns>
    public static AppBarData TaskbarPosition
    {
        get
        {
            AppBarData appBarData = AppBarData.Create();
            _ = AppBarMessage(AppBarMessages.GetTaskbarPosition, ref appBarData);
            return appBarData;
        }
    }

    /// <summary>Get the Icon from a file.</summary>
    /// <param name="filePath">Source file.</param>
    /// <param name="iconIndex">Icon index.</param>
    /// <param name="largeIconHandle">Large icon handle.</param>
    /// <param name="smallIconHandle">Small icon handle.</param>
    /// <param name="amountIcons">Number of icons to extract.</param>
    /// <returns>The number of extracted icons.</returns>
    public static int ExtractIconEx(
        string filePath,
        int iconIndex,
        out IntPtr largeIconHandle,
        out IntPtr smallIconHandle,
        int amountIcons) =>
        NativeMethods.ExtractIconEx(
            filePath,
            iconIndex,
            out largeIconHandle,
            out smallIconHandle,
            amountIcons);

    /// <summary>Retrieves information about an object in the file system.</summary>
    /// <param name="path">Path to the file system object.</param>
    /// <param name="fileAttributes">File attributes.</param>
    /// <param name="fileInfo">Shell file information.</param>
    /// <param name="fileInfoSize">Requested native shell file information buffer size, or zero to use the native structure size.</param>
    /// <param name="flags">Shell file information flags.</param>
    /// <returns>Native shell result.</returns>
    public static IntPtr SHGetFileInfo(
        string path,
        ShellFileAttributeFlags fileAttributes,
        ref ShellFileInfo fileInfo,
        uint fileInfoSize,
        ShellGetFileInfoFlags flags) =>
        NativeMethods.SHGetFileInfo(path, fileAttributes, ref fileInfo, fileInfoSize, flags);

    /// <summary>Gets the native buffer size required for shell file information.</summary>
    /// <param name="requestedSize">The caller-requested buffer size.</param>
    /// <param name="nativeSize">The minimum native structure size.</param>
    /// <returns>The larger of the requested and native structure sizes.</returns>
    internal static uint GetShellFileInfoBufferSize(uint requestedSize, uint nativeSize) =>
        Math.Max(requestedSize, nativeSize);

    /// <summary>Overrides the pinned shell-file-information operation for deterministic tests.</summary>
    /// <param name="operation">The replacement operation.</param>
    /// <returns>A scope that restores the previous operation.</returns>
    internal static IDisposable OverrideShellFileInfoOperationForTesting(ShellFileInfoOperation operation) =>
        NativeMethods.OverrideShellFileInfoOperationForTesting(operation);

    /// <summary>Sends an appbar message to the system. See <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/bb762108.aspx">SHAppBarMessage function</a>.</summary>
    /// <param name="message">Appbar message value to send.</param>
    /// <param name="data">Appbar data.</param>
    /// <returns>Native appbar result.</returns>
    private static IntPtr AppBarMessage(AppBarMessages message, ref AppBarData data)
    {
        AppBarData.NativeAppBarData nativeData = data.ToNative();
        IntPtr result = NativeMethods.AppBarMessage(message, ref nativeData);
        data.Apply(in nativeData);
        return result;
    }

    /// <summary>Native shell32 entry points.</summary>
#if NET462 || NET472 || NET48 || NET481
    private static class NativeMethods
#else
    private static partial class NativeMethods
#endif
    {
        /// <summary>The Shell32 DLL library name.</summary>
        private const string Shell32Dll = "shell32.dll";

        /// <summary>The number of bytes in a 32-bit native field.</summary>
        private const int UInt32Size = sizeof(uint);

        /// <summary>The shell display name character length.</summary>
        private const int DisplayNameCharLength = 260;

        /// <summary>The shell type name character length.</summary>
        private const int TypeNameCharLength = 80;

        /// <summary>The number of bytes in a UTF-16 character.</summary>
        private const int Utf16CharacterSize = sizeof(char);

        /// <summary>The shell file information icon handle offset.</summary>
        private const int IconHandleOffset = 0;

        /// <summary>The shell file information display name byte length.</summary>
        private const int DisplayNameByteLength = DisplayNameCharLength * Utf16CharacterSize;

        /// <summary>The shell file information type name byte length.</summary>
        private const int TypeNameByteLength = TypeNameCharLength * Utf16CharacterSize;

        /// <summary>The Shell32 module handle.</summary>
        private static readonly IntPtr Shell32Module = NativeLibrary.Load(
            Path.Combine(Environment.SystemDirectory, "shell32.dll"));

        /// <summary>The SHAppBarMessage export pointer.</summary>
        private static readonly IntPtr SHAppBarMessageExport = NativeLibrary.GetExport(
            Shell32Module,
            "SHAppBarMessage");

        /// <summary>The SHGetFileInfo export pointer.</summary>
        private static readonly IntPtr SHGetFileInfoExport = NativeLibrary.GetExport(
            Shell32Module,
            "SHGetFileInfoW");

        /// <summary>Stores the operation invoked for a pinned shell-information buffer.</summary>
        private static ShellFileInfoOperation _shellFileInfoOperation = InvokeShellFileInfo;

        /// <summary>Gets the shell file information icon index offset.</summary>
        private static int IconIndexOffset => IntPtr.Size;

        /// <summary>Gets the shell file information attributes offset.</summary>
        private static int AttributesOffset => checked(IconIndexOffset + UInt32Size);

        /// <summary>Gets the shell file information display name offset.</summary>
        private static int DisplayNameOffset => checked(AttributesOffset + UInt32Size);

        /// <summary>Gets the shell file information type name offset.</summary>
        private static int TypeNameOffset => checked(DisplayNameOffset + DisplayNameByteLength);

        /// <summary>Gets the shell file information native size.</summary>
        private static int NativeShellFileInfoSize => checked(TypeNameOffset + TypeNameByteLength);

        /// <summary>Extracts icons from a file.</summary>
        /// <param name="filePath">Source file.</param>
        /// <param name="iconIndex">Icon index.</param>
        /// <param name="largeIconHandle">Large icon handle.</param>
        /// <param name="smallIconHandle">Small icon handle.</param>
        /// <param name="amountIcons">Number of icons to extract.</param>
        /// <returns>Number of extracted icons.</returns>
#if NET462 || NET472 || NET48 || NET481
        [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int ExtractIconEx(
            string filePath,
            int iconIndex,
            out IntPtr largeIconHandle,
            out IntPtr smallIconHandle,
            int amountIcons);
#else
        [LibraryImport(
            "shell32.dll",
            EntryPoint = "ExtractIconExW",
            StringMarshalling = StringMarshalling.Utf16)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static partial int ExtractIconEx(
            string filePath,
            int iconIndex,
            out IntPtr largeIconHandle,
            out IntPtr smallIconHandle,
            int amountIcons);
#endif

        /// <summary>Sends an appbar message to the system.</summary>
        /// <param name="message">Appbar message.</param>
        /// <param name="data">Appbar data.</param>
        /// <returns>Native result.</returns>
        internal static unsafe IntPtr AppBarMessage(
            AppBarMessages message,
            ref AppBarData.NativeAppBarData data)
        {
            fixed (AppBarData.NativeAppBarData* appBarData = &data)
            {
                return (
                    (delegate* unmanaged[Stdcall]<
                        AppBarMessages,
                        AppBarData.NativeAppBarData*,
                        IntPtr>)
                        (void*)SHAppBarMessageExport)(message, appBarData);
            }
        }

        /// <summary>Retrieves shell file information.</summary>
        /// <param name="path">Path or PIDL pointer represented as a string.</param>
        /// <param name="fileAttributes">File attributes.</param>
        /// <param name="fileInfo">Shell file information.</param>
        /// <param name="fileInfoSize">Shell file information size.</param>
        /// <param name="flags">Shell file information flags.</param>
        /// <returns>Native result.</returns>
        internal static unsafe IntPtr SHGetFileInfo(
            string path,
            ShellFileAttributeFlags fileAttributes,
            ref ShellFileInfo fileInfo,
            uint fileInfoSize,
            ShellGetFileInfoFlags flags)
        {
            uint bufferSize = GetShellFileInfoBufferSize(
                fileInfoSize,
                checked((uint)NativeShellFileInfoSize));
            byte[] nativeInfo = new byte[checked((int)bufferSize)];
            ref byte nativeInfoReference = ref MemoryMarshal.GetReference(nativeInfo.AsSpan());
            fixed (char* pathPointer = path)
            {
                fixed (byte* nativeInfoPointer = &nativeInfoReference)
                {
                    IntPtr result = Volatile.Read(ref _shellFileInfoOperation)(
                        (IntPtr)pathPointer,
                        fileAttributes,
                        (IntPtr)nativeInfoPointer,
                        bufferSize,
                        flags);
                    fileInfo = ToShellFileInfo(nativeInfo);
                    return result;
                }
            }
        }

        /// <summary>Overrides the pinned shell-file-information operation for deterministic tests.</summary>
        /// <param name="operation">The replacement operation.</param>
        /// <returns>A scope that restores the previous operation.</returns>
        internal static IDisposable OverrideShellFileInfoOperationForTesting(ShellFileInfoOperation operation)
        {
            Throw.IfNull(operation);
            ShellFileInfoOperation previous = Interlocked.Exchange(ref _shellFileInfoOperation, operation);
            return Scope.Create(
                previous,
                static value => Volatile.Write(ref _shellFileInfoOperation, value));
        }

        /// <summary>Invokes the native shell-file-information entry point.</summary>
        /// <param name="path">The pinned path pointer.</param>
        /// <param name="fileAttributes">The file attributes.</param>
        /// <param name="fileInfo">The pinned native information buffer.</param>
        /// <param name="fileInfoSize">The native information buffer size.</param>
        /// <param name="flags">The shell information flags.</param>
        /// <returns>The native shell result.</returns>
        private static unsafe IntPtr InvokeShellFileInfo(
            IntPtr path,
            ShellFileAttributeFlags fileAttributes,
            IntPtr fileInfo,
            uint fileInfoSize,
            ShellGetFileInfoFlags flags) =>
            ((delegate* unmanaged[Stdcall]<
                char*,
                ShellFileAttributeFlags,
                void*,
                uint,
                ShellGetFileInfoFlags,
                IntPtr>)
                (void*)SHGetFileInfoExport)(
                (char*)path,
                fileAttributes,
                (void*)fileInfo,
                fileInfoSize,
                flags);

        /// <summary>Converts a native shell file information buffer to the public value.</summary>
        /// <param name="nativeInfo">The native shell file information buffer.</param>
        /// <returns>The public shell file information value.</returns>
        private static ShellFileInfo ToShellFileInfo(ReadOnlySpan<byte> nativeInfo)
        {
            IntPtr iconHandle = MemoryMarshal.Read<IntPtr>(nativeInfo.Slice(0));
            int iconIndex = MemoryMarshal.Read<int>(nativeInfo.Slice(IconIndexOffset));
            uint attributes = MemoryMarshal.Read<uint>(nativeInfo.Slice(AttributesOffset));
            string displayName = ReadNullTerminatedUtf16(
                nativeInfo.Slice(DisplayNameOffset, DisplayNameByteLength));
            string typeName = ReadNullTerminatedUtf16(
                nativeInfo.Slice(TypeNameOffset, TypeNameByteLength));
            return new(iconHandle, iconIndex, attributes, displayName, typeName);
        }

        /// <summary>Reads a null-terminated UTF-16 string from bytes.</summary>
        /// <param name="bytes">The UTF-16 byte span.</param>
        /// <returns>The string up to the first null character.</returns>
        private static string ReadNullTerminatedUtf16(ReadOnlySpan<byte> bytes) =>
            NativeUtf16String.ReadNullTerminated(bytes);
    }
}
