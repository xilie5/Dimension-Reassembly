param(
    [string]$UnityPath = $env:UNITY_EDITOR_PATH,
    [string]$OutputPath = "Builds/Windows/CompoundBox.exe",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $candidates = @(
        "E:\Unity\2022.3.62f3c1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe",
        "D:\Unity\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            $UnityPath = $candidate
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($UnityPath) -or -not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity.exe was not found. Set UNITY_EDITOR_PATH or pass -UnityPath."
}

if (-not $SkipTests) {
    & (Join-Path $PSScriptRoot "Run-EditModeTests.ps1") -UnityPath $UnityPath
}

$resolvedOutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath
} else {
    Join-Path $projectRoot $OutputPath
}
$outputDirectory = Split-Path -Parent $resolvedOutputPath
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

$env:COMPOUND_BOX_BUILD_PATH = $resolvedOutputPath
$logPath = Join-Path $projectRoot "Logs\windows-build.log"

& $UnityPath `
    -batchmode `
    -quit `
    -projectPath $projectRoot `
    -executeMethod CompoundBox.Editor.CommandLineBuild.BuildWindows64 `
    -logFile $logPath

if ($LASTEXITCODE -ne 0) {
    throw "Windows build failed with exit code $LASTEXITCODE. See $logPath"
}

Write-Host "Windows build succeeded: $resolvedOutputPath"
