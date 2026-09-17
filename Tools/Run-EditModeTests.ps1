param(
    [string]$UnityPath = $env:UNITY_EDITOR_PATH
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

$resultDirectory = Join-Path $projectRoot "TestResults"
New-Item -ItemType Directory -Path $resultDirectory -Force | Out-Null
$resultsPath = Join-Path $resultDirectory "editmode-results.xml"
$logPath = Join-Path $projectRoot "Logs\editmode-tests.log"

& $UnityPath `
    -batchmode `
    -projectPath $projectRoot `
    -runTests `
    -testPlatform EditMode `
    -testResults $resultsPath `
    -logFile $logPath

if ($LASTEXITCODE -ne 0) {
    throw "EditMode tests failed with exit code $LASTEXITCODE. See $logPath"
}

Write-Host "EditMode tests passed. Results: $resultsPath"
