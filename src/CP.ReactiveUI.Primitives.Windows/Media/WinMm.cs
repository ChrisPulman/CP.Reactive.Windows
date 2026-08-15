// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using ReactiveUI.Primitives.Disposables;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Media;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Media;
#endif
/// <summary>Windows Multi-Media API.</summary>
public static class WinMm
{
    /// <summary>Native WinMM entry points.</summary>
    private static class NativeMethods
    {
        /// <summary>Windows multimedia library name.</summary>
        private const string WinMmDll = "winmm.dll";

        /// <summary>Plays a sound from memory.</summary>
        /// <param name="soundBytes">Byte array with the wave information.</param>
        /// <param name="moduleHandle">Module handle for resource playback.</param>
        /// <param name="soundOptions">Flags for playing the sound.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        [DllImport("winmm.dll", EntryPoint = "PlaySoundW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PlaySound([In] byte[] soundBytes, UIntPtr moduleHandle, SoundSettings soundOptions);

        /// <summary>Plays a sound by name, file name, resource, or system event.</summary>
        /// <param name="soundName">The sound to play, or null to stop any currently playing waveform sound.</param>
        /// <param name="moduleHandle">Module handle for resource playback.</param>
        /// <param name="soundOptions">Flags for playing the sound.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        [DllImport("winmm.dll", CharSet = CharSet.Unicode, EntryPoint = "PlaySoundW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PlaySound(string soundName, UIntPtr moduleHandle, SoundSettings soundOptions);

        /// <summary>Plays a sound from a memory pointer.</summary>
        /// <param name="memoryPointer">Pointer to memory where a wav file is stored.</param>
        /// <param name="moduleHandle">Module handle for resource playback.</param>
        /// <param name="soundOptions">Flags for playing the sound.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        [DllImport("winmm.dll", EntryPoint = "PlaySoundW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PlaySound(IntPtr memoryPointer, UIntPtr moduleHandle, SoundSettings soundOptions);
    }

    /// <summary>Composes multimedia operations without invoking them during construction.</summary>
    /// <param name="playBytes">The byte-array playback operation.</param>
    /// <param name="playName">The named playback operation.</param>
    /// <param name="playPointer">The pointer playback operation.</param>
    private sealed class WinMmOperations(Func<byte[], UIntPtr, SoundSettings, bool> playBytes, Func<string, UIntPtr, SoundSettings, bool> playName, Func<IntPtr, UIntPtr, SoundSettings, bool> playPointer)
    {
        /// <summary>Plays byte-array wave data.</summary>
        /// <param name="soundBytes">The wave data.</param>
        /// <param name="moduleHandle">The module handle.</param>
        /// <param name="settings">The playback settings.</param>
        /// <returns>The configured operation result.</returns>
        public bool Play(byte[] soundBytes, UIntPtr moduleHandle, SoundSettings settings) => playBytes(soundBytes, moduleHandle, settings);

        /// <summary>Plays a named sound.</summary>
        /// <param name="soundName">The sound name.</param>
        /// <param name="moduleHandle">The module handle.</param>
        /// <param name="settings">The playback settings.</param>
        /// <returns>The configured operation result.</returns>
        public bool Play(string soundName, UIntPtr moduleHandle, SoundSettings settings) => playName(soundName, moduleHandle, settings);

        /// <summary>Plays sound data at a native memory address.</summary>
        /// <param name="memoryPointer">The wave-data address.</param>
        /// <param name="moduleHandle">The module handle.</param>
        /// <param name="settings">The playback settings.</param>
        /// <returns>The configured operation result.</returns>
        public bool Play(IntPtr memoryPointer, UIntPtr moduleHandle, SoundSettings settings) => playPointer(memoryPointer, moduleHandle, settings);
    }

    /// <summary>Multimedia operations used by this process.</summary>
    private static WinMmOperations _operations = new(NativeMethods.PlaySound, NativeMethods.PlaySound, NativeMethods.PlaySound);

    /// <summary>Play a system sound.</summary>
    /// <param name="systemSound">Value from the SystemSounds enum.</param>
    public static void PlaySystemSound(SystemSounds systemSound) => _operations.Play(systemSound.ToString(), UIntPtr.Zero, SoundSettings.AliasId | SoundSettings.Async);

    /// <summary>Play a resource.</summary>
    /// <param name="resource">Resource to play.</param>
    public static void Play(string resource) => _operations.Play(resource, UIntPtr.Zero, SoundSettings.Resource | SoundSettings.Async);

    /// <summary>Play a wav from memory.</summary>
    /// <param name="memoryPtr">Pointer to the wav file to play.</param>
    /// <param name="settings">Sound settings.</param>
    public static void Play(IntPtr memoryPtr, SoundSettings settings) => _operations.Play(memoryPtr, UIntPtr.Zero, settings);

    /// <summary>
    /// Play wave data.
    /// Note: The byte[] should be pinned into memory, and cannot be removed while playing!!
    /// See <a href="https://blogs.msdn.microsoft.com/larryosterman/2009/02/19/playsoundxxx-snd_memory-snd_async-is-almost-always-a-bad-idea/">PlaySound(xxx, SND_MEMORY | SND_ASYNC)</a>.
    /// </summary>
    /// <param name="soundBytes">Wave data to play.</param>
    public static void Play(byte[] soundBytes) => _operations.Play(soundBytes, UIntPtr.Zero, SoundSettings.Async | SoundSettings.Memory);

    /// <summary>Stop playing.</summary>
    public static void StopPlaying() => _operations.Play((string)null, UIntPtr.Zero, SoundSettings.None);

    /// <summary>Overrides multimedia operations for deterministic tests.</summary>
    /// <param name="playBytes">The replacement byte-array playback operation.</param>
    /// <param name="playName">The replacement named playback operation.</param>
    /// <param name="playPointer">The replacement pointer playback operation.</param>
    /// <returns>A scope that restores the previous operations.</returns>
    internal static IDisposable OverrideOperationsForTesting(Func<byte[], UIntPtr, SoundSettings, bool> playBytes, Func<string, UIntPtr, SoundSettings, bool> playName, Func<IntPtr, UIntPtr, SoundSettings, bool> playPointer)
    {
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(playBytes);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(playName);
        CP.ReactiveUI.Primitives.Windows.PolyFills.Throw.IfNull(playPointer);
        WinMmOperations operations = _operations;
        _operations = new(playBytes, playName, playPointer);
        return Scope.Create(operations, delegate(WinMmOperations previous)
        {
            _operations = previous;
        });
    }
}
