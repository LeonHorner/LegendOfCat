$ErrorActionPreference = 'Stop'
$evidenceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../docs/design/background-study')).Path
$motionRoot = (Resolve-Path (Join-Path $PSScriptRoot '../Logs/background-study-motion')).Path
foreach ($name in @('water', 'subtle')) {
    $inputPattern = Join-Path $motionRoot "$name-review-%03d.png"
    $outputName = if ($name -eq 'water') { 'water-flow.mp4' } else { 'subtle-animation.mp4' }
    $video = Join-Path $evidenceRoot $outputName
    & ffmpeg -hide_banner -loglevel error -y -framerate 25 -i $inputPattern -c:v libx264 -pix_fmt yuv420p -movflags +faststart $video
    if ($LASTEXITCODE -ne 0) { throw "Video export failed for $name." }
}
# Delete only this capture's numbered intermediates in the resolved motion directory.
Get-ChildItem -LiteralPath $motionRoot -File | Where-Object Name -Match '^(water|subtle)-review-[0-9]{3}\.png$' |
    ForEach-Object { Remove-Item -LiteralPath $_.FullName }
