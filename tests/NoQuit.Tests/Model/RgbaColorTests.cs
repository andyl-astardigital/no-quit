using NoQuit.Core.Model;

namespace NoQuit.Tests.Model;

public class RgbaColorTests
{
    [Fact]
    public void Argb_packs_bytes_in_alpha_red_green_blue_order()
    {
        var c = new RgbaColor(0x12, 0x34, 0x56, 0xAB);
        c.Argb.Should().Be(0xAB123456u);
    }

    [Fact]
    public void Default_alpha_is_opaque()
    {
        var c = new RgbaColor(1, 2, 3);
        c.A.Should().Be(255);
        c.Argb.Should().Be(0xFF010203u);
    }

    [Fact]
    public void Records_are_value_equal_by_components()
    {
        new RgbaColor(1, 2, 3, 4).Should().Be(new RgbaColor(1, 2, 3, 4));
    }
}
