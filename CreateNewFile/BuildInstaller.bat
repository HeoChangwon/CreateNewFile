@echo off
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

if exist "CreateNewFileSetup.msi" (
    del "CreateNewFileSetup.msi"
    echo 이전 MSI 파일 삭제됨
)

if exist "CreateNewFileSetup.wixpdb" (
    del "CreateNewFileSetup.wixpdb"
    echo 이전 PDB 파일 삭제됨
)

:: 3단계: WiX MSI 패키지 빌드
echo [3/3] WiX MSI 패키지 빌드 중...
wix build -arch x64 -src Package.wxs -out CreateNewFileSetup.msi

if %ERRORLEVEL% neq 0 (
    echo 오류: MSI 빌드 실패
    pause
    exit /b 1
)

echo [3/3] 완료: MSI 빌드 성공
echo.

:: 결과 확인
if exist "CreateNewFileSetup.msi" (
    echo ========================================
    echo 🎉 설치 관리자 빌드 완료!
    echo ========================================
    echo.
    echo 📁 출력 파일: CreateNewFileSetup.msi
    echo 📍 위치: %CD%\CreateNewFileSetup.msi
    
    :: 파일 크기 확인
    for %%A in ("CreateNewFileSetup.msi") do (
        set "size=%%~zA"
    )
    echo 📏 크기: %size% bytes
    
    echo.
    echo 💡 이 파일을 사용자에게 배포하면 됩니다.
    echo 💡 설치 시 .NET 8 Desktop Runtime이 필요합니다.
) else (
    echo ❌ 오류: MSI 파일이 생성되지 않았습니다.
)

echo.
echo 아무 키나 누르면 종료합니다...
pause > nul

:: 원래 디렉토리로 복원
cd /d "%ORIGINAL_DIR%"