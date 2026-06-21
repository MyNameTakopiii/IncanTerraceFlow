namespace IncanTerraceFlow;

public static class SampleGridData
{
    public const double InitialWater = 5000.0;

    public static GridData CreateGridData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        if (File.Exists(path))
        {
            return GridDataParser.ParseFile(path);
        }

        return new GridData(
            "Sample Case 1",
            Elevation,
            WallData,
            AbsorptionRequired,
            HasTortoise);
    }

    public static IncanTerraceSimulator CreateSimulator(double initialWater = InitialWater)
    {
        var data = CreateGridData();
        return new IncanTerraceSimulator(
            initialWater,
            data.Elevation,
            data.WallData,
            data.AbsorptionRequired,
            data.HasTortoise);
    }

    public static double[,] Elevation { get; } =
    {
        { 1200, 1300, 1250, 1200, 1320, 1150 },
        { 1100, 1099, 1050, 1101, 1100, 1090 },
        { 1200,  999, 1000, 1007,  950,  960 },
        { 1152,  950,  800,  903,  917,  888 },
        {  820,  701,  770,  800,  799, 1800 },
        {  669,  688,  720,  612,  600,  627 },
        {  572,  550,  601,  520, 1600,  498 },
        {  420,  551,  392,  400,  200,  205 },
    };

    public static int[,] WallData { get; } =
    {
        { 9,  8,  8, 12,  9, 12 },
        { 0,  2,  0,  0,  0,  4 },
        { 1,  8,  0,  0,  0,  6 },
        { 1,  0,  2,  0,  0, 12 },
        { 3,  0,  8,  0,  0,  0 },
        { 9,  0,  0,  0,  2,  4 },
        { 1,  0,  0,  2,  8,  4 },
        { 1,  2,  0,  8,  0,  6 },
    };

    public static double[,] AbsorptionRequired { get; } =
    {
        { 50, 50,  70, 50, 50, 50 },
        { 70, 50,  60, 100, 60, 50 },
        { 55, 50,  51, 52, 53, 54 },
        { 57, 56,  55, 54, 53, 52 },
        { 47, 50, 150, 60, 50, 52 },
        { 43, 60,  40, 77, 62, 51 },
        { 77, 50,  39, 52, 80, 44 },
        { 50, 50,  50, 50, 50, 50 },
    };

    public static bool[,] HasTortoise { get; } =
    {
        { false, false, false, true,  false, false },
        { false, false, false, false, false, false },
        { false, false, false, false, true,  false },
        { false, false, true,  false, false, false },
        { false, false, false, true,  false, false },
        { false, false, false, false, true,  false },
        { false, true,  false, false, false, false },
        { false, false, false, false, false, false },
    };
}
