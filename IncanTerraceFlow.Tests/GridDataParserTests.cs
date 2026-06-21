using IncanTerraceFlow;

namespace IncanTerraceFlow.Tests;

[TestFixture]
public class GridDataParserTests
{
    [Test]
    public void ParseCsv_LoadsSampleCaseDimensions()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        Assume.That(File.Exists(path), Is.True, "sample_case_1.csv must be copied to test output.");

        var data = GridDataParser.ParseFile(path);

        Assert.That(data.Rows, Is.EqualTo(8));
        Assert.That(data.Cols, Is.EqualTo(6));
        Assert.That(data.Elevation[0, 0], Is.EqualTo(1200).Within(1e-6));
        Assert.That(data.WallData[0, 0], Is.EqualTo(9));
        Assert.That(data.AbsorptionRequired[0, 2], Is.EqualTo(70).Within(1e-6));
        Assert.That(data.HasTortoise[0, 3], Is.True);
    }

    [Test]
    public void ParseCsv_InMemoryMiniCase()
    {
        string[] lines =
        {
            "# Mini Case",
            "ROWS,3",
            "COLS,3",
            "ELEVATION",
            "100,100,100",
            "90,80,70",
            "60,50,40",
            "WALLS",
            "11,10,14",
            "11,0,14",
            "11,10,14",
            "ABSORPTION",
            "50,50,50",
            "50,50,50",
            "50,50,50",
            "TORTOISE",
            "0,0,0",
            "0,1,0",
            "0,0,0",
        };

        var data = GridDataParser.ParseCsv(lines, "mini");

        Assert.That(data.Name, Is.EqualTo("Mini Case"));
        Assert.That(data.HasTortoise[1, 1], Is.True);
        Assert.That(data.Elevation[2, 2], Is.EqualTo(40).Within(1e-6));
    }

    [Test]
    public void ParseCsv_MismatchedDimensions_Throws()
    {
        string[] lines =
        {
            "ROWS,2",
            "COLS,2",
            "ELEVATION",
            "1,2",
            "3,4",
            "WALLS",
            "0,0",
            "ABSORPTION",
            "1,2",
            "3,4",
            "TORTOISE",
            "0,0",
            "0,0",
            "0,0",
        };

        Assert.Throws<FormatException>(() => GridDataParser.ParseCsv(lines, "bad"));
    }

    [Test]
    public void ParseJson_LoadsMatrices()
    {
        const string json = """
            {
              "name": "Hidden Case A",
              "elevation": [[100.0, 90.0], [80.0, 70.0]],
              "walls": [[11, 14], [11, 14]],
              "absorption": [[50, 50], [50, 50]],
              "tortoise": [[false, true], [false, false]]
            }
            """;

        var data = GridDataParser.ParseJson(json, "fallback");

        Assert.That(data.Name, Is.EqualTo("Hidden Case A"));
        Assert.That(data.Rows, Is.EqualTo(2));
        Assert.That(data.Cols, Is.EqualTo(2));
        Assert.That(data.HasTortoise[0, 1], Is.True);
    }

    [Test]
    public void ParseTxt_ArrayFormat_LoadsSampleCase1()
    {
        string arrayTxt = """
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
            """;

        string csvPath = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        Assume.That(File.Exists(csvPath), Is.True, "sample_case_1.csv must be copied to test output.");

        string txtPath = Path.Combine(Path.GetTempPath(), $"sample_case_1_{Guid.NewGuid():N}.txt");
        File.WriteAllText(txtPath, arrayTxt);

        try
        {
            var txtData = GridDataParser.ParseFile(txtPath);
            var csvData = GridDataParser.ParseFile(csvPath);

            Assert.That(txtData.Name, Is.EqualTo(Path.GetFileNameWithoutExtension(txtPath)));
            Assert.That(txtData.Rows, Is.EqualTo(8));
            Assert.That(txtData.Cols, Is.EqualTo(6));
            Assert.That(txtData.Elevation[0, 0], Is.EqualTo(csvData.Elevation[0, 0]).Within(1e-6));
            Assert.That(txtData.WallData[0, 0], Is.EqualTo(csvData.WallData[0, 0]));
            Assert.That(txtData.AbsorptionRequired[0, 2], Is.EqualTo(csvData.AbsorptionRequired[0, 2]).Within(1e-6));
            Assert.That(txtData.HasTortoise[0, 3], Is.EqualTo(csvData.HasTortoise[0, 3]));
        }
        finally
        {
            File.Delete(txtPath);
        }
    }

    [Test]
    public void ParseTxt_CsvFormat_StillWorks()
    {
        string csvTxt = """
            # Mini Case
            ROWS,3
            COLS,3
            ELEVATION
            100,100,100
            90,80,70
            60,50,40
            WALLS
            11,10,14
            11,0,14
            11,10,14
            ABSORPTION
            50,50,50
            50,50,50
            50,50,50
            TORTOISE
            0,0,0
            0,1,0
            0,0,0
            """;

        string txtPath = Path.Combine(Path.GetTempPath(), $"mini_case_{Guid.NewGuid():N}.txt");
        File.WriteAllText(txtPath, csvTxt);

        try
        {
            var data = GridDataParser.ParseFile(txtPath);

            Assert.That(data.Name, Is.EqualTo("Mini Case"));
            Assert.That(data.Rows, Is.EqualTo(3));
            Assert.That(data.Cols, Is.EqualTo(3));
            Assert.That(data.HasTortoise[1, 1], Is.True);
            Assert.That(data.Elevation[2, 2], Is.EqualTo(40).Within(1e-6));
        }
        finally
        {
            File.Delete(txtPath);
        }
    }

    [Test]
    public void ParseTxt_BuiltInSampleCase1_MatchesCsv()
    {
        string txtPath = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.txt");
        string csvPath = Path.Combine(AppContext.BaseDirectory, "TestCases", "sample_case_1.csv");
        Assume.That(File.Exists(txtPath), Is.True, "sample_case_1.txt must be copied to test output.");
        Assume.That(File.Exists(csvPath), Is.True, "sample_case_1.csv must be copied to test output.");

        var txtData = GridDataParser.ParseFile(txtPath);
        var csvData = GridDataParser.ParseFile(csvPath);

        Assert.That(txtData.Rows, Is.EqualTo(csvData.Rows));
        Assert.That(txtData.Cols, Is.EqualTo(csvData.Cols));
        Assert.That(txtData.Elevation[7, 5], Is.EqualTo(csvData.Elevation[7, 5]).Within(1e-6));
        Assert.That(txtData.WallData[3, 5], Is.EqualTo(csvData.WallData[3, 5]));
        Assert.That(txtData.HasTortoise[6, 1], Is.EqualTo(csvData.HasTortoise[6, 1]));
    }
}
