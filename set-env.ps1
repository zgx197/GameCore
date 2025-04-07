# 设置开发环境变量
param(
    [string]$UnityPath = "D:\unity\2022.3.16f1c1\Editor\Data\Managed"
)

# 设置Unity托管程序集目录
$env:UNITY_MANAGED_DIR = $UnityPath
[Environment]::SetEnvironmentVariable('UNITY_MANAGED_DIR', $UnityPath, 'User')

Write-Host "环境变量已设置:"
Write-Host "UNITY_MANAGED_DIR = $env:UNITY_MANAGED_DIR"