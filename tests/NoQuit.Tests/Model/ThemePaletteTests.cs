using NoQuit.Core.Model;

namespace NoQuit.Tests.Model;

public class ThemePaletteTests
{
    [Fact]
    public void BrightFor_returns_matrix_green_when_active()
    {
        ThemePalette.BrightFor(Status.Active).Should().Be(ThemePalette.Green);
    }

    [Fact]
    public void BrightFor_returns_grey_when_paused()
    {
        ThemePalette.BrightFor(Status.Paused).Should().Be(ThemePalette.Grey);
    }

    [Fact]
    public void DimFor_returns_dim_green_when_active()
    {
        ThemePalette.DimFor(Status.Active).Should().Be(ThemePalette.GreenFade);
    }

    [Fact]
    public void DimFor_returns_dark_grey_when_paused()
    {
        ThemePalette.DimFor(Status.Paused).Should().Be(ThemePalette.GreyDark);
    }

    [Fact]
    public void Green_is_classic_matrix_phosphor()
    {
        ThemePalette.Green.Should().Be(new RgbaColor(0, 255, 65));
    }
}
