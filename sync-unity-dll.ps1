# 同步Core DLL到Unity插件目录
param (
    [string]$Configuration = "Release"
)

# 确保插件目录存在
$pluginsDir = "GameCore.Unity\Runtime\Plugins"
if (-not (Test-Path $pluginsDir)) {
    New-Item -Path $pluginsDir -ItemType Directory -Force | Out-Null
    Write-Host "创建插件目录: $pluginsDir" -ForegroundColor Yellow
}

# 构建Core项目
Write-Host "正在构建Core项目..." -ForegroundColor Cyan
dotnet build GameCore.Core -c $Configuration

# 复制DLL
$coreDll = "GameCore.Core\bin\$Configuration\netstandard2.1\GameCore.Core.dll"
if (Test-Path $coreDll) {
    Copy-Item -Path $coreDll -Destination $pluginsDir -Force
    Write-Host "已复制 $coreDll 到 $pluginsDir" -ForegroundColor Green
} else {
    Write-Error "找不到Core DLL: $coreDll"
    exit 1
}