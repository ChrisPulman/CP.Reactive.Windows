// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

/// <summary>Identifies the display element whose system color is requested.</summary>
/// <param name="Value">The native system-color index.</param>
public readonly record struct SystemColorIndex(int Value)
{
    /// <summary>Gets the scroll bar gray-area index.</summary>
    public static SystemColorIndex ScrollBar => default;

    /// <summary>Gets the desktop background-color index.</summary>
    public static SystemColorIndex Background => ScrollBar.Next();

    /// <summary>Gets the active window-caption index.</summary>
    public static SystemColorIndex ActiveCaption => Background.Next();

    /// <summary>Gets the inactive window-caption index.</summary>
    public static SystemColorIndex InactiveCaption => ActiveCaption.Next();

    /// <summary>Gets the menu background index.</summary>
    public static SystemColorIndex Menu => InactiveCaption.Next();

    /// <summary>Gets the window background index.</summary>
    public static SystemColorIndex Window => Menu.Next();

    /// <summary>Gets the window-frame index.</summary>
    public static SystemColorIndex WindowFrame => Window.Next();

    /// <summary>Gets the menu-text index.</summary>
    public static SystemColorIndex MenuText => WindowFrame.Next();

    /// <summary>Gets the window-text index.</summary>
    public static SystemColorIndex WindowText => MenuText.Next();

    /// <summary>Gets the caption-text index.</summary>
    public static SystemColorIndex CaptionText => WindowText.Next();

    /// <summary>Gets the active window-border index.</summary>
    public static SystemColorIndex ActiveBorder => CaptionText.Next();

    /// <summary>Gets the inactive window-border index.</summary>
    public static SystemColorIndex InactiveBorder => ActiveBorder.Next();

    /// <summary>Gets the multiple-document-interface application workspace-background index.</summary>
    public static SystemColorIndex AppWorkspace => InactiveBorder.Next();

    /// <summary>Gets the selected control-item background index.</summary>
    public static SystemColorIndex Highlight => AppWorkspace.Next();

    /// <summary>Gets the selected control-item text index.</summary>
    public static SystemColorIndex HighlightText => Highlight.Next();

    /// <summary>Gets the three-dimensional control-face index.</summary>
    public static SystemColorIndex ButtonFace => HighlightText.Next();

    /// <summary>Gets the three-dimensional control-shadow index.</summary>
    public static SystemColorIndex ButtonShadow => ButtonFace.Next();

    /// <summary>Gets the disabled-text index.</summary>
    public static SystemColorIndex GrayText => ButtonShadow.Next();

    /// <summary>Gets the push-button text index.</summary>
    public static SystemColorIndex ButtonText => GrayText.Next();

    /// <summary>Gets the inactive-caption text index.</summary>
    public static SystemColorIndex InactiveCaptionText => ButtonText.Next();

    /// <summary>Gets the three-dimensional control-highlight index.</summary>
    public static SystemColorIndex ButtonHighlight => InactiveCaptionText.Next();

    /// <summary>Gets the three-dimensional dark-shadow index.</summary>
    public static SystemColorIndex ThreeDDarkShadow => ButtonHighlight.Next();

    /// <summary>Gets the three-dimensional light index.</summary>
    public static SystemColorIndex ThreeDLight => ThreeDDarkShadow.Next();

    /// <summary>Gets the tool-tip text index.</summary>
    public static SystemColorIndex InfoText => ThreeDLight.Next();

    /// <summary>Gets the tool-tip background index.</summary>
    public static SystemColorIndex InfoBackground => InfoText.Next();

    /// <summary>Gets the hyperlink or hot-tracked-item index.</summary>
    public static SystemColorIndex Hotlight => InfoBackground.Next().Next();

    /// <summary>Gets the active-caption gradient right-side index.</summary>
    public static SystemColorIndex GradientActiveCaption => Hotlight.Next();

    /// <summary>Gets the inactive-caption gradient right-side index.</summary>
    public static SystemColorIndex GradientInactiveCaption => GradientActiveCaption.Next();

    /// <summary>Gets the flat-menu highlighted-item index.</summary>
    public static SystemColorIndex MenuHighlight => GradientInactiveCaption.Next();

    /// <summary>Gets the flat menu-bar background index.</summary>
    public static SystemColorIndex MenuBar => MenuHighlight.Next();

    /// <summary>Creates the next sequential system-color index.</summary>
    /// <returns>The following system-color index.</returns>
    private SystemColorIndex Next() => new(checked(Value + 1));
}
