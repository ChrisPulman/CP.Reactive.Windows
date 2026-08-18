// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample;

internal sealed class OperationsCenterForm : Form, IViewFor<OperationsCenterViewModel>
{
    private static readonly Color Canvas = Color.FromArgb(10, 17, 30);
    private static readonly Color Surface = Color.FromArgb(18, 29, 49);
    private static readonly Color SurfaceRaised = Color.FromArgb(26, 40, 65);
    private static readonly Color Accent = Color.FromArgb(44, 201, 186);
    private static readonly Color AccentBlue = Color.FromArgb(75, 139, 245);
    private static readonly Color PrimaryText = Color.FromArgb(235, 241, 250);
    private static readonly Color SecondaryText = Color.FromArgb(151, 166, 188);
    private static readonly Color Border = Color.FromArgb(44, 59, 82);

    private readonly Label _statusLabel = CreateLabel("Ready", 10F, FontStyle.Bold, Accent);
    private readonly Label _lastRefreshLabel = CreateLabel("Not refreshed", 9F, FontStyle.Regular, SecondaryText);
    private readonly Label _windowCountLabel = CreateMetricValue();
    private readonly Label _processCountLabel = CreateMetricValue();
    private readonly Label _displayCountLabel = CreateMetricValue();
    private readonly Label _eventCountLabel = CreateMetricValue();
    private readonly Label _inputIdleLabel = CreateLabel("Unknown", 22F, FontStyle.Bold, AccentBlue);
    private readonly Label _clipboardSummaryLabel = CreateLabel(string.Empty, 10F, FontStyle.Regular, PrimaryText);
    private readonly Label _windowDetailsLabel = CreateLabel("Select a window to inspect its native state.", 10F, FontStyle.Regular, SecondaryText);
    private readonly Button _refreshAllButton = CreateButton("REFRESH ALL", AccentBlue);
    private readonly Button _refreshWindowsButton = CreateButton("Refresh windows", AccentBlue);
    private readonly Button _refreshProcessesButton = CreateButton("Refresh processes", AccentBlue);
    private readonly Button _refreshDisplaysButton = CreateButton("Refresh displays", AccentBlue);
    private readonly Button _refreshClipboardButton = CreateButton("Sample clipboard", AccentBlue);
    private readonly Button _bringToFrontButton = CreateButton("Bring to front", Accent);
    private readonly Button _restoreWindowButton = CreateButton("Restore", SurfaceRaised);
    private readonly Button _writeClipboardButton = CreateButton("Write text to clipboard", Accent);
    private readonly Button _copyReportButton = CreateButton("Copy diagnostic report", AccentBlue);
    private readonly Button _clearEventsButton = CreateButton("Clear stream", SurfaceRaised);
    private readonly Button _playSoundButton = CreateButton("Play safe notification", SurfaceRaised);
    private readonly CheckBox _inputCaptureCheckBox = CreateCheckBox("Enable global keyboard/mouse diagnostics (opt-in)");
    private readonly CheckBox _pauseEventsCheckBox = CreateCheckBox("Pause event capture");
    private readonly CheckBox _keepAwakeCheckBox = CreateCheckBox("Temporarily prevent system sleep while this view is active");
    private readonly TextBox _clipboardDraftTextBox = CreateTextBox(multiline: true);
    private readonly TextBox _eventSearchTextBox = CreateTextBox(multiline: false);
    private readonly ComboBox _eventCategoryComboBox = new ComboBox();
    private readonly DataGridView _windowsGrid = CreateGrid();
    private readonly DataGridView _processesGrid = CreateGrid();
    private readonly DataGridView _displaysGrid = CreateGrid();
    private readonly DataGridView _capabilitiesGrid = CreateGrid();
    private readonly DataGridView _eventsGrid = CreateGrid();

    public OperationsCenterForm(OperationsCenterViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Text = "ReactiveUI Windows Operations Center";
        BackColor = Canvas;
        ForeColor = PrimaryText;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        MinimumSize = new Size(1_080, 720);
        Size = new Size(1_420, 900);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;

        ConfigureGrids();
        Controls.Add(CreateShell());
        ConfigureBindings();
    }

    public OperationsCenterViewModel ViewModel { get; set; }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = value as OperationsCenterViewModel;
    }

    private static Button CreateButton(string text, Color backColor)
    {
        return new Button
        {
            AutoSize = true,
            BackColor = backColor,
            FlatStyle = FlatStyle.Flat,
            ForeColor = PrimaryText,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(6),
            Padding = new Padding(12, 6, 12, 6),
            Text = text,
            UseVisualStyleBackColor = false,
        };
    }

    private static CheckBox CreateCheckBox(string text)
    {
        return new CheckBox
        {
            AutoSize = true,
            ForeColor = PrimaryText,
            Text = text,
            Margin = new Padding(8),
        };
    }

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Surface,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ColumnHeadersHeight = 38,
            Dock = DockStyle.Fill,
            EnableHeadersVisualStyles = false,
            GridColor = Border,
            MultiSelect = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            RowTemplate = { Height = 34 },
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        };
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SurfaceRaised,
            ForeColor = SecondaryText,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
            SelectionBackColor = SurfaceRaised,
            SelectionForeColor = PrimaryText,
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = PrimaryText,
            SelectionBackColor = Color.FromArgb(34, 74, 105),
            SelectionForeColor = Color.White,
            Padding = new Padding(4),
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(21, 34, 56), ForeColor = PrimaryText };
        return grid;
    }

    private static Label CreateLabel(string text, float size, FontStyle style, Color color)
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = color,
            Font = new Font("Segoe UI", size, style, GraphicsUnit.Point),
            Text = text,
        };
    }

    private static Label CreateMetricValue() => CreateLabel("0", 25F, FontStyle.Bold, PrimaryText);

    private static TextBox CreateTextBox(bool multiline)
    {
        return new TextBox
        {
            AcceptsReturn = multiline,
            BackColor = Color.FromArgb(12, 22, 38),
            BorderStyle = BorderStyle.FixedSingle,
            ForeColor = PrimaryText,
            Multiline = multiline,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
        };
    }

    private static Panel CreateMetricCard(string title, Label value, string caption, Color indicator)
    {
        var card = new Panel
        {
            BackColor = Surface,
            Margin = new Padding(8),
            Padding = new Padding(18, 15, 18, 15),
            Size = new Size(245, 128),
        };
        var bar = new Panel { BackColor = indicator, Dock = DockStyle.Left, Width = 4 };
        var titleLabel = CreateLabel(title.ToUpperInvariant(), 8F, FontStyle.Bold, SecondaryText);
        titleLabel.Location = new Point(21, 15);
        value.Location = new Point(18, 40);
        var captionLabel = CreateLabel(caption, 8.5F, FontStyle.Regular, SecondaryText);
        captionLabel.Location = new Point(21, 94);
        card.Controls.Add(captionLabel);
        card.Controls.Add(value);
        card.Controls.Add(titleLabel);
        card.Controls.Add(bar);
        return card;
    }

    private static Panel CreateSection(string title, Control content)
    {
        var panel = new Panel { BackColor = Surface, Dock = DockStyle.Fill, Padding = new Padding(14) };
        var heading = CreateLabel(title, 11F, FontStyle.Bold, PrimaryText);
        heading.Dock = DockStyle.Top;
        heading.Height = 31;
        content.Dock = DockStyle.Fill;
        panel.Controls.Add(content);
        panel.Controls.Add(heading);
        return panel;
    }

    private static TabPage CreateTab(string text)
    {
        return new TabPage
        {
            BackColor = Canvas,
            ForeColor = PrimaryText,
            Padding = new Padding(12),
            Text = text,
            UseVisualStyleBackColor = false,
        };
    }

    private static void AddColumn(DataGridView grid, string property, string header, float fillWeight, int minimumWidth = 60)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            FillWeight = fillWeight,
            HeaderText = header,
            MinimumWidth = minimumWidth,
            Name = property,
            ReadOnly = true,
        });
    }

    private void ConfigureBindings()
    {
        _windowsGrid.DataSource = ViewModel.Windows;
        _processesGrid.DataSource = ViewModel.Processes;
        _displaysGrid.DataSource = ViewModel.Displays;
        _capabilitiesGrid.DataSource = ViewModel.Capabilities;
        _eventsGrid.DataSource = ViewModel.FilteredEvents;
        _eventCategoryComboBox.DataSource = ViewModel.EventCategories;

        _ = this.WhenActivated((Action<Action<IDisposable>>)(dispose =>
        {
            dispose(ViewModel.Activate(this));

            dispose(this.Bind(ViewModel, viewModel => viewModel.EventSearch, view => view._eventSearchTextBox.Text));
            dispose(this.Bind(ViewModel, viewModel => viewModel.EventCategory, view => view._eventCategoryComboBox.Text));
            dispose(this.Bind(ViewModel, viewModel => viewModel.ClipboardDraft, view => view._clipboardDraftTextBox.Text));
            dispose(this.Bind(ViewModel, viewModel => viewModel.InputCaptureEnabled, view => view._inputCaptureCheckBox.Checked));
            dispose(this.Bind(ViewModel, viewModel => viewModel.EventCapturePaused, view => view._pauseEventsCheckBox.Checked));
            dispose(this.Bind(ViewModel, viewModel => viewModel.KeepAwake, view => view._keepAwakeCheckBox.Checked));

            dispose(this.BindCommand(ViewModel, viewModel => viewModel.RefreshAllCommand, view => view._refreshAllButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.RefreshWindowsCommand, view => view._refreshWindowsButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.RefreshProcessesCommand, view => view._refreshProcessesButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.RefreshDisplaysCommand, view => view._refreshDisplaysButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.RefreshClipboardCommand, view => view._refreshClipboardButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.BringToFrontCommand, view => view._bringToFrontButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.RestoreWindowCommand, view => view._restoreWindowButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.WriteClipboardCommand, view => view._writeClipboardButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.CopyReportCommand, view => view._copyReportButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.ClearEventsCommand, view => view._clearEventsButton));
            dispose(this.BindCommand(ViewModel, viewModel => viewModel.PlayNotificationCommand, view => view._playSoundButton));

            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.Status).Subscribe(value => _statusLabel.Text = value));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.LastRefresh).Subscribe(value => _lastRefreshLabel.Text = value));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.WindowCount).Subscribe(value => _windowCountLabel.Text = value.ToString("N0")));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.ProcessCount).Subscribe(value => _processCountLabel.Text = value.ToString("N0")));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.DisplayCount).Subscribe(value => _displayCountLabel.Text = value.ToString("N0")));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.EventCount).Subscribe(value => _eventCountLabel.Text = value.ToString("N0")));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.InputIdle).Subscribe(value => _inputIdleLabel.Text = value));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.ClipboardSummary).Subscribe(value => _clipboardSummaryLabel.Text = value));
            dispose(ViewModel.WhenAnyValue(viewModel => viewModel.SelectedWindow).Subscribe(UpdateWindowDetails));

            dispose(Signal.FromEventPattern(
                    handler => _windowsGrid.SelectionChanged += handler,
                    handler => _windowsGrid.SelectionChanged -= handler)
                .Subscribe(_ => ViewModel.SelectedWindow = _windowsGrid.CurrentRow?.DataBoundItem as WindowSnapshot));
        }));
    }

    private void ConfigureGrids()
    {
        AddColumn(_windowsGrid, nameof(WindowSnapshot.HandleHex), "HANDLE", 22F, 100);
        AddColumn(_windowsGrid, nameof(WindowSnapshot.ProcessName), "PROCESS", 25F, 110);
        AddColumn(_windowsGrid, nameof(WindowSnapshot.Caption), "CAPTION", 70F, 180);
        AddColumn(_windowsGrid, nameof(WindowSnapshot.ClassName), "CLASS", 36F, 120);
        AddColumn(_windowsGrid, nameof(WindowSnapshot.Bounds), "BOUNDS", 32F, 140);
        AddColumn(_windowsGrid, nameof(WindowSnapshot.Visible), "VISIBLE", 16F, 70);

        AddColumn(_processesGrid, nameof(ProcessSnapshot.Id), "PID", 15F, 70);
        AddColumn(_processesGrid, nameof(ProcessSnapshot.Name), "PROCESS", 35F, 120);
        AddColumn(_processesGrid, nameof(ProcessSnapshot.WorkingSetText), "WORKING SET", 22F, 100);
        AddColumn(_processesGrid, nameof(ProcessSnapshot.ThreadCount), "THREADS", 16F, 75);
        AddColumn(_processesGrid, nameof(ProcessSnapshot.MainWindowTitle), "MAIN WINDOW", 65F, 180);

        AddColumn(_displaysGrid, nameof(DisplaySnapshot.Name), "DEVICE", 30F, 130);
        AddColumn(_displaysGrid, nameof(DisplaySnapshot.Resolution), "RESOLUTION", 24F, 110);
        AddColumn(_displaysGrid, nameof(DisplaySnapshot.Bounds), "VIRTUAL BOUNDS", 32F, 150);
        AddColumn(_displaysGrid, nameof(DisplaySnapshot.WorkingArea), "WORKING AREA", 32F, 150);
        AddColumn(_displaysGrid, nameof(DisplaySnapshot.Primary), "PRIMARY", 16F, 75);

        AddColumn(_capabilitiesGrid, nameof(CapabilitySnapshot.Capability), "CAPABILITY", 28F, 160);
        AddColumn(_capabilitiesGrid, nameof(CapabilitySnapshot.State), "STATE", 18F, 95);
        AddColumn(_capabilitiesGrid, nameof(CapabilitySnapshot.Detail), "OBSERVATION", 65F, 250);

        AddColumn(_eventsGrid, nameof(OperationsEvent.Time), "TIME", 17F, 100);
        AddColumn(_eventsGrid, nameof(OperationsEvent.Category), "CATEGORY", 18F, 100);
        AddColumn(_eventsGrid, nameof(OperationsEvent.Severity), "LEVEL", 16F, 90);
        AddColumn(_eventsGrid, nameof(OperationsEvent.Message), "EVENT", 80F, 300);

        _eventsGrid.CellFormatting += (_, eventArgs) =>
        {
            if (eventArgs.RowIndex < 0 || _eventsGrid.Rows[eventArgs.RowIndex].DataBoundItem is not OperationsEvent item)
            {
                return;
            }

            eventArgs.CellStyle.ForeColor = item.Severity == "Unavailable" || item.Severity == "Warning"
                ? Color.FromArgb(255, 191, 105)
                : item.Severity == "Success" ? Accent : PrimaryText;
        };
    }

    private Control CreateAboutTabContent()
    {
        var content = new TableLayoutPanel
        {
            BackColor = Surface,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            RowCount = 2,
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var title = CreateLabel("Real Windows APIs, composed reactively", 20F, FontStyle.Bold, PrimaryText);
        var subtitle = CreateLabel("All observations stay on this machine. Unsupported capabilities report errors instead of hiding them.", 10F, FontStyle.Regular, SecondaryText);
        subtitle.MaximumSize = new Size(620, 0);
        var heading = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        heading.Controls.Add(title);
        heading.Controls.Add(subtitle);
        content.Controls.Add(heading, 0, 0);
        content.SetColumnSpan(heading, 2);

        var reactiveApis = CreateLabel(
            "REACTIVE PIPELINES" + Environment.NewLine + Environment.NewLine
            + "• DisplayTopology.ObserveChanges" + Environment.NewLine
            + "• ClipboardNative.ClipboardUpdateEvents" + Environment.NewLine
            + "• EnvironmentMonitor.EnvironmentChangeEvents" + Environment.NewLine
            + "• WinEventHook lifecycle/title streams" + Environment.NewLine
            + "• PowerBroadcastListener" + Environment.NewLine
            + "• WindowsSessionListener" + Environment.NewLine
            + "• KeyboardHook / MouseHook (explicit opt-in)" + Environment.NewLine
            + "• SharedMessageWindow handle lifecycle",
            10F,
            FontStyle.Regular,
            PrimaryText);
        reactiveApis.Dock = DockStyle.Fill;

        var nativeApis = CreateLabel(
            "SNAPSHOTS & SAFE ACTIONS" + Environment.NewLine + Environment.NewLine
            + "• InteropWindow enumeration, fill and queries" + Environment.NewLine
            + "• DisplayInfo / NativeRect core values" + Environment.NewLine
            + "• NativeInput last-input diagnostics" + Environment.NewLine
            + "• DwmApi composition and accent status" + Environment.NewLine
            + "• Clipboard access-token reads/writes" + Environment.NewLine
            + "• SystemStateApi temporary sleep policy" + Environment.NewLine
            + "• WinMm system notification" + Environment.NewLine
            + "• InternetExplorerVersion integration probe",
            10F,
            FontStyle.Regular,
            PrimaryText);
        nativeApis.Dock = DockStyle.Fill;
        content.Controls.Add(reactiveApis, 0, 1);
        content.Controls.Add(nativeApis, 1, 1);
        return content;
    }

    private Control CreateClipboardTabContent()
    {
        var split = new SplitContainer
        {
            BackColor = Canvas,
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 650,
            SplitterWidth = 10,
        };

        _clipboardSummaryLabel.AutoSize = false;
        _clipboardSummaryLabel.Dock = DockStyle.Fill;
        _clipboardSummaryLabel.Padding = new Padding(6);
        var left = new TableLayoutPanel { BackColor = Surface, ColumnCount = 1, Dock = DockStyle.Fill, Padding = new Padding(18), RowCount = 5 };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        left.Controls.Add(CreateLabel("Clipboard diagnostics", 14F, FontStyle.Bold, PrimaryText), 0, 0);
        left.Controls.Add(_clipboardSummaryLabel, 0, 1);
        left.Controls.Add(CreateLabel("Explicit write payload", 9F, FontStyle.Bold, SecondaryText), 0, 2);
        _clipboardDraftTextBox.Dock = DockStyle.Fill;
        left.Controls.Add(_clipboardDraftTextBox, 0, 3);
        var clipboardActions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        clipboardActions.Controls.Add(_refreshClipboardButton);
        clipboardActions.Controls.Add(_writeClipboardButton);
        clipboardActions.Controls.Add(_copyReportButton);
        left.Controls.Add(clipboardActions, 0, 4);

        var right = new TableLayoutPanel { BackColor = Surface, ColumnCount = 1, Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 7 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        right.Controls.Add(CreateLabel("Input & power guardrails", 14F, FontStyle.Bold, PrimaryText), 0, 0);
        right.Controls.Add(CreateLabel("Last user input idle", 9F, FontStyle.Bold, SecondaryText), 0, 1);
        right.Controls.Add(_inputIdleLabel, 0, 2);
        right.Controls.Add(_inputCaptureCheckBox, 0, 3);
        right.Controls.Add(_keepAwakeCheckBox, 0, 4);
        right.Controls.Add(_playSoundButton, 0, 5);
        var privacy = CreateLabel("Input hooks are off by default, never mark events handled, and are disposed with view activation. Keep-awake is temporary and is reset on deactivation.", 9F, FontStyle.Regular, SecondaryText);
        privacy.MaximumSize = new Size(510, 0);
        right.Controls.Add(privacy, 0, 6);

        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);
        return split;
    }

    private Control CreateDisplayTabContent()
    {
        var panel = new TableLayoutPanel { BackColor = Canvas, ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 2 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var actions = new FlowLayoutPanel { BackColor = Surface, Dock = DockStyle.Fill, Padding = new Padding(8) };
        actions.Controls.Add(_refreshDisplaysButton);
        actions.Controls.Add(CreateLabel("Reactive WM_DISPLAYCHANGE stream + fresh User32 display snapshots", 9F, FontStyle.Regular, SecondaryText));
        panel.Controls.Add(actions, 0, 0);
        panel.Controls.Add(_displaysGrid, 0, 1);
        return panel;
    }

    private Control CreateEventsTabContent()
    {
        var panel = new TableLayoutPanel { BackColor = Canvas, ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 2 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var filters = new FlowLayoutPanel { BackColor = Surface, Dock = DockStyle.Fill, Padding = new Padding(8), WrapContents = false };
        filters.Controls.Add(CreateLabel("Category", 9F, FontStyle.Bold, SecondaryText));
        _eventCategoryComboBox.BackColor = Color.FromArgb(12, 22, 38);
        _eventCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _eventCategoryComboBox.ForeColor = PrimaryText;
        _eventCategoryComboBox.Width = 150;
        filters.Controls.Add(_eventCategoryComboBox);
        filters.Controls.Add(CreateLabel("Filter", 9F, FontStyle.Bold, SecondaryText));
        _eventSearchTextBox.Width = 280;
        filters.Controls.Add(_eventSearchTextBox);
        filters.Controls.Add(_pauseEventsCheckBox);
        filters.Controls.Add(_clearEventsButton);
        panel.Controls.Add(filters, 0, 0);
        panel.Controls.Add(_eventsGrid, 0, 1);
        return panel;
    }

    private Control CreateHeader()
    {
        var header = new TableLayoutPanel
        {
            BackColor = Surface,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Height = 82,
            Padding = new Padding(22, 12, 18, 10),
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));

        var brand = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        brand.Controls.Add(CreateLabel("WINDOWS OPERATIONS CENTER", 17F, FontStyle.Bold, PrimaryText));
        brand.Controls.Add(CreateLabel("ReactiveUI 24 • local desktop observability • safe operator controls", 9F, FontStyle.Regular, SecondaryText));
        header.Controls.Add(brand, 0, 0);

        var state = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        state.Controls.Add(_statusLabel);
        state.Controls.Add(_lastRefreshLabel);
        header.Controls.Add(state, 1, 0);
        _refreshAllButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        header.Controls.Add(_refreshAllButton, 2, 0);
        return header;
    }

    private Control CreateOverviewTabContent()
    {
        var layout = new TableLayoutPanel { BackColor = Canvas, ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 155F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));

        var metrics = new FlowLayoutPanel { BackColor = Canvas, Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 4), WrapContents = false };
        metrics.Controls.Add(CreateMetricCard("Windows", _windowCountLabel, "InteropWindow snapshot", AccentBlue));
        metrics.Controls.Add(CreateMetricCard("Processes", _processCountLabel, "Local process inventory", Color.FromArgb(160, 113, 247)));
        metrics.Controls.Add(CreateMetricCard("Displays", _displayCountLabel, "Live display topology", Accent));
        metrics.Controls.Add(CreateMetricCard("Events", _eventCountLabel, "Bounded reactive stream", Color.FromArgb(245, 166, 35)));
        layout.Controls.Add(metrics, 0, 0);
        layout.Controls.Add(CreateSection("Capability matrix", _capabilitiesGrid), 0, 1);

        var safety = new Panel { BackColor = SurfaceRaised, Dock = DockStyle.Fill, Padding = new Padding(18, 13, 18, 10) };
        var safetyTitle = CreateLabel("SAFE BY DESIGN", 9F, FontStyle.Bold, Accent);
        safetyTitle.Dock = DockStyle.Top;
        var safetyText = CreateLabel("No shutdown, sleep, registry writes, process termination, input injection, or arbitrary window movement. Interactive actions are explicit and report native failures.", 9F, FontStyle.Regular, SecondaryText);
        safetyText.Dock = DockStyle.Fill;
        safety.Controls.Add(safetyText);
        safety.Controls.Add(safetyTitle);
        layout.Controls.Add(safety, 0, 2);
        return layout;
    }

    private Control CreateProcessesTabContent()
    {
        var panel = new TableLayoutPanel { BackColor = Canvas, ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 2 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var actions = new FlowLayoutPanel { BackColor = Surface, Dock = DockStyle.Fill, Padding = new Padding(8) };
        actions.Controls.Add(_refreshProcessesButton);
        actions.Controls.Add(CreateLabel("Read-only working set, thread count, and main-window correlation", 9F, FontStyle.Regular, SecondaryText));
        panel.Controls.Add(actions, 0, 0);
        panel.Controls.Add(_processesGrid, 0, 1);
        return panel;
    }

    private Control CreateShell()
    {
        var shell = new Panel { BackColor = Canvas, Dock = DockStyle.Fill };
        var tabs = new TabControl
        {
            Appearance = TabAppearance.FlatButtons,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
            ItemSize = new Size(145, 34),
            Padding = new Point(16, 6),
            SizeMode = TabSizeMode.Fixed,
        };

        var overview = CreateTab("OVERVIEW");
        overview.Controls.Add(CreateOverviewTabContent());
        var windows = CreateTab("WINDOWS");
        windows.Controls.Add(CreateWindowsTabContent());
        var processes = CreateTab("PROCESSES");
        processes.Controls.Add(CreateProcessesTabContent());
        var displays = CreateTab("DISPLAYS");
        displays.Controls.Add(CreateDisplayTabContent());
        var clipboard = CreateTab("CLIPBOARD / INPUT");
        clipboard.Controls.Add(CreateClipboardTabContent());
        var eventsPage = CreateTab("EVENT STREAM");
        eventsPage.Controls.Add(CreateEventsTabContent());
        var about = CreateTab("API MAP");
        about.Controls.Add(CreateAboutTabContent());
        tabs.TabPages.AddRange(new[] { overview, windows, processes, displays, clipboard, eventsPage, about });
        shell.Controls.Add(tabs);
        shell.Controls.Add(CreateHeader());
        return shell;
    }

    private Control CreateWindowsTabContent()
    {
        var layout = new TableLayoutPanel { BackColor = Canvas, ColumnCount = 1, Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var actions = new FlowLayoutPanel { BackColor = Surface, Dock = DockStyle.Fill, Padding = new Padding(8), WrapContents = false };
        actions.Controls.Add(_refreshWindowsButton);
        actions.Controls.Add(_bringToFrontButton);
        actions.Controls.Add(_restoreWindowButton);
        actions.Controls.Add(CreateLabel("Actions operate only on the selected live HWND.", 9F, FontStyle.Regular, SecondaryText));
        layout.Controls.Add(actions, 0, 0);

        var split = new SplitContainer { BackColor = Canvas, Dock = DockStyle.Fill, SplitterDistance = 1_000, SplitterWidth = 10 };
        split.Panel1.Controls.Add(_windowsGrid);
        var details = new Panel { BackColor = Surface, Dock = DockStyle.Fill, Padding = new Padding(18) };
        var title = CreateLabel("NATIVE WINDOW", 10F, FontStyle.Bold, Accent);
        title.Dock = DockStyle.Top;
        _windowDetailsLabel.AutoSize = false;
        _windowDetailsLabel.Dock = DockStyle.Fill;
        _windowDetailsLabel.Padding = new Padding(0, 15, 0, 0);
        details.Controls.Add(_windowDetailsLabel);
        details.Controls.Add(title);
        split.Panel2.Controls.Add(details);
        layout.Controls.Add(split, 0, 1);
        return layout;
    }

    private void UpdateWindowDetails(WindowSnapshot window)
    {
        _windowDetailsLabel.Text = window == null
            ? "Select a window to inspect its native state."
            : window.HandleHex + Environment.NewLine + Environment.NewLine
              + "Process  " + window.ProcessName + " (" + window.ProcessId + ")" + Environment.NewLine
              + "Caption  " + (string.IsNullOrEmpty(window.Caption) ? "(none)" : window.Caption) + Environment.NewLine
              + "Class    " + window.ClassName + Environment.NewLine
              + "Bounds   " + window.Bounds + Environment.NewLine + Environment.NewLine
              + "Visible     " + window.Visible + Environment.NewLine
              + "Minimized   " + window.Minimized + Environment.NewLine
              + "Maximized   " + window.Maximized;
    }
}
