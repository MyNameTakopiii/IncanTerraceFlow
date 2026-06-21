namespace IncanTerraceFlow;

public static class RandomGridGenerator
{
    public static GridData Generate(int rows, int cols, Random? rng = null)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException("Grid dimensions must be positive.");
        }

        rng ??= Random.Shared;
        var elevation = new double[rows, cols];
        var walls = new int[rows, cols];
        var absorption = new double[rows, cols];
        var tortoise = new bool[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                double baseHeight = 1800 - (1800.0 * row / Math.Max(1, rows - 1));
                elevation[row, col] = Math.Clamp(baseHeight + rng.Next(-120, 120), 200, 1800);
                absorption[row, col] = rng.Next(40, 151);
                tortoise[row, col] = rng.NextDouble() < 0.05;

                int mask = 0;
                if (rng.NextDouble() > 0.3)
                {
                    mask |= 8;
                }

                if (rng.NextDouble() > 0.3)
                {
                    mask |= 4;
                }

                if (rng.NextDouble() > 0.3)
                {
                    mask |= 2;
                }

                if (rng.NextDouble() > 0.3)
                {
                    mask |= 1;
                }

                walls[row, col] = mask;
            }
        }

        return new GridData(
            $"Random {rows}x{cols}",
            elevation,
            walls,
            absorption,
            tortoise);
    }
}
