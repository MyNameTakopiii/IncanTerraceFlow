using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace IncanTerraceFlow;

public static class CustomArrayParser
{
    public static GridData Parse(string text, string name = "Custom Input")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException("Input text is empty.");
        }

        // 1. Remove comments
        string cleanText = RemoveComments(text);

        // 2. Extract matrices
        var matrices = ExtractMatrices(cleanText);

        if (matrices.Count < 4)
        {
            throw new FormatException($"Found only {matrices.Count} arrays in the input text. Expected exactly 4 arrays (Elevation, WallData, Absorption, Tortoise).");
        }

        // Consensus Dimension Filtering:
        // Group extracted matrices by their row/col dimensions to identify the main simulation arrays
        // and filter out any unrelated small bracket blocks from description text or comments.
        var groups = new Dictionary<(int, int), List<ParsedMatrix>>();
        foreach (var m in matrices)
        {
            var dim = (m.Data.GetLength(0), m.Data.GetLength(1));
            if (!groups.ContainsKey(dim))
            {
                groups[dim] = new List<ParsedMatrix>();
            }
            groups[dim].Add(m);
        }

        (int Rows, int Cols) consensusDim = (0, 0);
        int maxCount = 0;
        foreach (var kvp in groups)
        {
            if (kvp.Value.Count > maxCount)
            {
                maxCount = kvp.Value.Count;
                consensusDim = kvp.Key;
            }
        }

        List<ParsedMatrix> filteredMatrices = (maxCount >= 4) ? groups[consensusDim] : matrices;

        // 3. Match matrices to roles
        var matched = MatchMatrices(filteredMatrices);

        if (matched.Elevation == null)
            throw new FormatException("Could not identify Elevation (array1) in the input.");
        if (matched.WallData == null)
            throw new FormatException("Could not identify WallData (array2) in the input.");
        if (matched.Absorption == null)
            throw new FormatException("Could not identify Absorption (array3) in the input.");
        if (matched.Tortoise == null)
            throw new FormatException("Could not identify Tortoise (array4) in the input.");

        int rows = matched.Elevation.GetLength(0);
        int cols = matched.Elevation.GetLength(1);

        if (rows == 0 || cols == 0)
        {
            throw new FormatException("Parsed arrays have 0 dimensions.");
        }

        // Verify dimensions match across all 4 arrays
        ValidateDimensions(matched.WallData, rows, cols, "WallData");
        ValidateDimensions(matched.Absorption, rows, cols, "Absorption");
        ValidateDimensions(matched.Tortoise, rows, cols, "Tortoise");

        double[,] elevation = new double[rows, cols];
        int[,] wallData = new int[rows, cols];
        double[,] absorption = new double[rows, cols];
        bool[,] tortoise = new bool[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                string elStr = matched.Elevation[r, c];
                if (!double.TryParse(elStr, NumberStyles.Float, CultureInfo.InvariantCulture, out elevation[r, c]))
                    throw new FormatException($"Invalid double value '{elStr}' in Elevation array at row {r + 1}, col {c + 1}.");

                string wallStr = matched.WallData[r, c];
                if (!int.TryParse(wallStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out wallData[r, c]))
                    throw new FormatException($"Invalid integer value '{wallStr}' in WallData array at row {r + 1}, col {c + 1}.");

                string absStr = matched.Absorption[r, c];
                if (!double.TryParse(absStr, NumberStyles.Float, CultureInfo.InvariantCulture, out absorption[r, c]))
                    throw new FormatException($"Invalid double value '{absStr}' in Absorption array at row {r + 1}, col {c + 1}.");

                string tortStr = matched.Tortoise[r, c];
                if (!TryParseBool(tortStr, out tortoise[r, c]))
                    throw new FormatException($"Invalid boolean value '{tortStr}' in Tortoise array at row {r + 1}, col {c + 1}.");
            }
        }

        return new GridData(name, elevation, wallData, absorption, tortoise);
    }

    private static string RemoveComments(string text)
    {
        string step1 = Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return Regex.Replace(step1, @"//.*", "", RegexOptions.Multiline);
    }

    private class ParsedMatrix
    {
        public string Name { get; set; } = "";
        public string[,] Data { get; set; } = new string[0, 0];
        public bool ContainsBooleans { get; set; }
    }

    private static string CleanToken(string value)
    {
        string trimmed = value.Trim();
        if (trimmed.Length >= 2)
        {
            if ((trimmed.StartsWith('\'') && trimmed.EndsWith('\'')) ||
                (trimmed.StartsWith('"') && trimmed.EndsWith('"')))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }
        }
        return trimmed.Trim();
    }

    private static List<ParsedMatrix> ExtractMatrices(string text)
    {
        var matrices = new List<ParsedMatrix>();
        int index = 0;
        int len = text.Length;
        int lastMatrixEnd = 0;

        while (index < len)
        {
            char ch = text[index];
            if (ch == '[' || ch == '{')
            {
                int matrixStart = index;
                int depth = 0;
                int outerEnd = -1;
                var rowStrings = new List<List<string>>();
                var currentRow = new List<string>();
                var currentToken = new StringBuilder();

                for (int i = index; i < len; i++)
                {
                    char c = text[i];
                    if (c == '[' || c == '{')
                    {
                        depth++;
                        if (depth == 2)
                        {
                            currentRow = new List<string>();
                            currentToken.Clear();
                        }
                    }
                    else if (c == ']' || c == '}')
                    {
                        if (depth == 2)
                        {
                            if (currentToken.Length > 0)
                            {
                                currentRow.Add(CleanToken(currentToken.ToString()));
                                currentToken.Clear();
                            }
                            if (currentRow.Count > 0)
                            {
                                rowStrings.Add(currentRow);
                            }
                        }
                        depth--;
                        if (depth == 0)
                        {
                            outerEnd = i;
                            break;
                        }
                    }
                    else if (depth == 2)
                    {
                        if (c == ',')
                        {
                            currentRow.Add(CleanToken(currentToken.ToString()));
                            currentToken.Clear();
                        }
                        else
                        {
                            currentToken.Append(c);
                        }
                    }
                }

                if (outerEnd != -1 && rowStrings.Count > 0)
                {
                    // Find a preceding name/label in the text since the end of the last matrix
                    string precedingText = text.Substring(lastMatrixEnd, matrixStart - lastMatrixEnd);
                    string name = FindVarName(precedingText);

                    int rows = rowStrings.Count;
                    int cols = 0;
                    foreach (var row in rowStrings)
                    {
                        if (row.Count > cols)
                        {
                            cols = row.Count;
                        }
                    }

                    string[,] data = new string[rows, cols];
                    bool containsBools = false;

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            if (c < rowStrings[r].Count)
                            {
                                string cell = rowStrings[r][c];
                                data[r, c] = cell;
                                string cellLower = cell.ToLowerInvariant();
                                if (cellLower == "true" || cellLower == "false")
                                {
                                    containsBools = true;
                                }
                            }
                            else
                            {
                                data[r, c] = "";
                            }
                        }
                    }

                    matrices.Add(new ParsedMatrix
                    {
                        Name = name,
                        Data = data,
                        ContainsBooleans = containsBools
                    });

                    index = outerEnd;
                    lastMatrixEnd = outerEnd + 1;
                }
            }
            index++;
        }

        return matrices;
    }

    private static string FindVarName(string precedingText)
    {
        var match = Regex.Match(precedingText, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\s*(?:=|:|\{)\s*$", RegexOptions.RightToLeft);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
    }

    private class MatchedMatrices
    {
        public string[,]? Elevation { get; set; }
        public string[,]? WallData { get; set; }
        public string[,]? Absorption { get; set; }
        public string[,]? Tortoise { get; set; }
    }

    private static MatchedMatrices MatchMatrices(List<ParsedMatrix> matrices)
    {
        var matched = new MatchedMatrices();
        var remaining = new List<ParsedMatrix>(matrices);

        // 1. Match by specific name keywords
        for (int i = remaining.Count - 1; i >= 0; i--)
        {
            var m = remaining[i];
            string nameLower = m.Name.ToLowerInvariant();
            if (nameLower.Contains("elevation") || nameLower == "array1")
            {
                matched.Elevation = m.Data;
                remaining.RemoveAt(i);
            }
            else if (nameLower.Contains("wall") || nameLower == "array2")
            {
                matched.WallData = m.Data;
                remaining.RemoveAt(i);
            }
            else if (nameLower.Contains("absorption") || nameLower.Contains("absorb") || nameLower == "array3")
            {
                matched.Absorption = m.Data;
                remaining.RemoveAt(i);
            }
            else if (nameLower.Contains("tortoise") || nameLower.Contains("tort") || nameLower == "array4")
            {
                matched.Tortoise = m.Data;
                remaining.RemoveAt(i);
            }
        }

        // 2. Identify Tortoise by content (booleans) if not already mapped
        if (matched.Tortoise == null)
        {
            var tortoiseMatrix = remaining.Find(m => m.ContainsBooleans);
            if (tortoiseMatrix != null)
            {
                matched.Tortoise = tortoiseMatrix.Data;
                remaining.Remove(tortoiseMatrix);
            }
        }

        // 3. Match remaining by order of appearance
        if (matched.Elevation == null && remaining.Count > 0)
        {
            matched.Elevation = remaining[0].Data;
            remaining.RemoveAt(0);
        }
        if (matched.WallData == null && remaining.Count > 0)
        {
            matched.WallData = remaining[0].Data;
            remaining.RemoveAt(0);
        }
        if (matched.Absorption == null && remaining.Count > 0)
        {
            matched.Absorption = remaining[0].Data;
            remaining.RemoveAt(0);
        }
        if (matched.Tortoise == null && remaining.Count > 0)
        {
            matched.Tortoise = remaining[0].Data;
            remaining.RemoveAt(0);
        }

        return matched;
    }

    private static void ValidateDimensions(string[,] array, int expectedRows, int expectedCols, string name)
    {
        if (array.GetLength(0) != expectedRows || array.GetLength(1) != expectedCols)
        {
            throw new FormatException($"Dimension mismatch! The {name} array has dimension {array.GetLength(0)}x{array.GetLength(1)}, expected {expectedRows}x{expectedCols}.");
        }
    }

    private static bool TryParseBool(string value, out bool result)
    {
        string valLower = value.Trim().ToLowerInvariant();
        if (valLower == "true" || valLower == "t" || valLower == "1")
        {
            result = true;
            return true;
        }
        if (valLower == "false" || valLower == "f" || valLower == "0")
        {
            result = false;
            return true;
        }
        result = false;
        return false;
    }
}
