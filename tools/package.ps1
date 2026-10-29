# 编译并打包成可发布的 zip（解压后直接放进 mods\local\FastLevelUp 即可用）
param(
    [string]$Version = "1.1.2",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $root "bin\$Configuration"

Push-Location $root
try {
    dotnet build -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "构建失败" }
}
finally {
    Pop-Location
}

$outDir = Join-Path $root "dist"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$zip = Join-Path $outDir "FastLevelUp-v$Version.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }

Compress-Archive -Path (
    "$bin\FastLevelUp.dll",
    "$bin\PLib.dll",
    "$bin\mod.yaml",
    "$bin\mod_info.yaml",
    "$bin\translations"
) -DestinationPath $zip -CompressionLevel Optimal

Write-Host "[FastLevelUp] 打包完成: $zip"
