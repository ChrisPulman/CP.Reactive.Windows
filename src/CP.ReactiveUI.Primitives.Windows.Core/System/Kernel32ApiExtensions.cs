// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using CP.ReactiveUI.Primitives.Windows.PolyFills;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel;

/// <summary>Kernel32 extension members.</summary>
public static class Kernel32ApiExtensions
{
    extension(Process process)
    {
        /// <summary>Method to get the process path for a process.</summary>
        /// <returns>Process path.</returns>
        public string GetProcessPath()
        {
            Throw.IfNull(process);
            return Kernel32Api.GetProcessPath(process.Id);
        }
    }
}
