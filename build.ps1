# 参数
param {
    [string]$Configuration = "Debug"
}

# 构建所有项目
Write-Host "Building GameCore in $Configuration configuration..."
dotnet build -c $Configuration

# 运行测试单元
Write-Host "Running tests..."
dotnet test -c $Configuration --no-build

# 输出成功信息
Write-Host "Build completed successfully!" -ForegroundColor Green