namespace IncanTerraceFlow;

public sealed record ColumnBenchmarkResult(
    int StartColumn,
    int CoveredFarms,
    double WaterLoss,
    long ElapsedMs,
    int OperationsProcessed);
