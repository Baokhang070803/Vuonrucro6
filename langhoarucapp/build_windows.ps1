# PowerShell build script to create standalone EXE with PyInstaller
param(
    [string]$PythonExe = "python",
    [switch]$OneFile
)

$ErrorActionPreference = 'Stop'

Write-Host "==> Creating virtual environment (./.venv)" -ForegroundColor Cyan
if (!(Test-Path .venv)) {
    & $PythonExe -m venv .venv
}

$venvPython = Join-Path (Resolve-Path .venv) 'Scripts/python.exe'
& $venvPython -m pip install --upgrade pip
& $venvPython -m pip install -r requirements.txt
& $venvPython -m pip install pyinstaller

$extra = ""
if ($OneFile) { $extra = "--onefile" }

# Optional icon (put icon.ico in project root if you have it)
$iconArg = ""
if (Test-Path "icon.ico") { 
    $iconArg = "--icon icon.ico"
    Write-Host "==> Found icon.ico, will add to executable" -ForegroundColor Green
} else {
    Write-Host "==> No icon.ico found, building without icon" -ForegroundColor Yellow
}

# If using icon, ensure Pillow is installed so PyInstaller can convert if needed
if ($iconArg -ne "") {
    & $venvPython -c "import importlib,sys;sys.exit(0 if importlib.util.find_spec('PIL') else 1)"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "==> Installing Pillow (for icon processing)" -ForegroundColor Cyan
        & $venvPython -m pip install Pillow
    }
}

Write-Host "==> Building launcher executable" -ForegroundColor Cyan

# Remove old spec to avoid stale icon settings
$specFile = "LangHoaRucLauncher.spec"
if (Test-Path $specFile) { Remove-Item $specFile -Force }

# Build PyInstaller argument list (options must come BEFORE script name)
$pyiArgs = @(
    "--name","LangHoaRucLauncher",
    "--noconfirm",
    "--clean",
    "--windowed",
    "--add-data","img;img",
    "--exclude-module","tkinter"
)

# Nếu có br.png ở root thêm vào data (để resource_path tìm thấy)
if (Test-Path br.png) {
    $pyiArgs += @("--add-data","br.png;.")
}

if ($OneFile) { $pyiArgs += "--onefile" }
if ($iconArg -ne "") { 
    $pyiArgs += @("--icon","icon.ico")
    Write-Host "==> Adding icon to build arguments" -ForegroundColor Green
}

& $venvPython -m PyInstaller @pyiArgs "launcher.py"

if ($LASTEXITCODE -ne 0) {
    if ($pyiArgs -contains "--icon" ) {
        Write-Host "Icon build failed, retrying without icon..." -ForegroundColor Yellow
        if (Test-Path $specFile) { Remove-Item $specFile -Force }
        $pyiArgs = $pyiArgs | Where-Object { $_ -ne '--icon' -and $_ -ne 'icon.ico' }
        & $venvPython -m PyInstaller @pyiArgs "launcher.py"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "PyInstaller build failed again (exit code $LASTEXITCODE)" -ForegroundColor Red
            exit $LASTEXITCODE
        }
    } else {
        Write-Host "PyInstaller build failed (exit code $LASTEXITCODE)" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host "==> Output dist folder:" -ForegroundColor Green
if (Test-Path dist) {
    Get-ChildItem dist
    # Post-build verification (only for one-folder build)
    if (-not $OneFile) {
        $dllPath = Join-Path dist "LangHoaRucLauncher/_internal/python311.dll"
        if (Test-Path $dllPath) {
            Write-Host "[OK] Da tim thay python311.dll: $dllPath" -ForegroundColor Green
        } else {
            Write-Host "[CANH BAO] Khong tim thay python311.dll trong dist. Build bi loi hoac bi xoa." -ForegroundColor Yellow
        }
        Write-Host "`nLuu y: Phai copy NGUYEN THU MUC 'LangHoaRucLauncher' (ca _internal) chu KHONG chi moi LangHoaRucLauncher.exe." -ForegroundColor Cyan
        Write-Host "Muon chi 1 file duy nhat? Chay lai: powershell ./build_windows.ps1 -OneFile" -ForegroundColor Cyan
    }
} else {
    Write-Host "dist folder not found. Build may have failed." -ForegroundColor Yellow
}
