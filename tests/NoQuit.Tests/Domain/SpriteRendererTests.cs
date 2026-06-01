using NoQuit.Core.Domain;
using NoQuit.Core.Model;

namespace NoQuit.Tests.Domain;

public class SpriteRendererTests
{
    [Fact]
    public void Render_emits_one_instruction_per_non_empty_cell()
    {
        var sprite = Sprite.FromRows(new[]
        {
            "X.d",
            "..X",
        });

        var instructions = SpriteRenderer.Render(sprite, 0, 0, 1).ToList();
        instructions.Should().HaveCount(3);
    }

    [Fact]
    public void Render_offsets_and_scales_each_cell_correctly()
    {
        var sprite = Sprite.FromRows(new[]
        {
            "X.",
            ".X",
        });

        var ix = SpriteRenderer.Render(sprite, originX: 10, originY: 20, pixelSize: 5).ToList();
        ix.Should().HaveCount(2);
        ix[0].Should().Be(new PixelInstruction(10, 20, 5, SpriteCell.Bright));
        ix[1].Should().Be(new PixelInstruction(15, 25, 5, SpriteCell.Bright));
    }

    [Fact]
    public void Render_distinguishes_bright_and_dim_cells()
    {
        var sprite = Sprite.FromRows(new[] { "Xd" });

        var ix = SpriteRenderer.Render(sprite, 0, 0, 1).ToList();
        ix.Should().HaveCount(2);
        ix[0].Cell.Should().Be(SpriteCell.Bright);
        ix[1].Cell.Should().Be(SpriteCell.Dim);
    }

    [Fact]
    public void Render_skips_empty_cells_entirely()
    {
        var sprite = Sprite.FromRows(new[] { "...", "...", "..." });
        SpriteRenderer.Render(sprite, 0, 0, 1).Should().BeEmpty();
    }

    [Fact]
    public void Render_rejects_non_positive_pixel_size()
    {
        var sprite = SpriteAtlas.CoffeeCup16;
        FluentActions.Invoking(() => SpriteRenderer.Render(sprite, 0, 0, 0).ToList())
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => SpriteRenderer.Render(sprite, 0, 0, -1).ToList())
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Render_rejects_null_sprite()
    {
        FluentActions.Invoking(() => SpriteRenderer.Render(null!, 0, 0, 1).ToList())
            .Should().Throw<ArgumentNullException>();
    }
}
