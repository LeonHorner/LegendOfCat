$ErrorActionPreference = 'Stop'
$evidenceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../docs/design/background-study')).Path
$motionRoot = (Resolve-Path (Join-Path $PSScriptRoot '../Logs/background-study-motion')).Path
foreach ($mode in 0..2) {
    $inputPattern = Join-Path $motionRoot "motion-$mode-%03d.png"
    $video = Join-Path $evidenceRoot "candidate-$mode.mp4"
    & ffmpeg -hide_banner -loglevel error -y -framerate 5 -i $inputPattern -c:v libx264 -pix_fmt yuv420p -movflags +faststart $video
    if ($LASTEXITCODE -ne 0) { throw "Video export failed for candidate $mode." }
}
$videos = 0..2 | ForEach-Object { Join-Path $evidenceRoot "candidate-$_.mp4" }
if (Test-Path -LiteralPath (Join-Path $motionRoot 'shore-review-000.png')) {
    $reviewPattern = Join-Path $motionRoot 'shore-review-%03d.png'
    $reviewVideo = Join-Path $evidenceRoot 'world-shore-review.mp4'
    & ffmpeg -hide_banner -loglevel error -y -framerate 5 -i $reviewPattern -c:v libx264 -pix_fmt yuv420p -movflags +faststart $reviewVideo
    if ($LASTEXITCODE -ne 0) { throw 'World-shore review export failed.' }
}
$comparison = Join-Path $evidenceRoot 'comparison.gif'
& ffmpeg -hide_banner -loglevel error -y -i $videos[0] -i $videos[1] -i $videos[2] -filter_complex '[0:v]scale=320:180[a];[1:v]scale=320:180[b];[2:v]scale=320:180[c];[a][b][c]hstack=inputs=3,split[s0][s1];[s0]palettegen=max_colors=96[p];[s1][p]paletteuse' $comparison
if ($LASTEXITCODE -ne 0) { throw 'Comparison export failed.' }
# Delete only numbered intermediate captures inside the verified evidence directory.
Get-ChildItem -LiteralPath $motionRoot -File | Where-Object Name -Match '^(motion-[0-2]-|shore-review-)[0-9]{3}\.png$' |
    ForEach-Object { Remove-Item -LiteralPath $_.FullName }
