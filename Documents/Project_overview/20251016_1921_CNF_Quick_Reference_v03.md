# CreateNewFile Quick Reference v03
**문서 생성**: 2025-10-16 19:21    
**버전**: v03  
**작성자**: 허창원 ((주)그린파워)  
**AI 지원**: Claude Code Assistant

---

## 📌 프로젝트 개요

### 기본 정보
- **프로젝트명**: CreateNewFile
- **버전**: 1.0.003
- **플랫폼**: Windows 10 이상
- **프레임워크**: .NET 8.0 WPF
- **개발사**: (주)그린파워
- **개발자**: 허창원 (HeoChangwon)

### 프로젝트 목적
템플릿 기반 파일 생성 도구로, 사용자가 정의한 프리셋과 템플릿을 활용하여 다양한 형식의 파일을 빠르게 생성하는 생산성 향상 도구입니다.

---

## 🏗️ 프로젝트 구조

```
CreateNewFile/
├── CreateNewFile/                    # 메인 프로젝트
│   ├── src/CreateNewFile/
│   │   ├── Models/                  # 데이터 모델
│   │   │   ├── AppSettings.cs       # 애플리케이션 설정 모델
│   │   │   ├── PresetItem.cs        # 프리셋 아이템 모델 (간소화됨)
│   │   │   └── FileCreationRequest.cs
│   │   ├── ViewModels/              # MVVM 뷰모델
│   │   │   ├── MainViewModel.cs     # 메인 창 뷰모델
│   │   │   ├── SettingsViewModel.cs # 설정 창 뷰모델
│   │   │   └── BaseViewModel.cs
│   │   ├── Views/                   # WPF 뷰
│   │   │   ├── MainWindow.xaml.cs
│   │   │   └── SettingsWindow.xaml.cs
│   │   ├── Services/                # 비즈니스 로직
│   │   │   ├── ISettingsService.cs
│   │   │   ├── SettingsService.cs   # 설정 관리 (캐시 기능 포함)
│   │   │   ├── IFileGeneratorService.cs
│   │   │   └── FileGeneratorService.cs
│   │   ├── Helpers/                 # 유틸리티
│   │   │   ├── FileNameBuilder.cs
│   │   │   └── ValidationHelper.cs
│   │   └── Resources/               # 리소스
│   │       └── CreateNewFile.ico
│   ├── CreateNewFile.Tests/         # 단위 테스트
│   └── config/
│       ├── appsettings.json         # 사용자 설정 (런타임 생성)
│       └── appsettings.default.json # 기본 설정 템플릿
├── NSIS_installer/                  # NSIS 인스톨러
│   ├── CreateNewFile_Installer.nsi  # NSIS 스크립트
│   ├── 10_BuildAll.py              # 전체 빌드 스크립트
│   ├── 11_UpdateFromProject.py     # 프로젝트 업데이트 스크립트
│   ├── 12_BuildInstaller.py        # 인스톨러 빌드 스크립트
│   ├── LICENSE.txt                  # 라이센스 파일
│   └── publish/                     # 빌드 출력
│       └── framework-dependent/
└── Documents/                       # 문서
    ├── 2025/10/                    # 월별 작업 내역
    └── Project_overview/            # 프로젝트 개요 문서
```

---

## 🔧 기술 스택

### 핵심 기술
- **.NET 8.0**: 최신 .NET 플랫폼
- **WPF (Windows Presentation Foundation)**: UI 프레임워크
- **XAML**: UI 마크업 언어
- **MVVM Pattern**: Model-View-ViewModel 아키텍처
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

### 테스트 및 품질
- **xUnit**: 단위 테스트 프레임워크
- **Moq**: 모킹 라이브러리
- **FluentAssertions**: 어설션 라이브러리 (선택적)

### 설정 및 데이터
- **JSON**: 설정 파일 형식
- **System.Text.Json**: JSON 직렬화/역직렬화

### 배포
- **NSIS**: Nullsoft Scriptable Install System
- **Python 3.x**: 빌드 자동화 스크립트

---

## 💻 빌드 및 실행 명령어

### 프로젝트 빌드
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile"
dotnet build
```

### 애플리케이션 실행
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile"
dotnet run
```

### 테스트 실행
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile.Tests"
dotnet test
```

### 특정 테스트 클래스 실행
```bash
dotnet test --filter "ClassName=PresetItemTests"
```

### 특정 테스트 메서드 실행
```bash
dotnet test --filter "FullyQualifiedName~PresetItemTests.Constructor_DefaultValues_SetsCorrectDefaults"
```

### NSIS 인스톨러 빌드 (전체 프로세스)
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\NSIS_installer"
python 10_BuildAll.py
```

### NSIS 인스톨러 빌드 (인스톨러만)
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\NSIS_installer"
python 12_BuildInstaller.py
```

---

## 🎯 핵심 아키텍처

### MVVM 패턴
- **Model**: 데이터 모델 및 비즈니스 로직
  - `AppSettings`: 전체 설정 데이터
  - `PresetItem`: 프리셋 아이템 (Id, Value, IsEnabled만 포함)
  - `FileCreationRequest`: 파일 생성 요청

- **View**: XAML 기반 UI
  - `MainWindow.xaml`: 메인 창
  - `SettingsWindow.xaml`: 설정 창

- **ViewModel**: UI와 Model 중개
  - `MainViewModel`: 메인 창 로직
  - `SettingsViewModel`: 설정 창 로직
  - `BaseViewModel`: 공통 기능 (INotifyPropertyChanged)

### Dependency Injection
- **컨테이너 설정**: `App.xaml.cs`에서 구성
- **서비스 등록**:
  - `ISettingsService` → `SettingsService` (Singleton)
  - `IFileGeneratorService` → `FileGeneratorService` (Singleton)
  - `MainViewModel` (Transient)
  - `SettingsViewModel` (Transient)

- **중요**: MainWindow와 MainViewModel이 동일한 SettingsService 인스턴스를 사용하도록 주의

### 비동기 프로그래밍
- 모든 서비스 작업은 `async/await` 패턴 사용
- UI 스레드 차단 방지
- 파일 I/O, 설정 저장/로드는 비동기 처리

---

## 📂 주요 컴포넌트

### 1. SettingsService (설정 관리)

**파일**: `src/CreateNewFile/Services/SettingsService.cs`

**주요 기능**:
- JSON 기반 설정 파일 관리
- 캐시를 통한 성능 최적화
- 스레드 안전성 보장 (lock 사용)
- 기본 설정 복사 및 사용자 설정 생성

**중요 메서드**:
```csharp
Task<AppSettings> LoadSettingsAsync()           // 설정 로드
Task SaveSettingsAsync(AppSettings settings)     // 설정 저장
void ClearCache()                                // 캐시 무효화 ⭐ 중요!
```

**캐시 무효화**:
- 외부에서 설정 파일을 직접 수정한 경우 반드시 `ClearCache()` 호출
- 예: MainWindow에서 창 위치를 직접 저장한 후 캐시 무효화 필요

**설정 파일 위치**:
- 사용자 설정: `config/appsettings.json`
- 기본 설정: `config/appsettings.default.json`

### 2. PresetItem (프리셋 아이템 모델)

**파일**: `src/CreateNewFile/Models/PresetItem.cs`

**간소화된 구조** (v03):
```csharp
public class PresetItem
{
    public string Id { get; set; }           // 고유 식별자
    public string Value { get; set; }        // 표시 값
    public bool IsEnabled { get; set; }      // 활성화 여부
}
```

**제거된 속성** (더 이상 사용하지 않음):
- ~~Description~~
- ~~CreatedAt~~
- ~~IsFavorite~~
- ~~DisplayText~~

**주의사항**: 테스트 및 문서 작성 시 제거된 속성을 참조하지 않도록 주의

### 3. MainViewModel (메인 창 로직)

**파일**: `src/CreateNewFile/ViewModels/MainViewModel.cs`

**주요 기능**:
- 파일명 컴포넌트 선택 (체크박스 기반)
- 실시간 파일명 미리보기
- 파일 생성 및 템플릿 적용
- 드래그 앤 드롭 지원
- 마지막 선택 항목 저장/복원

**중요 메서드**:
```csharp
Task SaveCurrentStateAsync()                    // 현재 상태 저장
void ClearSettingsCache()                       // 캐시 무효화 래퍼 ⭐
Task SaveLastSelectedItemsAsync()               // 마지막 선택 항목 저장
```

### 4. MainWindow (메인 창)

**파일**: `src/CreateNewFile/Views/MainWindow.xaml.cs`

**주요 기능**:
- 창 위치 저장/복원 (동기 방식)
- 드래그 앤 드롭 이벤트 처리
- 프로그램 종료 시 상태 저장

**창 위치 관리**:
```csharp
void RestoreWindowPositionSync()                // 창 위치 복원 (동기)
void SaveWindowPositionSync()                   // 창 위치 저장 (동기)
```

**중요한 종료 시퀀스** (MainWindow_Closing):
```csharp
1. SaveWindowPositionSync()              // 창 위치 저장 (파일 직접 수정)
2. viewModel.ClearSettingsCache()        // 캐시 무효화 ⭐ 필수!
3. viewModel.SaveCurrentStateAsync()     // 현재 상태 저장
```

**왜 캐시 무효화가 필요한가?**
- `SaveWindowPositionSync()`가 파일을 직접 수정
- `MainViewModel`의 SettingsService 캐시가 오래된 데이터를 가지고 있음
- 캐시 무효화 없이 `SaveCurrentStateAsync()`를 호출하면 오래된 캐시로 덮어씀
- 결과: 창 위치가 초기값으로 다시 저장됨

### 5. FileGeneratorService (파일 생성)

**파일**: `src/CreateNewFile/Services/FileGeneratorService.cs`

**주요 기능**:
- 템플릿 기반 파일 생성
- 빈 파일 생성
- 파일 경로 및 이름 유효성 검사
- 문자열 치환 규칙 적용

---

## 🐛 알려진 이슈 및 해결 방법

### 1. 창 위치 저장 문제 (해결됨 ✅)

**문제**:
- 프로그램 종료 시 실제 창 위치가 아닌 초기값(100, 100)이 저장됨
- 다음 실행 시 항상 초기 위치에서 창이 열림

**원인**:
- `MainWindow`와 `MainViewModel`이 서로 다른 `SettingsService` 인스턴스 사용
- 캐시 불일치로 인한 데이터 덮어쓰기

**해결**:
- `SettingsService`에 `ClearCache()` 메서드 추가
- `MainWindow_Closing`에서 창 위치 저장 후 캐시 무효화 호출

**참고**: `Documents/2025/10/20251016_1917_CNF_Work_list.md` 참조

### 2. NSIS UTF-8 인코딩 오류 (해결됨 ✅)

**문제**:
```
Bad text encoding: temp_installer.nsi:10
```

**원인**:
- Python 스크립트가 UTF-8 without BOM으로 임시 NSI 파일 저장
- NSIS가 BOM 없는 UTF-8을 ANSI로 오인식
- 한글 주석이 깨지면서 인코딩 오류 발생

**해결**:
- `10_BuildAll.py`와 `12_BuildInstaller.py`에서 `encoding='utf-8-sig'` 사용
- NSI 스크립트의 한글 주석을 영문으로 변경

**참고**: `Documents/2025/10/20251016_1917_CNF_Work_list.md` 참조

---

## 🔑 개발 가이드라인

### 코드 작성 시 주의사항

#### 1. SettingsService 사용
```csharp
// ❌ 잘못된 예 - 새 인스턴스 생성
var settingsService = new SettingsService();

// ✅ 올바른 예 - DI를 통한 주입
public MainViewModel(ISettingsService settingsService)
{
    _settingsService = settingsService;
}
```

#### 2. 캐시 무효화
```csharp
// 파일을 직접 수정한 경우 반드시 캐시 무효화
await SaveWindowPositionDirectly();
_settingsService.ClearCache();  // ⭐ 필수!
await _settingsService.SaveSettingsAsync(settings);
```

#### 3. 비동기 메서드 사용
```csharp
// ❌ 잘못된 예 - 동기 차단
var settings = LoadSettingsAsync().Result;

// ✅ 올바른 예 - await 사용
var settings = await LoadSettingsAsync();
```

#### 4. PresetItem 모델 사용
```csharp
// ✅ 올바른 예 - 간소화된 속성만 사용
var preset = new PresetItem
{
    Id = "abbr1",
    Value = "Test",
    IsEnabled = true
};

// ❌ 잘못된 예 - 제거된 속성 사용
var preset = new PresetItem
{
    Id = "abbr1",
    Value = "Test",
    Description = "Test",    // ❌ 더 이상 존재하지 않음
    IsFavorite = false       // ❌ 더 이상 존재하지 않음
};
```

### NSIS 스크립트 작성

#### UTF-8 인코딩
- NSI 파일은 반드시 UTF-8 with BOM으로 저장
- Python에서 생성 시: `encoding='utf-8-sig'` 사용
- 가능한 한 주석은 영문으로 작성

#### 버전 형식
```nsis
; NSIS는 X.X.X.X 형식의 버전 필요
!define PRODUCT_VERSION "1.0.3.0"     ; ✅ 올바른 형식
!define PRODUCT_VERSION "1.0.001"     ; ❌ 오류 발생
```

Python 스크립트에서 자동 변환:
```python
# "1.0.001" → "1.0.1.0"로 변환
def convert_version_format(version_str):
    parts = version_str.split('.')
    if len(parts) == 3:
        third_part = str(int(parts[2]))
        return f"{parts[0]}.{parts[1]}.{third_part}.0"
```

---

## 📝 테스트 가이드

### 테스트 구조
- **단위 테스트**: 개별 컴포넌트 테스트
- **통합 테스트**: 서비스 간 상호작용 테스트
- **사용자 인수 테스트**: 엔드투엔드 시나리오 테스트

### 테스트 명명 규칙
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

예시:
```csharp
[Fact]
public void LoadSettings_WhenFileExists_ReturnsSettings()
{
    // Arrange
    var service = new SettingsService();

    // Act
    var settings = await service.LoadSettingsAsync();

    // Assert
    Assert.NotNull(settings);
}
```

### 중요한 테스트 시나리오
1. **설정 로드/저장**: 캐시 동작 확인
2. **캐시 무효화**: ClearCache() 후 최신 데이터 로드 확인
3. **창 위치 저장**: 실제 위치가 정확하게 저장되는지 확인
4. **PresetItem 모델**: 간소화된 속성만 사용하는지 확인
5. **파일 생성**: 템플릿 적용 및 빈 파일 생성 확인

---

## 🚀 배포 프로세스

### 버전 업데이트 체크리스트
1. `CreateNewFile.csproj`에서 버전 업데이트
2. `10_BuildAll.py`에서 `PRODUCT_VERSION` 및 `BUILD_DATE` 업데이트
3. `12_BuildInstaller.py`에서 동일하게 업데이트
4. `CreateNewFile_Installer.nsi`에서 `PRODUCT_VERSION` 및 `DISPLAY_BUILD_DATE` 업데이트

### 전체 빌드 단계
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\NSIS_installer"
python 10_BuildAll.py
```

**수행 작업**:
1. 이전 빌드 정리 (`publish` 폴더 삭제)
2. 프로젝트 Clean 및 Publish
3. 아이콘 파일 복사 (`Resources/CreateNewFile.ico`)
4. 필수 파일 확인
5. NSIS 설치 확인
6. 동적 NSI 스크립트 생성
7. NSIS 인스톨러 빌드

### 생성되는 파일
- **인스톨러**: `CreateNewFile_v{VERSION}_Build_{DATE}_Setup.exe`
- **애플리케이션 파일**: `publish/framework-dependent/`

### 설치 프로그램 기능
- 설치 경로: `C:\Program Files\CreateNewFile`
- 바로가기 생성: 데스크톱 및 시작 메뉴
- 자동 시작: 선택사항 (기본 비활성화)
- 깨끗한 제거: 사용자 데이터 삭제 옵션 제공

---

## 📚 참고 문서

### 프로젝트 문서
- **작업 내역**: `Documents/2025/10/YYYYMMDD_HHMM_CNF_Work_list.md`
- **Quick Reference**: `Documents/Project_overview/YYYYMMDD_HHMM_CNF_Quick_Reference_vXX.md`
- **개발 가이드**: `src/CreateNewFile/CLAUDE.md`

### 설정 파일
- **기본 설정**: `config/appsettings.default.json`
- **사용자 설정**: `config/appsettings.json`

### 외부 링크
- **GitHub**: https://github.com/HeoChangwon/CreateNewFile
- **.NET 8.0 문서**: https://learn.microsoft.com/dotnet/
- **NSIS 문서**: https://nsis.sourceforge.io/Docs/

---

## 🔍 트러블슈팅

### 창 위치가 저장되지 않는 경우
1. `MainWindow_Closing`에서 `ClearSettingsCache()` 호출 확인
2. `SaveWindowPositionSync()` 후에 캐시 무효화가 이루어지는지 확인
3. `MainWindow`와 `MainViewModel`이 같은 SettingsService 인스턴스를 사용하는지 확인

### NSIS 빌드 오류
1. NSIS 설치 확인: `C:\Program Files (x86)\NSIS\makensis.exe`
2. NSI 파일이 UTF-8 with BOM으로 저장되었는지 확인
3. 버전 형식이 `X.X.X.X` 형식인지 확인
4. 한글 주석 제거 또는 영문으로 변경

### 설정이 저장되지 않는 경우
1. `config/appsettings.json` 파일이 존재하는지 확인
2. 파일 권한 확인 (읽기/쓰기 권한)
3. JSON 형식이 올바른지 확인
4. 예외가 발생하는지 디버그 로그 확인

### 빌드 오류
1. .NET 8.0 SDK 설치 확인
2. NuGet 패키지 복원: `dotnet restore`
3. 프로젝트 파일 (.csproj) 구문 오류 확인
4. 종속성 버전 충돌 확인

---

## 💡 베스트 프랙티스

### 1. 캐시 관리
- 파일을 직접 수정하는 경우 항상 캐시 무효화
- 캐시 무효화는 최소한으로, 성능 영향 고려
- 멀티스레드 환경에서는 lock 사용

### 2. 비동기 프로그래밍
- I/O 작업은 항상 비동기로 처리
- UI 스레드 차단 방지
- `ConfigureAwait(false)` 사용 고려 (라이브러리 코드)

### 3. MVVM 패턴
- ViewModel에 UI 관련 코드 금지 (예: MessageBox)
- View에 비즈니스 로직 금지
- 느슨한 결합 유지 (인터페이스 사용)

### 4. 테스트
- 모든 새로운 기능에 단위 테스트 작성
- 테스트 커버리지 80% 이상 유지
- TDD(Test-Driven Development) 고려

### 5. 문서화
- 복잡한 로직에는 주석 추가
- Public API는 XML 문서 주석 작성
- README 및 Quick Reference 최신 상태 유지

---

## 🎓 학습 포인트

### 핵심 개념
1. **Cache Invalidation Pattern**: 캐시와 실제 데이터 동기화
2. **Dependency Injection**: 느슨한 결합 및 테스트 용이성
3. **MVVM Architecture**: UI와 비즈니스 로직 분리
4. **Async/Await**: 비차단 I/O 작업
5. **UTF-8 BOM**: 인코딩 명시 및 다국어 지원

### 실무 경험
- 서로 다른 서비스 인스턴스로 인한 데이터 불일치 문제
- 캐시 무효화 시점의 중요성
- NSIS 인코딩 이슈 및 BOM의 역할
- WPF 창 위치 관리 (동기 vs 비동기)
- Python 스크립트를 활용한 빌드 자동화

---

## 📞 문의 및 지원

### 개발자 정보
- **이름**: 허창원 (HeoChangwon)
- **회사**: (주)그린파워 (Green Power Co., Ltd.)
- **GitHub**: https://github.com/HeoChangwon/CreateNewFile

### 이슈 보고
- GitHub Issues 사용
- 재현 단계, 예상 결과, 실제 결과 포함
- 스크린샷 및 로그 첨부

### 기여 가이드
1. Fork 프로젝트
2. Feature 브랜치 생성
3. 변경 사항 커밋
4. Pull Request 생성
5. 코드 리뷰 대기

---

## 📅 버전 히스토리

### v1.0.003 (2025-10-16)
- ✅ 창 위치 저장 문제 해결
- ✅ SettingsService 캐시 무효화 기능 추가
- ✅ NSIS UTF-8 BOM 인코딩 지원
- ✅ NSIS BrandingText 추가
- ✅ 디버그 로그 제거

### v1.0.002 (이전 버전)
- PresetItem 모델 간소화 (Description, CreatedAt, IsFavorite, DisplayText 제거)
- 설정 파일 구조 최적화
- 테스트 코드 업데이트

### v1.0.001 (초기 버전)
- 기본 기능 구현
- MVVM 아키텍처 적용
- Dependency Injection 도입
- NSIS 인스톨러 구현

---

## 🔮 향후 계획

### 기능 개선
- [ ] 다중 템플릿 지원 강화
- [ ] 프리셋 즐겨찾기 기능 복원 (선택사항)
- [ ] 파일명 패턴 커스터마이징
- [ ] 설정 내보내기/가져오기
- [ ] 다국어 지원 (한국어/영어)

### 기술 개선
- [ ] 성능 최적화
- [ ] 테스트 커버리지 향상
- [ ] 문서 자동화
- [ ] CI/CD 파이프라인 구축

---

**문서 끝**

이 문서는 CreateNewFile 프로젝트의 빠른 참조용 가이드입니다.  
새로운 Chat 세션을 시작할 때 이 문서를 참조하여 프로젝트 컨텍스트를 빠르게 파악할 수 있습니다.

**최종 수정**: 2025-10-16 19:21  
**버전**: v03
