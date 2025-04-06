param (
    [Parameter(Mandatory=$true)]
    [string]$UnityProjectPath
)

# Sync DLL
.\sync-unity-dll.ps1

# Ensure target directory exists
$targetPath = Join-Path $UnityProjectPath "Packages\com.gamecore.unity"
if (-not (Test-Path $targetPath)) {
    New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
}
else {
    # Clear existing files but preserve meta files
    Get-ChildItem -Path $targetPath -Exclude "*.meta" | ForEach-Object {
        if ($_.PSIsContainer) {
            # For directories, remove all non-meta files inside
            Get-ChildItem -Path $_.FullName -Recurse -Exclude "*.meta" | Remove-Item -Force -Recurse
        }
        else {
            # For files, remove directly
            Remove-Item $_.FullName -Force
        }
    }
    Write-Host "Cleared existing package files while preserving .meta files" -ForegroundColor Yellow
}

# Copy Unity adapter as a local package
Write-Host "Copying Unity adapter to: $targetPath" -ForegroundColor Cyan
Get-ChildItem -Path "GameCore.Unity" -Exclude @("bin", "obj", "*.csproj") |
    ForEach-Object {
        if ($_.PSIsContainer) {
            Copy-Item -Path $_.FullName -Destination $targetPath -Recurse -Force
        } else {
            Copy-Item -Path $_.FullName -Destination $targetPath -Force
        }
    }

Write-Host "Preparation complete! Open your Unity project at $UnityProjectPath and check Package Manager" -ForegroundColor Green