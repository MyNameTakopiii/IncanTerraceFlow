#nullable enable
namespace IncanTerraceFlow;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private TableLayoutPanel rootLayout = null!;
    private Panel leftPanel = null!;
    private Panel middlePanel = null!;
    private Panel rightPanel = null!;
    private ComboBox caseSelector = null!;
    private Label activeGridLabel = null!;
    private Button openFileButton = null!;
    private Button pasteArraysButton = null!;
    private Label waterLabel = null!;
    private NumericUpDown waterInput = null!;
    private Label speedLabel = null!;
    private TrackBar speedSlider = null!;
    private Button nextStepButton = null!;
    private Button runAnimatedButton = null!;
    private Button stopAnimationButton = null!;
    private Button instantRunButton = null!;
    private Button benchmarkAllButton = null!;
    private Label randomLabel = null!;
    private NumericUpDown rowsInput = null!;
    private NumericUpDown colsInput = null!;
    private Button generateRandomButton = null!;
    private Label startColumnLabel = null!;
    private Label coveredFarmsLabel = null!;
    private Label waterLossLabel = null!;
    private Label elapsedLabel = null!;
    private Label operationsLabel = null!;
    private CheckBox heatmapCheckBox = null!;
    private Panel gridScrollPanel = null!;
    private GridCanvas gridCanvas = null!;
    private Label summaryTitleLabel = null!;
    private Label optimalLabel = null!;
    private Label complexityExplainLabel = null!;
    private DataGridView summaryGrid = null!;
    private BenchmarkChartPanel benchmarkChart = null!;
    private Label logTitleLabel = null!;
    private ListBox eventLog = null!;
    private System.Windows.Forms.Timer animationTimer = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        rootLayout = new TableLayoutPanel();
        leftPanel = new Panel();
        caseSelector = new ComboBox();
        activeGridLabel = new Label();
        openFileButton = new Button();
        pasteArraysButton = new Button();
        waterLabel = new Label();
        waterInput = new NumericUpDown();
        speedLabel = new Label();
        speedSlider = new TrackBar();
        nextStepButton = new Button();
        runAnimatedButton = new Button();
        stopAnimationButton = new Button();
        instantRunButton = new Button();
        benchmarkAllButton = new Button();
        randomLabel = new Label();
        rowsInput = new NumericUpDown();
        colsInput = new NumericUpDown();
        generateRandomButton = new Button();
        startColumnLabel = new Label();
        coveredFarmsLabel = new Label();
        waterLossLabel = new Label();
        elapsedLabel = new Label();
        operationsLabel = new Label();
        heatmapCheckBox = new CheckBox();
        middlePanel = new Panel();
        gridScrollPanel = new Panel();
        gridCanvas = new GridCanvas();
        rightPanel = new Panel();
        summaryTitleLabel = new Label();
        optimalLabel = new Label();
        complexityExplainLabel = new Label();
        summaryGrid = new DataGridView();
        benchmarkChart = new BenchmarkChartPanel();
        logTitleLabel = new Label();
        eventLog = new ListBox();
        animationTimer = new System.Windows.Forms.Timer(components);

        rootLayout.SuspendLayout();
        leftPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)waterInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)speedSlider).BeginInit();
        ((System.ComponentModel.ISupportInitialize)rowsInput).BeginInit();
        ((System.ComponentModel.ISupportInitialize)colsInput).BeginInit();
        middlePanel.SuspendLayout();
        gridScrollPanel.SuspendLayout();
        rightPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)summaryGrid).BeginInit();
        SuspendLayout();

        rootLayout.ColumnCount = 3;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
        rootLayout.Controls.Add(leftPanel, 0, 0);
        rootLayout.Controls.Add(middlePanel, 1, 0);
        rootLayout.Controls.Add(rightPanel, 2, 0);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.RowCount = 1;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        leftPanel.AutoScroll = true;
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.Padding = new Padding(8);

        caseSelector.Dock = DockStyle.Top;
        caseSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        caseSelector.Name = "caseSelector";
        caseSelector.TabIndex = 0;

        activeGridLabel.AutoSize = true;
        activeGridLabel.Dock = DockStyle.Top;
        activeGridLabel.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        activeGridLabel.ForeColor = Color.FromArgb(60, 60, 60);
        activeGridLabel.Margin = new Padding(0, 4, 0, 0);
        activeGridLabel.Name = "activeGridLabel";
        activeGridLabel.Text = "Active grid: —";

        openFileButton.AutoSize = true;
        openFileButton.Dock = DockStyle.Top;
        openFileButton.Margin = new Padding(0, 6, 0, 0);
        openFileButton.Name = "openFileButton";
        openFileButton.TabIndex = 1;
        openFileButton.Text = "Open File...";
        openFileButton.UseVisualStyleBackColor = true;
        openFileButton.Click += OpenFileButton_Click;

        pasteArraysButton.AutoSize = true;
        pasteArraysButton.Dock = DockStyle.Top;
        pasteArraysButton.Margin = new Padding(0, 6, 0, 0);
        pasteArraysButton.Name = "pasteArraysButton";
        pasteArraysButton.TabIndex = 15;
        pasteArraysButton.Text = "Paste Arrays...";
        pasteArraysButton.UseVisualStyleBackColor = true;
        pasteArraysButton.Click += PasteArraysButton_Click;

        waterLabel.AutoSize = true;
        waterLabel.Dock = DockStyle.Top;
        waterLabel.Margin = new Padding(0, 10, 0, 0);
        waterLabel.Text = "Initial Water (W)";

        waterInput.Dock = DockStyle.Top;
        waterInput.Maximum = 1000000;
        waterInput.Minimum = 1;
        waterInput.Name = "waterInput";
        waterInput.TabIndex = 2;
        waterInput.Value = 5000;
        waterInput.ValueChanged += WaterInput_ValueChanged;

        speedLabel.AutoSize = true;
        speedLabel.Dock = DockStyle.Top;
        speedLabel.Margin = new Padding(0, 10, 0, 0);
        speedLabel.Text = "Step delay: 200 ms";

        speedSlider.Dock = DockStyle.Top;
        speedSlider.Maximum = 1000;
        speedSlider.Minimum = 10;
        speedSlider.Name = "speedSlider";
        speedSlider.TabIndex = 3;
        speedSlider.TickFrequency = 100;
        speedSlider.Value = 200;
        speedSlider.ValueChanged += SpeedSlider_ValueChanged;

        nextStepButton.AutoSize = true;
        nextStepButton.Dock = DockStyle.Top;
        nextStepButton.Margin = new Padding(0, 10, 0, 0);
        nextStepButton.Text = "Next Step";
        nextStepButton.Click += NextStepButton_Click;

        runAnimatedButton.AutoSize = true;
        runAnimatedButton.Dock = DockStyle.Top;
        runAnimatedButton.Margin = new Padding(0, 6, 0, 0);
        runAnimatedButton.Text = "Run Animated";
        runAnimatedButton.Click += RunAnimatedButton_Click;

        stopAnimationButton.AutoSize = true;
        stopAnimationButton.Dock = DockStyle.Top;
        stopAnimationButton.Margin = new Padding(0, 6, 0, 0);
        stopAnimationButton.Text = "Stop Animation";
        stopAnimationButton.Enabled = false;
        stopAnimationButton.Click += StopAnimationButton_Click;

        instantRunButton.AutoSize = true;
        instantRunButton.Dock = DockStyle.Top;
        instantRunButton.Margin = new Padding(0, 6, 0, 0);
        instantRunButton.Text = "Instant Run";
        instantRunButton.Click += InstantRunButton_Click;

        benchmarkAllButton.AutoSize = true;
        benchmarkAllButton.Dock = DockStyle.Top;
        benchmarkAllButton.Margin = new Padding(0, 6, 0, 0);
        benchmarkAllButton.Text = "Benchmark All";
        benchmarkAllButton.Click += BenchmarkAllButton_Click;

        randomLabel.AutoSize = true;
        randomLabel.Dock = DockStyle.Top;
        randomLabel.Margin = new Padding(0, 12, 0, 0);
        randomLabel.Text = "Random Map (M x N)";

        rowsInput.Dock = DockStyle.Top;
        rowsInput.Maximum = 100;
        rowsInput.Minimum = 1;
        rowsInput.Value = 20;
        rowsInput.Name = "rowsInput";

        colsInput.Dock = DockStyle.Top;
        colsInput.Maximum = 100;
        colsInput.Minimum = 1;
        colsInput.Value = 20;
        colsInput.Name = "colsInput";

        generateRandomButton.AutoSize = true;
        generateRandomButton.Dock = DockStyle.Top;
        generateRandomButton.Margin = new Padding(0, 6, 0, 0);
        generateRandomButton.Text = "Generate Random Map";
        generateRandomButton.Click += GenerateRandomButton_Click;

        startColumnLabel.AutoSize = true;
        startColumnLabel.Dock = DockStyle.Top;
        startColumnLabel.Margin = new Padding(0, 12, 0, 0);
        startColumnLabel.Text = "Start Column: 0";

        coveredFarmsLabel.AutoSize = true;
        coveredFarmsLabel.Dock = DockStyle.Top;
        coveredFarmsLabel.Text = "Covered Farms: 0";

        waterLossLabel.AutoSize = true;
        waterLossLabel.Dock = DockStyle.Top;
        waterLossLabel.Text = "Water Loss: 0";

        elapsedLabel.AutoSize = true;
        elapsedLabel.Dock = DockStyle.Top;
        elapsedLabel.Text = "Elapsed: 0 ms";

        operationsLabel.AutoSize = true;
        operationsLabel.Dock = DockStyle.Top;
        operationsLabel.Text = "Operations: 0";

        heatmapCheckBox.AutoSize = true;
        heatmapCheckBox.Dock = DockStyle.Top;
        heatmapCheckBox.Margin = new Padding(0, 8, 0, 0);
        heatmapCheckBox.Text = "Show BFS heatmap";
        heatmapCheckBox.CheckedChanged += HeatmapCheckBox_CheckedChanged;

        leftPanel.Controls.Add(heatmapCheckBox);
        leftPanel.Controls.Add(operationsLabel);
        leftPanel.Controls.Add(elapsedLabel);
        leftPanel.Controls.Add(waterLossLabel);
        leftPanel.Controls.Add(coveredFarmsLabel);
        leftPanel.Controls.Add(startColumnLabel);
        leftPanel.Controls.Add(generateRandomButton);
        leftPanel.Controls.Add(colsInput);
        leftPanel.Controls.Add(rowsInput);
        leftPanel.Controls.Add(randomLabel);
        leftPanel.Controls.Add(benchmarkAllButton);
        leftPanel.Controls.Add(instantRunButton);
        leftPanel.Controls.Add(stopAnimationButton);
        leftPanel.Controls.Add(runAnimatedButton);
        leftPanel.Controls.Add(nextStepButton);
        leftPanel.Controls.Add(speedSlider);
        leftPanel.Controls.Add(speedLabel);
        leftPanel.Controls.Add(waterInput);
        leftPanel.Controls.Add(waterLabel);
        leftPanel.Controls.Add(pasteArraysButton);
        leftPanel.Controls.Add(openFileButton);
        leftPanel.Controls.Add(activeGridLabel);
        leftPanel.Controls.Add(caseSelector);

        middlePanel.Dock = DockStyle.Fill;
        middlePanel.Padding = new Padding(4);

        gridScrollPanel.AutoScroll = true;
        gridScrollPanel.Dock = DockStyle.Fill;

        gridCanvas.Dock = DockStyle.Fill;
        gridCanvas.MinimumSize = new Size(200, 200);

        gridScrollPanel.Controls.Add(gridCanvas);
        middlePanel.Controls.Add(gridScrollPanel);

        rightPanel.Dock = DockStyle.Fill;
        rightPanel.Padding = new Padding(8);

        summaryTitleLabel.Dock = DockStyle.Top;
        summaryTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        summaryTitleLabel.Height = 22;
        summaryTitleLabel.Text = "Leaderboard";

        optimalLabel.Dock = DockStyle.Top;
        optimalLabel.Height = 36;
        optimalLabel.Text = "Optimal start will appear after benchmark.";

        complexityExplainLabel.Dock = DockStyle.Top;
        complexityExplainLabel.Height = 135;
        complexityExplainLabel.Font = new Font("Segoe UI", 8.25F);
        complexityExplainLabel.ForeColor = Color.FromArgb(50, 50, 50);
        complexityExplainLabel.BackColor = Color.FromArgb(245, 247, 250);
        complexityExplainLabel.BorderStyle = BorderStyle.FixedSingle;
        complexityExplainLabel.Padding = new Padding(5);
        complexityExplainLabel.Margin = new Padding(0, 4, 0, 4);
        complexityExplainLabel.Text = "Big O Complexity Analysis:\n• Time Complexity: O(M×N) per column simulation, O(M×N²) for benchmarking all columns where M=Rows, N=Cols.\n• Space Complexity: O(M×N) for queue and visited states.\n• 'Ops' shows actual queue iterations. Tortoise wall-breaks and flow splits increase operations.";

        summaryGrid.AllowUserToAddRows = false;
        summaryGrid.AllowUserToDeleteRows = false;
        summaryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        summaryGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        summaryGrid.Dock = DockStyle.Top;
        summaryGrid.Height = 160;
        summaryGrid.MultiSelect = false;
        summaryGrid.ReadOnly = true;
        summaryGrid.RowHeadersVisible = false;
        summaryGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        summaryGrid.CellClick += SummaryGrid_CellClick;
        summaryGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StartColumn", HeaderText = "Col" });
        summaryGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CoveredFarms", HeaderText = "Covered" });
        summaryGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WaterLoss", HeaderText = "Loss" });
        summaryGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ElapsedMs", HeaderText = "Time ms" });
        summaryGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Operations", HeaderText = "Ops" });

        benchmarkChart.Dock = DockStyle.Top;
        benchmarkChart.Height = 160;
        benchmarkChart.Margin = new Padding(0, 8, 0, 0);

        logTitleLabel.Dock = DockStyle.Top;
        logTitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        logTitleLabel.Height = 22;
        logTitleLabel.Margin = new Padding(0, 8, 0, 0);
        logTitleLabel.Text = "Event Log";

        eventLog.Dock = DockStyle.Fill;
        eventLog.IntegralHeight = false;

        rightPanel.Controls.Add(eventLog);
        rightPanel.Controls.Add(logTitleLabel);
        rightPanel.Controls.Add(benchmarkChart);
        rightPanel.Controls.Add(complexityExplainLabel);
        rightPanel.Controls.Add(summaryGrid);
        rightPanel.Controls.Add(optimalLabel);
        rightPanel.Controls.Add(summaryTitleLabel);

        animationTimer.Interval = 200;
        animationTimer.Tick += AnimationTimer_Tick;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 720);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1000, 640);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Incan Terrace Flow — Ritual of the Andean Cloud";

        rootLayout.ResumeLayout(false);
        leftPanel.ResumeLayout(false);
        leftPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)waterInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)speedSlider).EndInit();
        ((System.ComponentModel.ISupportInitialize)rowsInput).EndInit();
        ((System.ComponentModel.ISupportInitialize)colsInput).EndInit();
        middlePanel.ResumeLayout(false);
        gridScrollPanel.ResumeLayout(false);
        rightPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)summaryGrid).EndInit();
        ResumeLayout(false);
    }
}
