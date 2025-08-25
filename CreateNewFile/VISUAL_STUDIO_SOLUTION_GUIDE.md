# Visual Studio Solution Guide

**문서 작성자**: 허창원 ((주)그린파워) with Claude Code Assistant

## 🎯 해결된 문제

### ❌ 이전 문제:
- WiX 프로젝트가 솔루션에 포함되어 Visual Studio 로딩 실패
- 솔루션 빌드 시 WiX 오류로 인한 전체 빌드 실패
- "일부 중요한 가져오기가 없거나 참조된 SDK를 찾을 수 없음" 오류

### ✅ 현재 상태:
- **CreateNewFile.sln**: CreateNewFile 메인 프로젝트만 포함
- **WiX 프로젝트**: 별도 관리 (배치 스크립트로 빌드)
- **Visual Studio**: 정상 로딩 및 개발 가능

---

## 🚀 사용 방법

### Visual Studio에서 개발
1. **솔루션 열기**: `CreateNewFile.sln` 
2. **정상 개발**: F5 디버깅, 빌드, 테스트 모두 정상 작동
3. **프로젝트만 포함**: CreateNewFile 메인 애플리케이션만

### 설치 관리자 빌드
Visual Studio와 **별도로** 배치 스크립트 사용:

#### 방법 1: 전체 빌드 (가장 간단)
```cmd
00_BuildAll.bat
```

#### 방법 2: 단계별 빌드
```cmd
01_UpdateFromProject.bat  # CreateNewFile 빌드 및 게시
02_BuildInstaller.bat     # WiX MSI 생성
```

---

## 📁 프로젝트 구조

```
CreateNewFile/
├── src/
│   ├── CreateNewFile/              # 메인 프로젝트
│   │   ├── CreateNewFile.sln      # ✅ Visual Studio 솔루션
│   │   ├── CreateNewFile.csproj   # ✅ 메인 프로젝트
│   │   └── ... (소스 코드)
│   └── CreateNewFile.Installer/    # 설치 관리자 (독립)
│       ├── 00_BuildAll.bat        # 🆕 전체 빌드
│       ├── 01_UpdateFromProject.bat
│       ├── 02_BuildInstaller.bat
│       ├── Package.wxs
│       └── CreateNewFileSetup.msi # 생성 결과
```

---

## 🔧 개발 워크플로우

### 일반적인 개발 과정:
1. **Visual Studio에서 CreateNewFile 개발**
   - `CreateNewFile.sln` 열기
   - 코드 작성/수정
   - F5로 디버깅
   - 빌드 테스트

2. **배포 준비**
   - `CreateNewFile.Installer` 폴더로 이동
   - `00_BuildAll.bat` 실행
   - 생성된 MSI 파일 배포

### 설치 관리자만 재빌드:
```cmd
cd CreateNewFile.Installer
02_BuildInstaller.bat
```

---

## ✅ 장점

### Visual Studio 사용:
- ✅ 빠른 로딩
- ✅ 정상적인 IntelliSense
- ✅ 디버깅 완벽 지원
- ✅ NuGet 패키지 관리 정상
- ✅ Git 통합 정상

### 설치 관리자:
- ✅ WiX v6 완전 지원
- ✅ 자동화된 빌드 프로세스
- ✅ 오류 처리 및 가이드
- ✅ 다양한 빌드 옵션

---

## 🚨 주의사항

### Visual Studio에서:
- **WiX 프로젝트 추가 금지**: 솔루션에 WiX 프로젝트를 추가하면 로딩 오류 발생
- **솔루션 빌드**: CreateNewFile만 빌드됨 (정상)

### 설치 관리자에서:
- **WiX 도구 필요**: `dotnet tool install --global wix`
- **순서 중요**: 01번 먼저 실행 후 02번 실행
- **경로 의존성**: CreateNewFile.Installer 폴더에서 실행

---

## 💡 팁

### Visual Studio 최적화:
- **시작 프로젝트**: CreateNewFile로 설정
- **솔루션 구성**: Release/Debug 정상 작동
- **NuGet 복원**: 자동 복원 활성화

### 빌드 자동화:
```cmd
# 빠른 개발 사이클
01_UpdateFromProject.bat    # 코드 변경 후
02_BuildInstaller.bat       # 설치 관리자만 재빌드

# 완전한 빌드
00_BuildAll.bat            # 모든 과정 자동화
```

---

## 📊 문제 해결 결과

| 구분 | 이전 | 현재 | 개선 효과 |
|------|------|------|----------|
| Visual Studio 로딩 | ❌ 실패 | ✅ 성공 | 정상 개발 가능 |
| 솔루션 빌드 | ❌ WiX 오류 | ✅ 성공 | 안정적 빌드 |
| 설치 관리자 빌드 | ❌ 복잡 | ✅ 자동화 | 간편한 배포 |
| 개발 생산성 | ⚠️ 제한적 | ✅ 최적화 | 효율적 개발 |

---

**업데이트**: 2025-08-25  
**상태**: 모든 문제 해결 완료 ✅