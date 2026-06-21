namespace IncanTerraceFlow;

public sealed record GridData(
    string Name,
    double[,] Elevation,
    int[,] WallData,
    double[,] AbsorptionRequired,
    bool[,] HasTortoise)
{
    public int Rows => Elevation.GetLength(0);

    public int Cols => Elevation.GetLength(1);
}
