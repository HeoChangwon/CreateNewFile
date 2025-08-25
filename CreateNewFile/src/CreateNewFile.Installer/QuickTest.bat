@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo Quick MSI Build Test
echo ========================================
echo.

cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"

set "MSI_FILE=CreateNewFileSetup.msi"

echo Cleaning previous MSI file...
if exist "%MSI_FILE%" del "%MSI_FILE%"

echo.
echo Running WiX build...
wix build -arch x64 -src Package.wxs -out "%MSI_FILE%"

echo.
if exist "%MSI_FILE%" (
    for %%A in ("%MSI_FILE%") do (
        echo ✅ SUCCESS: MSI file created
        echo    File: %MSI_FILE%
        echo    Size: %%~zA bytes
        echo    Time: %%~tA
    )
) else (
    echo ❌ ERROR: MSI file was not created
)

echo.
pause