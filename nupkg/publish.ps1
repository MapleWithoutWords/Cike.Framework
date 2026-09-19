# publish.ps1 - 一键构建并发布 NuGet 包
# 用法: ./publish.ps1 -Version "1.0.60026"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

# 从 .env 文件读取 API Key
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$envFile = Join-Path $scriptPath ".env"

if (Test-Path $envFile) {
    $envContent = Get-Content $envFile -Raw
    if ($envContent -match 'NUGET_API_KEY=(.+)') {
        $apiKey = $Matches[1].Trim()
    }
}

if ([string]::IsNullOrEmpty($apiKey)) {
    Write-Host "错误: 未找到 NUGET_API_KEY" -ForegroundColor Red
    Write-Host "请在 nupkg/.env 文件中设置: NUGET_API_KEY=your-api-key" -ForegroundColor Yellow
    exit 1
}

$rootFolder = Split-Path -Parent $scriptPath
$propsFile = Join-Path $rootFolder "Directory.Build.props"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NuGet Package Publisher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 步骤1: 更新版本号
Write-Host "[1/3] 更新版本号为: $Version" -ForegroundColor Yellow
$content = Get-Content -Path $propsFile -Raw
$content = $content -replace '<Version>.*?</Version>', "<Version>$Version</Version>"
Set-Content -Path $propsFile -Value $content -NoNewline
Write-Host "  ✓ 版本已更新" -ForegroundColor Green

# 步骤2: 运行 common.ps1 和 pack.ps1
Write-Host ""
Write-Host "[2/3] 打包项目..." -ForegroundColor Yellow
Set-Location $scriptPath

Write-Host "  运行 common.ps1..."
powershell -ExecutionPolicy Bypass -File .\common.ps1

Write-Host "  运行 pack.ps1..."
powershell -ExecutionPolicy Bypass -File .\pack.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ 打包失败" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ 打包完成" -ForegroundColor Green

# 步骤3: 推送到 NuGet
Write-Host ""
Write-Host "[3/3] 推送包到 NuGet.org..." -ForegroundColor Yellow
powershell -ExecutionPolicy Bypass -File .\push_packages.ps1 $apiKey

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ 推送失败" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ 全部完成！版本 $Version 已发布" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
