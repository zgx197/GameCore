param {
    [Paramater(mandatory=$true)]
    [string]$Version
    [string]$Message = "Release $Version"
}

# 确保所有文件已提交
$status = git status --porcelain
if ($status) {
    Write-Host "Error: Working directory not clean. Commit all changes before tagging." -ForegroundColor Red
    exit 1
}

# 创建注释标签
git tag -a "v$Version" -m "$Message"

# 推送标签到远程
git push origin "v$Version"

# 输出成功信息
Write-Host "Tagged release v$Version and pushed to remote." -ForegroundColor Green