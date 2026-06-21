using IncanTerraceFlow;

namespace IncanTerraceFlow.Tests;

[TestFixture]
public class SampleCase2ComplexityTests
{
    private const double InitialWater = 5000.0;

    private static IncanTerraceSimulator CreateSimulator()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_2.csv");
        Assume.That(File.Exists(path), Is.True, "sample_case_2.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);
        return new IncanTerraceSimulator(
            InitialWater,
            data.Elevation,
            data.WallData,
            data.AbsorptionRequired,
            data.HasTortoise);
    }

    [Test]
    public void ParseCsv_LoadsSampleCase2Dimensions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_2.csv");
        Assume.That(File.Exists(path), Is.True, "sample_case_2.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);

        Assert.That(data.Name, Is.EqualTo("Sample Case 2"));
        Assert.That(data.Rows, Is.EqualTo(5));
        Assert.That(data.Cols, Is.EqualTo(5));
        Assert.That(data.Elevation[0, 2], Is.EqualTo(100).Within(1e-6));
        Assert.That(data.WallData[2, 2], Is.EqualTo(15));
        Assert.That(data.HasTortoise[2, 2], Is.True);
    }

    [Test]
    public void SampleCase2_ComplexityTest()
    {
        var simulator = CreateSimulator();

        var centerSession = simulator.BeginSimulation(2);
        centerSession.RunToCompletion();

        Assert.That(centerSession.CoveredCount, Is.EqualTo(5));
        Assert.That(centerSession.TotalWaterLoss, Is.EqualTo(4750.0).Within(1e-6));
        Assert.That(centerSession.OperationsProcessed, Is.EqualTo(5));
        Assert.That(centerSession.Walls[2, 2], Is.EqualTo(0), "Tortoise should break the walled center cell.");

        var edgeSession = simulator.BeginSimulation(0);
        edgeSession.RunToCompletion();

        Assert.That(edgeSession.CoveredCount, Is.EqualTo(13));
        Assert.That(edgeSession.TotalWaterLoss, Is.EqualTo(4350.0).Within(1e-6));
        Assert.That(edgeSession.OperationsProcessed, Is.EqualTo(35));

        var (bestColumn, maxCovered) = simulator.FindOptimalStartColumn();
        Assert.That(bestColumn, Is.EqualTo(0));
        Assert.That(maxCovered, Is.EqualTo(13));

        var benchmarks = simulator.BenchmarkAllColumns();
        Assert.That(benchmarks, Has.Count.EqualTo(5));
        Assert.That(benchmarks.Sum(result => result.OperationsProcessed), Is.EqualTo(105));
        Assert.That(benchmarks.Max(result => result.OperationsProcessed), Is.EqualTo(35));
        Assert.That(benchmarks.Min(result => result.OperationsProcessed), Is.EqualTo(5));
    }

    [Test]
    public void SampleCase2_TortoiseUnlocksCenterChannel()
    {
        var simulator = CreateSimulator();
        var session = simulator.BeginSimulation(2);
        session.RunToCompletion();

        Assert.That(session.Absorbed[2, 2], Is.EqualTo(50).Within(1e-6));
        Assert.That(session.Absorbed[4, 2], Is.EqualTo(50).Within(1e-6));
    }

    [Test]
    public void SampleCase2_EdgeColumnsCoverMoreFarmsThanCenter()
    {
        var simulator = CreateSimulator();

        var leftSession = simulator.BeginSimulation(0);
        leftSession.RunToCompletion();

        var centerSession = simulator.BeginSimulation(2);
        centerSession.RunToCompletion();

        Assert.That(leftSession.CoveredCount, Is.GreaterThan(centerSession.CoveredCount));
        Assert.That(leftSession.TotalWaterLoss, Is.LessThan(centerSession.TotalWaterLoss));
        Assert.That(leftSession.OperationsProcessed, Is.GreaterThan(centerSession.OperationsProcessed));
    }
}
