param (
    [Parameter(Mandatory=$true)]
    [string]$Project = "Core",  # Core, Unity, Godot
    [Parameter(Mandatory=$true)]
    [string]$Version
)

# 确认项目类型有效
if ($Project -notin @("Core", "Unity", "Godot")) {
    Write-Error "Invalid project type. Use Core, Unity, or Godot."
    exit 1
}

# 更新Directory.Build.props中的版本
$buildProps = "Directory.Build.props"
if (-not (Test-Path $buildProps)) {
    Write-Error "Directory.Build.props not found. Please create it first."
    exit 1
}

# 读取文件内容
$content = Get-Content $buildProps

# 根据项目类型更新对应版本
$propertyName = "${Project}Version"
$content = $content -replace "<$propertyName>.*</$propertyName>", "<$propertyName>$Version</$propertyName>"

# 如果是Core项目，也更新VersionPrefix
if ($Project -eq "Core") {
    $content = $content -replace "<VersionPrefix>.*</VersionPrefix>", "<VersionPrefix>$Version</VersionPrefix>"
    
    # 更新GameCore.cs中的静态版本字段(如果存在)
    $coreFile = "GameCore.Core/GameCore.cs"
    if (Test-Path $coreFile) {
        $coreContent = Get-Content $coreFile
        if ($coreContent -match 'public static readonly string Version = ".*";') {
            $coreContent = $coreContent -replace 'public static readonly string Version = ".*";', "public static readonly string Version = `"$Version`";"
            Set-Content -Path $coreFile -Value $coreContent
        }
    }
}

# 保存更新后的文件
Set-Content -Path $buildProps -Value $content

Write-Host "Updated $Project version to $Version in Directory.Build.props" -ForegroundColor Green