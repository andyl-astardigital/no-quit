using NoQuit.Core.Model;

namespace NoQuit.Tests.Model;

public class SpriteTests
{
    [Fact]
    public void FromRows_parses_X_d_and_dots_correctly()
    {
        var s = Sprite.FromRows(new[]
        {
            "Xd.",
            ".Xd",
        });

        s.Width.Should().Be(3);
        s.Height.Should().Be(2);

        s.At(0, 0).Should().Be(SpriteCell.Bright);
        s.At(1, 0).Should().Be(SpriteCell.Dim);
        s.At(2, 0).Should().Be(SpriteCell.Empty);
        s.At(0, 1).Should().Be(SpriteCell.Empty);
        s.At(1, 1).Should().Be(SpriteCell.Bright);
        s.At(2, 1).Should().Be(SpriteCell.Dim);
    }

    [Fact]
    public void FromRows_treats_any_unknown_char_as_empty()
    {
        var s = Sprite.FromRows(new[] { "X?#@" });
        s.At(0, 0).Should().Be(SpriteCell.Bright);
        s.At(1, 0).Should().Be(SpriteCell.Empty);
        s.At(2, 0).Should().Be(SpriteCell.Empty);
        s.At(3, 0).Should().Be(SpriteCell.Empty);
    }

    [Fact]
    public void FromRows_rejects_empty_input()
    {
        FluentActions.Invoking(() => Sprite.FromRows(Array.Empty<string>()))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromRows_rejects_jagged_rows()
    {
        FluentActions.Invoking(() => Sprite.FromRows(new[] { "XX", "X" }))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_rejects_wrong_cell_count()
    {
        FluentActions.Invoking(() => new Sprite(2, 2, new[]
            {
                SpriteCell.Empty, SpriteCell.Empty, SpriteCell.Empty,
            }))
            .Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2, 0)]
    [InlineData(0, 2)]
    public void At_rejects_out_of_range_coordinates(int x, int y)
    {
        var s = Sprite.FromRows(new[] { "XX", "XX" });
        FluentActions.Invoking(() => s.At(x, y))
            .Should().Throw<ArgumentOutOfRangeException>();
    }
}
