# CreateNewFile 설치 관리자 빌드 스크립트 (PowerShell)
param(
    [switch]$Clean = $false,
    [switch]$Verbose = $false
)

# 색상 출력을 위한 함수
function Write-ColorOutput($Message, $Color = "White") {
    Write-Host $Message -ForegroundColor $Color
}

# 진행 상황 표시
function Write-Progress-Step($StepNumber, $TotalSteps, $Description) {
    Write-ColorOutput "[$StepNumber/$TotalSteps] $Description" "Cyan"
}

# 오류 처리
function Test-CommandSuccess($Description) {
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ 오류: $Description 실패 (Exit Code: $LASTEXITCODE)" "Red"
        exit 1
    }
    Write-ColorOutput "✅ 완료: $Description 성공" "Green"
}

# 현재 디렉토리 저장
$OriginalLocation = Get-Location

try {
    Write-ColorOutput "========================================" "Yellow"
    Write-ColorOutput "CreateNewFile 설치 관리자 빌드 스크립트" "Yellow"
    Write-ColorOutput "========================================" "Yellow"
    Write-Host ""

    # 경로 설정
    $ProjectDir = "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile"
    $InstallerDir = "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"

    # 1단계: CreateNewFile 애플리케이션 게시
    Write-Progress-Step 1 4 "CreateNewFile 애플리케이션 게시 중..."
    Set-Location $ProjectDir

    if ($Clean) {
        Write-ColorOutput "🧹 Clean 빌드 수행 중..." "Yellow"
        & dotnet clean -c Release
        if ($Verbose) { Test-CommandSuccess "Clean 빌드" }
    }

    & dotnet publish -c Release --self-contained false -o "bin\Release\Publish"
    Test-CommandSuccess "CreateNewFile 게시"
    Write-Host ""

    # 2단계: 파일 크기 확인
    Write-Progress-Step 2 4 "게시된 파일 정보 확인 중..."
    $PublishPath = Join-Path $ProjectDir "bin\Release\Publish"
    $ExeFile = Join-Path $PublishPath "CreateNewFile.exe"
    
    if (Test-Path $ExeFile) {
        $ExeSize = (Get-Item $ExeFile).Length
        Write-ColorOutput "📁 CreateNewFile.exe: $([math]::Round($ExeSize/1MB, 2)) MB" "Gray"
    }
    Write-Host ""

    # 3단계: 이전 MSI 파일 정리
    Write-Progress-Step 3 4 "이전 설치 파일 정리 중..."
    Set-Location $InstallerDir

    $MsiFile = "CreateNewFileSetup.msi"
    $PdbFile = "CreateNewFileSetup.wixpdb"

    if (Test-Path $MsiFile) {
        Remove-Item $MsiFile -Force
        Write-ColorOutput "🗑️ 이전 MSI 파일 삭제됨" "Gray"
    }

    if (Test-Path $PdbFile) {
        Remove-Item $PdbFile -Force
        Write-ColorOutput "🗑️ 이전 PDB 파일 삭제됨" "Gray"
    }
    Write-Host ""

    # 4단계: WiX MSI 패키지 빌드
    Write-Progress-Step 4 4 "WiX MSI 패키지 빌드 중..."
    
    if ($Verbose) {
        & wix build -arch x64 -src Package.wxs -out $MsiFile -v
    } else {
        & wix build -arch x64 -src Package.wxs -out $MsiFile
    }
    
    Test-CommandSuccess "MSI 빌드"
    Write-Host ""

    # 결과 확인 및 출력
    if (Test-Path $MsiFile) {
        Write-ColorOutput "========================================" "Green"
        Write-ColorOutput "🎉 설치 관리자 빌드 완료!" "Green"
        Write-ColorOutput "========================================" "Green"
        Write-Host ""
        
        $MsiSize = (Get-Item $MsiFile).Length
        $MsiPath = (Resolve-Path $MsiFile).Path
        
        Write-ColorOutput "📁 출력 파일: $MsiFile" "White"
        Write-ColorOutput "📍 전체 경로: $MsiPath" "Gray"
        Write-ColorOutput "📏 파일 크기: $([math]::Round($MsiSize/1MB, 2)) MB" "White"
        Write-Host ""
        
        Write-ColorOutput "💡 이 파일을 사용자에게 배포하면 됩니다." "Yellow"
        Write-ColorOutput "💡 설치 시 .NET 8 Desktop Runtime이 필요합니다." "Yellow"
        
        # 설치 파일이 생성된 폴더 열기 옵션
        Write-Host ""
        $OpenFolder = Read-Host "설치 파일이 있는 폴더를 여시겠습니까? (y/n)"
        if ($OpenFolder -eq 'y' -or $OpenFolder -eq 'Y') {
            Start-Process "explorer.exe" -ArgumentList "/select,`"$MsiPath`""
        }
        
    } else {
        Write-ColorOutput "❌ 오류: MSI 파일이 생성되지 않았습니다." "Red"
        exit 1
    }

} catch {
    Write-ColorOutput "❌ 예상치 못한 오류 발생: $($_.Exception.Message)" "Red"
    exit 1
} finally {
    # 원래 디렉토리로 복원
    Set-Location $OriginalLocation
}

Write-Host ""
Write-ColorOutput "빌드 스크립트 실행 완료." "Green"