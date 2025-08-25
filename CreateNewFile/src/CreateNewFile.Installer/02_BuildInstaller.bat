@echo off
setlocal EnableDelayedExpansion
chcp 65001 >nul 2>&1
echo ========================================
echo CreateNewFile Installer Build
echo ========================================
echo.

:: Color setting
color 0B

:: Current directory (installer directory)
set "INSTALLER_DIR=%CD%"
:: 동적 파일명 생성 (버전 + 타임스탬프)
if "%2" neq "" (
    set "BUILD_TIMESTAMP=%2"
) else (
    for /f "tokens=2 delims==" %%i in ('wmic OS Get localdatetime /value') do set "dt=%%i"
    set "YYYY=%dt:~0,4%"
    set "MM=%dt:~4,2%"
    set "DD=%dt:~6,2%"
    set "HH=%dt:~8,2%"
    set "MIN=%dt:~10,2%"
    set "BUILD_TIMESTAMP=%YYYY%%MM%%DD%_%HH%%MIN%"
)
set "MSI_FILE=CreateNewFileSetup_v1.0.001_Build_%BUILD_TIMESTAMP%.msi"
set "PDB_FILE=CreateNewFileSetup_v1.0.001_Build_%BUILD_TIMESTAMP%.wixpdb"

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
:: 이전 버전의 MSI 파일들 정리 (패턴 매칭)
for %%f in (CreateNewFileSetup*.msi) do (
    if exist "%%f" (
        echo Removing previous MSI file: %%f
        del "%%f" /f /q 2>nul
    )
)

for %%f in (CreateNewFileSetup*.wixpdb) do (
    if exist "%%f" (
        echo Removing previous PDB file: %%f
        del "%%f" /f /q 2>nul
    )
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
echo Running: dotnet build CreateNewFile.Installer.wixproj -c Release
echo.

dotnet build CreateNewFile.Installer.wixproj -c Release

echo.
:: WiX 프로젝트 빌드 시 실제 생성 위치 확인 (x64/Release/ko-KR 또는 다른 위치)
set "BUILT_MSI_PATH1=bin\x64\Release\ko-KR\%MSI_FILE%"
set "BUILT_MSI_PATH2=bin\Release\%MSI_FILE%"
set "BUILT_MSI_PATH3=bin\x64\Release\%MSI_FILE%"

:: 실제로 생성된 MSI 파일을 패턴으로 찾아서 복사
set "FOUND_MSI="
set "FOUND_PDB="

:: bin\x64\Release\ko-KR\ 폴더에서 패턴 매칭으로 찾기
for %%f in (bin\x64\Release\ko-KR\CreateNewFileSetup_v*.msi) do (
    if exist "%%f" (
        set "FOUND_MSI=%%f"
        set "FOUND_PDB=%%~dpnf.wixpdb"
        goto :copy_files
    )
)

:: bin\Release\ 폴더에서 패턴 매칭으로 찾기
for %%f in (bin\Release\CreateNewFileSetup_v*.msi) do (
    if exist "%%f" (
        set "FOUND_MSI=%%f"
        set "FOUND_PDB=%%~dpnf.wixpdb"
        goto :copy_files
    )
)

:: bin\x64\Release\ 폴더에서 패턴 매칭으로 찾기
for %%f in (bin\x64\Release\CreateNewFileSetup_v*.msi) do (
    if exist "%%f" (
        set "FOUND_MSI=%%f"
        set "FOUND_PDB=%%~dpnf.wixpdb"
        goto :copy_files
    )
)

:copy_files
if defined FOUND_MSI (
    echo OK: MSI build completed - Found: !FOUND_MSI!
    echo Copying MSI file to current directory...
    :: 실제 파일명 추출
    for %%f in ("!FOUND_MSI!") do set "ACTUAL_MSI_NAME=%%~nxf"
    for %%f in ("!FOUND_PDB!") do set "ACTUAL_PDB_NAME=%%~nxf"
    
    copy "!FOUND_MSI!" "!ACTUAL_MSI_NAME!" >nul 2>&1
    if exist "!FOUND_PDB!" (
        copy "!FOUND_PDB!" "!ACTUAL_PDB_NAME!" >nul 2>&1
    )
    :: 복사 성공 확인
    if exist "!ACTUAL_MSI_NAME!" (
        echo Success: MSI file copied to !ACTUAL_MSI_NAME!
        :: MSI_FILE 변수를 실제 파일명으로 업데이트
        set "MSI_FILE=!ACTUAL_MSI_NAME!"
        set "PDB_FILE=!ACTUAL_PDB_NAME!"
    ) else (
        echo Warning: MSI file copy may have failed
    )
) else if exist "%MSI_FILE%" (
    echo OK: MSI build completed - Found in current directory
) else (
    echo ERROR: MSI file not found in expected locations
    echo.
    echo Checked locations:
    echo   - %BUILT_MSI_PATH1%
    echo   - %BUILT_MSI_PATH2%
    echo   - %BUILT_MSI_PATH3%
    echo   - %MSI_FILE%
    echo.
    echo Directory contents in bin folder:
    dir bin /s *.msi 2>nul
    echo.
    echo Troubleshooting:
    echo   1. Check Package.wxs file for XML syntax errors
    echo   2. Verify all referenced file paths are correct
    echo   3. Run with verbose logging: dotnet build -v detailed
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
    dir CreateNewFileSetup*.msi 2>nul
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
    exit /b 0
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