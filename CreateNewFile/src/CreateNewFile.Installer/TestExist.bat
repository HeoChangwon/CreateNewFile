@echo off
chcp 65001 >nul 2>&1

cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"

set "MSI_FILE=CreateNewFileSetup.msi"

echo Testing file existence check...
echo File to check: %MSI_FILE%
echo Current directory: %CD%
echo.

if exist "%MSI_FILE%" (
    echo ✅ File EXISTS
    for %%A in ("%MSI_FILE%") do (
        echo    Size: %%~zA bytes
        echo    Date: %%~tA
        echo    Full path: %%~fA
    )
) else (
    echo ❌ File NOT EXISTS
    echo Directory contents:
    dir CreateNewFile*
)

pause