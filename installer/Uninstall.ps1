<#
    NoQuit uninstaller. Reverses Install.ps1.

    Right-click -> "Run with PowerShell". Self-elevates to Administrator.
      1. Removes the auto-start scheduled task and kills any running instance.
      2. Removes the Microsoft Defender exclusion.
      3. Deletes %LOCALAPPDATA%\Programs\NoQuit.
#>

[CmdletBinding()]
param(
    [switch]$NoElevate
)

$ErrorActionPreference = 'Stop'

$isAdmin = ([Security.Principal.WindowsPrincipal] `
    [Security.Principal.WindowsIdentity]::GetCurrent()
).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin -and -not $NoElevate) {
    $psi = @{
        FilePath     = 'powershell.exe'
        ArgumentList = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSCommandPath`"")
        Verb         = 'RunAs'
    }
    try   { Start-Process @psi }
    catch { Write-Host "Elevation cancelled. Aborting." -ForegroundColor Red; exit 1 }
    exit 0
}

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\NoQuit'
$target     = Join-Path $installDir 'NoQuit.exe'

Write-Host ""
Write-Host "  no_quit :: uninstaller" -ForegroundColor Yellow
Write-Host "  ----------------------" -ForegroundColor DarkYellow
Write-Host ""

# 1. let the app tear down its own scheduled task + kill running copies
if (Test-Path $target) {
    try {
        Start-Process -FilePath $target -ArgumentList '--uninstall' -Wait
        Write-Host "  [ok]   removed scheduled task" -ForegroundColor Green
    } catch {
        Write-Host "  [warn] --uninstall failed: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}
# belt and braces: drop the task directly and kill any stragglers
schtasks /Delete /TN 'NoQuit' /F 2>$null | Out-Null
Get-Process -Name 'NoQuit' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

# 2. remove Defender exclusion
try {
    Remove-MpPreference -ExclusionPath $installDir -ErrorAction Stop
    Write-Host "  [ok]   removed Defender exclusion" -ForegroundColor Green
} catch {
    Write-Host "  [warn] no Defender exclusion to remove" -ForegroundColor DarkYellow
}

# 3. delete the install folder
if (Test-Path $installDir) {
    Remove-Item -Path $installDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  [ok]   deleted $installDir" -ForegroundColor Green
}

Write-Host ""
Write-Host "  done." -ForegroundColor Green
Write-Host ""
Start-Sleep -Seconds 2
