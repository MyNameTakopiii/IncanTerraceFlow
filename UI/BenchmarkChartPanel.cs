using System.Drawing.Drawing2D;

namespace IncanTerraceFlow;

public sealed class BenchmarkChartPanel : Control
{
    private IReadOnlyList<ColumnBenchmarkResult> _results = Array.Empty<ColumnBenchmarkResult>();
    private int _bestColumn = -1;

    public BenchmarkChartPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        MinimumSize = new Size(200, 120);
    }

    public void SetResults(IReadOnlyList<ColumnBenchmarkResult> results, int bestColumn)
    {
        _results = results;
        _bestColumn = bestColumn;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(BackColor);

        if (_results.Count == 0)
        {
            DrawPlaceholder(e.Graphics, "Run Benchmark All to compare columns.");
            return;
        }

        const float marginLeft = 36f;
        const float marginRight = 36f;
        const float marginTop = 24f;
        const float marginBottom = 28f;

        float chartWidth = Math.Max(1, Width - marginLeft - marginRight);
        float chartHeight = Math.Max(1, Height - marginTop - marginBottom);
        var chartRect = new RectangleF(marginLeft, marginTop, chartWidth, chartHeight);

        long maxTime = Math.Max(1, _results.Max(r => r.ElapsedMs));
        int maxCovered = Math.Max(1, _results.Max(r => r.CoveredFarms));

        using var axisPen = new Pen(Color.Gray, 1f);
        e.Graphics.DrawLine(axisPen, chartRect.Left, chartRect.Bottom, chartRect.Right, chartRect.Bottom);
        e.Graphics.DrawLine(axisPen, chartRect.Left, chartRect.Top, chartRect.Left, chartRect.Bottom);
        e.Graphics.DrawLine(axisPen, chartRect.Right, chartRect.Top, chartRect.Right, chartRect.Bottom);

        using var titleFont = new Font(Font.FontFamily, 8f, FontStyle.Bold);
        using var labelFont = new Font(Font.FontFamily, 7f);
        e.Graphics.DrawString("Time (ms)", titleFont, Brushes.SteelBlue, chartRect.Left, 4);
        e.Graphics.DrawString("Covered", titleFont, Brushes.ForestGreen, chartRect.Right - 42, 4);

        float groupWidth = chartRect.Width / _results.Count;
        float barWidth = Math.Max(2f, groupWidth * 0.28f);
        float gap = barWidth * 0.15f;

        for (int i = 0; i < _results.Count; i++)
        {
            var result = _results[i];
            float groupX = chartRect.Left + i * groupWidth;
            float centerX = groupX + groupWidth / 2f;

            float timeHeight = (float)(result.ElapsedMs / (double)maxTime * chartRect.Height);
            float coveredHeight = (float)(result.CoveredFarms / (double)maxCovered * chartRect.Height);

            var timeRect = new RectangleF(
                centerX - barWidth - gap / 2f,
                chartRect.Bottom - timeHeight,
                barWidth,
                timeHeight);

            var coveredRect = new RectangleF(
                centerX + gap / 2f,
                chartRect.Bottom - coveredHeight,
                barWidth,
                coveredHeight);

            bool isBest = result.StartColumn == _bestColumn;
            using var timeBrush = new SolidBrush(isBest ? Color.RoyalBlue : Color.LightSteelBlue);
            using var coveredBrush = new SolidBrush(isBest ? Color.ForestGreen : Color.LightGreen);

            if (timeHeight > 0)
            {
                e.Graphics.FillRectangle(timeBrush, timeRect);
            }

            if (coveredHeight > 0)
            {
                e.Graphics.FillRectangle(coveredBrush, coveredRect);
            }

            string label = result.StartColumn.ToString();
            var labelSize = e.Graphics.MeasureString(label, labelFont);
            e.Graphics.DrawString(
                label,
                labelFont,
                Brushes.Black,
                centerX - labelSize.Width / 2f,
                chartRect.Bottom + 4);
        }
    }

    private void DrawPlaceholder(Graphics g, string text)
    {
        using var font = new Font(Font.FontFamily, 9f);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, Brushes.Gray, (Width - size.Width) / 2f, (Height - size.Height) / 2f);
    }
}
