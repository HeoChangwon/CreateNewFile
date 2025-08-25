@echo off
chcp 65001 >nul 2>&1
echo ========================================
echo CreateNewFile Complete Build Process
echo ========================================
echo.

:: Color setting
color 0E

echo This script will:
echo   1. Update CreateNewFile project
echo   2. Build WiX installer
echo   3. Display results
echo.

set /p "confirm=Continue? (y/n): "
if /i not "%confirm%" == "y" (
    echo Operation cancelled.
    pause
    exit /b 0
)

echo.
echo ========================================
echo Step 1: Updating CreateNewFile Project  
echo ========================================
call "01_UpdateFromProject.bat" BATCH_AUTO
:: Note: BATCH_AUTO parameter prevents duplicate prompts and runs installer build automatically

echo.
echo ========================================
echo Complete Build Process Finished!
echo ========================================
echo.

:: Check if MSI file was actually created (using pattern matching for versioned filename)
set "MSI_FOUND="
for %%f in (CreateNewFileSetup_v*.msi) do (
    set "MSI_FOUND=%%f"
    goto :check_result
)

:check_result
if defined MSI_FOUND (
    echo ✅ SUCCESS: All operations completed successfully!
    echo Your installer is ready for distribution.
    echo.
    for %%A in ("%MSI_FOUND%") do (
        echo 📦 Final Result:
        echo    File: %MSI_FOUND%
        echo    Size: %%~zA bytes
        echo    Created: %%~tA
    )
    echo    Full Path: %CD%\%MSI_FOUND%
) else (
    echo ❌ ERROR: MSI file was not created
    echo Please check the build logs above for errors.
    echo.
    echo Checked for: CreateNewFileSetup_v*.msi
    echo Current directory: %CD%
    echo Directory contents:
    dir CreateNewFile*.msi 2>nul
)

echo.
:: Restore default console colors before exit
color
pause