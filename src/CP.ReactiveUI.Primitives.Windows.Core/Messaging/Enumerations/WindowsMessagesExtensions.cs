// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations;

/// <summary>Extension methods for <see cref="T:CP.ReactiveUI.Primitives.Windows.Desktop.Messaging.Enumerations.WindowsMessages" />.</summary>
public static class WindowsMessagesExtensions
{
    /// <summary>Provides operations for a single Windows message.</summary>
    /// <param name="message">The message to examine.</param>
    extension(WindowsMessages message)
    {
        /// <summary>Checks whether the message matches any of the specified values.</summary>
        /// <param name="messages">The candidate messages.</param>
        /// <returns><see langword="true" /> when <paramref name="message" /> is in <paramref name="messages" />.</returns>
        public bool IsIn(params WindowsMessages[] messages) =>
            Array.IndexOf(messages, message) >= 0;
    }
}
