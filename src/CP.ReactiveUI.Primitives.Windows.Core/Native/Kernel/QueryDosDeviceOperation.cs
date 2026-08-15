// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Defines a DOS-device-query operation.</summary>
/// <param name="deviceName">The MS-DOS device name.</param>
/// <param name="targetPath">The destination buffer.</param>
/// <param name="maximumLength">The destination buffer capacity.</param>
/// <returns>The number of characters written.</returns>
internal unsafe delegate int QueryDosDeviceOperation(string deviceName, char* targetPath, int maximumLength);
