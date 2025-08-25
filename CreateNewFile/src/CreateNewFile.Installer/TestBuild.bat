@echo off
chcp 65001 >nul 2>&1
echo Testing WiX build process...

cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"

echo Current directory: %CD%
echo.

echo Deleting old MSI file...
if exist "CreateNewFileSetup.msi" del "CreateNewFileSetup.msi"

echo Running WiX build...
wix build -arch x64 -src Package.wxs -out CreateNewFileSetup.msi

echo Error level: %ERRORLEVEL%
echo.

if exist "CreateNewFileSetup.msi" (
    for %%A in ("CreateNewFileSetup.msi") do (
        echo SUCCESS: MSI file created
        echo Size: %%~zA bytes
        echo Time: %%~tA
    )
) else (
    echo ERROR: MSI file not created
)

pause