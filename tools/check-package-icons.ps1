param([Parameter(Mandatory)][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'

if ($IsWindows) {
    Add-Type -AssemblyName System.Drawing
    foreach ($path in @('publish/LegendOfCat.exe', 'publish/Game/LegendOfCat.exe') +
        @(Get-ChildItem $PackageDirectory -Filter '*Setup.exe' | ForEach-Object FullName)) {
        $icon = [System.Drawing.Icon]::ExtractAssociatedIcon((Resolve-Path $path).Path)
        if ($null -eq $icon) { throw "Missing embedded icon: $path" }
        $bitmap = $icon.ToBitmap()
        try {
            $blue = 0; $orange = 0
            for ($y = 0; $y -lt $bitmap.Height; $y++) {
                for ($x = 0; $x -lt $bitmap.Width; $x++) {
                    $pixel = $bitmap.GetPixel($x, $y)
                    if ($pixel.B -gt 120 -and $pixel.B -gt $pixel.R * 1.5) { $blue++ }
                    if ($pixel.R -gt 180 -and $pixel.G -gt 80 -and $pixel.R -gt $pixel.B * 1.5) { $orange++ }
                }
            }
            # Detect a missing/default engine icon as well as a missing PE icon resource.
            if ($blue -lt 20 -or $orange -lt 20) { throw "Expected blue/orange application icon missing: $path" }
        } finally { $bitmap.Dispose(); $icon.Dispose() }
    }
} else {
    $appImage = @(Get-ChildItem $PackageDirectory -Filter '*.AppImage')
    if ($appImage.Count -ne 1) { throw 'Expected one AppImage' }
    $appImagePath = $appImage[0].FullName
    $inspection = Join-Path ([System.IO.Path]::GetTempPath()) ('LegendOfCat-icons-' + [guid]::NewGuid())
    New-Item -ItemType Directory $inspection | Out-Null
    Push-Location $inspection
    try {
        chmod +x $appImagePath
        & $appImagePath --appimage-extract | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Could not extract AppImage for icon inspection' }
        $desktop = @(Get-ChildItem squashfs-root -Filter '*.desktop')
        if ($desktop.Count -ne 1) { throw 'Missing desktop integration' }
        $iconLine = @(Get-Content $desktop[0].FullName | Where-Object { $_ -match '^Icon=' })
        if ($iconLine.Count -ne 1) { throw 'Desktop entry has no unique icon' }
        $iconName = $iconLine[0].Substring(5)
        if (-not (Test-Path (Join-Path 'squashfs-root' ($iconName + '.png')))) {
            throw "Missing desktop icon $iconName.png"
        }
        if (-not (Test-Path 'squashfs-root/.DirIcon')) { throw 'Missing AppImage directory icon' }
    } finally { Pop-Location }
    # Retain extraction only in the ephemeral runner temp directory for diagnostics.
}
Write-Output 'Application and package icons verified.'
