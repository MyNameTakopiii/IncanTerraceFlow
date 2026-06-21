using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace IncanTerraceFlow;

public sealed class GridCanvas : Control
{
    private const int NorthBit = 8;
    private const int EastBit = 4;
    private const int SouthBit = 2;
    private const int WestBit = 1;
    private const float GridPadding = 20f;
    private const float LargeGridCellSize = 6f;
    private const int LargeGridThreshold = 50;

    private static readonly Color LowElevationColor = Color.FromArgb(215, 204, 200);
    private static readonly Color HighElevationColor = Color.FromArgb(62, 39, 35);

    private IncanTerraceSimulator? _simulator;
    private SimulationSession? _session;
    private SimulationStep? _currentStep;
    private int[,]? _visitCounts;
    private int _selectedStartColumn;
    private int _animationFrame;
    private bool _showTortoiseFlash;
    private bool _showHeatmap;

    public GridCanvas()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(240, 235, 228);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public int SelectedStartColumn
    {
        get => _selectedStartColumn;
        set
        {
            if (_selectedStartColumn != value)
            {
                _selectedStartColumn = value;
                Invalidate();
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool ShowHeatmap
    {
        get => _showHeatmap;
        set
        {
            if (_showHeatmap != value)
            {
                _showHeatmap = value;
                Invalidate();
            }
        }
    }

    public void SetSimulator(IncanTerraceSimulator simulator, int startColumn)
    {
        _simulator = simulator;
        _selectedStartColumn = startColumn;
        _session = null;
        _currentStep = null;
        _visitCounts = null;
        _showHeatmap = false;
        _animationFrame = 0;
        _showTortoiseFlash = false;
        UpdateCanvasSize();
        Invalidate();
    }

    public void SetSession(SimulationSession? session)
    {
        _session = session;
        _currentStep = null;
        _animationFrame = 0;
        _showTortoiseFlash = false;
        Invalidate();
    }

    public void SetVisitCounts(int[,]? visitCounts)
    {
        _visitCounts = visitCounts;
        Invalidate();
    }

    public void ClearHeatmap()
    {
        _visitCounts = null;
        _showHeatmap = false;
        Invalidate();
    }

    public void ApplyStep(SimulationStep step)
    {
        _currentStep = step;
        _animationFrame = 0;
        _showTortoiseFlash = step.TortoiseWallBreak;
        Invalidate();
    }

    public bool HasActiveAnimation => _currentStep is not null || _showTortoiseFlash;

    public void AdvanceAnimation()
    {
        if (_currentStep is null && !_showTortoiseFlash)
        {
            return;
        }

        _animationFrame++;
        if (_animationFrame >= 4)
        {
            _currentStep = null;
            _showTortoiseFlash = false;
        }

        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (!IsLargeGridMode())
        {
            Invalidate();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        if (_simulator is null)
        {
            return;
        }

        var cell = HitTest(e.Location);
        if (cell is { Row: 0 })
        {
            StartColumnSelected?.Invoke(this, cell.Value.Col);
        }
    }

    public event EventHandler<int>? StartColumnSelected;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_simulator is null)
        {
            DrawPlaceholder(e.Graphics);
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        int rows = _simulator.Rows;
        int cols = _simulator.Cols;
        var cellSize = GetCellSize(rows, cols);
        var origin = GetGridOrigin(cellSize, rows, cols);
        var visibleBounds = GetVisibleBounds();

        double minElevation = double.MaxValue;
        double maxElevation = double.MinValue;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double elevation = _simulator.Elevation[r, c];
                minElevation = Math.Min(minElevation, elevation);
                maxElevation = Math.Max(maxElevation, elevation);
            }
        }

        int maxVisits = 0;
        if (_showHeatmap && _visitCounts is not null)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    maxVisits = Math.Max(maxVisits, _visitCounts[r, c]);
                }
            }
        }

        var walls = _session?.Walls ?? _simulator.WallData;
        var absorbed = _session?.Absorbed;
        var visitCounts = _showHeatmap ? _visitCounts : null;
        float wallThickness = Math.Max(1f, Math.Min(4f, cellSize.Width * 0.08f));

        GetVisibleCellRange(origin, cellSize, rows, cols, visibleBounds, out int startRow, out int endRow, out int startCol, out int endCol);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var rect = GetCellRect(origin, cellSize, row, col);
                if (!rect.IntersectsWith(visibleBounds))
                {
                    continue;
                }

                DrawCell(
                    e.Graphics,
                    rect,
                    row,
                    col,
                    minElevation,
                    maxElevation,
                    walls,
                    absorbed,
                    visitCounts,
                    maxVisits,
                    cellSize,
                    wallThickness);
            }
        }

        if (startRow <= 0 && endRow >= 0)
        {
            DrawStartColumnHighlight(e.Graphics, origin, cellSize);
        }

        DrawFlowArrows(e.Graphics, origin, cellSize);
        DrawCliffDrops(e.Graphics, origin, cellSize);
        DrawTortoiseFlash(e.Graphics, origin, cellSize);
    }

    public void UpdateCanvasSize()
    {
        if (_simulator is null)
        {
            return;
        }

        if (!IsLargeGridMode())
        {
            return;
        }

        var cellSize = GetCellSize(_simulator.Rows, _simulator.Cols);
        int width = (int)Math.Ceiling(cellSize.Width * _simulator.Cols + GridPadding * 2);
        int height = (int)Math.Ceiling(cellSize.Height * _simulator.Rows + GridPadding * 2);
        Size = new Size(Math.Max(width, 1), Math.Max(height, 1));
    }

    private bool IsLargeGridMode() =>
        _simulator is not null &&
        (_simulator.Rows > LargeGridThreshold || _simulator.Cols > LargeGridThreshold);

    private RectangleF GetVisibleBounds()
    {
        if (Parent is ScrollableControl scrollable && scrollable.AutoScroll)
        {
            var scrollPosition = scrollable.AutoScrollPosition;
            return new RectangleF(
                -scrollPosition.X,
                -scrollPosition.Y,
                scrollable.ClientSize.Width,
                scrollable.ClientSize.Height);
        }

        return new RectangleF(0, 0, Width, Height);
    }

    private static void GetVisibleCellRange(
        PointF origin,
        SizeF cellSize,
        int rows,
        int cols,
        RectangleF visibleBounds,
        out int startRow,
        out int endRow,
        out int startCol,
        out int endCol)
    {
        startRow = Math.Clamp((int)Math.Floor((visibleBounds.Top - origin.Y) / cellSize.Height) - 1, 0, rows - 1);
        endRow = Math.Clamp((int)Math.Ceiling((visibleBounds.Bottom - origin.Y) / cellSize.Height) + 1, 0, rows - 1);
        startCol = Math.Clamp((int)Math.Floor((visibleBounds.Left - origin.X) / cellSize.Width) - 1, 0, cols - 1);
        endCol = Math.Clamp((int)Math.Ceiling((visibleBounds.Right - origin.X) / cellSize.Width) + 1, 0, cols - 1);
    }

    private void DrawPlaceholder(Graphics g)
    {
        using var font = new Font(Font.FontFamily, 12f);
        var text = "Load data to display the terrace grid.";
        var size = g.MeasureString(text, font);
        g.DrawString(
            text,
            font,
            Brushes.Gray,
            (Width - size.Width) / 2f,
            (Height - size.Height) / 2f);
    }

    private void DrawCell(
        Graphics g,
        RectangleF rect,
        int row,
        int col,
        double minElevation,
        double maxElevation,
        int[,] walls,
        double[,]? absorbed,
        int[,]? visitCounts,
        int maxVisits,
        SizeF cellSize,
        float wallThickness)
    {
        double elevation = _simulator!.Elevation[row, col];
        double t = maxElevation > minElevation
            ? (elevation - minElevation) / (maxElevation - minElevation)
            : 0;
        Color baseColor = LerpColor(LowElevationColor, HighElevationColor, t);

        using (var brush = new SolidBrush(baseColor))
        {
            g.FillRectangle(brush, rect);
        }

        if (visitCounts is not null && maxVisits > 0)
        {
            double heat = visitCounts[row, col] / (double)maxVisits;
            if (heat > 0)
            {
                Color heatColor = LerpColor(Color.FromArgb(120, Color.Gold), Color.FromArgb(180, Color.Red), heat);
                using var heatBrush = new SolidBrush(heatColor);
                g.FillRectangle(heatBrush, rect);
            }
        }

        if (absorbed is not null)
        {
            double required = _simulator.AbsorptionRequired[row, col];
            if (Math.Abs(absorbed[row, col] - required) <= 1e-9)
            {
                using var coveredBrush = new SolidBrush(Color.FromArgb(220, Color.DodgerBlue));
                g.FillRectangle(coveredBrush, rect);
            }
            else if (absorbed[row, col] > 1e-9)
            {
                using var partialBrush = new SolidBrush(Color.FromArgb(100, Color.LightBlue));
                g.FillRectangle(partialBrush, rect);
            }
        }

        if (_currentStep is not null &&
            _currentStep.Row == row &&
            _currentStep.Col == col &&
            _animationFrame < 4)
        {
            int alpha = _currentStep.AbsorbedAmount > 1e-9 ? 140 : 90;
            using var activeBrush = new SolidBrush(Color.FromArgb(alpha, Color.LightBlue));
            g.FillRectangle(activeBrush, rect);
        }

        DrawWalls(g, rect, walls[row, col], wallThickness);

        if (cellSize.Width >= 14f)
        {
            DrawElevationLabel(g, rect, elevation);
        }

        if (_simulator.HasTortoise[row, col] && cellSize.Width >= 10f)
        {
            DrawTortoise(g, rect);
        }
    }

    private static void DrawWalls(Graphics g, RectangleF rect, int wallMask, float thickness)
    {
        using var pen = new Pen(Color.SaddleBrown, thickness);
        if ((wallMask & NorthBit) != 0)
        {
            g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
        }

        if ((wallMask & EastBit) != 0)
        {
            g.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom);
        }

        if ((wallMask & SouthBit) != 0)
        {
            g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
        }

        if ((wallMask & WestBit) != 0)
        {
            g.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom);
        }
    }

    private static void DrawElevationLabel(Graphics g, RectangleF rect, double elevation)
    {
        using var font = new Font("Segoe UI", 7f);
        string text = elevation.ToString("0");
        var size = g.MeasureString(text, font);
        g.DrawString(
            text,
            font,
            Brushes.Black,
            rect.X + (rect.Width - size.Width) / 2f,
            rect.Y + 2);
    }

    private static void DrawTortoise(Graphics g, RectangleF rect)
    {
        float cx = rect.X + rect.Width / 2f;
        float cy = rect.Y + rect.Height / 2f + 2f; // slightly below center to leave room for elevation label at top
        float size = Math.Min(rect.Width * 0.5f, 20f);
        if (size < 6f) return;

        float shellW = size;
        float shellH = size * 0.8f;
        float headSize = size * 0.35f;
        float limbW = size * 0.25f;
        float limbH = size * 0.35f;

        // Colors
        using var shellBrush = new SolidBrush(Color.FromArgb(56, 142, 60)); // Dark forest green
        using var skinBrush = new SolidBrush(Color.FromArgb(139, 195, 74));  // Light lime green
        using var outlinePen = new Pen(Color.White, 1.5f); // White outline for contrast

        // Draw 4 limbs
        // Top-left
        g.FillEllipse(skinBrush, cx - shellW * 0.45f, cy - shellH * 0.45f, limbW, limbH);
        // Top-right
        g.FillEllipse(skinBrush, cx + shellW * 0.25f, cy - shellH * 0.45f, limbW, limbH);
        // Bottom-left
        g.FillEllipse(skinBrush, cx - shellW * 0.45f, cy + shellH * 0.15f, limbW, limbH);
        // Bottom-right
        g.FillEllipse(skinBrush, cx + shellW * 0.25f, cy + shellH * 0.15f, limbW, limbH);

        // Draw tail
        g.FillPolygon(skinBrush, new PointF[] {
            new(cx - shellW * 0.4f, cy),
            new(cx - shellW * 0.6f, cy + shellH * 0.1f),
            new(cx - shellW * 0.3f, cy + shellH * 0.2f)
        });

        // Draw head (facing right/east)
        g.FillEllipse(skinBrush, cx + shellW * 0.35f, cy - headSize / 2f, headSize, headSize);
        // Eye (little black dot)
        using var eyeBrush = new SolidBrush(Color.Black);
        g.FillEllipse(eyeBrush, cx + shellW * 0.55f, cy - headSize * 0.2f, headSize * 0.25f, headSize * 0.25f);

        // Draw shell (ellipse on top)
        g.FillEllipse(shellBrush, cx - shellW / 2f, cy - shellH / 2f, shellW, shellH);

        // Draw white outline around shell for readability on blue/brown
        g.DrawEllipse(outlinePen, cx - shellW / 2f, cy - shellH / 2f, shellW, shellH);
    }

    private void DrawStartColumnHighlight(Graphics g, PointF origin, SizeF cellSize)
    {
        var rect = GetCellRect(origin, cellSize, 0, _selectedStartColumn);
        using var pen = new Pen(Color.Gold, 3f);
        g.DrawRectangle(pen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
    }

    private void DrawFlowArrows(Graphics g, PointF origin, SizeF cellSize)
    {
        if (_currentStep?.Flows is not { Count: > 0 } flows || _animationFrame >= 4)
        {
            return;
        }

        double maxDiff = flows.Max(f => f.HeightDiff);
        if (maxDiff <= 1e-9)
        {
            return;
        }

        float fade = 1f - _animationFrame / 4f;

        foreach (var flow in flows)
        {
            if (flow.IsCliff)
            {
                continue;
            }

            var from = GetCellCenter(origin, cellSize, flow.FromRow, flow.FromCol);
            var to = GetCellCenter(origin, cellSize, flow.ToRow, flow.ToCol);
            float width = 1f + 8f * (float)(flow.HeightDiff / maxDiff);
            int alpha = (int)(220 * fade);
            using var pen = new Pen(Color.FromArgb(alpha, Color.DeepSkyBlue), width)
            {
                EndCap = LineCap.ArrowAnchor,
                StartCap = LineCap.Round,
            };
            g.DrawLine(pen, from, to);
        }
    }

    private void DrawCliffDrops(Graphics g, PointF origin, SizeF cellSize)
    {
        if (_currentStep?.Flows is not { Count: > 0 } flows || _animationFrame >= 4)
        {
            return;
        }

        float offset = 8f + _animationFrame * 6f;
        float fade = 1f - _animationFrame / 4f;
        int alpha = (int)(200 * fade);

        foreach (var flow in flows.Where(f => f.IsCliff))
        {
            var center = GetCellCenter(origin, cellSize, flow.FromRow, flow.FromCol);
            var rect = GetCellRect(origin, cellSize, flow.FromRow, flow.FromCol);
            PointF drop = flow.Direction switch
            {
                FlowDirection.North => new(center.X, rect.Top - offset),
                FlowDirection.East => new(rect.Right + offset, center.Y),
                FlowDirection.South => new(center.X, rect.Bottom + offset),
                FlowDirection.West => new(rect.Left - offset, center.Y),
                _ => center,
            };

            using var brush = new SolidBrush(Color.FromArgb(alpha, Color.RoyalBlue));
            g.FillEllipse(brush, drop.X - 4, drop.Y - 4, 8, 8);
            g.FillEllipse(brush, drop.X - 3, drop.Y + 6, 6, 6);
        }
    }

    private void DrawTortoiseFlash(Graphics g, PointF origin, SizeF cellSize)
    {
        if (!_showTortoiseFlash || _currentStep is null || _animationFrame >= 3)
        {
            return;
        }

        var rect = GetCellRect(origin, cellSize, _currentStep.Row, _currentStep.Col);
        int alpha = 160 - _animationFrame * 40;
        using var brush = new SolidBrush(Color.FromArgb(alpha, Color.Red));
        g.FillRectangle(brush, rect);
    }

    private (int Row, int Col)? HitTest(Point location)
    {
        if (_simulator is null)
        {
            return null;
        }

        int rows = _simulator.Rows;
        int cols = _simulator.Cols;
        var cellSize = GetCellSize(rows, cols);
        var origin = GetGridOrigin(cellSize, rows, cols);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var rect = GetCellRect(origin, cellSize, row, col);
                if (rect.Contains(location))
                {
                    return (row, col);
                }
            }
        }

        return null;
    }

    private SizeF GetCellSize(int rows, int cols)
    {
        if (IsLargeGridMode())
        {
            return new SizeF(LargeGridCellSize, LargeGridCellSize);
        }

        float availableWidth = Math.Max(1, Width - GridPadding * 2);
        float availableHeight = Math.Max(1, Height - GridPadding * 2);
        float cellWidth = availableWidth / cols;
        float cellHeight = availableHeight / rows;
        float size = Math.Min(cellWidth, cellHeight);
        return new SizeF(size, size);
    }

    private PointF GetGridOrigin(SizeF cellSize, int rows, int cols)
    {
        float gridWidth = cellSize.Width * cols;
        float gridHeight = cellSize.Height * rows;

        if (IsLargeGridMode())
        {
            return new PointF(GridPadding, GridPadding);
        }

        return new PointF((Width - gridWidth) / 2f, (Height - gridHeight) / 2f);
    }

    private static RectangleF GetCellRect(PointF origin, SizeF cellSize, int row, int col)
    {
        return new RectangleF(
            origin.X + col * cellSize.Width,
            origin.Y + row * cellSize.Height,
            cellSize.Width,
            cellSize.Height);
    }

    private static PointF GetCellCenter(PointF origin, SizeF cellSize, int row, int col)
    {
        var rect = GetCellRect(origin, cellSize, row, col);
        return new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
    }

    private static Color LerpColor(Color from, Color to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        int r = (int)(from.R + (to.R - from.R) * t);
        int g = (int)(from.G + (to.G - from.G) * t);
        int b = (int)(from.B + (to.B - from.B) * t);
        int a = (int)(from.A + (to.A - from.A) * t);
        return Color.FromArgb(a, r, g, b);
    }
}
