// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests.ComInterfaces;

/// <summary>Defines the I Excel Application COM test contract.</summary>
[Guid("000208D5-0000-0000-C000-000000000046")]
[ComImport]
internal interface IExcelApplication
{
    /// <summary>Quits the Excel application.</summary>
    void Quit();
}
