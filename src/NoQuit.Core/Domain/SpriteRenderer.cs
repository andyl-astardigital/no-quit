using NoQuit.Core.Model;

namespace NoQuit.Core.Domain;

public static class SpriteRenderer
{
    public static IEnumerable<PixelInstruction> Render(Sprite sprite, int originX, int originY, int pixelSize)
    {
        ArgumentNullException.ThrowIfNull(sprite);
        if (pixelSize <= 0) throw new ArgumentOutOfRangeException(nameof(pixelSize));

        for (int y = 0; y < sprite.Height; y++)
        {
            for (int x = 0; x < sprite.Width; x++)
            {
                var cell = sprite.At(x, y);
                if (cell == SpriteCell.Empty) continue;
                yield return new PixelInstruction(
                    X:    originX + x * pixelSize,
                    Y:    originY + y * pixelSize,
                    Size: pixelSize,
                    Cell: cell);
            }
        }
    }
}
