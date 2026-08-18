// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Icons;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Shell.Icons;
#endif
/// <summary>Extracts associated large and small icon handles from a file.</summary>
/// <param name="filePath">The source file path.</param>
/// <param name="index">The zero-based icon index.</param>
/// <param name="largeIcon">Receives the large icon handle.</param>
/// <param name="smallIcon">Receives the small icon handle.</param>
/// <param name="iconCount">The number of icons to extract.</param>
/// <returns>The number of extracted icons.</returns>
internal delegate int ExtractAssociatedIconOperation(
    string filePath,
    int index,
    out IntPtr largeIcon,
    out IntPtr smallIcon,
    int iconCount);
