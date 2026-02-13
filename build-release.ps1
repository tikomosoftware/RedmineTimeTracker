# Redmine Time Tracker デュアルリリースビルドスクリプト
# 2つのビルドを作成: フレームワーク依存版（軽量）と自己完結型版（単一EXE）+ Vector用パッケージ

param(
    [string]$Version = "1.0.0"
)

Write-Host "--- Redmine Time Tracker v$Version Dual Release Build ---" -ForegroundColor Cyan
Write-Host ""

# 変数定義
$ProjectFile = "redmineSupTool.csproj"
$OutputName = "RedmineTimeTracker" # Zipファイル名などに使用
$DistDir = "dist"
$TempFrameworkDir = "$DistDir\temp_framework"
$TempStandaloneDir = "$DistDir\temp_standalone"
$TempVectorDir = "$DistDir\temp_vector"
$FrameworkZipFile = "$DistDir\$OutputName-v$Version-framework-dependent.zip"
$StandaloneZipFile = "$DistDir\$OutputName-v$Version-standalone.zip"
$VectorZipFile = "$DistDir\$OutputName-v$Version-vector.zip"

# ビルド開始時刻を記録
# $BuildStartTime = Get-Date

# Create dist directory if it doesn't exist
if (!(Test-Path $DistDir)) {
    New-Item -ItemType Directory -Path $DistDir | Out-Null
}

# Cleanup
foreach ($path in @($TempFrameworkDir, $TempStandaloneDir, $TempVectorDir, $FrameworkZipFile, $StandaloneZipFile, $VectorZipFile)) {
    if (Test-Path $path) {
        try { Remove-Item -Path $path -Recurse -Force -ErrorAction Stop } catch {
            Write-Host "Warning: Could not remove $path. It might be locked." -ForegroundColor Magenta
        }
    }
}

# ========================================
# 1. フレームワーク依存ビルド（軽量版）
# ========================================
Write-Host "Building Framework-Dependent (Lightweight)..." -ForegroundColor Yellow
try {
    dotnet publish $ProjectFile -c Release --self-contained false /p:PublishSingleFile=false /p:DebugType=none /p:DebugSymbols=false --output $TempFrameworkDir
    if ($LASTEXITCODE -eq 0) {
        if (Test-Path "README.md") { Copy-Item "README.md" -Destination $TempFrameworkDir }
        if (Test-Path "README.en.md") { Copy-Item "README.en.md" -Destination $TempFrameworkDir }
        if (Test-Path "USER_GUIDE.md") { Copy-Item "USER_GUIDE.md" -Destination $TempFrameworkDir }
        if (Test-Path "USER_GUIDE.en.md") { Copy-Item "USER_GUIDE.en.md" -Destination $TempFrameworkDir }
        # Add a small delay to ensure file handles are released
        Start-Sleep -Seconds 1
        Compress-Archive -Path "$TempFrameworkDir\*" -DestinationPath $FrameworkZipFile
        Write-Host "  [OK] Framework-dependent build completed" -ForegroundColor Green
    }
}
catch { Write-Host "  [ERROR] Framework-dependent build failed: $($_.Exception.Message)" -ForegroundColor Red }

# ========================================
# 2. 自己完結型ビルド（単一EXE版）
# ========================================
Write-Host ""
Write-Host "Building Self-Contained (Single EXE)..." -ForegroundColor Yellow
$standaloneBuildSuccess = $false
try {
    # Clean before build
    if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue }

    dotnet publish $ProjectFile -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true /p:DebugType=none /p:DebugSymbols=false --output $TempStandaloneDir
    if ($LASTEXITCODE -eq 0) {
        if (Test-Path "README.md") { Copy-Item "README.md" -Destination $TempStandaloneDir }
        if (Test-Path "README.en.md") { Copy-Item "README.en.md" -Destination $TempStandaloneDir }
        if (Test-Path "USER_GUIDE.md") { Copy-Item "USER_GUIDE.md" -Destination $TempStandaloneDir }
        if (Test-Path "USER_GUIDE.en.md") { Copy-Item "USER_GUIDE.en.md" -Destination $TempStandaloneDir }
        # Add a delay to ensure file handles are released
        Start-Sleep -Seconds 5
        Compress-Archive -Path "$TempStandaloneDir\*" -DestinationPath $StandaloneZipFile
        Write-Host "  [OK] Self-contained build completed" -ForegroundColor Green
        $standaloneBuildSuccess = $true
    }
}
catch { Write-Host "  [ERROR] Self-contained build failed: $($_.Exception.Message)" -ForegroundColor Red }

# ========================================
# 3. Vector用パッケージ（自己完結型 + Vector用README）
# ========================================
Write-Host ""
Write-Host "Building Vector Package..." -ForegroundColor Yellow
try {
    if ($standaloneBuildSuccess) {
        New-Item -ItemType Directory -Path $TempVectorDir | Out-Null
        Copy-Item -Path "$TempStandaloneDir\*" -Destination $TempVectorDir -Recurse -Force
        
        # README_VECTOR.md を README.md として配置
        if (Test-Path (Join-Path $TempVectorDir "README.md")) { Remove-Item (Join-Path $TempVectorDir "README.md") -Force }
        if (Test-Path "README_VECTOR.md") { Copy-Item "README_VECTOR.md" (Join-Path $TempVectorDir "README.md") -Force }
        
        Compress-Archive -Path "$TempVectorDir\*" -DestinationPath $VectorZipFile
        Write-Host "  [OK] Vector package completed" -ForegroundColor Green
    }
}
catch { Write-Host "  [ERROR] Vector package failed: $($_.Exception.Message)" -ForegroundColor Red }

# Cleanup
foreach ($path in @($TempFrameworkDir, $TempStandaloneDir, $TempVectorDir)) {
    if (Test-Path $path) { Remove-Item -Path $path -Recurse -Force }
}

Write-Host ""
Write-Host "--- Build Finished! ---" -ForegroundColor Green
Write-Host "Packages are located in: $DistDir\" -ForegroundColor White
