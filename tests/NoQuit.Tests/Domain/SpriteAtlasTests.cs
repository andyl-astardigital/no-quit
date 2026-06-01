using NoQuit.Core.Domain;
using NoQuit.Core.Model;

namespace NoQuit.Tests.Domain;

public class SpriteAtlasTests
{
    [Fact]
    public void CoffeeCup16_is_16_by_16()
    {
        SpriteAtlas.CoffeeCup16.Width.Should().Be(16);
        SpriteAtlas.CoffeeCup16.Height.Should().Be(16);
    }

    [Fact]
    public void CoffeeCup16_has_bright_pixels_at_steam_and_cup()
    {
        var s = SpriteAtlas.CoffeeCup16;
        s.Cells.Should().Contain(SpriteCell.Bright);
        s.Cells.Should().Contain(SpriteCell.Dim);
    }

    [Fact]
    public void CoffeeCup16_is_majority_empty()
    {
        var s = SpriteAtlas.CoffeeCup16;
        int total = s.Width * s.Height;
        int empty = s.Cells.Count(c => c == SpriteCell.Empty);
        empty.Should().BeGreaterThan(total / 2);
    }
}
