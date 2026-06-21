using System.Linq;

namespace IncanTerraceFlow;

public sealed class SimulationSession
{
    private const double Epsilon = 1e-9;

    private static readonly (int Dr, int Dc, int WallBit, FlowDirection Direction)[] Directions =
    {
        (-1, 0, 8, FlowDirection.North),
        (0, 1, 4, FlowDirection.East),
        (1, 0, 2, FlowDirection.South),
        (0, -1, 1, FlowDirection.West),
    };

    private readonly double _initialWater;
    private readonly double[,] _elevation;
    private readonly double[,] _absorptionRequired;
    private readonly bool[,] _hasTortoise;
    private readonly int _rows;
    private readonly int _cols;

    private readonly int[,] _walls;
    private readonly double[,] _absorbed;
    private readonly int[,] _visitCount;
    private readonly Queue<CellState> _queue;
    private int _operationsProcessed;

    internal SimulationSession(
        double initialWater,
        double[,] elevation,
        int[,] wallData,
        double[,] absorptionRequired,
        bool[,] hasTortoise,
        int startCol)
    {
        _initialWater = initialWater;
        _elevation = elevation;
        _absorptionRequired = absorptionRequired;
        _hasTortoise = hasTortoise;
        _rows = elevation.GetLength(0);
        _cols = elevation.GetLength(1);
        _walls = (int[,])wallData.Clone();
        _absorbed = new double[_rows, _cols];
        _visitCount = new int[_rows, _cols];
        _queue = new Queue<CellState>();
        _queue.Enqueue(new CellState(0, startCol, _initialWater));
    }

    public bool IsComplete => _queue.Count == 0;

    public double[,] Absorbed => _absorbed;

    public int[,] Walls => _walls;

    public int[,] VisitCount => _visitCount;

    public double TotalWaterLoss
    {
        get
        {
            double absorbedByCovered = 0;
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    if (Math.Abs(_absorbed[r, c] - _absorptionRequired[r, c]) <= Epsilon)
                    {
                        absorbedByCovered += _absorptionRequired[r, c];
                    }
                }
            }
            return _initialWater - absorbedByCovered;
        }
    }

    public int OperationsProcessed => _operationsProcessed;

    public int CoveredCount => CountCoveredFarms();

    public SimulationStep? StepNext()
    {
        while (_queue.Count > 0)
        {
            var (row, col, water) = _queue.Dequeue();
            _operationsProcessed++;

            if (water <= Epsilon)
            {
                continue;
            }

            _visitCount[row, col]++;

            double absorbedAmount = 0;
            double remainingNeed = _absorptionRequired[row, col] - _absorbed[row, col];
            if (remainingNeed > Epsilon)
            {
                absorbedAmount = Math.Min(water, remainingNeed);
                _absorbed[row, col] += absorbedAmount;
                water -= absorbedAmount;
            }

            if (water <= Epsilon)
            {
                return new SimulationStep(
                    row,
                    col,
                    water + absorbedAmount,
                    absorbedAmount,
                    Array.Empty<FlowArrow>(),
                    false,
                    CoveredCount,
                    TotalWaterLoss);
            }

            var targets = FindFlowTargets(row, col);
            bool tortoiseBreak = false;

            if (targets.Count == 0 && _hasTortoise[row, col])
            {
                _walls[row, col] = 0;
                tortoiseBreak = true;
                targets = FindFlowTargets(row, col);
            }

            if (targets.Count == 0)
            {
                return new SimulationStep(
                    row,
                    col,
                    water + absorbedAmount,
                    absorbedAmount,
                    Array.Empty<FlowArrow>(),
                    tortoiseBreak,
                    CoveredCount,
                    TotalWaterLoss);
            }

            targets = targets.OrderBy(t => t.HeightDiff).ToList();

            double totalHeightDiff = 0;
            foreach (var target in targets)
            {
                totalHeightDiff += target.HeightDiff;
            }

            if (totalHeightDiff <= Epsilon)
            {
                return new SimulationStep(
                    row,
                    col,
                    water + absorbedAmount,
                    absorbedAmount,
                    Array.Empty<FlowArrow>(),
                    tortoiseBreak,
                    CoveredCount,
                    TotalWaterLoss);
            }

            var flows = new List<FlowArrow>(targets.Count);
            foreach (var target in targets)
            {
                double share = water * (target.HeightDiff / totalHeightDiff);
                if (share <= Epsilon)
                {
                    continue;
                }

                flows.Add(new FlowArrow(
                    row,
                    col,
                    target.Row,
                    target.Col,
                    share,
                    target.HeightDiff,
                    target.IsCliff,
                    target.Direction));

                if (!target.IsCliff)
                {
                    _queue.Enqueue(new CellState(target.Row, target.Col, share));
                }
            }

            return new SimulationStep(
                row,
                col,
                water + absorbedAmount,
                absorbedAmount,
                flows,
                tortoiseBreak,
                CoveredCount,
                TotalWaterLoss);
        }

        return null;
    }

    public void RunToCompletion()
    {
        while (!IsComplete)
        {
            StepNext();
        }
    }

    private List<FlowTarget> FindFlowTargets(int row, int col)
    {
        var targets = new List<FlowTarget>(4);
        double currentHeight = _elevation[row, col];

        foreach (var (dr, dc, wallBit, direction) in Directions)
        {
            if ((_walls[row, col] & wallBit) != 0)
            {
                continue;
            }

            int neighborRow = row + dr;
            int neighborCol = col + dc;
            bool outOfBounds = neighborRow < 0 || neighborRow >= _rows ||
                               neighborCol < 0 || neighborCol >= _cols;

            if (outOfBounds)
            {
                double cliffDiff = currentHeight;
                if (cliffDiff > Epsilon)
                {
                    targets.Add(new FlowTarget(neighborRow, neighborCol, cliffDiff, true, direction));
                }

                continue;
            }

            double neighborHeight = _elevation[neighborRow, neighborCol];
            if (currentHeight > neighborHeight + Epsilon)
            {
                targets.Add(new FlowTarget(
                    neighborRow,
                    neighborCol,
                    currentHeight - neighborHeight,
                    false,
                    direction));
            }
        }

        return targets;
    }

    private int CountCoveredFarms()
    {
        int covered = 0;

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                if (Math.Abs(_absorbed[row, col] - _absorptionRequired[row, col]) <= Epsilon)
                {
                    covered++;
                }
            }
        }

        return covered;
    }

    private readonly record struct CellState(int Row, int Col, double Water);

    private readonly record struct FlowTarget(
        int Row,
        int Col,
        double HeightDiff,
        bool IsCliff,
        FlowDirection Direction);
}
