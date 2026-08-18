// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.

using System;
using System.Windows.Forms;
using ReactiveUI.Builder;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        _ = RxAppBuilder.CreateReactiveUIBuilder()
            .WithWinForms()
            .BuildApp();

        using var service = new WindowsOperationsService();
        using var viewModel = new OperationsCenterViewModel(service);
        Application.Run(new OperationsCenterForm(viewModel));
    }
}
