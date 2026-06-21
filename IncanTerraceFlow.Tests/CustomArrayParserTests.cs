using System;
using NUnit.Framework;
using IncanTerraceFlow;

namespace IncanTerraceFlow.Tests;

[TestFixture]
public class CustomArrayParserTests
{
    [Test]
    public void Parse_ValidJavascriptStyleArrays_ParsesCorrectly()
    {
        string input = @"
            // Elevation
            const array1 = [
              [1200.0, 1300.0],
              [1100.0, 1099.0]
            ];

            /* Wall Data */
            const array2 = [
              [9, 8],
              [0, 2]
            ];

            const array3 = [
              [50, 70],
              [70, 50]
            ];

            const array4 = [
              [false, true],
              [false, false]
            ];
        ";

        var data = CustomArrayParser.Parse(input, "JS Test");

        Assert.That(data.Name, Is.EqualTo("JS Test"));
        Assert.That(data.Rows, Is.EqualTo(2));
        Assert.That(data.Cols, Is.EqualTo(2));

        Assert.That(data.Elevation[0, 0], Is.EqualTo(1200.0).Within(1e-6));
        Assert.That(data.Elevation[1, 1], Is.EqualTo(1099.0).Within(1e-6));

        Assert.That(data.WallData[0, 0], Is.EqualTo(9));
        Assert.That(data.WallData[1, 1], Is.EqualTo(2));

        Assert.That(data.AbsorptionRequired[0, 1], Is.EqualTo(70.0).Within(1e-6));

        Assert.That(data.HasTortoise[0, 0], Is.False);
        Assert.That(data.HasTortoise[0, 1], Is.True);
    }

    [Test]
    public void Parse_ValidCSharpStyleArrays_ParsesCorrectly()
    {
        string input = @"
            double[,] elevation = {
              { 100, 200 },
              { 300, 400 }
            };
            int[,] walls = {
              { 1, 2 },
              { 3, 4 }
            };
            double[,] absorb = {
              { 10.5, 20.5 },
              { 30.5, 40.5 }
            };
            bool[,] tortoise = {
              { true, false },
              { false, true }
            };
        ";

        var data = CustomArrayParser.Parse(input, "C# Test");

        Assert.That(data.Rows, Is.EqualTo(2));
        Assert.That(data.Cols, Is.EqualTo(2));
        Assert.That(data.Elevation[0, 0], Is.EqualTo(100.0).Within(1e-6));
        Assert.That(data.WallData[1, 0], Is.EqualTo(3));
        Assert.That(data.AbsorptionRequired[1, 1], Is.EqualTo(40.5).Within(1e-6));
        Assert.That(data.HasTortoise[0, 0], Is.True);
    }

    [Test]
    public void Parse_DimensionMismatch_ThrowsFormatException()
    {
        string input = @"
            const array1 = [[1, 2], [3, 4]];
            const array2 = [[1, 2, 3], [4, 5, 6]];
            const array3 = [[1, 2], [3, 4]];
            const array4 = [[false, false], [false, false]];
        ";

        var ex = Assert.Throws<FormatException>(() => CustomArrayParser.Parse(input));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("Dimension mismatch").Or.Contain("dimension"));
    }

    [Test]
    public void Parse_TooFewArrays_ThrowsFormatException()
    {
        string input = @"
            const array1 = [[1, 2], [3, 4]];
            const array2 = [[1, 2], [3, 4]];
            const array3 = [[1, 2], [3, 4]];
        ";

        var ex = Assert.Throws<FormatException>(() => CustomArrayParser.Parse(input));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("Expected exactly 4 arrays"));
    }

    [Test]
    public void Parse_InvalidValues_ThrowsFormatException()
    {
        string input = @"
            const array1 = [[1, 2], [3, 4]];
            const array2 = [[1, 2], [3, 4]];
            const array3 = [[1, 2], [3, 4]];
            const array4 = [[false, 'maybe'], [false, false]];
        ";

        Assert.Throws<FormatException>(() => CustomArrayParser.Parse(input));
    }

    [Test]
    public void Parse_UserRequestArrays_ParsesCorrectly()
    {
        string input = @"
const array1 = [
  [1200, 1300, 1250, 1200, 1320, 1150],
  [1100, 1099, 1050, 1101, 1100, 1090],
  [1200,  999, 1000, 1007,  950,  960],
  [1152,  950,  800,  903,  917,  888],
  [ 820,  701,  770,  800,  799, 1800],
  [ 669,  688,  720,  612,  600,  627],
  [ 572,  550,  601,  520, 1600,  498],
  [ 420,  551,  392,  400,  200,  205]
];

const array2 = [
  [9, 8, 8, 12, 9, 12],
  [0, 2, 0,  0, 0,  4],
  [1, 8, 0,  0, 0,  6],
  [1, 0, 2,  0, 0,  12],
  [3, 0, 8,  0, 0,  0],
  [9, 0, 0,  0, 2,  4],
  [1, 0, 0,  2, 8,  4], 
  [1, 2, 0,  8, 0,  6]
];

const array3 = [
  [50, 50,  70, 50, 50, 50],
  [70, 50,  60,100, 60, 50],
  [55, 50,  51, 52, 53, 54],
  [57, 56,  55, 54, 53, 52],
  [47, 50, 150, 60, 50, 52],
  [43, 60,  40, 77, 62, 51],
  [77, 50,  39, 52, 80, 44],
  [50, 50,  50, 50, 50, 50]
];

const array4 = [
  [false, false, false,  true, false, false],
  [false, false, false, false, false, false],
  [false, false, false, false,  true, false],
  [false, false,  true, false, false, false],
  [false, false, false,  true, false, false],
  [false, false, false, false,  true, false],
  [false,  true, false, false, false, false],
  [false, false, false, false, false, false]
];
        ";

        var data = CustomArrayParser.Parse(input, "User Case");

        Assert.That(data.Rows, Is.EqualTo(8));
        Assert.That(data.Cols, Is.EqualTo(6));
        Assert.That(data.Elevation[0, 0], Is.EqualTo(1200.0).Within(1e-6));
        Assert.That(data.WallData[0, 0], Is.EqualTo(9));
        Assert.That(data.AbsorptionRequired[0, 2], Is.EqualTo(70.0).Within(1e-6));
        Assert.That(data.HasTortoise[0, 3], Is.True);
    }

    [Test]
    public void Parse_UserRequestArrays_MatchesSampleCase1CsvBenchmarks()
    {
        const double initialWater = 5000.0;
        string pastedInput = @"
const array1 = [
  [1200, 1300, 1250, 1200, 1320, 1150],
  [1100, 1099, 1050, 1101, 1100, 1090],
  [1200,  999, 1000, 1007,  950,  960],
  [1152,  950,  800,  903,  917,  888],
  [ 820,  701,  770,  800,  799, 1800],
  [ 669,  688,  720,  612,  600,  627],
  [ 572,  550,  601,  520, 1600,  498],
  [ 420,  551,  392,  400,  200,  205]
];

const array2 = [
  [9, 8, 8, 12, 9, 12],
  [0, 2, 0,  0, 0,  4],
  [1, 8, 0,  0, 0,  6],
  [1, 0, 2,  0, 0,  12],
  [3, 0, 8,  0, 0,  0],
  [9, 0, 0,  0, 2,  4],
  [1, 0, 0,  2, 8,  4],
  [1, 2, 0,  8, 0,  6]
];

const array3 = [
  [50, 50,  70, 50, 50, 50],
  [70, 50,  60,100, 60, 50],
  [55, 50,  51, 52, 53, 54],
  [57, 56,  55, 54, 53, 52],
  [47, 50, 150, 60, 50, 52],
  [43, 60,  40, 77, 62, 51],
  [77, 50,  39, 52, 80, 44],
  [50, 50,  50, 50, 50, 50]
];

const array4 = [
  [false, false, false,  true, false, false],
  [false, false, false, false, false, false],
  [false, false, false, false,  true, false],
  [false, false,  true, false, false, false],
  [false, false, false,  true, false, false],
  [false, false, false, false,  true, false],
  [false,  true, false, false, false, false],
  [false, false, false, false, false, false]
];
        ";

        string csvPath = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        Assume.That(File.Exists(csvPath), Is.True, "sample_case_1.csv must be copied to test output.");

        var pastedData = CustomArrayParser.Parse(pastedInput, "Pasted");
        var csvData = GridDataParser.ParseFile(csvPath);

        var pastedSimulator = new IncanTerraceSimulator(
            initialWater,
            pastedData.Elevation,
            pastedData.WallData,
            pastedData.AbsorptionRequired,
            pastedData.HasTortoise);

        var csvSimulator = new IncanTerraceSimulator(
            initialWater,
            csvData.Elevation,
            csvData.WallData,
            csvData.AbsorptionRequired,
            csvData.HasTortoise);

        var pastedResults = pastedSimulator.BenchmarkAllColumns();
        var csvResults = csvSimulator.BenchmarkAllColumns();

        Assert.That(pastedResults, Has.Count.EqualTo(csvResults.Count));

        for (int i = 0; i < pastedResults.Count; i++)
        {
            Assert.That(pastedResults[i].StartColumn, Is.EqualTo(csvResults[i].StartColumn));
            Assert.That(pastedResults[i].CoveredFarms, Is.EqualTo(csvResults[i].CoveredFarms));
            Assert.That(pastedResults[i].WaterLoss, Is.EqualTo(csvResults[i].WaterLoss).Within(1e-6));
            Assert.That(pastedResults[i].OperationsProcessed, Is.EqualTo(csvResults[i].OperationsProcessed));
        }

        var (pastedBestColumn, pastedMaxCovered) = pastedSimulator.FindOptimalStartColumn();
        var (csvBestColumn, csvMaxCovered) = csvSimulator.FindOptimalStartColumn();

        Assert.That(pastedBestColumn, Is.EqualTo(csvBestColumn));
        Assert.That(pastedMaxCovered, Is.EqualTo(csvMaxCovered));
        Assert.That(pastedMaxCovered, Is.EqualTo(25));
        Assert.That(pastedBestColumn, Is.EqualTo(3));
    }
}
