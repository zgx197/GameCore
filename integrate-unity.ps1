param {
    [string]$UnityProjectPath = "D:\work\GameCoreUnity";
    [string]$Configuration = "Debug";
}

# 确保Unity项目存在
if (-Not (Test-Path $UnityProjectPath)) {
    Write-Host "Creating Unity sample project directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "$UnityProjectPath/Assets/Plugins/GameCore" -Force
}

# 复制核心库到Unity项目
$targetPath = "$UnityProjectPath/Assets/Plugins/GameCore"
$coreBinPath = "GameCore.Core/bin/$Configuration/netstandard2.1"
$unityBinPath = "GameCore.Unity/bin/$Configuration/netstandard2.1"

# 复制DLL和XML文档
Write-Host "Copying DLLs to Unity project..." -ForegroundColor Yellow
Copy-Item "$coreBinPath/GameCore.Core.dll" -Destination $targetPath -Force
Copy-Item "$coreBinPath/GameCore.Core.xml" -Destination $targetPath -Force
Copy-Item "$unityBinPath/GameCore.Unity.dll" -Destination $targetPath -Force
Copy-Item "$unityBinPath/GameCore.Unity.xml" -Destination $targetPath -Force

# 复制依赖项
$deps = Get-ChildItem "$coreBinPath/*.dll" -Exclude "GameCore.*.dll"
foreach ($dep in $deps) {
    Copy-Item $dep.FullName -Destination $targetPath -Force
}

Write-Host "Unity integration complete!" -ForegroundColor Green