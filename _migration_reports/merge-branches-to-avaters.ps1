param(
  [switch]$DryRun,
  [string]$Target = "C:\Users\wyndf\Documents\unity\Avaters"
)

$ErrorActionPreference = "Stop"

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$targetRoot = $Target
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportDir = Join-Path $targetRoot "_migration_reports"
$reportPath = Join-Path $reportDir ("merge-$stamp.csv")
$conflictRoot = Join-Path $targetRoot ("_conflicts\merge-$stamp")
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ("avaters-merge-$stamp")

$branchProducts = [ordered]@{
  "catchy-lp" = "CatchyLp"
  "iris" = "Iris"
  "iris-bk1" = "IrisBk1"
  "jack-o-nyantan-2022" = "JackONyantan2022"
  "jack-o-nyantan2" = "JackONyantan2"
  "latest" = "Latest"
  "mocha" = "Mocha"
  "neneko" = "Neneko"
  "red-dragon-lp" = "RedDragonLp"
  "reptan" = "Reptan"
  "tora" = "Tora"
  "usa" = "Shiromaru"
  "whip-lp" = "WhipLp"
  "windra-lp" = "WindraLp"
  "wip" = "Wip"
  "unity-chang" = "UnityChang"
}

function Convert-ToSafeName([string]$name) {
  return ($name -replace '[\\/:*?"<>|]', '_')
}

function Get-RelativePath([string]$base, [string]$path) {
  $baseUri = [Uri]((Resolve-Path $base).Path.TrimEnd('\') + '\')
  $pathUri = [Uri](Resolve-Path $path).Path
  return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($pathUri).ToString()).Replace('/', '\')
}

function Get-HashOrNull([string]$path) {
  if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    return $null
  }
  return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
}

function Add-ReportRow(
  [System.Collections.Generic.List[object]]$rows,
  [string]$branch,
  [string]$kind,
  [string]$source,
  [string]$destination,
  [string]$action,
  [string]$note
) {
  $rows.Add([pscustomobject]@{
    Branch = $branch
    Kind = $kind
    Source = $source
    Destination = $destination
    Action = $action
    Note = $note
  })
}

function Copy-Or-Report(
  [System.Collections.Generic.List[object]]$rows,
  [string]$branch,
  [string]$kind,
  [string]$sourcePath,
  [string]$destPath,
  [string]$sourceLabel
) {
  $sourceHash = Get-HashOrNull $sourcePath
  $destKey = $destPath.ToLowerInvariant()
  $destHash = $null
  $destSource = $null

  if ($script:plannedHashes.ContainsKey($destKey)) {
    $destHash = $script:plannedHashes[$destKey]
    $destSource = $script:plannedSources[$destKey]
  }
  else {
    $destHash = Get-HashOrNull $destPath
    if ($null -ne $destHash) {
      $destSource = "existing-target"
      $script:plannedHashes[$destKey] = $destHash
      $script:plannedSources[$destKey] = $destSource
    }
  }

  if ($null -eq $destHash) {
    Add-ReportRow $rows $branch $kind $sourceLabel $destPath "copy" ""
    $script:plannedHashes[$destKey] = $sourceHash
    $script:plannedSources[$destKey] = "$branch`:$sourceLabel"
    if (-not $DryRun) {
      New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destPath) | Out-Null
      Copy-Item -LiteralPath $sourcePath -Destination $destPath -Force
    }
    return
  }

  if ($sourceHash -eq $destHash) {
    Add-ReportRow $rows $branch $kind $sourceLabel $destPath "skip-identical" $destSource
    return
  }

  $conflictPath = Join-Path $conflictRoot (Join-Path (Convert-ToSafeName $branch) $sourceLabel)
  Add-ReportRow $rows $branch $kind $sourceLabel $destPath "conflict-copy-to" "$conflictPath ; existing=$destSource"
  if (-not $DryRun) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $conflictPath) | Out-Null
    Copy-Item -LiteralPath $sourcePath -Destination $conflictPath -Force
  }
}

Set-Location $repo
New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

$rows = [System.Collections.Generic.List[object]]::new()
$plannedHashes = @{}
$plannedSources = @{}
$branches = & git branch --format='%(refname:short)'

try {
  foreach ($branch in $branches) {
    if ($branch -in @("master", "feature/unity2022")) {
      continue
    }

    $product = $branchProducts[$branch]
    if ([string]::IsNullOrWhiteSpace($product)) {
      $product = Convert-ToSafeName $branch
    }

    $safeBranch = Convert-ToSafeName $branch
    $worktree = Join-Path $tempRoot $safeBranch
    & git worktree add --detach $worktree $branch | Out-Null

    try {
      $assetsRoot = Join-Path $worktree "unity\Assets"
      if (Test-Path -LiteralPath $assetsRoot -PathType Container) {
        Get-ChildItem -LiteralPath $assetsRoot -Recurse -File -Force | ForEach-Object {
          $rel = Get-RelativePath (Join-Path $worktree "unity") $_.FullName
          $dest = Join-Path $targetRoot $rel
          Copy-Or-Report $rows $branch "unity" $_.FullName $dest $rel
        }
      }

      foreach ($dir in @("booth", "src")) {
        $sourceRoot = Join-Path $worktree $dir
        if (Test-Path -LiteralPath $sourceRoot -PathType Container) {
          Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Force | ForEach-Object {
            $rel = Get-RelativePath $worktree $_.FullName
            $dest = Join-Path $targetRoot (Join-Path "Products\$product" $rel)
            Copy-Or-Report $rows $branch "product" $_.FullName $dest $rel
          }
        }
      }
    }
    finally {
      & git worktree remove --force $worktree | Out-Null
    }
  }
}
finally {
  if (Test-Path -LiteralPath $tempRoot) {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force
  }
}

$rows | Export-Csv -LiteralPath $reportPath -NoTypeInformation -Encoding UTF8

$summary = $rows |
  Group-Object Action |
  Sort-Object Name |
  ForEach-Object { [pscustomobject]@{ Action = $_.Name; Count = $_.Count } }

$summary | Format-Table -AutoSize
Write-Host "Report: $reportPath"
if (-not $DryRun) {
  Write-Host "Conflicts: $conflictRoot"
}
