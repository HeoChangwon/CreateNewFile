@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo CreateNewFile Installer Build
echo ========================================
echo.

:: Color setting
color 0B

:: Current directory (installer directory)
set "INSTALLER_DIR=%CD%"
set "MSI_FILE=CreateNewFileSetup.msi"
set "PDB_FILE=CreateNewFileSetup.wixpdb"

echo Working Directory: %INSTALLER_DIR%
echo Output File: %MSI_FILE%
echo.

:: Check WiX tool
echo [Pre-check] Verifying WiX Toolset installation...
wix --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ERROR: WiX Toolset is not installed or not in PATH
    echo.
    echo Solution:
    echo   Install WiX Toolset with the following command:
    echo   dotnet tool install --global wix
    echo.
    color
    pause
    exit /b 1
)

for /f "delims=" %%i in ('wix --version 2^>nul') do set "wix_version=%%i"
echo OK: WiX Toolset found (Version: %wix_version%)
echo.

:: Check published files
echo [Step 1] Checking published files...
set "PROJECT_PUBLISH=..\CreateNewFile\bin\Release\Publish"
if exist "%PROJECT_PUBLISH%\CreateNewFile.exe" (
    echo OK: CreateNewFile.exe file found
    for %%A in ("%PROJECT_PUBLISH%\CreateNewFile.exe") do (
        set "exe_size=%%~zA"
    )
    echo    Location: %PROJECT_PUBLISH%\CreateNewFile.exe
    echo    Size: %exe_size% bytes
) else (
    echo ERROR: Published CreateNewFile.exe not found
    echo    Location: %PROJECT_PUBLISH%\CreateNewFile.exe
    echo.
    echo Solution:
    echo    Run 01_UpdateFromProject.bat first to publish the project
    color
    pause
    exit /b 1
)

if exist "%PROJECT_PUBLISH%\config\appsettings.default.json" (
    echo OK: Configuration file found
) else (
    echo WARNING: Configuration file not found. Continuing...
)
echo.

:: Step 2: Clean previous MSI files
echo [Step 2] Cleaning previous installer files...
if exist "%MSI_FILE%" (
    echo Removing previous MSI file: %MSI_FILE%
    del "%MSI_FILE%" /f /q 2>nul
    if exist "%MSI_FILE%" (
        echo WARNING: %MSI_FILE% may be in use
        echo    Close the file in File Explorer and try again
        color
        pause
        exit /b 1
    )
)

if exist "%PDB_FILE%" (
    echo Removing previous PDB file: %PDB_FILE%
    del "%PDB_FILE%" /f /q 2>nul
)

echo OK: Previous files cleaned
echo.

:: Step 3: Check WiX source files
echo [Step 3] Checking WiX source files...
if exist "Package.wxs" (
    echo OK: Package.wxs file found
) else (
    echo ERROR: Package.wxs file not found
    color
    pause
    exit /b 1
)

if exist "1042.wxl" (
    echo OK: Korean resource file found
) else (
    echo WARNING: Korean resource file ^(1042.wxl^) missing
)
echo.

:: Step 4: Build WiX MSI
echo [Step 4] Building WiX MSI...
echo Running: wix build -arch x64 -src Package.wxs -out %MSI_FILE%
echo.

wix build -arch x64 -src Package.wxs -out "%MSI_FILE%"

echo.
:: Check if MSI file was actually created (more reliable than ERRORLEVEL)
if exist "%MSI_FILE%" (
    echo OK: MSI build completed
) else (
    echo ERROR: MSI build failed - file not created
    echo.
    echo Troubleshooting:
    echo   1. Check Package.wxs file for XML syntax errors
    echo   2. Verify all referenced file paths are correct
    echo   3. Build with verbose logging:
    echo      wix build -arch x64 -src Package.wxs -out %MSI_FILE% -v
    echo.
    pause
    exit /b 1
)
echo.

:: Step 5: Verify build results
echo [Step 5] Verifying build results...

:: Use a more robust file check
if not exist "%MSI_FILE%" (
    echo ERROR: MSI file was not created
    echo Checking directory contents...
    dir CreateNewFile*.msi 2>nul
    color
    pause
    exit /b 1
)

echo OK: MSI file created successfully!
echo    Filename: %MSI_FILE%
for %%A in ("%MSI_FILE%") do (
    echo    Size: %%~zA bytes
    echo    Created: %%~tA
)
echo    Full Path: %INSTALLER_DIR%\%MSI_FILE%
echo.

:: Completion message
echo ========================================
echo Installer Build Completed!
echo ========================================
echo.
echo Generated File: %MSI_FILE%
echo Location: %INSTALLER_DIR%
for %%A in ("%MSI_FILE%") do echo Size: %%~zA bytes
echo.
echo Deployment Info:
echo   - Requires .NET 8 Desktop Runtime (auto-check)
echo   - Install Path: C:\GreenPower\CreateNewFile
echo   - Creates Start Menu shortcut
echo   - Creates Desktop shortcut
echo   - Removable from Control Panel
echo.

:: Check if called from batch auto mode
if "%1"=="BATCH_AUTO" (
    echo.
    echo Build completed successfully for batch automation.
) else (
    :: Interactive mode - show additional actions
    echo Additional Actions:
    echo   1. Open folder containing installer file
    echo   2. Test installation
    echo   3. Exit
    echo.

    set /p "choice=Select option (1-3): "

    if "%choice%"=="1" (
        echo Opening folder...
        explorer /select,"%INSTALLER_DIR%\%MSI_FILE%"
    ) else if "%choice%"=="2" (
        echo Running test installation...
        echo WARNING: Administrator privileges may be required
        start "" "%INSTALLER_DIR%\%MSI_FILE%"
    ) else (
        echo Build completed. Exiting...
    )
)

echo.
echo Press any key to exit...
:: Restore default console colors before exit
color
pause > nul