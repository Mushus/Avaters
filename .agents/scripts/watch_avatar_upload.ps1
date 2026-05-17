param(
    [int]$IntervalSeconds = 20,
    [int]$MaxMinutes = 20
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$snapshotPath = Join-Path $root "Products\_publish_reports\sdk-builder-snapshot.md"
$deadline = (Get-Date).AddMinutes($MaxMinutes)
$finishedSeen = $false

function Invoke-UnityCode([string]$Code) {
    $argv = @("execute-dynamic-code", "--code", $Code)
    & uloop @argv | Out-Host
}

Push-Location $root
try {
    while ((Get-Date) -lt $deadline) {
        Invoke-UnityCode "Mushus.EditorTools.AvatarPublishPipeline.PumpVisibleSdkUploadUi(); return 1;"

        if (Test-Path $snapshotPath) {
            $snapshot = Get-Content $snapshotPath -Raw
            $status = [regex]::Match($snapshot, "Status: `([^`]+)`").Groups[1].Value
            $platforms = [regex]::Match($snapshot, "Supported platforms: `([^`]+)`").Groups[1].Value
            $updated = [regex]::Match($snapshot, "Last updated: `([^`]+)`").Groups[1].Value
            Write-Host "SDK status=$status platforms=$platforms updated=$updated"

            if ($status -eq "upload-finished" -and $platforms -match "Android" -and $platforms -match "iOS" -and $platforms -match "Windows") {
                if ($finishedSeen) {
                    Write-Host "Upload finished and finish panel close was already pumped."
                    exit 0
                }

                $finishedSeen = $true
                Start-Sleep -Seconds 2
                continue
            }
        }

        Start-Sleep -Seconds $IntervalSeconds
    }

    throw "Timed out waiting for SDK upload after $MaxMinutes minute(s). Check $snapshotPath or take a screenshot fallback."
}
finally {
    Pop-Location
}
