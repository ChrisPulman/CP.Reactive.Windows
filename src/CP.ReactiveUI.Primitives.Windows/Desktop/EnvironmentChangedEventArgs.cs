// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using CP.ReactiveUI.Primitives.Windows.Native.UserInterface.Enums;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Windows;
#endif
/// <summary>Event arguments for the WM_SETTINGCHANGE message.</summary>
public class EnvironmentChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Gets the area containing the system parameter that changed.
    ///     When the system sends this message as a result of a SystemParametersInfo call, longParameter is a pointer to a string that
    ///     indicates the area containing the system parameter that was changed. This parameter does not usually indicate which
    ///     specific system parameter changed. (Note that some applications send this message with longParameter set to NULL.) In
    ///     general, when you receive this message, you should check and reload any system parameter settings that are used by
    ///     your application.
    ///     This string can be the name of a registry key or the name of a section in the Win.ini file. When the string is a
    ///     registry name, it typically indicates only the leaf node in the registry, not the full path.
    ///     When the system sends this message as a result of a change in policy settings, this parameter points to the string
    ///     "Policy".
    ///     When the system sends this message as a result of a change in locale settings, this parameter points to the string
    ///     "intl".
    ///     To effect a change in the environment variables for the system or the user, broadcast this message with longParameter set
    ///     to the string "Environment".
    /// </summary>
    public string Area { get; private set; }

    /// <summary>
    ///     Gets the SystemParametersInfo action that triggered the change.
    ///     When the system sends this message as a result of a SystemParametersInfo call, the wordParameter parameter is the value of
    ///     the action parameter passed to the SystemParametersInfo function. For a list of values, see SystemParametersInfo.
    ///     When the system sends this message as a result of a change in policy settings, this parameter indicates the type of
    ///     policy that was applied. This value is 1 if computer policy was applied or zero if user policy was applied.
    ///     When the system sends this message as a result of a change in locale settings, this parameter is zero.
    ///     When an application sends this message, this parameter must be NULL.
    /// </summary>
    public SystemParametersInfoActions SystemParametersInfoAction { get; private set; }

    /// <summary>Factory for the EnvironmentChangedEventArgs.</summary>
    /// <returns>The created event arguments.</returns>
    public static EnvironmentChangedEventArgs Create() => Create(SystemParametersInfoActions.SPI_NONE, null);

    /// <summary>Factory for the EnvironmentChangedEventArgs.</summary>
    /// <param name="systemParametersInfoAction">SystemParametersInfo action that triggered the change.</param>
    /// <returns>The created event arguments.</returns>
    public static EnvironmentChangedEventArgs Create(SystemParametersInfoActions systemParametersInfoAction) => Create(systemParametersInfoAction, null);

    /// <summary>Factory for the EnvironmentChangedEventArgs.</summary>
    /// <param name="systemParametersInfoAction">SystemParametersInfo action that triggered the change.</param>
    /// <param name="area">Area containing the changed system parameter.</param>
    /// <returns>The created event arguments.</returns>
    public static EnvironmentChangedEventArgs Create(SystemParametersInfoActions systemParametersInfoAction, string area) => new EnvironmentChangedEventArgs
        {
            SystemParametersInfoAction = systemParametersInfoAction,
            Area = area
        };
}
