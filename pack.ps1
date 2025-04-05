param (
    [string]$Configuration = "Release",
    [string[]]$Projects = @("Core", "Unity", "Godot")
)

# 从Directory.Build.props获取版本
$buildProps = "Directory.Build.props"
if (-not (Test-Path $buildProps)) {
    Write-Error "Directory.Build.props not found. Please create it first."
    exit 1
}

$xml = [xml](Get-Content $buildProps)

# 清理旧的构建
Remove-Item -Path artifacts -Recurse -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path artifacts

# 构建项目
dotnet build -c $Configuration

# 打包指定的项目
foreach ($project in $Projects) {
    $projectPath = "GameCore.$project"
    $version = $xml.Project.PropertyGroup."${project}Version"
    
    Write-Host "Packing $projectPath (Version $version)..." -ForegroundColor Cyan
    dotnet pack $projectPath -c $Configuration -o artifacts
}

# 输出包路径
Write-Host "Packages created in ./artifacts:" -ForegroundColor Green
Get-ChildItem artifacts