param (
    [Parameter(Mandatory=$true)]
    [string]$Project, # Core, Unity, All
    
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$Message = ""
)

# 验证项目类型
if ($Project -notin @("Core", "Unity", "All")) {
    Write-Error "无效的项目类型. 使用 Core, Unity, 或 All"
    exit 1
}

# 更新版本号
if ($Project -eq "All") {
    Write-Host "更新所有项目版本号为 $Version" -ForegroundColor Cyan
    .\update-version.ps1 -Project Core -Version $Version
    .\update-version.ps1 -Project Unity -Version $Version
} else {
    Write-Host "更新 $Project 版本号为 $Version" -ForegroundColor Cyan
    .\update-version.ps1 -Project $Project -Version $Version
}

# 确保所有更改已提交
$status = git status --porcelain
if ($status) {
    Write-Host "警告: 工作目录不干净. 继续之前请提交所有更改." -ForegroundColor Yellow
    Write-Host $status
    $continue = Read-Host "确定要继续吗? (y/N)"
    if ($continue -ne "y") {
        exit 0
    }
}

# 生成默认消息
if (-not $Message) {
    if ($Project -eq "All") {
        $Message = "发布 Core $Version 和 Unity 适配器 $Version"
    } else {
        $Message = "发布 $Project $Version"
    }
}

# 提交更改
git add Directory.Build.props
if ($Project -eq "Unity" -or $Project -eq "All") {
    git add GameCore.Unity/CHANGELOG.md GameCore.Unity/package.json
}

git commit -m "chore(release): $Message"

# 推送更改
git push origin main

# 提示下一步操作
if ($Project -eq "Core" -or $Project -eq "All") {
    Write-Host "`n要完成Core库发布，请在GitHub上创建新Release:" -ForegroundColor Green
    Write-Host "1. 访问: https://github.com/yourusername/GameCore/releases/new"
    Write-Host "2. 标签: v$Version"
    Write-Host "3. 标题: GameCore $Version"
    Write-Host "4. 添加发布说明"
    Write-Host "5. 点击 'Publish release'"
}

if ($Project -eq "Unity" -or $Project -eq "All") {
    Write-Host "`nUnity适配器将自动发布到UPM分支" -ForegroundColor Green
    Write-Host "用户可以通过以下URL添加包:"
    Write-Host "https://github.com/yourusername/GameCore.git#upm"
}