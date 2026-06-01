namespace NoQuit.Core.Model;

public static class ThemePalette
{
    public static readonly RgbaColor Bg        = new(  6,  10,   6);
    public static readonly RgbaColor BgLight   = new( 12,  20,  12);
    public static readonly RgbaColor Green     = new(  0, 255,  65);
    public static readonly RgbaColor GreenDim  = new(  0, 150,  40);
    public static readonly RgbaColor GreenFade = new(  0,  90,  25);
    public static readonly RgbaColor Red       = new(255,  60,  60);
    public static readonly RgbaColor Amber     = new(255, 180,   0);
    public static readonly RgbaColor Grey      = new(110, 110, 110);
    public static readonly RgbaColor GreyDark  = new( 60,  60,  60);

    public static RgbaColor BrightFor(Status status) =>
        status == Status.Active ? Green : Grey;

    public static RgbaColor DimFor(Status status) =>
        status == Status.Active ? GreenFade : GreyDark;
}
