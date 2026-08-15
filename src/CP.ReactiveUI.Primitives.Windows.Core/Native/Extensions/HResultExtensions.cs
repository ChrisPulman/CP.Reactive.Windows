// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using CP.ReactiveUI.Primitives.Windows.Native.Enums;

namespace CP.ReactiveUI.Primitives.Windows.Native.Extensions;

/// <summary>Extensions to handle the HResult.</summary>
public static class HResultExtensions
{
    extension(HResult result)
    {
        /// <summary>Test if the HResult represents a fail.</summary>
        /// <returns>bool.</returns>
        public bool Failed() => (int)result < 0;

        /// <summary>Test if the HResult represents a success.</summary>
        /// <returns>bool.</returns>
        public bool Succeeded() => !result.Failed();

        /// <summary>Throw an exception on Failure.</summary>
        public void ThrowOnFailure()
        {
            if (result.Failed())
            {
                throw Marshal.GetExceptionForHR(checked((int)result));
            }
        }
    }
}
