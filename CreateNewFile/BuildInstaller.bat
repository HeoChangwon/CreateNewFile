@echo off
setlocal EnableDelayedExpansion
echo ========================================
echo CreateNewFile 설치 관리자 빌드 스크립트
echo ========================================
echo.

:: 현재 디렉토리 저장
set "ORIGINAL_DIR=%CD%"

:: 1단계: CreateNewFile 애플리케이션 게시
echo [1/3] CreateNewFile 애플리케이션 게시 중...
cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile"

dotnet publish -c Release --self-contained false -o "bin\Release\Publish"
if %ERRORLEVEL% neq 0 (
    echo 오류: CreateNewFile 게시 실패
    pause
    exit /b 1
)

echo [1/3] 완료: CreateNewFile 게시 성공
echo.

:: 2단계: 이전 MSI 파일 정리
echo [2/3] 이전 설치 파일 정리 중...
cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"

:: 이전 버전의 MSI 파일들 정리 (패턴 매칭)
for %%f in (CreateNewFileSetup*.msi) do (
    if exist "%%f" (
        del "%%f"
        echo 이전 MSI 파일 삭제됨: %%f
    )
)

for %%f in (CreateNewFileSetup*.wixpdb) do (
    if exist "%%f" (
        del "%%f"
        echo 이전 PDB 파일 삭제됨: %%f
    )
)

:: 3단계: WiX MSI 패키지 빌드
echo [3/3] WiX MSI 패키지 빌드 중...
:: 타임스탬프 생성
for /f "tokens=2 delims==" %%i in ('wmic OS Get localdatetime /value') do set "dt=%%i"
set "YYYY=%dt:~0,4%"
set "MM=%dt:~4,2%"
set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%"
set "MIN=%dt:~10,2%"
set "BUILD_TIMESTAMP=%YYYY%%MM%%DD%_%HH%%MIN%"

echo 타임스탬프: %BUILD_TIMESTAMP%
echo 이전 빌드 결과물 정리 중...
dotnet clean CreateNewFile.Installer.wixproj -c Release >nul 2>&1

:: 02_BuildInstaller.bat를 BATCH_AUTO 모드로 실행 (타임스탬프 전달)
cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"
call 02_BuildInstaller.bat BATCH_AUTO %BUILD_TIMESTAMP%
set "INSTALLER_ERRORLEVEL=%ERRORLEVEL%"
cd /d "D:\Work_Claude\CreateNewFile\CreateNewFile"

echo Installer 스크립트 종료 코드: %INSTALLER_ERRORLEVEL%
if %INSTALLER_ERRORLEVEL% neq 0 (
    echo 오류: MSI 빌드 실패 (종료 코드: %INSTALLER_ERRORLEVEL%)
    pause
    exit /b 1
)

echo [3/3] 완료: MSI 빌드 성공
echo.

:: MSI 파일이 생성되었는지 확인 (Installer 디렉토리에서)
set "MSI_FILENAME="

echo 확인 중: 생성된 MSI 파일 검색
echo 검색 경로: src\CreateNewFile.Installer\CreateNewFileSetup_v*.msi

:: 패턴으로 생성된 MSI 파일 찾기 (02_BuildInstaller.bat가 성공했으므로 반드시 있음)
for %%f in (src\CreateNewFile.Installer\CreateNewFileSetup_v*.msi) do (
    echo 발견: %%f
    set "MSI_FILENAME=%%f"
    goto :found_msi
)

:: 만약 패턴으로도 못 찾으면 직접 디렉토리 확인
if not defined MSI_FILENAME (
    echo 경고: 패턴 매칭으로 MSI 파일을 찾지 못함
    echo Installer 디렉토리 내용 확인:
    dir src\CreateNewFile.Installer\*.msi 2>nul
)

:found_msi
:: 결과 확인
if defined MSI_FILENAME if exist "%MSI_FILENAME%" (
    echo ========================================
    echo 🎉 설치 관리자 빌드 완료!
    echo ========================================
    echo.
    :: 실제 파일명 추출
    for %%f in ("%MSI_FILENAME%") do set "ACTUAL_FILENAME=%%~nxf"
    echo 📁 출력 파일: !ACTUAL_FILENAME!
    echo 📍 위치: %CD%\%MSI_FILENAME%
    
    :: 파일 크기 확인
    for %%A in ("%MSI_FILENAME%") do (
        set "size=%%~zA"
    )
    echo 📏 크기: %size% bytes
    
    echo.
    echo 💡 이 파일을 사용자에게 배포하면 됩니다.
    echo 💡 설치 시 .NET 8 Desktop Runtime이 필요합니다.
) else (
    echo ❌ 오류: MSI 파일을 최종 위치에서 찾을 수 없습니다.
    echo.
    echo 예상 파일명: %EXPECTED_MSI%
    echo 예상 경로: %INSTALLER_MSI_PATH%
    echo 현재 디렉토리: %CD%
    echo.
    echo 상세 확인 중...
    
    :: Installer 디렉토리의 MSI 파일들 확인
    echo [Installer 디렉토리 MSI 파일 목록]:
    dir src\CreateNewFile.Installer\CreateNewFileSetup_v*.msi 2>nul
    if %ERRORLEVEL% neq 0 (
        echo   - Installer 디렉토리에 MSI 파일 없음
    )
    
    :: bin 폴더 확인
    echo.
    echo [bin 폴더 하위 MSI 파일들]:
    dir src\CreateNewFile.Installer\bin\*.msi /s 2>nul
    if %ERRORLEVEL% neq 0 (
        echo   - bin 폴더에 MSI 파일 없음
    )
    
    echo.
    echo 가능한 원인:
    echo   1. 02_BuildInstaller.bat에서 파일 복사가 실패했을 수 있음
    echo   2. 타임스탬프 불일치로 잘못된 파일명을 찾고 있을 수 있음
    echo   3. 빌드는 성공했지만 파일이 예상 위치로 이동되지 않았을 수 있음
)

echo.
echo 아무 키나 누르면 종료합니다...
pause > nul

:: 원래 디렉토리로 복원
cd /d "%ORIGINAL_DIR%"