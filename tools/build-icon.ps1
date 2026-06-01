# Generates src/NoQuit/icon.ico from the coffee-cup sprite at multiple resolutions.
# Re-run after editing the sprite to refresh the exe icon.

Add-Type -AssemblyName System.Drawing

$sprite = @(
    '....X...X...X...',
    '...X.X.X.X.X.X..',
    '....X...X...X...',
    '................',
    '................',
    '..XXXXXXXXX.....',
    '..X.......X.....',
    '..X.......XXX...',
    '..Xdddddd.X.X...',
    '..Xdddddd.X.X...',
    '..Xdddddd.XXX...',
    '..Xdddddd.X.....',
    '..XXXXXXXXX.....',
    '...XXXXXXX......',
    '.XXXXXXXXXXX....',
    '................'
)

$bright = [System.Drawing.Color]::FromArgb(255,   0, 255,  65)
$dim    = [System.Drawing.Color]::FromArgb(255,   0,  90,  25)
$sizes  = @(16, 32, 48, 64, 128, 256)
$pngs   = @()

foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g   = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode   = [System.Drawing.Drawing2D.SmoothingMode]::None
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    $g.Clear([System.Drawing.Color]::Transparent)
    $brightBrush = New-Object System.Drawing.SolidBrush $bright
    $dimBrush    = New-Object System.Drawing.SolidBrush $dim
    $px = [int]($size / 16)
    if ($px -lt 1) { $px = 1 }
    for ($y = 0; $y -lt 16; $y++) {
        for ($x = 0; $x -lt 16; $x++) {
            $c = $sprite[$y][$x]
            $brush = $null
            if     ($c -eq 'X') { $brush = $brightBrush }
            elseif ($c -eq 'd') { $brush = $dimBrush    }
            if ($brush) { $g.FillRectangle($brush, $x*$px, $y*$px, $px, $px) }
        }
    }
    $brightBrush.Dispose()
    $dimBrush.Dispose()
    $g.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $pngs += ,@($size, $ms.ToArray())
}

# Build the ICO container: ICONDIR + N x ICONDIRENTRY + N PNG payloads.
$out = New-Object System.IO.MemoryStream
$bw  = New-Object System.IO.BinaryWriter $out
$bw.Write([uint16]0)             # reserved
$bw.Write([uint16]1)             # type = icon
$bw.Write([uint16]$pngs.Count)
$dataOffset = 6 + 16 * $pngs.Count
foreach ($p in $pngs) {
    $sz   = $p[0]
    $data = $p[1]
    $bw.Write([byte]($sz % 256))   # width  (0 means 256)
    $bw.Write([byte]($sz % 256))   # height (0 means 256)
    $bw.Write([byte]0)             # palette
    $bw.Write([byte]0)             # reserved
    $bw.Write([uint16]1)           # planes
    $bw.Write([uint16]32)          # bpp
    $bw.Write([uint32]$data.Length)
    $bw.Write([uint32]$dataOffset)
    $dataOffset += $data.Length
}
foreach ($p in $pngs) { $bw.Write([byte[]]$p[1]) }
$bw.Flush()

$outPath = Join-Path $PSScriptRoot '..\src\NoQuit\icon.ico'
[System.IO.File]::WriteAllBytes((Resolve-Path -LiteralPath (Split-Path $outPath -Parent)).Path + '\' + (Split-Path $outPath -Leaf), $out.ToArray())
Write-Host ("wrote {0} ({1} bytes, {2} sizes)" -f $outPath, $out.Length, $pngs.Count)
