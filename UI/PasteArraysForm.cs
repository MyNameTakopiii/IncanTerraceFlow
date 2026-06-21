using System;
using System.Drawing;
using System.Windows.Forms;

namespace IncanTerraceFlow;

public sealed class PasteArraysForm : Form
{
    private TextBox txtArrays = null!;
    private Label lblInstructions = null!;
    private Panel topPanel = null!;
    private Panel bottomPanel = null!;
    private Button btnLoadTemplate = null!;
    private Button btnParse = null!;
    private Button btnCancel = null!;

    public GridData? ResultGridData { get; private set; }

    public PasteArraysForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        txtArrays = new TextBox();
        lblInstructions = new Label();
        topPanel = new Panel();
        bottomPanel = new Panel();
        btnLoadTemplate = new Button();
        btnParse = new Button();
        btnCancel = new Button();

        SuspendLayout();

        // 
        // topPanel
        // 
        topPanel.Dock = DockStyle.Top;
        topPanel.Height = 50;
        topPanel.Padding = new Padding(12, 10, 12, 10);
        topPanel.Controls.Add(lblInstructions);

        // 
        // lblInstructions
        // 
        lblInstructions.Dock = DockStyle.Fill;
        lblInstructions.Text = "Paste four M x N nested arrays containing: 1) Elevation, 2) WallData, 3) Absorption, and 4) Tortoises. All arrays must have the same dimensions.";
        lblInstructions.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        lblInstructions.ForeColor = Color.FromArgb(40, 40, 40);

        // 
        // txtArrays
        // 
        txtArrays.Multiline = true;
        txtArrays.ScrollBars = ScrollBars.Both;
        txtArrays.Font = new Font("Consolas", 9.5F, FontStyle.Regular);
        txtArrays.Dock = DockStyle.Fill;
        txtArrays.WordWrap = false;

        // 
        // bottomPanel
        // 
        bottomPanel.Dock = DockStyle.Bottom;
        bottomPanel.Height = 45;
        bottomPanel.Controls.Add(btnLoadTemplate);
        bottomPanel.Controls.Add(btnParse);
        bottomPanel.Controls.Add(btnCancel);

        // 
        // btnLoadTemplate
        // 
        btnLoadTemplate.Text = "Load Example";
        btnLoadTemplate.Location = new Point(12, 10);
        btnLoadTemplate.Size = new Size(110, 26);
        btnLoadTemplate.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        btnLoadTemplate.UseVisualStyleBackColor = true;
        btnLoadTemplate.Click += BtnLoadTemplate_Click;

        // 
        // btnCancel
        // 
        btnCancel.Text = "Cancel";
        btnCancel.Location = new Point(595, 10);
        btnCancel.Size = new Size(85, 26);
        btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.DialogResult = DialogResult.Cancel;

        // 
        // btnParse
        // 
        btnParse.Text = "Parse & Load";
        btnParse.Location = new Point(500, 10);
        btnParse.Size = new Size(90, 26);
        btnParse.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        btnParse.UseVisualStyleBackColor = true;
        btnParse.Click += BtnParse_Click;

        // 
        // PasteArraysForm
        // 
        ClientSize = new Size(692, 490);
        MinimumSize = new Size(520, 360);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Paste Custom Grid Arrays";
        ShowInTaskbar = false;
        MaximizeBox = true;
        MinimizeBox = false;

        var mainPanel = new Panel();
        mainPanel.Dock = DockStyle.Fill;
        mainPanel.Padding = new Padding(12, 0, 12, 0);
        mainPanel.Controls.Add(txtArrays);

        Controls.Add(mainPanel);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        AcceptButton = btnParse;
        CancelButton = btnCancel;

        ResumeLayout(false);
    }

    private void BtnLoadTemplate_Click(object? sender, EventArgs e)
    {
        txtArrays.Text = @"const array1 = [
  [100, 100, 100],
  [ 90,  80,  70],
  [ 60,  50,  40]
];

const array2 = [
  [11, 10, 14],
  [11,  0, 14],
  [11, 10, 14]
];

const array3 = [
  [50, 50, 50],
  [50, 50, 50],
  [50, 50, 50]
];

const array4 = [
  [false, false, false],
  [false,  true, false],
  [false, false, false]
];";
    }

    private void BtnParse_Click(object? sender, EventArgs e)
    {
        try
        {
            ResultGridData = CustomArrayParser.Parse(txtArrays.Text, "Custom Pasted Grid");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Parsing failed:\n{ex.Message}", "Syntax or Dimension Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
