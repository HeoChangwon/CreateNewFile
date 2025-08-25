# Visual Studio에서 CreateNewFile 개발 가이드

**문서 작성자**: 허창원 ((주)그린파워) with Claude Code Assistant

## ⚠️ 중요 안내: WiX v6 Visual Studio 호환성

현재 **WiX Toolset v6**는 Visual Studio에서 완전히 지원되지 않습니다. 이는 알려진 호환성 문제이며, WiX 팀에서 해결 작업 중입니다.

---

## 🎯 권장 개발 워크플로우

### Visual Studio에서 CreateNewFile 개발
✅ **정상 작동**: CreateNewFile 메인 애플리케이션 개발
```
1. Visual Studio에서 CreateNewFile.sln 열기
2. CreateNewFile 프로젝트에서 코드 개발
3. F5로 디버그/실행
4. Ctrl+Shift+B로 빌드
```

### 설치 관리자 빌드
❌ **Visual Studio에서 직접 빌드 불가**: WiX v6 호환성 문제  
✅ **대안**: 자동화된 스크립트 사용

---

## 🔧 설치 관리자 빌드 방법

### 방법 1: 간단한 배치 파일 (권장)
```cmd
BuildInstaller.bat
```

### 방법 2: PowerShell 스크립트
```powershell
.\BuildInstaller.ps1
```

### 방법 3: Visual Studio 통합 터미널
1. Visual Studio에서 `보기` → `터미널` 열기
2. 다음 명령 실행:
```cmd
BuildInstaller.bat
```

### 방법 4: Visual Studio Code
1. VSCode에서 프로젝트 폴더 열기
2. `Ctrl+Shift+` ` 로 터미널 열기
3. PowerShell 스크립트 실행:
```powershell
.\BuildInstaller.ps1 -Clean -Verbose
```

---

## 🛠️ Visual Studio에서 설치 관리자 개발

### WiX 파일 편집
Visual Studio에서 WiX 파일을 편집할 수 있습니다:

1. **Package.wxs**: XML 에디터로 열림
   - 문법 하이라이팅 지원
   - IntelliSense 부분 지원

2. **1042.wxl**: XML 에디터로 열림
   - 한국어 리소스 편집 가능

### 권장 확장 프로그램
- **XML Tools**: XML 문법 검사 및 포맷팅
- **PowerShell**: PowerShell 스크립트 편집 지원

---

## 🚀 개발 → 배포 워크플로우

### 1. CreateNewFile 수정
```
Visual Studio → CreateNewFile 프로젝트 → 코드 수정 → 빌드 테스트
```

### 2. 설치 관리자 설정 수정 (필요시)
```
Visual Studio → Package.wxs 편집 → 저장
```

### 3. 배포 파일 생성
```
BuildInstaller.bat 실행 → CreateNewFileSetup.msi 생성
```

### 4. 배포
```
CreateNewFileSetup.msi → 사용자에게 배포
```

---

## 🔍 디버깅 및 문제 해결

### CreateNewFile 애플리케이션 디버깅
✅ **Visual Studio에서 정상 지원**:
- F9: 중단점 설정
- F5: 디버그 시작
- F10/F11: 단계별 실행

### 설치 관리자 디버깅
❌ **Visual Studio에서 직접 디버깅 불가**  
✅ **대안 방법**:

1. **상세 빌드 로그 확인**:
```powershell
.\BuildInstaller.ps1 -Verbose
```

2. **WiX 빌드 로그**:
```cmd
wix build -arch x64 -src Package.wxs -out CreateNewFileSetup.msi -v > build.log
```

3. **MSI 내용 확인**:
- Orca 도구 사용 (Windows SDK 포함)
- 또는 7-Zip으로 MSI 파일 열기

---

## 🎛️ Visual Studio 설정 최적화

### 솔루션 탐색기 설정
1. **솔루션 탐색기**에서 `모든 파일 표시` 활성화
2. WiX 프로젝트가 회색으로 표시되어도 정상 (로드되지 않음)
3. CreateNewFile 프로젝트만 활성 상태로 개발

### 빌드 구성
- **시작 프로젝트**: CreateNewFile 설정
- **빌드 구성**: Release (배포용) / Debug (개발용)
- **플랫폼**: Any CPU 또는 x64

---

## 📋 빠른 참조

### 자주 사용하는 명령어
```cmd
# CreateNewFile 빌드만
dotnet build -c Release

# CreateNewFile 게시
dotnet publish -c Release --self-contained false -o "bin\Release\Publish"

# 설치 관리자 빌드
BuildInstaller.bat

# 상세 출력과 함께 빌드
.\BuildInstaller.ps1 -Clean -Verbose
```

### 중요 파일 경로
- **메인 애플리케이션**: `src/CreateNewFile/`
- **설치 관리자**: `src/CreateNewFile.Installer/`
- **배포 파일**: `src/CreateNewFile.Installer/CreateNewFileSetup.msi`
- **게시된 파일**: `src/CreateNewFile/bin/Release/Publish/`

---

## 💡 팁과 권장사항

### 1. 개발 효율성
- CreateNewFile 코드 개발은 Visual Studio에서
- 설치 관리자 빌드는 스크립트로 자동화
- 두 과정을 분리해서 생각하기

### 2. 버전 관리
- CreateNewFile.csproj에서 버전 업데이트
- Package.wxs에서도 동일한 버전으로 업데이트
- 빌드 스크립트로 자동 동기화

### 3. 테스트
- Visual Studio에서 CreateNewFile 기능 테스트
- 별도 시스템에서 MSI 설치 테스트

---

## 🔮 향후 개선 계획

WiX Toolset v6의 Visual Studio 지원이 개선되면:
1. Visual Studio에서 직접 WiX 프로젝트 빌드 가능
2. IntelliSense 완전 지원
3. 통합 디버깅 지원

현재로는 **하이브리드 접근법** (VS + 스크립트)이 가장 효율적입니다.

---

**업데이트**: 2025-08-25  
**다음 확인**: WiX Toolset 공식 Visual Studio 지원 업데이트 시