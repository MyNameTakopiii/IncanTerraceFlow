namespace IncanTerraceFlow;

public enum FlowDirection
{
    North,
    East,
    South,
    West,
}

public sealed record FlowArrow(
    int FromRow,
    int FromCol,
    int ToRow,
    int ToCol,
    double Amount,
    double HeightDiff,
    bool IsCliff,
    FlowDirection Direction);

public sealed record SimulationStep(
    int Row,
    int Col,
    double WaterIn,
    double AbsorbedAmount,
    IReadOnlyList<FlowArrow> Flows,
    bool TortoiseWallBreak,
    int CoveredCount,
    double TotalWaterLoss);
