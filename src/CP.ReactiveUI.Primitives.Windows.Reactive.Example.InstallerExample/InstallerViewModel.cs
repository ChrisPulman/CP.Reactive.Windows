// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;
using CP.ReactiveUI.Primitives.Windows.Native.Kernel;
using CP.ReactiveUI.Primitives.Windows.Native.Security;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Shell.Dialogs;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Software;
using CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Windows;
using ReactiveUI.Reactive;

namespace CPDeploymentStudio.Reactive.Example;

/// <summary>Coordinates the non-destructive Deployment Studio simulation.</summary>
public sealed class InstallerViewModel : ReactiveObject, IDisposable
{
    private readonly Subject<DeploymentEvent> _events = new();
    private readonly ObservableAsPropertyHelper<bool> _isBusy;
    private readonly ObservableAsPropertyHelper<bool> _planIsValid;
    private readonly ObservableAsPropertyHelper<string> _validationSummary;
    private readonly IDisposable _errorSubscription;
    private CancellationTokenSource _workflowCancellation;
    private string _packagePath;
    private string _installRoot;
    private string _environmentName = "Production";
    private string _releaseChannel = "Stable";
    private bool _createStartMenuShortcut = true;
    private bool _updateUserPath;
    private bool _dryRun = true;
    private bool _acceptSimulation = true;
    private double _progress;
    private string _headline = "Ready to inspect this workstation";
    private string _status = "No changes are made by this studio.";
    private string _lastError = string.Empty;
    private EnvironmentSnapshot _environment;
    private DeploymentStageViewModel _currentStage;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="InstallerViewModel"/> class.</summary>
    public InstallerViewModel()
    {
        var executablePath = GetExecutablePath();
        _packagePath = executablePath;
        _installRoot = Path.Combine(
            global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.ProgramFiles),
            "Contoso",
            "Orbit Desktop");

        Stages = new ObservableCollection<DeploymentStageViewModel>
        {
            new("Inspect", "Inventory OS, identity, installed products and active windows"),
            new("Validate", "Normalize paths and verify package, space and policy prerequisites"),
            new("Resolve", "Ask Restart Manager which processes hold the simulated payload"),
            new("Stage", "Model package extraction and environment-variable mutations"),
            new("Apply", "Simulate files, shortcuts, registration and process execution"),
            new("Verify", "Run read-only diagnostics and produce the deployment report"),
        };

        EnvironmentFacts = new ObservableCollection<FactRow>();
        PlannedOperations = new ObservableCollection<PlanOperation>();
        Diagnostics = new ObservableCollection<DiagnosticEntry>();

        var validation = this.WhenAnyValue(
                x => x.PackagePath,
                x => x.InstallRoot,
                x => x.AcceptSimulation,
                ValidatePlan)
            .Replay(1)
            .RefCount();

        _validationSummary = validation.ToProperty(this, x => x.ValidationSummary);
        _planIsValid = validation.Select(static message => string.Equals(message, "Plan is valid", StringComparison.Ordinal))
            .ToProperty(this, x => x.PlanIsValid);

        InspectEnvironment = ReactiveCommand.CreateFromTask(InspectEnvironmentAsync);
        BrowsePackage = ReactiveCommand.Create(BrowseForPackage);
        RefreshPlan = ReactiveCommand.Create(BuildPlan);
        RunWorkflow = ReactiveCommand.CreateFromTask(RunWorkflowAsync, this.WhenAnyValue(x => x.PlanIsValid));
        CancelWorkflow = ReactiveCommand.Create(CancelCurrentWorkflow, RunWorkflow.IsExecuting);
        RunReadOnlyProbe = ReactiveCommand.CreateFromTask(RunReadOnlyProbeAsync, RunWorkflow.IsExecuting.Select(static running => !running));
        RevealPackage = ReactiveCommand.Create(RevealPackageInExplorer, this.WhenAnyValue(x => x.PackagePath).Select(File.Exists));
        ClearDiagnostics = ReactiveCommand.Create(ClearDiagnosticLog);

        _isBusy = Observable.Merge(InspectEnvironment.IsExecuting, RunWorkflow.IsExecuting, RunReadOnlyProbe.IsExecuting)
            .ToProperty(this, x => x.IsBusy, false);

        _errorSubscription = Observable.Merge(
                InspectEnvironment.ThrownExceptions,
                BrowsePackage.ThrownExceptions,
                RefreshPlan.ThrownExceptions,
                RunWorkflow.ThrownExceptions,
                CancelWorkflow.ThrownExceptions,
                RunReadOnlyProbe.ThrownExceptions,
                RevealPackage.ThrownExceptions)
            .Subscribe(ReportCommandError);

        BuildPlan();
        AddDiagnostic(DiagnosticSeverity.Information, "Studio", "Deployment Studio initialized in dry-run mode.");
    }

    /// <summary>Gets or sets the package to inspect and simulate.</summary>
    public string PackagePath
    {
        get => _packagePath;
        set => this.RaiseAndSetIfChanged(ref _packagePath, value);
    }

    /// <summary>Gets or sets the proposed installation root.</summary>
    public string InstallRoot
    {
        get => _installRoot;
        set => this.RaiseAndSetIfChanged(ref _installRoot, value);
    }

    /// <summary>Gets or sets the selected deployment environment.</summary>
    public string EnvironmentName
    {
        get => _environmentName;
        set => this.RaiseAndSetIfChanged(ref _environmentName, value);
    }

    /// <summary>Gets or sets the selected release channel.</summary>
    public string ReleaseChannel
    {
        get => _releaseChannel;
        set => this.RaiseAndSetIfChanged(ref _releaseChannel, value);
    }

    /// <summary>Gets or sets a value indicating whether a Start menu shortcut is planned.</summary>
    public bool CreateStartMenuShortcut
    {
        get => _createStartMenuShortcut;
        set => this.RaiseAndSetIfChanged(ref _createStartMenuShortcut, value);
    }

    /// <summary>Gets or sets a value indicating whether a user PATH update is planned.</summary>
    public bool UpdateUserPath
    {
        get => _updateUserPath;
        set => this.RaiseAndSetIfChanged(ref _updateUserPath, value);
    }

    /// <summary>Gets or sets a value indicating whether the studio is in dry-run mode.</summary>
    public bool DryRun
    {
        get => _dryRun;
        set
        {
            if (!value)
            {
                AddDiagnostic(DiagnosticSeverity.Warning, "Safety", "Simulation lock prevented live mode. This example never writes to the machine.");
                this.RaisePropertyChanged(nameof(DryRun));
                return;
            }

            this.RaiseAndSetIfChanged(ref _dryRun, true);
        }
    }

    /// <summary>Gets or sets a value indicating acceptance of the simulation notice.</summary>
    public bool AcceptSimulation
    {
        get => _acceptSimulation;
        set => this.RaiseAndSetIfChanged(ref _acceptSimulation, value);
    }

    /// <summary>Gets the workflow completion percentage.</summary>
    public double Progress
    {
        get => _progress;
        private set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    /// <summary>Gets the prominent workflow heading.</summary>
    public string Headline
    {
        get => _headline;
        private set => this.RaiseAndSetIfChanged(ref _headline, value);
    }

    /// <summary>Gets the detailed workflow status.</summary>
    public string Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>Gets the most recent surfaced command error.</summary>
    public string LastError
    {
        get => _lastError;
        private set => this.RaiseAndSetIfChanged(ref _lastError, value);
    }

    /// <summary>Gets the latest environment snapshot.</summary>
    public EnvironmentSnapshot Environment
    {
        get => _environment;
        private set => this.RaiseAndSetIfChanged(ref _environment, value);
    }

    /// <summary>Gets the active deployment stage.</summary>
    public DeploymentStageViewModel CurrentStage
    {
        get => _currentStage;
        private set => this.RaiseAndSetIfChanged(ref _currentStage, value);
    }

    /// <summary>Gets a value indicating whether any asynchronous operation is active.</summary>
    public bool IsBusy => _isBusy.Value;

    /// <summary>Gets a value indicating whether the current plan passes validation.</summary>
    public bool PlanIsValid => _planIsValid.Value;

    /// <summary>Gets the current plan validation message.</summary>
    public string ValidationSummary => _validationSummary.Value;

    /// <summary>Gets the simulated deployment stages.</summary>
    public ObservableCollection<DeploymentStageViewModel> Stages { get; }

    /// <summary>Gets the environment facts displayed by the studio.</summary>
    public ObservableCollection<FactRow> EnvironmentFacts { get; }

    /// <summary>Gets the operations in the generated deployment plan.</summary>
    public ObservableCollection<PlanOperation> PlannedOperations { get; }

    /// <summary>Gets the structured diagnostic log.</summary>
    public ObservableCollection<DiagnosticEntry> Diagnostics { get; }

    /// <summary>Gets the typed workflow event stream.</summary>
    public IObservable<DeploymentEvent> Events => _events.AsObservable();

    /// <summary>Gets the command that refreshes the read-only workstation inventory.</summary>
    public ReactiveCommand<Unit, Unit> InspectEnvironment { get; }

    /// <summary>Gets the command that opens the local Windows file picker.</summary>
    public ReactiveCommand<Unit, Unit> BrowsePackage { get; }

    /// <summary>Gets the command that rebuilds the in-memory deployment plan.</summary>
    public ReactiveCommand<Unit, Unit> RefreshPlan { get; }

    /// <summary>Gets the command that runs the simulated deployment.</summary>
    public ReactiveCommand<Unit, Unit> RunWorkflow { get; }

    /// <summary>Gets the command that cancels the active simulation.</summary>
    public ReactiveCommand<Unit, Unit> CancelWorkflow { get; }

    /// <summary>Gets the command that runs a harmless redirected process probe.</summary>
    public ReactiveCommand<Unit, Unit> RunReadOnlyProbe { get; }

    /// <summary>Gets the command that reveals the selected package in Explorer.</summary>
    public ReactiveCommand<Unit, Unit> RevealPackage { get; }

    /// <summary>Gets the command that clears diagnostics.</summary>
    public ReactiveCommand<Unit, Unit> ClearDiagnostics { get; }

    /// <summary>Adds an environment-change notification from the Windows message stream.</summary>
    /// <param name="area">The changed settings area.</param>
    public void RecordEnvironmentChange(string area) =>
        AddDiagnostic(DiagnosticSeverity.Information, "Environment stream", string.IsNullOrWhiteSpace(area) ? "Windows settings changed." : $"Windows settings changed: {area}");

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _workflowCancellation?.Cancel();
        _workflowCancellation?.Dispose();
        _errorSubscription.Dispose();
        _isBusy.Dispose();
        _planIsValid.Dispose();
        _validationSummary.Dispose();
        _events.OnCompleted();
        _events.Dispose();
        InspectEnvironment.Dispose();
        BrowsePackage.Dispose();
        RefreshPlan.Dispose();
        RunWorkflow.Dispose();
        CancelWorkflow.Dispose();
        RunReadOnlyProbe.Dispose();
        RevealPackage.Dispose();
        ClearDiagnostics.Dispose();
    }

    private static string GetExecutablePath()
    {
        var processId = Kernel32Api.GetCurrentProcessId();
        try
        {
            return Kernel32Api.GetProcessPath(processId);
        }
        catch (Win32Exception)
        {
            using var process = Process.GetCurrentProcess();
            return process.MainModule?.FileName ?? string.Empty;
        }
    }

    private static string ValidatePlan(string packagePath, string installRoot, bool accepted)
    {
        if (!accepted)
        {
            return "Acknowledge the simulation-only safety notice.";
        }

        if (string.IsNullOrWhiteSpace(packagePath))
        {
            return "Select a package or executable to inspect.";
        }

        if (!File.Exists(packagePath))
        {
            return "The selected package does not exist.";
        }

        if (string.IsNullOrWhiteSpace(installRoot))
        {
            return "Enter a proposed installation root.";
        }

        try
        {
            var fullPath = Path.GetFullPath(installRoot);
            if (string.Equals(Path.GetPathRoot(fullPath), fullPath, StringComparison.OrdinalIgnoreCase))
            {
                return "The installation root must not be a drive root.";
            }
        }
        catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return "The installation root is not a valid Windows path.";
        }

        return "Plan is valid";
    }

    private async Task InspectEnvironmentAsync()
    {
        Headline = "Inspecting workstation";
        Status = "Reading Windows, identity, registry and desktop state…";
        Publish("Inspect", "Read-only inventory started", 0);

        var snapshot = await Task.Run(CreateEnvironmentSnapshot);
        Environment = snapshot;
        EnvironmentFacts.Clear();
        EnvironmentFacts.Add(new FactRow("Operating system", snapshot.OperatingSystem));
        EnvironmentFacts.Add(new FactRow("Process / thread", $"{snapshot.ProcessId} / {snapshot.ThreadId}"));
        EnvironmentFacts.Add(new FactRow("Architecture", snapshot.Architecture));
        EnvironmentFacts.Add(new FactRow("Identity", snapshot.UserName));
        EnvironmentFacts.Add(new FactRow("Administrator", snapshot.IsAdministrator ? "Elevated" : "Standard token"));
        EnvironmentFacts.Add(new FactRow("Logon session SID", snapshot.SessionId));
        EnvironmentFacts.Add(new FactRow("Installed products", snapshot.InstalledProductCount.ToString(CultureInfo.InvariantCulture)));
        EnvironmentFacts.Add(new FactRow("Package metadata", snapshot.PackageMetadataCount.ToString(CultureInfo.InvariantCulture)));
        EnvironmentFacts.Add(new FactRow("Top-level windows", snapshot.VisibleWindowSample.ToString(CultureInfo.InvariantCulture) + " sampled"));
        EnvironmentFacts.Add(new FactRow("Legacy browser mode", snapshot.BrowserVersion));
        EnvironmentFacts.Add(new FactRow("Available system drive", FormatBytes(snapshot.AvailableSystemDriveBytes)));

        BuildPlan();
        Headline = snapshot.IsAdministrator ? "Elevated deployment simulation" : "Standard-user deployment simulation";
        Status = "Inventory complete. Review validation and planned operations.";
        Publish("Inspect", "Inventory completed", 100);
        AddDiagnostic(DiagnosticSeverity.Success, "Inspection", "Read-only environment inspection completed.");
    }

    private static EnvironmentSnapshot CreateEnvironmentSnapshot()
    {
        var processId = Kernel32Api.GetCurrentProcessId();
        var threadId = Kernel32Api.GetCurrentThreadId();
        var systemDrive = new DriveInfo(Path.GetPathRoot(global::System.Environment.SystemDirectory));
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        var isAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);
        var installedProductCount = InstallationInformation.InstalledSoftware()
            .Count(static item => !string.IsNullOrWhiteSpace(item.DisplayName) && !item.SystemComponent);
        var packageMetadataCount = InstallationInformation.InstalledSoftware()
            .Take(250)
            .Count(static item => item.Id != Guid.Empty || !string.IsNullOrWhiteSpace(item.DisplayVersion));
        var visibleWindowSample = CountRxWindowStream();
        var browserMajor = InternetExplorerVersion.Version;
        var browserMode = browserMajor > 0
            ? $"IE {browserMajor} / emulation {InternetExplorerVersion.GetEmbVersion()}"
            : "Not detected";

        return new EnvironmentSnapshot(
            global::System.Environment.OSVersion.VersionString,
            global::System.Environment.Is64BitProcess ? "X64" : "X86",
            global::System.Environment.UserDomainName + "\\" + global::System.Environment.UserName,
            processId,
            threadId,
            isAdministrator,
            SafeGetSessionId(),
            installedProductCount,
            packageMetadataCount,
            visibleWindowSample,
            browserMode,
            systemDrive.AvailableFreeSpace);
    }

    private static int CountRxWindowStream()
    {
        const int sampleLimit = 75;
        using var completed = new ManualResetEventSlim();
        var count = 0;
        Exception streamError = null;
        using var subscription = WindowsEnumerator.ObserveWindowHandles()
            .Take(sampleLimit)
            .Subscribe(
                _ => Interlocked.Increment(ref count),
                error =>
                {
                    streamError = error;
                    completed.Set();
                },
                completed.Set);
        _ = completed.Wait(TimeSpan.FromSeconds(3));
        if (streamError is not null)
        {
            throw streamError;
        }

        return count;
    }

    private static string SafeGetSessionId()
    {
        try
        {
            return Advapi32Api.CurrentSessionId;
        }
        catch (Exception error) when (error is Win32Exception or SecurityException)
        {
            return "Unavailable";
        }
    }

    private void BrowseForPackage()
    {
        var selectedPath = FileDialog.PickFileToOpen();
        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            PackagePath = selectedPath;
            BuildPlan();
            AddDiagnostic(DiagnosticSeverity.Information, "Shell", "Package selected through CP file-dialog integration.");
        }
    }

    private void BuildPlan()
    {
        PlannedOperations.Clear();
        var root = NormalizePath(InstallRoot);
        var packageName = string.IsNullOrWhiteSpace(PackagePath) ? "<not selected>" : Path.GetFileName(PackagePath);
        PlannedOperations.Add(new PlanOperation("Filesystem", $"Inspect {packageName} and model extraction to {root}", "SIMULATED"));
        PlannedOperations.Add(new PlanOperation("Environment", UpdateUserPath ? $"Append {root} to user PATH" : "Leave user and machine PATH unchanged", UpdateUserPath ? "PLANNED" : "SKIPPED"));
        PlannedOperations.Add(new PlanOperation("Shell", CreateStartMenuShortcut ? "Model a per-user Start menu shortcut" : "Do not create shortcuts", CreateStartMenuShortcut ? "PLANNED" : "SKIPPED"));
        PlannedOperations.Add(new PlanOperation("Registration", $"Model {ReleaseChannel} channel registration for {EnvironmentName}", "SIMULATED"));
        PlannedOperations.Add(new PlanOperation("Process", "Run only the read-only `cmd /c ver` diagnostic probe", "SAFE"));
        PlannedOperations.Add(new PlanOperation("Recovery", "Inspect package locks with Restart Manager; never shut down a process", "READ ONLY"));
        Publish("Plan", "Deployment plan rebuilt", Progress);
    }

    private async Task RunWorkflowAsync()
    {
        _workflowCancellation?.Dispose();
        _workflowCancellation = new CancellationTokenSource();
        var token = _workflowCancellation.Token;
        LastError = string.Empty;
        Progress = 0;
        ResetStages();
        BuildPlan();
        Headline = "Deployment simulation running";
        AddDiagnostic(DiagnosticSeverity.Information, "Workflow", "Six-stage dry-run started. No write operations are implemented.");

        try
        {
            await ExecuteStageAsync(0, "Refreshing environment inventory", InspectEnvironmentAsync, token);
            await ExecuteStageAsync(1, "Validating package, path, capacity and elevation policy", ValidateWorkflowAsync, token);
            await ExecuteStageAsync(2, "Inspecting resource locks with Windows Restart Manager", InspectLocksAsync, token);
            await ExecuteStageAsync(3, "Modeling package staging and environment changes", SimulateStagingAsync, token);
            await ExecuteStageAsync(4, "Simulating registration, shortcuts and process launch", SimulateApplyAsync, token);
            await ExecuteStageAsync(5, "Executing read-only verification probe", VerifyAsync, token);
            Progress = 100;
            Headline = "Simulation completed safely";
            Status = "All stages completed. Zero files, registry values, variables or processes were changed.";
            Publish("Complete", Status, 100);
            AddDiagnostic(DiagnosticSeverity.Success, "Workflow", "Dry-run completed with zero machine changes.");
        }
        catch (OperationCanceledException)
        {
            if (CurrentStage is not null)
            {
                CurrentStage.State = DeploymentStageState.Cancelled;
                CurrentStage.Detail = "Cancelled by operator";
            }

            Headline = "Simulation cancelled";
            Status = "Cancellation observed between safe simulation operations.";
            Publish("Cancelled", Status, Progress);
            AddDiagnostic(DiagnosticSeverity.Warning, "Workflow", "Simulation cancelled; no rollback was required.");
        }
    }

    private async Task ExecuteStageAsync(int index, string detail, Func<Task> action, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var stage = Stages[index];
        CurrentStage = stage;
        stage.State = DeploymentStageState.Running;
        stage.Detail = detail;
        Status = detail;
        Progress = index * (100d / Stages.Count);
        Publish(stage.Name, detail, Progress);
        await action();
        token.ThrowIfCancellationRequested();
        stage.State = DeploymentStageState.Complete;
        stage.Detail = "Completed safely";
        Progress = (index + 1) * (100d / Stages.Count);
        Publish(stage.Name, "Completed", Progress);
    }

    private Task ValidateWorkflowAsync()
    {
        if (!PlanIsValid)
        {
            throw new InvalidOperationException(ValidationSummary);
        }

        var fullRoot = Path.GetFullPath(InstallRoot);
        var package = new FileInfo(PackagePath);
        AddDiagnostic(DiagnosticSeverity.Success, "Validation", $"Package: {package.Name} ({FormatBytes(package.Length)}); normalized target: {fullRoot}");
        return DelaySimulationAsync(450);
    }

    private async Task InspectLocksAsync()
    {
        var token = _workflowCancellation.Token;
        var result = await Task.Run(
            () =>
            {
                token.ThrowIfCancellationRequested();
                using var manager = RestartManager.CreateSession();
                manager.RegisterFile(Path.GetFullPath(PackagePath));
                var holders = manager.GetProcessesUsingResources();
                var rebootReason = manager.GetRebootReason();
                var message = holders.Count == 0
                    ? $"No locking processes. Reboot reason: {rebootReason}."
                    : $"{holders.Count} process(es) reference the package. Reboot reason: {rebootReason}. No shutdown requested.";
                return new DiagnosticEntry(
                    DateTimeOffset.Now,
                    holders.Count == 0 ? DiagnosticSeverity.Success : DiagnosticSeverity.Information,
                    "Restart Manager",
                    message);
            },
            token);
        AddDiagnostic(result.Severity, result.Source, result.Message);
        await DelaySimulationAsync(350);
    }

    private async Task SimulateStagingAsync()
    {
        AddDiagnostic(DiagnosticSeverity.Information, "Staging", $"Would create: {NormalizePath(InstallRoot)}");
        AddDiagnostic(DiagnosticSeverity.Information, "Staging", UpdateUserPath ? "Would update the current user's PATH." : "PATH mutation omitted by plan.");
        await DelaySimulationAsync(850);
    }

    private async Task SimulateApplyAsync()
    {
        AddDiagnostic(DiagnosticSeverity.Information, "Apply", CreateStartMenuShortcut ? "Would create a per-user Start menu shortcut." : "Shortcut creation skipped.");
        AddDiagnostic(DiagnosticSeverity.Information, "Apply", "Registration and file copy are represented as events only.");
        await DelaySimulationAsync(900);
    }

    private async Task VerifyAsync()
    {
        await RunReadOnlyProbeAsync();
        await DelaySimulationAsync(300);
    }

    private async Task RunReadOnlyProbeAsync()
    {
        const string fileName = "cmd.exe";
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = "/d /c ver",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("The read-only process probe did not start.");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await Task.Run(() => process.WaitForExit());
        var output = (await outputTask).Trim();
        var error = (await errorTask).Trim();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Read-only process probe exited with {process.ExitCode}: {error}");
        }

        AddDiagnostic(DiagnosticSeverity.Success, "Process", string.IsNullOrWhiteSpace(output) ? "`cmd /c ver` completed." : output);
    }

    private Task DelaySimulationAsync(int milliseconds) => Task.Delay(milliseconds, _workflowCancellation?.Token ?? CancellationToken.None);

    private void CancelCurrentWorkflow()
    {
        _workflowCancellation?.Cancel();
        Status = "Cancellation requested…";
    }

    private void RevealPackageInExplorer()
    {
        var fullPath = Path.GetFullPath(PackagePath);
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{fullPath}\"",
            UseShellExecute = true,
        });
        AddDiagnostic(DiagnosticSeverity.Information, "Shell", "Selected package revealed in Windows Explorer.");
    }

    private void ClearDiagnosticLog()
    {
        Diagnostics.Clear();
        LastError = string.Empty;
        AddDiagnostic(DiagnosticSeverity.Information, "Studio", "Diagnostic log cleared.");
    }

    private void ResetStages()
    {
        foreach (var stage in Stages)
        {
            stage.State = DeploymentStageState.Pending;
            stage.Detail = stage.Description;
        }

        CurrentStage = null;
    }

    private void ReportCommandError(Exception error)
    {
        LastError = error.Message;
        Headline = "An operation needs attention";
        Status = error.Message;
        AddDiagnostic(DiagnosticSeverity.Error, "Command", error.GetType().Name + ": " + error.Message);
    }

    private void AddDiagnostic(DiagnosticSeverity severity, string source, string message)
    {
        Diagnostics.Insert(0, new DiagnosticEntry(DateTimeOffset.Now, severity, source, message));
        while (Diagnostics.Count > 200)
        {
            Diagnostics.RemoveAt(Diagnostics.Count - 1);
        }
    }

    private void Publish(string stage, string message, double progress)
    {
        _events.OnNext(new DeploymentEvent(DateTimeOffset.Now, stage, message, progress));
    }

    private static string NormalizePath(string path)
    {
        try
        {
            return string.IsNullOrWhiteSpace(path) ? "<not configured>" : Path.GetFullPath(path);
        }
        catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return "<invalid path>";
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var suffixIndex = 0;
        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return value.ToString("0.##", CultureInfo.InvariantCulture) + " " + suffixes[suffixIndex];
    }
}

/// <summary>Represents a simulated deployment stage.</summary>
public sealed class DeploymentStageViewModel : ReactiveObject
{
    private DeploymentStageState _state;
    private string _detail;

    /// <summary>Initializes a new instance of the <see cref="DeploymentStageViewModel"/> class.</summary>
    /// <param name="name">Stage name.</param>
    /// <param name="description">Stage description.</param>
    public DeploymentStageViewModel(string name, string description)
    {
        Name = name;
        Description = description;
        _detail = description;
    }

    /// <summary>Gets the short stage name.</summary>
    public string Name { get; }

    /// <summary>Gets the original stage description.</summary>
    public string Description { get; }

    /// <summary>Gets or sets the stage state.</summary>
    public DeploymentStageState State
    {
        get => _state;
        set => this.RaiseAndSetIfChanged(ref _state, value);
    }

    /// <summary>Gets or sets the current stage detail.</summary>
    public string Detail
    {
        get => _detail;
        set => this.RaiseAndSetIfChanged(ref _detail, value);
    }
}

/// <summary>Describes the state of a simulated deployment stage.</summary>
public enum DeploymentStageState
{
    /// <summary>The stage has not started.</summary>
    Pending,

    /// <summary>The stage is active.</summary>
    Running,

    /// <summary>The stage completed.</summary>
    Complete,

    /// <summary>The stage was cancelled.</summary>
    Cancelled,
}

/// <summary>Stores a key/value environment fact.</summary>
public sealed class FactRow
{
    /// <summary>Initializes a new instance of the <see cref="FactRow"/> class.</summary>
    public FactRow(string name, string value) => (Name, Value) = (name, value);

    /// <summary>Gets the fact name.</summary>
    public string Name { get; }

    /// <summary>Gets the fact value.</summary>
    public string Value { get; }
}

/// <summary>Describes a proposed, never-applied deployment operation.</summary>
public sealed class PlanOperation
{
    /// <summary>Initializes a new instance of the <see cref="PlanOperation"/> class.</summary>
    public PlanOperation(string category, string description, string disposition) =>
        (Category, Description, Disposition) = (category, description, disposition);

    /// <summary>Gets the operation category.</summary>
    public string Category { get; }

    /// <summary>Gets the proposed operation.</summary>
    public string Description { get; }

    /// <summary>Gets its safety disposition.</summary>
    public string Disposition { get; }
}

/// <summary>Represents one item in the structured diagnostic log.</summary>
public sealed class DiagnosticEntry
{
    /// <summary>Initializes a new instance of the <see cref="DiagnosticEntry"/> class.</summary>
    public DiagnosticEntry(DateTimeOffset timestamp, DiagnosticSeverity severity, string source, string message) =>
        (Timestamp, Severity, Source, Message) = (timestamp, severity, source, message);

    /// <summary>Gets the timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Gets the diagnostic severity.</summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>Gets the source component.</summary>
    public string Source { get; }

    /// <summary>Gets the message.</summary>
    public string Message { get; }
}

/// <summary>Classifies a diagnostic entry.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Informational event.</summary>
    Information,

    /// <summary>Successful result.</summary>
    Success,

    /// <summary>Recoverable warning.</summary>
    Warning,

    /// <summary>Surfaced command failure.</summary>
    Error,
}

/// <summary>Represents a typed item on the deployment event stream.</summary>
public sealed class DeploymentEvent
{
    /// <summary>Initializes a new instance of the <see cref="DeploymentEvent"/> class.</summary>
    public DeploymentEvent(DateTimeOffset timestamp, string stage, string message, double progress) =>
        (Timestamp, Stage, Message, Progress) = (timestamp, stage, message, progress);

    /// <summary>Gets the timestamp.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Gets the stage.</summary>
    public string Stage { get; }

    /// <summary>Gets the event message.</summary>
    public string Message { get; }

    /// <summary>Gets the completion percentage.</summary>
    public double Progress { get; }
}

/// <summary>Contains a point-in-time, read-only Windows inventory.</summary>
public sealed class EnvironmentSnapshot
{
    /// <summary>Initializes a new instance of the <see cref="EnvironmentSnapshot"/> class.</summary>
    public EnvironmentSnapshot(
        string operatingSystem,
        string architecture,
        string userName,
        int processId,
        int threadId,
        bool isAdministrator,
        string sessionId,
        int installedProductCount,
        int packageMetadataCount,
        int visibleWindowSample,
        string browserVersion,
        long availableSystemDriveBytes) =>
        (OperatingSystem, Architecture, UserName, ProcessId, ThreadId, IsAdministrator, SessionId, InstalledProductCount, PackageMetadataCount, VisibleWindowSample, BrowserVersion, AvailableSystemDriveBytes) =
        (operatingSystem, architecture, userName, processId, threadId, isAdministrator, sessionId, installedProductCount, packageMetadataCount, visibleWindowSample, browserVersion, availableSystemDriveBytes);

    /// <summary>Gets the operating system description.</summary>
    public string OperatingSystem { get; }

    /// <summary>Gets the process architecture.</summary>
    public string Architecture { get; }

    /// <summary>Gets the current identity.</summary>
    public string UserName { get; }

    /// <summary>Gets the native process identifier.</summary>
    public int ProcessId { get; }

    /// <summary>Gets the native thread identifier captured by the inspection worker.</summary>
    public int ThreadId { get; }

    /// <summary>Gets a value indicating whether the token is an administrator token.</summary>
    public bool IsAdministrator { get; }

    /// <summary>Gets the logon session SID.</summary>
    public string SessionId { get; }

    /// <summary>Gets the product count from the standard Windows package.</summary>
    public int InstalledProductCount { get; }

    /// <summary>Gets the number of sampled products with version or product-code metadata.</summary>
    public int PackageMetadataCount { get; }

    /// <summary>Gets the bounded native window sample count.</summary>
    public int VisibleWindowSample { get; }

    /// <summary>Gets the read-only legacy browser compatibility information.</summary>
    public string BrowserVersion { get; }

    /// <summary>Gets the available system-drive bytes.</summary>
    public long AvailableSystemDriveBytes { get; }
}
