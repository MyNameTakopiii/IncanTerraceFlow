using IncanTerraceFlow;

namespace IncanTerraceFlow.Tests;

[TestFixture]
public class SampleCase1ComplexityTests
{
    private const double InitialWater = 5000.0;

    private static IncanTerraceSimulator CreateSimulator()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        Assume.That(File.Exists(path), Is.True, "sample_case_1.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);
        return new IncanTerraceSimulator(
            InitialWater,
            data.Elevation,
            data.WallData,
            data.AbsorptionRequired,
            data.HasTortoise);
    }

    [Test]
    public void ParseCsv_LoadsSampleCase1Dimensions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        Assume.That(File.Exists(path), Is.True, "sample_case_1.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);

        Assert.That(data.Name, Is.EqualTo("Sample Case 1"));
        Assert.That(data.Rows, Is.EqualTo(8));
        Assert.That(data.Cols, Is.EqualTo(6));
        Assert.That(data.Elevation[0, 0], Is.EqualTo(1200).Within(1e-6));
        Assert.That(data.WallData[0, 0], Is.EqualTo(9));
        Assert.That(data.HasTortoise[0, 3], Is.True);
    }

    [Test]
    public void SampleCase1_ComplexityTest()
    {
        var simulator = CreateSimulator();
        var benchmarks = simulator.BenchmarkAllColumns();
        string msg = string.Join("\n", benchmarks.Select(res => $"Col {res.StartColumn}: Covered={res.CoveredFarms}, Loss={res.WaterLoss}, Ops={res.OperationsProcessed}"));
        TestContext.Out.WriteLine(msg);
    }

    [Test]
    public void SampleCase1_TortoiseBreaksWalls()
    {
        var simulator = CreateSimulator();
        var session = simulator.BeginSimulation(3);
        session.RunToCompletion();

        // (0,3) has tortoise and starts with wall = 12, but remains 12 because it has other flow directions.
        Assert.That(session.Walls[0, 3], Is.EqualTo(12));

        // (3,2) has tortoise and starts with wall = 2, but becomes 0 because it runs out of flow targets.
        Assert.That(session.Walls[3, 2], Is.EqualTo(0));

        // (5,4) has tortoise and starts with wall = 2, but becomes 0 because it runs out of flow targets.
        Assert.That(session.Walls[5, 4], Is.EqualTo(0));
    }

    [Test]
    public void SampleCase1_BestColumnCoversMoreFarmsThanEdges()
    {
        var simulator = CreateSimulator();

        var bestSession = simulator.BeginSimulation(3);
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
