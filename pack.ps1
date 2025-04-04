param (
    [string]$Version = "0.0.1",
    [string]$Configuration = "Release"
)

# 清理旧的构建
Remove-Item -Path artifacts -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path artifacts

# 设置版本号
$coreProj = "GameCore.Core/GameCore.Core.csproj"
$unityProj = "GameCore.Unity/GameCore.Unity.csproj"
$godotProj = "GameCore.Godot/GameCore.Godot.csproj"

# 更新版本号
(Get-Content $coreProj) -replace '<Version>(.*)</Version>', "<Version>$Version</Version>" | Set-Content $coreProj

# 构建项目
dotnet build -c $Configuration

# 打包
dotnet pack GameCore.Core -c $Configuration -o artifacts
dotnet pack GameCore.Unity -c $Configuration -o artifacts
dotnet pack GameCore.Godot -c $Configuration -o artifacts

# 输出包路径
Write-Host "Packages created in ./artifacts:" -ForegroundColor Green
Get-ChildItem artifacts