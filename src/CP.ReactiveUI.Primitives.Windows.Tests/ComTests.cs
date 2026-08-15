// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Interop.Com;
using CP.ReactiveUI.Primitives.Windows.Tests.ComInterfaces;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Com Tests behavior.</summary>
public class ComTests
{
    /// <summary>Writes diagnostic messages for these tests.</summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(ComTests));

    /// <summary>Test the clsId and progId conversion code, works only when Excel is installed.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task Test_ClsIdProgIdAsync()
    {
        const string progId = "Excel.Application";
        var clsId = Ole32Api.ClassIdFromProgId(progId);
        Log.InfoFormat("The prog-id {0} has clsid {1}", progId, clsId);
        await Assert.That(clsId == default).IsFalse();
        var progIdResolve = Ole32Api.ProgIdFromClassId(clsId);
        Log.InfoFormat("The prog-id {0}, resolve back from {1}, is: {2}", progId, clsId, progIdResolve);
        await Assert.That(progIdResolve).StartsWith(progId);

        var excelApplicationType = Type.GetTypeFromCLSID(clsId);
        await Assert.That(excelApplicationType).IsNotNull();
        using var app = DisposableCom.Create(Activator.CreateInstance(excelApplicationType));
        await Assert.That(app).IsNotNull();

        using var excelApp = OleAut32Api.GetActiveObject(progId, static activeObject => (IExcelApplication)activeObject);
        await Assert.That(excelApp).IsNotNull();
        await Assert.That(Marshal.IsComObject(app.ComObject)).IsTrue();
        await Assert.That(Marshal.IsComObject(excelApp.ComObject)).IsTrue();
    }
}
