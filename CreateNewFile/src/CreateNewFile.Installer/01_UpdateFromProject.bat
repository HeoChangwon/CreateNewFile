@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo CreateNewFile Project Update
echo ========================================
echo.

:: Color setting
color 0A

:: Save current directory
set "INSTALLER_DIR=%CD%"

:: CreateNewFile project paths
set "PROJECT_DIR=%INSTALLER_DIR%\..\CreateNewFile"
set "PUBLISH_DIR=%PROJECT_DIR%\bin\Release\Publish"

echo Working Directory: %INSTALLER_DIR%
echo Project Directory: %PROJECT_DIR%
echo Publish Directory: %PUBLISH_DIR%
echo.

:: Step 1: Move to CreateNewFile project
echo [Step 1] Moving to CreateNewFile project directory...
cd /d "%PROJECT_DIR%"
if %ERRORLEVEL% neq 0 (
    echo ERROR: Cannot find CreateNewFile project directory
    echo Path: %PROJECT_DIR%
    color
    pause
    exit /b 1
)
echo OK: Project directory confirmed
echo.

:: Step 2: Clean previous publish files
echo [Step 2] Cleaning previous publish files...
if exist "%PUBLISH_DIR%" (
    echo Removing previous publish directory...
    rmdir /s /q "%PUBLISH_DIR%" 2>nul
    if exist "%PUBLISH_DIR%" (
        echo WARNING: Some files could not be deleted. Continuing...
    ) else (
        echo OK: Previous publish files cleaned
    )
) else (
    echo INFO: No previous publish files to clean
)
echo.

:: Step 3: Build CreateNewFile
echo [Step 3] Building CreateNewFile Release...
echo Running: dotnet build CreateNewFile.csproj -c Release
dotnet build CreateNewFile.csproj -c Release
if %ERRORLEVEL% neq 0 (
    echo ERROR: CreateNewFile build failed
    echo.
    echo Troubleshooting:
    echo   1. Check if project builds normally in Visual Studio
    echo   2. Ensure all NuGet packages are restored
    echo   3. Verify .NET 8 SDK is installed
    color
    pause
    exit /b 1
)
echo OK: Release build completed
echo.

:: Step 4: Publish CreateNewFile
echo [Step 4] Publishing CreateNewFile Framework-dependent...
echo Running: dotnet publish CreateNewFile.csproj
dotnet publish CreateNewFile.csproj -c Release --self-contained false -o "bin\Release\Publish"
if %ERRORLEVEL% neq 0 (
    echo ERROR: CreateNewFile publish failed
    echo.
    echo Troubleshooting:
    echo   1. Ensure build completed successfully
    echo   2. Check available disk space
    echo   3. Verify write permissions to bin directory
    color
    pause
    exit /b 1
)
echo OK: Publish completed
echo.

:: Step 5: Verify published files
echo [Step 5] Verifying published files...
if exist "%PUBLISH_DIR%\CreateNewFile.exe" (
    echo OK: CreateNewFile.exe found
    for %%A in ("%PUBLISH_DIR%\CreateNewFile.exe") do (
        set "exe_size=%%~zA"
    )
    echo    Size: %exe_size% bytes
) else (
    echo ERROR: CreateNewFile.exe was not published
    color
    pause
    exit /b 1
)

if exist "%PUBLISH_DIR%\config\appsettings.default.json" (
    echo OK: Configuration file found
) else (
    echo WARNING: Configuration file not found
)
echo.

:: Step 6: Return to installer directory
echo [Step 6] Returning to installer directory...
cd /d "%INSTALLER_DIR%"
echo OK: Returned to: %CD%
echo.

:: Completion message
echo ========================================
echo Project Update Completed!
echo ========================================
echo.
echo Completed Tasks:
echo   - CreateNewFile Release build
echo   - Framework-dependent publish (bin\Release\Publish)
echo   - Published files verification
echo.
echo Next Step:
echo   Run 02_BuildInstaller.bat to create MSI file
echo.

:: Check if called from 00_BuildAll.bat (skip auto-run prompt)
if "%1"=="BATCH_AUTO" (
    echo.
    echo Starting installer build...
    call "02_BuildInstaller.bat" BATCH_AUTO
) else (
    :: Option to auto-run next step (interactive mode)
    set /p "auto_next=Run installer build now? (y/n): "
    if /i "%auto_next%" == "y" (
        echo.
        echo Starting installer build...
        call "02_BuildInstaller.bat"
    ) else (
        echo.
        echo Press any key to exit...
        :: Restore default console colors before exit
        color
        pause > nul
    )
)