<#
    NoQuit installer.

    Right-click this file -> "Run with PowerShell", or run from an elevated prompt.
    It will self-elevate to Administrator if needed.

    What it does (all reversible via Uninstall.ps1):
      1. Removes the "downloaded from the internet" mark from NoQuit.exe (stops SmartScreen).
      2. Copies NoQuit.exe to %LOCALAPPDATA%\Programs\NoQuit.
      3. Adds a Microsoft Defender exclusion for that folder (needs admin).
      4. Launches it. The app registers its own auto-start task (logon | unlock | resume).
#>

[CmdletBinding()]
param(
    [switch]$NoElevate
)

$ErrorActionPreference = 'Stop'

# --- self-elevate ------------------------------------------------------------
$isAdmin = ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin -and -not $NoElevate) {
    Write-Host "Requesting administrator rights..." -ForegroundColor Cyan
    $psi = @{
        FilePath     = 'powershell.exe'
        ArgumentList = @(
            '-NoProfile', '-ExecutionPolicy', 'Bypass',
            '-File', "`"$PSCommandPath`""
        )
        Verb         = 'RunAs'
    }
    try   { Start-Process @psi }
    catch { Write-Host "Elevation cancelled. Aborting." -ForegroundColor Red; exit 1 }
    exit 0
}

# --- locate the exe shipped next to this script ------------------------------
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$source    = Join-Path $scriptDir 'NoQuit.exe'

if (-not (Test-Path $source)) {
    Write-Host "ERROR: NoQuit.exe not found next to this script ($scriptDir)." -ForegroundColor Red
    Write-Host "Extract the whole zip and run Install.ps1 from inside it." -ForegroundColor Yellow
    Read-Host "Press Enter to close"
    exit 1
}

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\NoQuit'
$target     = Join-Path $installDir 'NoQuit.exe'

Write-Host ""
Write-Host "  no_quit :: installer" -ForegroundColor Green
Write-Host "  --------------------" -ForegroundColor DarkGreen
Write-Host ""

# 1. strip Mark-of-the-Web so SmartScreen stops complaining
try {
    Unblock-File -Path $source
    Write-Host "  [ok]   cleared download flag" -ForegroundColor Green
} catch {
    Write-Host "  [warn] could not clear download flag: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. stop any running instance, then copy into place
Get-Process -Name 'NoQuit' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Copy-Item -Path $source -Destination $target -Force
Unblock-File -Path $target -ErrorAction SilentlyContinue
Write-Host "  [ok]   copied to $installDir" -ForegroundColor Green

# 3. Defender exclusion (this is why we needed admin)
try {
    Add-MpPreference -ExclusionPath $installDir -ErrorAction Stop
    Write-Host "  [ok]   added Defender exclusion for install folder" -ForegroundColor Green
} catch {
    Write-Host "  [warn] could not add Defender exclusion: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "         (the app will still run; SmartScreen flag is already cleared)" -ForegroundColor DarkYellow
}

# 4. launch — the app self-registers its scheduled task (logon | unlock | resume)
Start-Process -FilePath $target
Write-Host "  [ok]   launched. look for the green coffee cup in your tray." -ForegroundColor Green
Write-Host ""
Write-Host "  auto-start is registered automatically (logon | unlock | resume)." -ForegroundColor DarkGreen
Write-Host "  to remove: run Uninstall.ps1 from this folder." -ForegroundColor DarkGreen
Write-Host ""

Start-Sleep -Seconds 2
