using NUnit.Framework;
using IncanTerraceFlow;

namespace IncanTerraceFlow.Tests
{
    [TestFixture]
    public class IncanTerraceSimulatorTests
    {
        [Test]
        public void AbsorptionAndFlowTest()
        {
            // Create a simple 1x2 grid.
            // Cell(0,0) has elevation 100m and requires 50 units.
            // Cell(0,1) has elevation 90m and requires 50 units.
            // 200 units of water start at (0,0).
            // To prevent water from spilling off grid boundary cliffs:
            // - Cell(0,0) has North(8), South(2), West(1) walls active: 8+2+1 = 11.
            // - Cell(0,1) has North(8), South(2), East(4) walls active: 8+2+4 = 14.
            double initialWater = 200.0;
            double[,] elevation = { { 100.0, 90.0 } };
            int[,] wallData = { { 11, 14 } };
            double[,] absorptionRequired = { { 50.0, 50.0 } };
            bool[,] hasTortoise = { { false, false } };

            var simulator = new IncanTerraceSimulator(
                initialWater,
                elevation,
                wallData,
                absorptionRequired,
                hasTortoise
            );

            var (_, absorbed) = simulator.RunSimulationDetailed(0);

            // Verify Cell(0,0) absorbs exactly 50 units, and Cell(0,1) absorbs exactly 50 units.
            Assert.AreEqual(50.0, absorbed[0, 0], 1e-6);
            Assert.AreEqual(50.0, absorbed[0, 1], 1e-6);
        }

        [Test]
        public void ProportionalSplittingTest()
        {
            // Create a T-shaped grid (1x3 representation) where Cell(0,1) is the source (height 100m).
            // It has two lower neighbors: Cell(0,0) (height 90m) and Cell(0,2) (height 80m).
            // 110 units of water start at (0,1).
            // To prevent water spilling off boundary cliffs:
            // - Cell(0,0): North(8), South(2), West(1) walls: 8+2+1 = 11.
            // - Cell(0,1): North(8), South(2) walls: 8+2 = 10.
            // - Cell(0,2): North(8), South(2), East(4) walls: 8+2+4 = 14.
            // Cell(0,1) requires 50 units. Remaining 60 units split in 1:2 ratio:
            // Cell(0,0) gets 20 units. Cell(0,2) gets 40 units.
            double initialWater = 110.0;
            double[,] elevation = { { 90.0, 100.0, 80.0 } };
            int[,] wallData = { { 11, 10, 14 } };
            double[,] absorptionRequired = { { 50.0, 50.0, 50.0 } };
            bool[,] hasTortoise = { { false, false, false } };

            var simulator = new IncanTerraceSimulator(
                initialWater,
                elevation,
                wallData,
                absorptionRequired,
                hasTortoise
            );

            var (_, absorbed) = simulator.RunSimulationDetailed(1);

            // Verify Cell(0,1) absorbs exactly 50 units
            Assert.AreEqual(50.0, absorbed[0, 1], 1e-6);
            
            // Verify remaining 60 units are split 1:2 (20 units vs 40 units)
            Assert.AreEqual(20.0, absorbed[0, 0], 1e-6);
            Assert.AreEqual(40.0, absorbed[0, 2], 1e-6);
        }

        [Test]
        public void TortoiseWallBreakingTest_WithTortoise()
        {
            // Cell(0,0) is surrounded by walls (Bitmask 15) but has Tortoise Flag set to true.
            // Cell(0,1) is at a lower elevation.
            // When walls are destroyed (walls[0,0] set to 0), the boundary cliffs (N, S, W)
            // also lose their walls. This means water splits between 4 directions:
            // - N, S, W cliffs: height diff 100 each.
            // - E neighbor: height diff 10.
            // - Total height diff = 310.
            // To ensure Cell(0,1) gets its full 50 units, we start with 1600 units of water.
            // Cell(0,0) absorbs 50, leaving 1550.
            // Flow to Cell(0,1) = 1550 * (10 / 310) = 50 units.
            double initialWater = 1600.0;
            double[,] elevation = { { 100.0, 90.0 } };
            int[,] wallData = { { 15, 14 } };
            double[,] absorptionRequired = { { 50.0, 50.0 } };
            bool[,] hasTortoise = { { true, false } };

            var simulator = new IncanTerraceSimulator(
                initialWater,
                elevation,
                wallData,
                absorptionRequired,
                hasTortoise
            );

            var (_, absorbed) = simulator.RunSimulationDetailed(0);

            // Verify Cell(0,0) absorbs exactly 50 units, and Cell(0,1) absorbs its required 50 units because walls were broken
            Assert.AreEqual(50.0, absorbed[0, 0], 1e-6);
            Assert.AreEqual(50.0, absorbed[0, 1], 1e-6);
        }

        [Test]
        public void TortoiseWallBreakingTest_NoTortoise()
        {
            // Negative control: Cell(0,0) is surrounded by walls (Bitmask 15) and has Tortoise Flag set to false.
            // Cell(0,1) is at a lower elevation.
            // 1600 units of water start at (0,0).
            // Walls should NOT be destroyed, trapping water, leaving Cell(0,1) with 0 units absorbed.
            double initialWater = 1600.0;
            double[,] elevation = { { 100.0, 90.0 } };
            int[,] wallData = { { 15, 14 } };
            double[,] absorptionRequired = { { 50.0, 50.0 } };
            bool[,] hasTortoise = { { false, false } };

            var simulator = new IncanTerraceSimulator(
                initialWater,
                elevation,
                wallData,
                absorptionRequired,
                hasTortoise
            );

            var (_, absorbed) = simulator.RunSimulationDetailed(0);

            // Verify Cell(0,0) absorbs exactly 50 units, but Cell(0,1) absorbs 0 units because water was trapped
            Assert.AreEqual(50.0, absorbed[0, 0], 1e-6);
            Assert.AreEqual(0.0, absorbed[0, 1], 1e-6);
        }
    }
}
