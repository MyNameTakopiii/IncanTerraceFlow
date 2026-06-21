using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace IncanTerraceFlow;

public static class GridDataParser
{
    private enum Section
    {
        None,
        Elevation,
        Walls,
        Absorption,
        Tortoise,
    }

    public static GridData ParseFile(string path)
    {
        string content = File.ReadAllText(path);
        string name = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".json" => ParseJson(content, name),
            ".txt" when LooksLikeCustomArrayFormat(content) => CustomArrayParser.Parse(content, name),
            _ => ParseCsv(content.Split('\n'), name),
        };
    }

    private static bool LooksLikeCustomArrayFormat(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        if (Regex.IsMatch(content, @"(?m)^\s*(ELEVATION|WALLS|ABSORPTION|TORTOISE)\s*$", RegexOptions.IgnoreCase))
        {
            return false;
        }

        return Regex.IsMatch(content, @"const\s+array\d", RegexOptions.IgnoreCase)
            || Regex.IsMatch(content, @"const\s+\w+\s*=\s*\[", RegexOptions.IgnoreCase)
            || Regex.IsMatch(content, @"\[\s*\[");
    }

    public static GridData ParseCsv(IReadOnlyList<string> lines, string defaultName)
    {
        string name = defaultName;
        int? explicitRows = null;
        int? explicitCols = null;
        var section = Section.None;
        var elevationRows = new List<double[]>();
        var wallRows = new List<int[]>();
        var absorptionRows = new List<double[]>();
        var tortoiseRows = new List<bool[]>();

        for (int i = 0; i < lines.Count; i++)
        {
            string rawLine = lines[i];
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                if (line.StartsWith("# ") && section == Section.None && !line.Contains(','))
                {
                    name = line[2..].Trim();
                }

                continue;
            }

            if (TryParseKeyValue(line, out string key, out string value))
            {
                switch (key.ToUpperInvariant())
                {
                    case "ROWS":
                        explicitRows = ParsePositiveInt(value, i + 1, "ROWS");
                        continue;
                    case "COLS":
                        explicitCols = ParsePositiveInt(value, i + 1, "COLS");
                        continue;
                }
            }

            switch (line.ToUpperInvariant())
            {
                case "ELEVATION":
                    section = Section.Elevation;
                    continue;
                case "WALLS":
                    section = Section.Walls;
                    continue;
                case "ABSORPTION":
                    section = Section.Absorption;
                    continue;
                case "TORTOISE":
                    section = Section.Tortoise;
                    continue;
            }

            string[] parts = line.Split(',');
            switch (section)
            {
                case Section.Elevation:
                    elevationRows.Add(ParseDoubleRow(parts, i + 1));
                    break;
                case Section.Walls:
                    wallRows.Add(ParseIntRow(parts, i + 1));
                    break;
                case Section.Absorption:
                    absorptionRows.Add(ParseDoubleRow(parts, i + 1));
                    break;
                case Section.Tortoise:
                    tortoiseRows.Add(ParseBoolRow(parts, i + 1));
                    break;
                default:
                    throw new FormatException($"Unexpected data on line {i + 1}. Expected a section header (ELEVATION, WALLS, ABSORPTION, TORTOISE).");
            }
        }

        if (elevationRows.Count == 0)
        {
            throw new FormatException("Missing ELEVATION section.");
        }

        int rows = explicitRows ?? elevationRows.Count;
        int cols = explicitCols ?? elevationRows[0].Length;

        return new GridData(
            name,
            ToMatrix(elevationRows, rows, cols, "ELEVATION"),
            ToMatrix(wallRows, rows, cols, "WALLS"),
            ToMatrix(absorptionRows, rows, cols, "ABSORPTION"),
            ToMatrix(tortoiseRows, rows, cols, "TORTOISE"));
    }

    public static GridData ParseJson(string json, string defaultName)
    {
        var dto = JsonSerializer.Deserialize<JsonGridDto>(json, JsonOptions)
            ?? throw new FormatException("Invalid JSON grid file.");

        string name = string.IsNullOrWhiteSpace(dto.Name) ? defaultName : dto.Name!;
        var elevation = ToArray2D(dto.Elevation, "elevation");
        int rows = elevation.GetLength(0);
        int cols = elevation.GetLength(1);

        return new GridData(
            name,
            elevation,
            ToArray2D(dto.Walls, "walls", rows, cols),
            ToArray2D(dto.Absorption, "absorption", rows, cols),
            ToBoolArray2D(dto.Tortoise, rows, cols));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private static bool TryParseKeyValue(string line, out string key, out string value)
    {
        int comma = line.IndexOf(',');
        if (comma <= 0)
        {
            key = string.Empty;
            value = string.Empty;
            return false;
        }

        key = line[..comma].Trim();
        value = line[(comma + 1)..].Trim();
        return key.Length > 0;
    }

    private static int ParsePositiveInt(string value, int lineNumber, string fieldName)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) || parsed <= 0)
        {
            throw new FormatException($"Invalid {fieldName} value on line {lineNumber}: '{value}'.");
        }

        return parsed;
    }

    private static double[] ParseDoubleRow(string[] parts, int lineNumber)
    {
        if (parts.Length == 0)
        {
            throw new FormatException($"Empty row on line {lineNumber}.");
        }

        var row = new double[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out row[i]))
            {
                throw new FormatException($"Invalid number '{parts[i]}' on line {lineNumber}, column {i + 1}.");
            }
        }

        return row;
    }

    private static int[] ParseIntRow(string[] parts, int lineNumber)
    {
        if (parts.Length == 0)
        {
            throw new FormatException($"Empty row on line {lineNumber}.");
        }

        var row = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out row[i]))
            {
                throw new FormatException($"Invalid integer '{parts[i]}' on line {lineNumber}, column {i + 1}.");
            }
        }

        return row;
    }

    private static bool[] ParseBoolRow(string[] parts, int lineNumber)
    {
        if (parts.Length == 0)
        {
            throw new FormatException($"Empty row on line {lineNumber}.");
        }

        var row = new bool[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            row[i] = ParseBoolToken(parts[i], lineNumber, i + 1);
        }

        return row;
    }

    private static bool ParseBoolToken(string token, int lineNumber, int columnNumber)
    {
        string value = token.Trim();
        if (value is "1" or "true" or "True" or "T" or "t" or "yes" or "Yes")
        {
            return true;
        }

        if (value is "0" or "false" or "False" or "F" or "f" or "no" or "No")
        {
            return false;
        }

        throw new FormatException($"Invalid tortoise flag '{token}' on line {lineNumber}, column {columnNumber}.");
    }

    private static double[,] ToMatrix(List<double[]> rows, int expectedRows, int expectedCols, string sectionName)
    {
        ValidateRowDimensions(rows, expectedRows, expectedCols, sectionName);
        var matrix = new double[expectedRows, expectedCols];
        for (int r = 0; r < expectedRows; r++)
        {
            for (int c = 0; c < expectedCols; c++)
            {
                matrix[r, c] = rows[r][c];
            }
        }

        return matrix;
    }

    private static int[,] ToMatrix(List<int[]> rows, int expectedRows, int expectedCols, string sectionName)
    {
        ValidateRowDimensions(rows, expectedRows, expectedCols, sectionName);
        var matrix = new int[expectedRows, expectedCols];
        for (int r = 0; r < expectedRows; r++)
        {
            for (int c = 0; c < expectedCols; c++)
            {
                matrix[r, c] = rows[r][c];
            }
        }

        return matrix;
    }

    private static bool[,] ToMatrix(List<bool[]> rows, int expectedRows, int expectedCols, string sectionName)
    {
        ValidateRowDimensions(rows, expectedRows, expectedCols, sectionName);
        var matrix = new bool[expectedRows, expectedCols];
        for (int r = 0; r < expectedRows; r++)
        {
            for (int c = 0; c < expectedCols; c++)
            {
                matrix[r, c] = rows[r][c];
            }
        }

        return matrix;
    }

    private static void ValidateRowDimensions<T>(List<T[]> rows, int expectedRows, int expectedCols, string sectionName)
    {
        if (rows.Count != expectedRows)
        {
            throw new FormatException($"{sectionName} section has {rows.Count} rows, expected {expectedRows}.");
        }

        for (int r = 0; r < rows.Count; r++)
        {
            if (rows[r].Length != expectedCols)
            {
                throw new FormatException($"{sectionName} row {r + 1} has {rows[r].Length} columns, expected {expectedCols}.");
            }
        }
    }

    private static double[,] ToArray2D(double[][]? source, string fieldName)
    {
        if (source is null || source.Length == 0)
        {
            throw new FormatException($"Missing '{fieldName}' matrix in JSON.");
        }

        int rows = source.Length;
        int cols = source[0].Length;
        return ToArray2D(source, fieldName, rows, cols);
    }

    private static double[,] ToArray2D(double[][]? source, string fieldName, int rows, int cols)
    {
        if (source is null)
        {
            throw new FormatException($"Missing '{fieldName}' matrix in JSON.");
        }

        ValidateJsonDimensions(source, rows, cols, fieldName);
        var matrix = new double[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                matrix[r, c] = source[r][c];
            }
        }

        return matrix;
    }

    private static int[,] ToArray2D(int[][]? source, string fieldName, int rows, int cols)
    {
        if (source is null)
        {
            throw new FormatException($"Missing '{fieldName}' matrix in JSON.");
        }

        ValidateJsonDimensions(source, rows, cols, fieldName);
        var matrix = new int[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                matrix[r, c] = source[r][c];
            }
        }

        return matrix;
    }

    private static bool[,] ToBoolArray2D(bool[][]? source, int rows, int cols)
    {
        if (source is null)
        {
            throw new FormatException("Missing 'tortoise' matrix in JSON.");
        }

        ValidateJsonDimensions(source, rows, cols, "tortoise");
        var matrix = new bool[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                matrix[r, c] = source[r][c];
            }
        }

        return matrix;
    }

    private static void ValidateJsonDimensions<T>(T[][] source, int rows, int cols, string fieldName)
    {
        if (source.Length != rows)
        {
            throw new FormatException($"{fieldName} has {source.Length} rows, expected {rows}.");
        }

        for (int r = 0; r < source.Length; r++)
        {
            if (source[r].Length != cols)
            {
                throw new FormatException($"{fieldName} row {r + 1} has {source[r].Length} columns, expected {cols}.");
            }
        }
    }

    private sealed class JsonGridDto
    {
        public string? Name { get; set; }

        public double[][]? Elevation { get; set; }

        public int[][]? Walls { get; set; }

        public double[][]? Absorption { get; set; }

        public bool[][]? Tortoise { get; set; }
    }
}
