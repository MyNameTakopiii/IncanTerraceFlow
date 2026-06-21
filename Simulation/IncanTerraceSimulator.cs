using System.Diagnostics;

namespace IncanTerraceFlow;

/// <summary>
/// Simulates water flow across an M×N Incan terrace farm grid.
/// </summary>
public sealed class IncanTerraceSimulator
{
    private readonly double _initialWater;
    private readonly double[,] _elevation;
    private readonly int[,] _wallData;
    private readonly double[,] _absorptionRequired;
    private readonly bool[,] _hasTortoise;
    private readonly int _rows;
    private readonly int _cols;

    public IncanTerraceSimulator(
        double initialWater,
        double[,] elevation,
        int[,] wallData,
        double[,] absorptionRequired,
        bool[,] hasTortoise)
    {
        _initialWater = initialWater;
        _elevation = elevation;
        _wallData = wallData;
        _absorptionRequired = absorptionRequired;
        _hasTortoise = hasTortoise;
        _rows = elevation.GetLength(0);
        _cols = elevation.GetLength(1);

        ValidateDimensions(wallData, _rows, _cols, nameof(wallData));
        ValidateDimensions(absorptionRequired, _rows, _cols, nameof(absorptionRequired));
        ValidateDimensions(hasTortoise, _rows, _cols, nameof(hasTortoise));
    }

    public int Rows => _rows;

    public int Cols => _cols;

    public double InitialWater => _initialWater;

    public double[,] Elevation => _elevation;

    public int[,] WallData => _wallData;

    public double[,] AbsorptionRequired => _absorptionRequired;

    public bool[,] HasTortoise => _hasTortoise;

    /// <summary>
    /// Runs one simulation starting from column <paramref name="startCol"/> on row 0.
    /// Returns the number of farms that absorbed their full required volume.
    /// </summary>
    public int RunSimulation(int startCol)
    {
        return RunSimulationDetailed(startCol).CoveredCount;
    }

    /// <summary>
    /// Runs one simulation starting from column <paramref name="startCol"/> on row 0.
    /// Returns both the covered count and the detailed matrix of absorbed water.
    /// </summary>
    public (int CoveredCount, double[,] Absorbed) RunSimulationDetailed(int startCol)
    {
        var session = BeginSimulation(startCol);
        session.RunToCompletion();
        return (session.CoveredCount, (double[,])session.Absorbed.Clone());
    }

    /// <summary>
    /// Starts a steppable simulation from the given top-row column.
    /// </summary>
    public SimulationSession BeginSimulation(int startCol)
    {
        if (startCol < 0 || startCol >= _cols)
        {
            throw new ArgumentOutOfRangeException(nameof(startCol));
        }

        return new SimulationSession(
            _initialWater,
            _elevation,
            _wallData,
            _absorptionRequired,
            _hasTortoise,
            startCol);
    }

    /// <summary>
    /// Evaluates every top-row starting column and returns the one with the most covered farms.
    /// </summary>
    public (int BestStartColumn, int MaxCoveredFarms) FindOptimalStartColumn()
    {
        int bestColumn = 0;
        int maxCovered = int.MinValue;

        for (int column = 0; column < _cols; column++)
        {
            int covered = RunSimulation(column);
            if (covered > maxCovered)
            {
                maxCovered = covered;
                bestColumn = column;
            }
        }

        return (bestColumn, maxCovered);
    }

    /// <summary>
    /// Returns covered-farm and water-loss results for every top-row starting column.
    /// </summary>
    public IReadOnlyList<(int StartColumn, int CoveredFarms, double WaterLoss)> GetAllStartColumnResults()
    {
        return BenchmarkAllColumns()
            .Select(result => (result.StartColumn, result.CoveredFarms, result.WaterLoss))
            .ToList();
    }

    /// <summary>
    /// Benchmarks every top-row starting column with timing and operation counts.
    /// </summary>
    public IReadOnlyList<ColumnBenchmarkResult> BenchmarkAllColumns()
    {
        var results = new List<ColumnBenchmarkResult>(_cols);

        for (int column = 0; column < _cols; column++)
        {
            var session = BeginSimulation(column);
            var stopwatch = Stopwatch.StartNew();
            session.RunToCompletion();
            stopwatch.Stop();

            results.Add(new ColumnBenchmarkResult(
                column,
                session.CoveredCount,
                session.TotalWaterLoss,
                stopwatch.ElapsedMilliseconds,
                session.OperationsProcessed));
        }

        return results;
    }

    private static void ValidateDimensions<T>(T[,] array, int rows, int cols, string name)
    {
        if (array.GetLength(0) != rows || array.GetLength(1) != cols)
        {
            throw new ArgumentException($"Expected a {rows}x{cols} array.", name);
        }
    }
}
