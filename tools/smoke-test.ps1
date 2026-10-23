param(
    [Parameter(Mandatory)][string]$Executable,
    [Parameter(Mandatory)][string]$LogPath,
    [switch]$VirtualDisplay
)
$ErrorActionPreference = 'Stop'
$executablePath = (Resolve-Path -LiteralPath $Executable).Path
$logFile = [IO.Path]::GetFullPath($LogPath)
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($logFile)) | Out-Null
$start = [Diagnostics.ProcessStartInfo]::new()
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.WorkingDirectory = [IO.Path]::GetDirectoryName($executablePath)
if ($VirtualDisplay) {
    if ($IsWindows) { throw 'A virtual X display is only supported on Linux.' }
    $start.FileName = (Get-Command xvfb-run -CommandType Application).Source
    $start.ArgumentList.Add('--auto-servernum')
    $start.ArgumentList.Add($executablePath)
} else {
    $start.FileName = $executablePath
}
foreach ($argument in @('--smoke-test', '-batchmode', '-nographics', '-logFile', $logFile)) {
    $start.ArgumentList.Add($argument)
}
$process = [Diagnostics.Process]::new()
$process.StartInfo = $start
$passed = $false
try {
    if (-not $process.Start()) { throw 'Could not start the packaged game.' }
    if (-not $process.WaitForExit(90000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw 'Game startup timed out.'
    }
    if ($process.ExitCode -ne 0) { throw "Game exited with $($process.ExitCode)." }
    $passed = $true
    Write-Host 'Packaged game startup passed.'
} finally {
    if (-not $passed -and (Test-Path -LiteralPath $logFile)) {
        Write-Host 'Unity smoke log (last 100 lines):'
        Get-Content -LiteralPath $logFile -Tail 100
    }
    $process.Dispose()
}
