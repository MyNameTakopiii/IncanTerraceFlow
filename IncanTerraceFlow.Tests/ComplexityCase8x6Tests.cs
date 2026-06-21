using IncanTerraceFlow;

namespace IncanTerraceFlow.Tests;

[TestFixture]
public class ComplexityCase8x6Tests
{
    private const double InitialWater = 5000.0;

    private static IncanTerraceSimulator CreateSimulator()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "complexity_case_8x6.csv");
        Assume.That(File.Exists(path), Is.True, "complexity_case_8x6.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);
        return new IncanTerraceSimulator(
            InitialWater,
            data.Elevation,
            data.WallData,
            data.AbsorptionRequired,
            data.HasTortoise);
    }

    [Test]
    public void ParseCsv_LoadsComplexityCase8x6Dimensions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "complexity_case_8x6.csv");
        Assume.That(File.Exists(path), Is.True, "complexity_case_8x6.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);

        Assert.That(data.Name, Is.EqualTo("Complexity Case 8x6"));
        Assert.That(data.Rows, Is.EqualTo(8));
        Assert.That(data.Cols, Is.EqualTo(6));
        Assert.That(data.Elevation[0, 0], Is.EqualTo(1500).Within(1e-6));
        Assert.That(data.WallData[0, 0], Is.EqualTo(9));
        Assert.That(data.HasTortoise[2, 1], Is.True);
    }

    [Test]
    public void ComplexityCase8x6_ComplexityTest()
    {
        var simulator = CreateSimulator();
        var benchmarks = simulator.BenchmarkAllColumns();
        string msg = string.Join("\n", benchmarks.Select(res => $"Col {res.StartColumn}: Covered={res.CoveredFarms}, Loss={res.WaterLoss}, Ops={res.OperationsProcessed}"));
        TestContext.Out.WriteLine(msg);
    }

    [Test]
    public void ComplexityCase8x6_TortoiseBreaksWalls()
    {
        var simulator = CreateSimulator();
        var session = simulator.BeginSimulation(2);
        session.RunToCompletion();

        // Verify that all 6 tortoises triggered and broke their walls
        Assert.That(session.Walls[2, 1], Is.EqualTo(0));
        Assert.That(session.Walls[2, 3], Is.EqualTo(0));
        Assert.That(session.Walls[4, 2], Is.EqualTo(0));
        Assert.That(session.Walls[4, 4], Is.EqualTo(0));
        Assert.That(session.Walls[6, 1], Is.EqualTo(0));
        Assert.That(session.Walls[6, 3], Is.EqualTo(0));
    }

    [Test]
    public void ComplexityCase8x6_BestColumnCoversMoreFarmsThanEdges()
    {
        var simulator = CreateSimulator();

        var bestSession = simulator.BeginSimulation(2);
        bestSession.RunToCompletion();

        var leftEdgeSession = simulator.BeginSimulation(0);
        leftEdgeSession.RunToCompletion();

        var rightEdgeSession = simulator.BeginSimulation(5);
        rightEdgeSession.RunToCompletion();

        Assert.That(bestSession.CoveredCount, Is.GreaterThan(leftEdgeSession.CoveredCount));
        Assert.That(bestSession.CoveredCount, Is.GreaterThan(rightEdgeSession.CoveredCount));
        Assert.That(bestSession.TotalWaterLoss, Is.LessThan(leftEdgeSession.TotalWaterLoss));
        Assert.That(bestSession.TotalWaterLoss, Is.LessThan(rightEdgeSession.TotalWaterLoss));
    }
}
