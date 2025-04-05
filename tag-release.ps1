param (
    [Parameter(Mandatory=$true)]
    [string]$Project = "Core",  # Core, Unity, Godot
    [string]$Message = ""
)

# 确认项目类型有效
if ($Project -notin @("Core", "Unity", "Godot")) {
    Write-Error "Invalid project type. Use Core, Unity, or Godot."
    exit 1
}

# 从Directory.Build.props获取版本
$buildProps = "Directory.Build.props"
if (-not (Test-Path $buildProps)) {
    Write-Error "Directory.Build.props not found."
    exit 1
}

$xml = [xml](Get-Content $buildProps)
$version = $xml.Project.PropertyGroup."${Project}Version"

# 如果没有提供消息，生成默认消息
if (-not $Message) {
    $Message = "Release GameCore.$Project v$version"
}

# 确保所有文件已提交
$status = git status --porcelain
if ($status) {
    Write-Host "Error: Working directory not clean. Commit all changes before tagging." -ForegroundColor Red
    exit 1
}

# 创建注释标签 (使用项目前缀区分不同项目)
$tagName = "$Project/v$version"
git tag -a $tagName -m "$Message"

# 推送标签到远程
git push origin $tagName

# 输出成功信息
Write-Host "Tagged $Project release $tagName and pushed to remote." -ForegroundColor Green