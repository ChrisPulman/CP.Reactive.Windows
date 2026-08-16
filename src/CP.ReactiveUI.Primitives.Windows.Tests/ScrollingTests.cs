// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Forms;

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Tests Scrolling Tests behavior.</summary>
public class ScrollingTests
{
    /// <summary>The test form width.</summary>
    private const int TestFormWidth = 400;

    /// <summary>The test form height.</summary>
    private const int TestFormHeight = 300;

    /// <summary>The number of lines added to the test editor.</summary>
    private const int TestEditorLineCount = 200;

    /// <summary>Test scrolling a window.</summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Test]
    public async Task TestScrollingAsync()
    {
        using var form = new Form { Width = TestFormWidth, Height = TestFormHeight };
        var lines = new string[TestEditorLineCount];
        for (var index = 0; index < lines.Length; index++)
        {
            lines[index] = $"Line {index}";
        }

        using var editor = new RichTextBox { Dock = DockStyle.Fill, ScrollBars = RichTextBoxScrollBars.Vertical, Lines = lines };

        form.Controls.Add(editor);
        form.Show();
        Application.DoEvents();

        var scroller = InteropWindowFactory.CreateFor(editor.Handle).GetWindowScroller();
        scroller.ScrollMode = ScrollModes.WindowsMessage;
        scroller.ShowChanges = false;

        _ = scroller.GetScrollbarInfo();
        await Assert.That(scroller.ScrollBar.HasValue).IsTrue();
        await Assert.That(scroller.Start()).IsTrue();
        await Assert.That(scroller.IsAtStart).IsTrue();
        await Assert.That(scroller.Next()).IsTrue();
        _ = scroller.Reset();
    }
}
