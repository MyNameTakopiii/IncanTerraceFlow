using System.Diagnostics;

namespace IncanTerraceFlow;

public sealed partial class MainForm : Form
{
    private const string CustomPastedCaseName = "(Custom Pasted)";

    private readonly List<(string Name, string Path)> _builtInCases = new();
    private GridData? _currentGridData;
    private GridData? _customPastedGridData;
    private IncanTerraceSimulator? _simulator;
    private SimulationSession? _session;
    private IReadOnlyList<ColumnBenchmarkResult> _benchmarkResults = Array.Empty<ColumnBenchmarkResult>();
    private int _selectedStartColumn;
    private bool _autoRunning;
    private bool _loadingCase;
    private Stopwatch? _runStopwatch;

    public MainForm()
    {
        InitializeComponent();
        gridCanvas.StartColumnSelected += GridCanvas_StartColumnSelected;
        caseSelector.SelectedIndexChanged += CaseSelector_SelectedIndexChanged;
        PopulateBuiltInCases();
        LoadGridData(SampleGridData.CreateGridData(), logLoad: false);
        AppendLog("Ready. Select a case or open a hidden test file.");
    }

    private void PopulateBuiltInCases()
    {
        _loadingCase = true;
        caseSelector.Items.Clear();
        _builtInCases.Clear();

        string testCasesDir = Path.Combine(AppContext.BaseDirectory, "TestCases");
        if (Directory.Exists(testCasesDir))
        {
            foreach (string file in Directory.GetFiles(testCasesDir, "*.*")
                         .Where(path => path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
                                        path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                                        path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                _builtInCases.Add((name, file));
                caseSelector.Items.Add(name);
            }
        }

        if (_builtInCases.Count == 0)
        {
            caseSelector.Items.Add("Sample Case 1");
            _builtInCases.Add(("Sample Case 1", string.Empty));
        }

        caseSelector.SelectedIndex = 0;
        _loadingCase = false;
    }

    private void CaseSelector_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_loadingCase || caseSelector.SelectedIndex < 0)
        {
            return;
        }

        var selected = _builtInCases[caseSelector.SelectedIndex];
        if (string.IsNullOrEmpty(selected.Path))
        {
            if (selected.Name == CustomPastedCaseName && _customPastedGridData is not null)
            {
                LoadGridData(_customPastedGridData);
            }
            else
            {
                LoadGridData(SampleGridData.CreateGridData());
            }

            return;
        }

        try
        {
            LoadGridData(GridDataParser.ParseFile(selected.Path));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Failed to load case", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenFileButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Grid files (*.csv;*.txt;*.json)|*.csv;*.txt;*.json|All files (*.*)|*.*",
            Title = "Open Test Case",
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var data = GridDataParser.ParseFile(dialog.FileName);
            LoadGridData(data);
            AppendLog($"Loaded external file: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Failed to open file", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void WaterInput_ValueChanged(object? sender, EventArgs e)
    {
        if (_currentGridData is null || _loadingCase)
        {
            return;
        }

        RecreateSimulatorFromCurrentGrid(resetSession: true);
        gridCanvas.SetSimulator(_simulator!, _selectedStartColumn);
        _benchmarkResults = Array.Empty<ColumnBenchmarkResult>();
        RefreshSummaryTable();
    }

    private void SpeedSlider_ValueChanged(object? sender, EventArgs e)
    {
        animationTimer.Interval = speedSlider.Value;
        speedLabel.Text = $"Step delay: {speedSlider.Value} ms";
    }

    private void HeatmapCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        gridCanvas.ShowHeatmap = heatmapCheckBox.Checked;
    }

    private void GenerateRandomButton_Click(object? sender, EventArgs e)
    {
        int rows = (int)rowsInput.Value;
        int cols = (int)colsInput.Value;
        LoadGridData(RandomGridGenerator.Generate(rows, cols));
    }

    private void NextStepButton_Click(object? sender, EventArgs e)
    {
        StopAutoRun();
        ClearHeatmapForNewRun();
        EnsureSession();
        if (_session is null || _session.IsComplete)
        {
            return;
        }

        _runStopwatch ??= Stopwatch.StartNew();
        var step = _session.StepNext();
        if (step is not null)
        {
            gridCanvas.ApplyStep(step);
            LogStepEvents(step);
            UpdateStats();
            animationTimer.Start();
        }
    }

    private void RunAnimatedButton_Click(object? sender, EventArgs e)
    {
        ClearHeatmapForNewRun();
        EnsureSession();
        if (_session is null || _session.IsComplete)
        {
            UpdateStats();
            return;
        }

        _runStopwatch = Stopwatch.StartNew();
        _autoRunning = true;
        runAnimatedButton.Enabled = false;
        instantRunButton.Enabled = false;
        benchmarkAllButton.Enabled = false;
        nextStepButton.Enabled = false;
        stopAnimationButton.Enabled = true;
        animationTimer.Start();
    }

    private void InstantRunButton_Click(object? sender, EventArgs e)
    {
        if (_simulator is null)
        {
            return;
        }

        StopAutoRun();
        ClearHeatmapForNewRun();

        _session = _simulator.BeginSimulation(_selectedStartColumn);
        var stopwatch = Stopwatch.StartNew();
        _session.RunToCompletion();
        stopwatch.Stop();

        gridCanvas.SetSession(_session);
        gridCanvas.SetVisitCounts((int[,])_session.VisitCount.Clone());
        heatmapCheckBox.Checked = true;
        gridCanvas.ShowHeatmap = true;

        UpdateStats(stopwatch.ElapsedMilliseconds);
        AppendLog($"Instant run col {_selectedStartColumn}: {_session.CoveredCount} covered in {stopwatch.ElapsedMilliseconds} ms ({_session.OperationsProcessed} ops).");
    }

    private void BenchmarkAllButton_Click(object? sender, EventArgs e)
    {
        if (_simulator is null)
        {
            return;
        }

        StopAutoRun();
        ClearHeatmapForNewRun();
        _session = null;
        gridCanvas.SetSession(null);

        var stopwatch = Stopwatch.StartNew();
        _benchmarkResults = _simulator.BenchmarkAllColumns();
        stopwatch.Stop();

        RefreshSummaryTable(_benchmarkResults);
        AppendLog($"Benchmarked {_benchmarkResults.Count} columns in {stopwatch.ElapsedMilliseconds} ms.");
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (_session is null)
        {
            StopAutoRun();
            return;
        }

        if (_autoRunning)
        {
            if (gridCanvas.HasActiveAnimation)
            {
                gridCanvas.AdvanceAnimation();
                return;
            }

            if (_session.IsComplete)
            {
                StopAutoRun();
                UpdateStats(_runStopwatch?.ElapsedMilliseconds);
                AppendLog($"Animated run complete: {_session.CoveredCount} covered farms.");
                return;
            }

            var step = _session.StepNext();
            if (step is not null)
            {
                gridCanvas.ApplyStep(step);
                LogStepEvents(step);
                UpdateStats(_runStopwatch?.ElapsedMilliseconds);
            }

            return;
        }

        gridCanvas.AdvanceAnimation();
        if (!gridCanvas.HasActiveAnimation)
        {
            animationTimer.Stop();
        }
    }

    private void SummaryGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _simulator is null)
        {
            return;
        }

        var row = summaryGrid.Rows[e.RowIndex];
        if (row.Cells["StartColumn"].Value is int column)
        {
            SelectStartColumn(column, resetSession: true);
        }
    }

    private void GridCanvas_StartColumnSelected(object? sender, int column)
    {
        SelectStartColumn(column, resetSession: true);
    }

    private void LoadGridData(GridData data, bool logLoad = true)
    {
        StopAutoRun();
        _currentGridData = data;
        _selectedStartColumn = 0;
        _session = null;
        _benchmarkResults = Array.Empty<ColumnBenchmarkResult>();
        _runStopwatch = null;

        RecreateSimulatorFromCurrentGrid(resetSession: false);
        gridCanvas.SetSimulator(_simulator!, _selectedStartColumn);
        gridCanvas.ClearHeatmap();
        heatmapCheckBox.Checked = false;
        ConfigureGridScroll(data.Rows, data.Cols);
        UpdateStartColumnLabel();
        UpdateActiveGridLabel(data);
        UpdateComplexityExplanation(data.Rows, data.Cols);
        UpdateStats(reset: true);
        _benchmarkResults = _simulator!.BenchmarkAllColumns();
        RefreshSummaryTable(_benchmarkResults);

        if (logLoad)
        {
            var best = _benchmarkResults
                 .OrderByDescending(r => r.CoveredFarms)
                 .ThenBy(r => r.WaterLoss)
                 .ThenBy(r => r.StartColumn)
                 .First();
            AppendLog(
                $"Loaded '{data.Name}' ({data.Rows}x{data.Cols}). Optimal: column {best.StartColumn} with {best.CoveredFarms} covered farms.");
        }
    }

    private void UpdateActiveGridLabel(GridData data)
    {
        activeGridLabel.Text = $"Active grid: {data.Name} ({data.Rows}x{data.Cols})";
    }

    private void UpdateComplexityExplanation(int rows, int cols)
    {
        int cellCount = rows * cols;
        int benchmarkMaxOps = rows * cols * cols;
        complexityExplainLabel.Text = 
            $"Big O Complexity Analysis (Grid: {rows}x{cols}):\n" +
            $"• Time: O(M×N) per column [~{cellCount} max steps]\n" +
            $"  Benchmark: O(M×N²) [~{benchmarkMaxOps} max steps total]\n" +
            $"• Space: O(M×N) [~{cellCount} state spaces]\n" +
            $"• 'Ops' shows actual processed queue iterations.\n\n" +
            $"วิเคราะห์ความซับซ้อน Big O (ตาราง: {rows}x{cols}):\n" +
            $"• เวลา: O(M×N) ต่อคอลัมน์ [~{cellCount} ขั้นตอนสูงสุด]\n" +
            $"  Benchmark: O(M×N²) [~{benchmarkMaxOps} ขั้นตอนรวม]\n" +
            $"• พื้นที่: O(M×N) [~{cellCount} สถานะในหน่วยความจำ]";
    }

    private void SelectCustomPastedCaseInDropdown()
    {
        _loadingCase = true;

        int existingIndex = -1;
        for (int i = 0; i < _builtInCases.Count; i++)
        {
            if (_builtInCases[i].Name == CustomPastedCaseName)
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex < 0)
        {
            _builtInCases.Add((CustomPastedCaseName, string.Empty));
            caseSelector.Items.Add(CustomPastedCaseName);
            existingIndex = _builtInCases.Count - 1;
        }

        caseSelector.SelectedIndex = existingIndex;
        _loadingCase = false;
    }

    private void RecreateSimulatorFromCurrentGrid(bool resetSession)
    {
        if (_currentGridData is null)
        {
            return;
        }

        _simulator = new IncanTerraceSimulator(
            (double)waterInput.Value,
            _currentGridData.Elevation,
            _currentGridData.WallData,
            _currentGridData.AbsorptionRequired,
            _currentGridData.HasTortoise);

        if (resetSession)
        {
            _session = null;
            gridCanvas.SetSession(null);
            gridCanvas.ClearHeatmap();
            heatmapCheckBox.Checked = false;
            _runStopwatch = null;
            UpdateStats(reset: true);
        }
    }

    private void ConfigureGridScroll(int rows, int cols)
    {
        bool largeGrid = rows > 50 || cols > 50;
        gridScrollPanel.AutoScroll = largeGrid;

        if (largeGrid)
        {
            gridCanvas.Dock = DockStyle.None;
            gridCanvas.UpdateCanvasSize();
        }
        else
        {
            gridCanvas.Dock = DockStyle.Fill;
            gridCanvas.Size = gridScrollPanel.ClientSize;
        }

        gridCanvas.Invalidate();
    }

    private void EnsureSession()
    {
        if (_simulator is null)
        {
            return;
        }

        if (_session is null || _session.IsComplete)
        {
            _session = _simulator.BeginSimulation(_selectedStartColumn);
            gridCanvas.SetSession(_session);
            _runStopwatch = Stopwatch.StartNew();
        }
    }

    private void SelectStartColumn(int column, bool resetSession)
    {
        if (_simulator is null || column < 0 || column >= _simulator.Cols)
        {
            return;
        }

        StopAutoRun();
        _selectedStartColumn = column;
        gridCanvas.SelectedStartColumn = column;
        UpdateStartColumnLabel();

        if (resetSession)
        {
            _session = null;
            gridCanvas.SetSession(null);
            gridCanvas.ClearHeatmap();
            heatmapCheckBox.Checked = false;
            _runStopwatch = null;
            UpdateStats(reset: true);
            AppendLog($"Start column set to {column}.");
        }
    }

    private void UpdateStartColumnLabel()
    {
        startColumnLabel.Text = $"Start Column: {_selectedStartColumn} (click top row)";
    }

    private void UpdateStats(bool reset = false)
    {
        UpdateStats(reset ? null : _runStopwatch?.ElapsedMilliseconds);
    }

    private void UpdateStats(long? elapsedMs)
    {
        if (_session is null)
        {
            coveredFarmsLabel.Text = "Covered Farms: 0";
            waterLossLabel.Text = "Water Loss: 0";
            elapsedLabel.Text = elapsedMs.HasValue ? $"Elapsed: {elapsedMs.Value} ms" : "Elapsed: 0 ms";
            operationsLabel.Text = "Operations: 0";
            return;
        }

        coveredFarmsLabel.Text = $"Covered Farms: {_session.CoveredCount}";
        waterLossLabel.Text = $"Water Loss: {_session.TotalWaterLoss:0.##}";
        elapsedLabel.Text = elapsedMs.HasValue ? $"Elapsed: {elapsedMs.Value} ms" : "Elapsed: 0 ms";
        operationsLabel.Text = $"Operations: {_session.OperationsProcessed}";
    }

    private void RefreshSummaryTable()
    {
        if (_simulator is null)
        {
            summaryGrid.Rows.Clear();
            benchmarkChart.SetResults(_benchmarkResults, -1);
            return;
        }

        if (_benchmarkResults.Count == 0)
        {
            _benchmarkResults = _simulator.BenchmarkAllColumns();
        }

        RefreshSummaryTable(_benchmarkResults);
    }

    private void RefreshSummaryTable(IReadOnlyList<ColumnBenchmarkResult> results)
    {
        if (_simulator is null)
        {
            return;
        }

        _benchmarkResults = results;
        int bestColumn = results.OrderByDescending(r => r.CoveredFarms)
            .ThenBy(r => r.WaterLoss)
            .ThenBy(r => r.StartColumn)
            .First().StartColumn;
        int maxCovered = results.Max(r => r.CoveredFarms);

        summaryGrid.Rows.Clear();
        foreach (var result in results)
        {
            int rowIndex = summaryGrid.Rows.Add(
                result.StartColumn,
                result.CoveredFarms,
                result.WaterLoss.ToString("0.##"),
                result.ElapsedMs,
                result.OperationsProcessed);

            var row = summaryGrid.Rows[rowIndex];
            if (result.StartColumn == bestColumn)
            {
                row.DefaultCellStyle.BackColor = Color.LightGreen;
                row.DefaultCellStyle.Font = new Font(summaryGrid.Font, FontStyle.Bold);
            }

            if (result.CoveredFarms == maxCovered)
            {
                row.Cells["CoveredFarms"].Style.Font = new Font(summaryGrid.Font, FontStyle.Bold);
            }
        }

        optimalLabel.Text =
            $"Optimal start: column {bestColumn} with {maxCovered} covered farms (highlighted in green).";

        benchmarkChart.SetResults(results, bestColumn);
    }

    private void LogStepEvents(SimulationStep step)
    {
        if (step.TortoiseWallBreak)
        {
            AppendLog($"Tortoise panic at ({step.Row},{step.Col}) — walls destroyed.");
        }

        foreach (var flow in step.Flows.Where(f => f.IsCliff))
        {
            AppendLog($"Cliff loss {flow.Amount:0.##} from ({flow.FromRow},{flow.FromCol}).");
        }
    }

    private void AppendLog(string message)
    {
        eventLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        if (eventLog.Items.Count > 200)
        {
            eventLog.Items.RemoveAt(eventLog.Items.Count - 1);
        }
    }

    private void ClearHeatmapForNewRun()
    {
        gridCanvas.ClearHeatmap();
        heatmapCheckBox.Checked = false;
    }

    private void PasteArraysButton_Click(object? sender, EventArgs e)
    {
        using var form = new PasteArraysForm();
        if (form.ShowDialog(this) == DialogResult.OK && form.ResultGridData != null)
        {
            _customPastedGridData = form.ResultGridData;
            SelectCustomPastedCaseInDropdown();
            LoadGridData(form.ResultGridData);
        }
    }

    private void StopAutoRun()
    {
        _autoRunning = false;
        animationTimer.Stop();
        runAnimatedButton.Enabled = true;
        instantRunButton.Enabled = true;
        benchmarkAllButton.Enabled = true;
        nextStepButton.Enabled = true;
        stopAnimationButton.Enabled = false;
    }

    private void StopAnimationButton_Click(object? sender, EventArgs e)
    {
        StopAutoRun();
        AppendLog("Animation stopped by user.");
    }
}
