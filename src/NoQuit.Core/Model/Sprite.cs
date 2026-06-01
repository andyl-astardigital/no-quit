namespace NoQuit.Core.Model;

public sealed class Sprite
{
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<SpriteCell> Cells { get; }

    public Sprite(int width, int height, IReadOnlyList<SpriteCell> cells)
    {
        if (width <= 0)  throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (cells.Count != width * height)
            throw new ArgumentException($"Expected {width * height} cells, got {cells.Count}.", nameof(cells));

        Width = width;
        Height = height;
        Cells = cells;
    }

    public SpriteCell At(int x, int y)
    {
        if (x < 0 || x >= Width)  throw new ArgumentOutOfRangeException(nameof(x));
        if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));
        return Cells[y * Width + x];
    }

    // 'X' = Bright, 'd' = Dim, anything else = Empty. All rows must have equal length.
    public static Sprite FromRows(IReadOnlyList<string> rows)
    {
        if (rows.Count == 0)
            throw new ArgumentException("Sprite must have at least one row.", nameof(rows));

        int height = rows.Count;
        int width = rows[0].Length;
        var cells = new SpriteCell[width * height];

        for (int y = 0; y < height; y++)
        {
            string row = rows[y];
            if (row.Length != width)
                throw new ArgumentException($"Row {y} length {row.Length} does not match expected width {width}.", nameof(rows));
            for (int x = 0; x < width; x++)
            {
                cells[y * width + x] = row[x] switch
                {
                    'X' => SpriteCell.Bright,
                    'd' => SpriteCell.Dim,
                    _   => SpriteCell.Empty,
                };
            }
        }
        return new Sprite(width, height, cells);
    }
}
