# CreateNewFile 설치 관리자 사용 가이드

**문서 작성자**: 허창원 ((주)그린파워) with Claude Code Assistant

## 📋 개요

WiX Toolset을 사용한 CreateNewFile 설치 관리자 빌드 및 배포 가이드입니다.

---

## 🚀 빠른 시작

### 자동화된 빌드 (권장)

**Windows 배치 파일:**
```cmd
BuildInstaller.bat
```

**PowerShell 스크립트:**
```powershell
# 기본 빌드
.\BuildInstaller.ps1

# Clean 빌드
.\BuildInstaller.ps1 -Clean

# 상세 출력과 함께 빌드
.\BuildInstaller.ps1 -Verbose

# Clean + Verbose 빌드
.\BuildInstaller.ps1 -Clean -Verbose
```

---

## 🛠️ 수동 빌드 과정

### 1단계: CreateNewFile 애플리케이션 빌드
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile"

# Release 빌드 (테스트용)
dotnet build -c Release

# 배포용 게시 (권장)
dotnet publish -c Release --self-contained false -o "bin\Release\Publish"
```

### 2단계: MSI 설치 파일 생성
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Installer"

# MSI 빌드
wix build -arch x64 -src Package.wxs -out CreateNewFileSetup.msi
```

---

## 🖥️ Visual Studio에서 사용하기

### Visual Studio 2022에서
1. **솔루션 열기**: `CreateNewFile.sln`
2. **빌드 구성**: `Release` 선택
3. **솔루션 빌드**: `Ctrl+Shift+B` 또는 메뉴에서 빌드
4. **설치 관리자 빌드**: 
   - 솔루션 탐색기에서 `CreateNewFile.Installer` 프로젝트 선택
   - 오른쪽 클릭 → 빌드

### Visual Studio Code에서
1. **터미널 열기**: `Ctrl+Shift+` `
2. **빌드 스크립트 실행**:
   ```bash
   .\BuildInstaller.ps1
   ```

---

## 📁 프로젝트 구조

```
CreateNewFile/
├── src/
│   ├── CreateNewFile/                 # 메인 애플리케이션
│   │   ├── CreateNewFile.csproj
│   │   ├── bin/Release/Publish/       # 게시된 파일들
│   │   └── ...
│   └── CreateNewFile.Installer/       # 설치 관리자
│       ├── CreateNewFile.Installer.wixproj
│       ├── Package.wxs               # WiX 패키지 정의
│       ├── 1042.wxl                  # 한국어 리소스
│       ├── CreateNewFileSetup.msi    # 생성된 MSI 파일
│       └── ...
├── BuildInstaller.bat                # Windows 빌드 스크립트
├── BuildInstaller.ps1                # PowerShell 빌드 스크립트
└── INSTALLER_USAGE.md               # 이 파일
```

---

## ⚙️ 설치 관리자 설정 변경

### 버전 업데이트
**파일**: `src/CreateNewFile.Installer/Package.wxs`
```xml
<Package Name="CreateNewFile"
         Version="1.0.2"  <!-- 여기서 버전 변경 -->
         ...>
```

### 설치 경로 변경
**파일**: `src/CreateNewFile.Installer/Package.wxs`
```xml
<Property Id="INSTALLFOLDER" Value="C:\MyCompany\MyApp" />
```

### 제품 정보 변경
**파일**: `src/CreateNewFile.Installer/Package.wxs`
```xml
<Package Name="제품명"
         Manufacturer="제조사명"
         ...>
```

### 한국어 메시지 변경
**파일**: `src/CreateNewFile.Installer/1042.wxl`
```xml
<String Id="NetFrameworkRequired" 
        Value="사용자 정의 메시지" />
```

---

## 🔧 고급 설정

### .NET 런타임 체크 비활성화
**파일**: `src/CreateNewFile.Installer/Package.wxs`
```xml
<!-- 이 줄을 주석 처리 -->
<!--
<Launch Condition="NETFRAMEWORK8 OR Installed"
        Message="..." />
-->
```

### 추가 파일 포함
**파일**: `src/CreateNewFile.Installer/Package.wxs`
```xml
<Component Id="AdditionalFiles">
  <File Id="ReadmeFile"
        Source="..\..\README.txt"
        KeyPath="yes" />
</Component>
```

### 레지스트리 키 추가
```xml
<Component Id="RegistryEntries">
  <RegistryKey Root="HKLM" 
               Key="Software\GreenPower\CreateNewFile">
    <RegistryValue Name="InstallPath" 
                   Type="string" 
                   Value="[INSTALLFOLDER]" />
  </RegistryKey>
</Component>
```

---

## 🚨 문제 해결

### 자주 발생하는 오류

#### 1. "Undefined preprocessor variable" 오류
**원인**: 프로젝트 참조 또는 경로 문제
**해결**: Package.wxs에서 절대 경로 사용
```xml
<!-- 오류 발생 -->
Source="$(var.CreateNewFile.TargetDir)..."

<!-- 해결 -->
Source="..\CreateNewFile\bin\Release\Publish\..."
```

#### 2. "File not found" 오류
**원인**: CreateNewFile이 게시되지 않음
**해결**: 먼저 dotnet publish 실행
```bash
dotnet publish -c Release --self-contained false -o "bin\Release\Publish"
```

#### 3. WiX 빌드 오류
**원인**: WiX 도구가 설치되지 않음
**해결**: 
```bash
dotnet tool install --global wix
```

### 디버깅 팁

1. **상세 출력 활성화**:
   ```bash
   wix build -arch x64 -src Package.wxs -out CreateNewFileSetup.msi -v
   ```

2. **빌드 로그 확인**:
   ```bash
   wix build ... > build.log 2>&1
   ```

3. **MSI 내용 확인**:
   - Orca (Microsoft SDK 포함)
   - 또는 7-Zip으로 MSI 파일 열기

---

## 📝 배포 체크리스트

설치 관리자 배포 전 확인사항:

- [ ] CreateNewFile 애플리케이션이 정상 빌드됨
- [ ] 버전 번호가 올바르게 설정됨
- [ ] MSI 파일이 성공적으로 생성됨
- [ ] 테스트 시스템에서 설치 테스트 완료
- [ ] .NET 8 미설치 환경에서 오류 메시지 확인
- [ ] 제거 테스트 완료
- [ ] 바로가기 생성 확인

---

## 📞 지원 및 문의

- **개발자**: 허창원 (GreenPower Co., Ltd.)
- **프로젝트**: CreateNewFile
- **문서 업데이트**: 2025-08-25

---

## 🔄 업데이트 이력

- **v1.0.1** (2025-08-25): 초기 WiX 설치 관리자 구현
  - .NET 8 런타임 체크 기능
  - 한국어 UI 지원
  - Framework-dependent 배포