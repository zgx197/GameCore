param (
    [Parameter(Mandatory=$true)]
    [string]$UnityProjectPath
)

# 同步DLL
.\sync-unity-dll.ps1

# 确保目标目录存在
$targetPath = Join-Path $UnityProjectPath "Packages\com.gamecore.unity"
if (-not (Test-Path $targetPath)) {
    New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
}

# 复制Unity适配器作为本地包
Write-Host "正在复制Unity适配器到: $targetPath" -ForegroundColor Cyan
Get-ChildItem -Path "GameCore.Unity" -Exclude @("bin", "obj", "*.csproj") |
    ForEach-Object {
        if ($_.PSIsContainer) {
            Copy-Item -Path $_.FullName -Destination $targetPath -Recurse -Force
        } else {
            Copy-Item -Path $_.FullName -Destination $targetPath -Force
        }
    }

Write-Host "准备完成! 在Unity编辑器中打开 $UnityProjectPath 并检查Package Manager" -ForegroundColor Green