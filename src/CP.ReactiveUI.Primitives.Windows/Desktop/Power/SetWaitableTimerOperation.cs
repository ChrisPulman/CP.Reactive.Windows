// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Power;
#endif

/// <summary>Defines a safe waitable-timer operation.</summary>
/// <param name="timerHandle">The timer safe handle.</param>
/// <param name="dueTime">The timer due time.</param>
/// <param name="period">The timer period.</param>
/// <param name="completionRoutine">The completion routine.</param>
/// <param name="completionRoutineArgument">The completion routine argument.</param>
/// <param name="resume">Whether the timer resumes the system.</param>
/// <returns><see langword="true"/> when the timer is set.</returns>
internal delegate bool SetWaitableTimerOperation(
    SafeWaitHandle timerHandle,
    ref long dueTime,
    int period,
    IntPtr completionRoutine,
    IntPtr completionRoutineArgument,
    bool resume);
