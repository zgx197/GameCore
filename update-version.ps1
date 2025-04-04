param (
    [Parameter(Mandatory=$true)]
    [string]$Version
)

# 更新项目文件版本
$files = @(
    "GameCore.Core/GameCore.Core.csproj",
    "GameCore.Unity/GameCore.Unity.csproj",
    "GameCore.Godot/GameCore.Godot.csproj"
)

foreach ($file in $files) {
    (Get-Content $file) -replace '<Version>.*</Version>', "<Version>$Version</Version>" | Set-Content $file
}

# 更新GameCore.cs中的版本常量
$coreFile = "GameCore.Core/GameCore.cs"
(Get-Content $coreFile) -replace 'public static readonly string Version = ".*";', "public static readonly string Version = `"$Version`";" | Set-Content $coreFile

Write-Host "Updated version to $Version in all project files" -ForegroundColor Green