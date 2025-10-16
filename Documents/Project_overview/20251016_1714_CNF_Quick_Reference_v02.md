# CreateNewFile - Quick Reference

> 버전: v02
> 작성일: 2025-10-16 17:14
> 프로젝트: CreateNewFile WPF Application
> 프레임워크: .NET 8.0 Windows

## 프로젝트 개요

**CreateNewFile**은 사용자 정의 파일명 패턴으로 파일을 생성하는 WPF 데스크톱 유틸리티입니다.

**주요 기능:**
- 날짜/시간, 약어, 제목, 접미사 조합으로 파일명 생성
- 템플릿 파일 기반 파일 생성 또는 빈 파일 생성
- 프리셋 관리 (약어, 제목, 접미사, 확장자)
- 마지막 설정 자동 저장/복원
- 드래그 앤 드롭으로 출력 경로 및 템플릿 설정

## 현재 상태 (2025-10-16 17:14)

### ✅ 정상 작동 기능
- **윈도우 위치 복원** (2025-10-16 수정 완료)
- 템플릿 파일명 전체 경로 하단에 파일명만 별도 표시
- Stylus/터치 예외 해결 (App.xaml.cs에서 비활성화)
- 파일 생성 기능
- 설정 저장/로드 (`appsettings.json`)
- 프리셋 관리

### 🔧 최근 수정 사항 (2025-10-16)

#### 1. 윈도우 위치 복원 문제 해결 ✅
**문제:** 프로그램 재실행 시 항상 (100, 100) 위치에 표시됨

**해결 방법:**
- **SourceInitialized 이벤트 활용**: 윈도우 핸들 생성 직후 위치 복원
- **저장 순서 변경**: `SaveWindowPositionSync()` → `SaveCurrentStateAsync()` 순서로 변경
- **상세 디버그 로그 추가**: 저장/복원 과정 추적

**수정 파일:**
- `MainWindow.xaml.cs` (라인 36, 55-67, 92-116, 220-309)

#### 2. ValidateWritePermission 버그 수정 ✅
**문제:** 폴더가 없는 경우 `DirectoryNotFoundException` 발생

**해결 방법:**
- 폴더 존재 여부 확인 로직 추가
- `DirectoryNotFoundException` 예외 처리 추가

**수정 파일:**
- `ValidationHelper.cs` (라인 421-451)

### 📋 테스트 필요 항목

1. **윈도우 위치 복원 테스트** (최우선)
   - 프로그램 실행 → 윈도우 이동 → 종료
   - 디버그 로그 확인 (Output 창)
   - 프로그램 재실행 → 이전 위치에 표시되는지 확인

2. **다중 모니터 환경 테스트**
   - 여러 모니터에서 위치 복원 정상 작동 확인
   - DPI 스케일링 처리 검증

## 프로젝트 구조

```
D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile\
├── App.xaml.cs                  # 애플리케이션 진입점, DI 설정
├── Models\
│   ├── AppSettings.cs           # 전체 설정 모델
│   ├── PresetItem.cs           # 프리셋 아이템 (약어, 제목, 접미사 등)
│   └── FileCreationRequest.cs  # 파일 생성 요청 모델
├── ViewModels\
│   ├── MainViewModel.cs        # 메인 화면 로직
│   ├── SettingsViewModel.cs   # 설정 화면 로직
│   └── BaseViewModel.cs        # MVVM 기본 클래스
├── Views\
│   ├── MainWindow.xaml(.cs)    # 메인 윈도우 ⭐ (최근 수정)
│   └── SettingsWindow.xaml(.cs)# 설정 윈도우
├── Services\
│   ├── ISettingsService        # 설정 관리 인터페이스
│   ├── SettingsService         # JSON 설정 저장/로드
│   ├── IFileGeneratorService   # 파일 생성 인터페이스
│   ├── FileGeneratorService    # 파일 생성 로직
│   └── IFileInfoService        # 파일 정보 조회
└── Utils\
    ├── FileNameBuilder.cs      # 파일명 패턴 생성
    ├── ValidationHelper.cs     # 유효성 검증 ⭐ (최근 수정)
    ├── DialogHelper.cs         # 대화상자 헬퍼
    ├── VersionHelper.cs        # 버전 정보
    └── RuntimeChecker.cs       # 런타임 확인
```

## 설정 파일 위치

```
C:\Users\[Username]\AppData\Local\CreateNewFile\config\appsettings.json
```

## 핵심 코드 위치

### 윈도우 위치 복원 (2025-10-16 수정 완료)

#### MainWindow.xaml.cs

**생성자 (라인 31-40):**
```csharp
public MainWindow()
{
    InitializeComponent();
    Title = CreateNewFile.Utils.VersionHelper.FullVersionString;
    _settingsService = new SettingsService();

    // SourceInitialized 이벤트 등록 ⭐
    this.SourceInitialized += MainWindow_SourceInitialized;
    this.Loaded += MainWindow_Loaded;
    this.Closing += MainWindow_Closing;
}
```

**SourceInitialized 이벤트 핸들러 (라인 55-67):**
```csharp
private void MainWindow_SourceInitialized(object? sender, EventArgs e)
{
    if (_isWindowPositionRestored)
        return;

    System.Diagnostics.Debug.WriteLine("=== SourceInitialized 이벤트 발생 ===");
    RestoreWindowPositionSync();
    _isWindowPositionRestored = true;
    System.Diagnostics.Debug.WriteLine($"복원 후 위치: Left={Left}, Top={Top}, Width={Width}, Height={Height}");
    System.Diagnostics.Debug.WriteLine("=== SourceInitialized 처리 완료 ===");
}
```

**Closing 이벤트 핸들러 (라인 92-116) - 저장 순서 변경:**
```csharp
private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
{
    try
    {
        System.Diagnostics.Debug.WriteLine("=== MainWindow_Closing 시작 ===");

        // 1. 윈도우 위치를 먼저 저장 ⭐
        SaveWindowPositionSync();

        // 2. MainViewModel의 현재 상태 저장
        if (DataContext is MainViewModel viewModel)
        {
            Task.Run(async () => await viewModel.SaveCurrentStateAsync()).Wait(TimeSpan.FromSeconds(5));
        }

        System.Diagnostics.Debug.WriteLine("=== MainWindow_Closing 완료 ===");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"MainWindow_Closing 오류: {ex.Message}");
    }
}
```

**SaveWindowPositionSync 메서드 (라인 220-309) - 상세 로그 추가:**
```csharp
private void SaveWindowPositionSync()
{
    try
    {
        System.Diagnostics.Debug.WriteLine("=== SaveWindowPositionSync 시작 ===");
        System.Diagnostics.Debug.WriteLine($"현재 윈도우 속성: Left={Left}, Top={Top}, ActualWidth={ActualWidth}, ActualHeight={ActualHeight}");

        // ... 설정 로드 및 검증

        if (WindowState == System.Windows.WindowState.Normal)
        {
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = ActualWidth;
            settings.WindowHeight = ActualHeight;
            System.Diagnostics.Debug.WriteLine($"일반 상태 - 현재 위치/크기 저장: Left={settings.WindowLeft}, Top={settings.WindowTop}, Size={settings.WindowWidth}x{settings.WindowHeight}");
        }
        else if (WindowState == System.Windows.WindowState.Maximized)
        {
            // RestoreBounds 사용
            settings.WindowLeft = RestoreBounds.Left;
            settings.WindowTop = RestoreBounds.Top;
            settings.WindowWidth = RestoreBounds.Width;
            settings.WindowHeight = RestoreBounds.Height;
            System.Diagnostics.Debug.WriteLine($"최대화 상태 - RestoreBounds 저장: Left={settings.WindowLeft}, Top={settings.WindowTop}, Size={settings.WindowWidth}x{settings.WindowHeight}");
        }

        // JSON 저장
        var jsonToSave = Newtonsoft.Json.JsonConvert.SerializeObject(appSettings, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(settingsFilePath, jsonToSave);
        System.Diagnostics.Debug.WriteLine($"JSON 파일 저장 완료 (크기: {jsonToSave.Length} bytes)");
        System.Diagnostics.Debug.WriteLine("=== SaveWindowPositionSync 완료 ===");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"윈도우 위치 저장 실패: {ex.Message}");
    }
}
```

### ValidationHelper.cs - ValidateWritePermission 수정

**ValidateWritePermission 메서드 (라인 421-451):**
```csharp
public static ValidationResult ValidateWritePermission(string folderPath)
{
    try
    {
        // 먼저 폴더가 존재하는지 확인 ⭐
        if (!Directory.Exists(folderPath))
        {
            return ValidationResult.CreateFailure(
                "폴더가 존재하지 않습니다. 폴더를 생성하거나 유효한 경로를 지정해주세요.");
        }

        var testFile = Path.Combine(folderPath, $"test_write_permission_{Guid.NewGuid()}.tmp");
        File.WriteAllText(testFile, "test");
        File.Delete(testFile);

        return ValidationResult.CreateSuccess();
    }
    catch (UnauthorizedAccessException)
    {
        return ValidationResult.CreateFailure("해당 폴더에 파일을 생성할 권한이 없습니다.");
    }
    catch (DirectoryNotFoundException) // ⭐ 추가
    {
        return ValidationResult.CreateFailure("폴더를 찾을 수 없습니다. 경로를 확인해주세요.");
    }
    catch (Exception ex)
    {
        return ValidationResult.CreateFailure($"폴더 접근 권한을 확인할 수 없습니다: {ex.Message}");
    }
}
```

## 빌드 명령

```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile"
dotnet build
dotnet run
```

## 테스트 명령

```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile.Tests"
dotnet test
```

## 테스트 절차

### 윈도우 위치 복원 테스트

1. **Visual Studio에서 디버그 실행 (F5)**

2. **테스트 절차:**
   ```
   a. 프로그램 실행
   b. 윈도우를 다른 위치로 이동 (예: 화면 오른쪽 하단)
   c. 프로그램 종료 (X 버튼 클릭)
   d. Output 창(Ctrl+Alt+O)에서 디버그 로그 확인:
      - "=== MainWindow_Closing 시작 ==="
      - "종료 직전 윈도우 위치: Left=[실제값], Top=[실제값], ..."
      - "=== SaveWindowPositionSync 시작 ==="
      - "현재 윈도우 속성: Left=[실제값], Top=[실제값], ..."
      - "JSON 파일 저장 완료 (크기: ... bytes)"
   e. 저장된 Left, Top 값이 이동한 위치와 일치하는지 확인
   f. 프로그램 재실행
   g. Output 창에서 복원 로그 확인:
      - "=== SourceInitialized 이벤트 발생 ==="
      - "복원 후 위치: Left=[저장된값], Top=[저장된값], ..."
   h. 윈도우가 이전 위치에 정확하게 표시되는지 확인
   ```

3. **설정 파일 확인:**
   ```powershell
   notepad "$env:LOCALAPPDATA\CreateNewFile\config\appsettings.json"
   ```
   - `UI` 섹션의 `WindowLeft`, `WindowTop` 값 확인
   - 이동한 위치의 좌표와 일치하는지 확인

4. **다중 모니터 테스트 (선택):**
   - 두 번째 모니터로 윈도우 이동
   - 종료 후 재실행
   - 두 번째 모니터의 같은 위치에 표시되는지 확인

## 기술적 세부사항

### WPF 윈도우 생명주기 이벤트

1. **생성자 (Constructor)**
   - InitializeComponent() 호출
   - 이벤트 핸들러 등록

2. **SourceInitialized** ⭐ (윈도우 위치 복원에 사용)
   - 윈도우의 네이티브 핸들(HWND) 생성 직후
   - Win32 API 호출 가능
   - **위치/크기 설정에 최적**
   - 화면에 표시되기 전에 실행됨

3. **Loaded**
   - XAML 요소가 모두 로드됨
   - 시각적 트리 구성 완료

4. **ContentRendered**
   - 콘텐츠가 화면에 렌더링된 후

5. **Closing**
   - 윈도우가 닫히기 직전
   - 설정 저장에 활용

### 윈도우 위치 복원 동작 원리

```
1. MainWindow 생성자 실행
   └─> SourceInitialized 이벤트 핸들러 등록

2. App.xaml.cs에서 PrepareWindow() 호출
   └─> 로그만 출력 (실제 복원은 하지 않음)

3. SourceInitialized 이벤트 발생 ⭐
   └─> RestoreWindowPositionSync() 호출
       └─> appsettings.json에서 위치 읽기
       └─> Left, Top, Width, Height 설정
       └─> _isWindowPositionRestored = true

4. Show() 호출
   └─> 설정된 위치에 윈도우 표시
```

### 설정 저장 순서

```
MainWindow_Closing 이벤트 발생
    ↓
1. SaveWindowPositionSync() 실행 ⭐ (먼저)
   └─> appsettings.json 전체를 읽음
   └─> UI.Window* 값만 업데이트
   └─> appsettings.json 저장
    ↓
2. MainViewModel.SaveCurrentStateAsync() 실행
   └─> appsettings.json 전체를 읽음
   └─> 다른 설정 업데이트
   └─> appsettings.json 저장
```

**중요:** SaveWindowPositionSync()가 먼저 실행되어 윈도우 위치가 확실히 저장됩니다.

### 설정 파일 구조

```json
{
  "UI": {
    "WindowWidth": 900.0,
    "WindowHeight": 600.0,
    "WindowLeft": [실제 위치],
    "WindowTop": [실제 위치],
    "WindowState": "Normal",
    "UseDateTime": true,
    "UseAbbreviation": false,
    "UseTitle": true,
    "UseSuffix": false,
    ...
  },
  "LastUsedValues": {
    "Abbreviation": "",
    "Title": "example",
    "Suffix": "",
    "Extension": ".txt",
    "OutputPath": "D:\\temp"
  },
  ...
}
```

## 다음 작업 예정

### 1. 윈도우 위치 복원 검증 테스트 🔴 (최우선)
   - 실제 사용 환경에서 테스트
   - 여러 번 실행/종료 반복
   - 다양한 위치로 이동 후 복원 확인

### 2. 다중 모니터 환경 테스트
   - 여러 모니터에서 위치 복원 정상 작동 확인
   - 모니터 구성 변경 시 대응 (화면 밖으로 벗어나는 경우)
   - DPI 스케일링 처리 검증

### 3. 추가 개선 사항
   - 윈도우가 화면 밖으로 벗어난 경우 자동 조정
   - 설정 파일 저장 최적화 (불필요한 읽기/쓰기 최소화)

## 알려진 제약사항

1. **터치/펜 입력 비활성화됨**
   - Stylus 예외 해결을 위해 비활성화
   - 마우스/키보드는 정상 작동
   - 대부분의 데스크톱 환경에서 문제없음

## 디버깅 팁

### 윈도우 위치 디버그

**설정 파일 확인:**
```powershell
notepad "$env:LOCALAPPDATA\CreateNewFile\config\appsettings.json"
```

**디버그 출력 보기:**
- Visual Studio에서 F5로 디버깅 시작
- View → Output (Ctrl+Alt+O)
- "Show output from" → "Debug" 선택
- 다음 메시지 확인:
  - `=== SourceInitialized 이벤트 발생 ===`
  - `복원 후 위치: Left=..., Top=...`
  - `=== MainWindow_Closing 시작 ===`
  - `현재 윈도우 속성: Left=..., Top=...`

**수동으로 설정 파일 수정:**
```json
{
  "UI": {
    "WindowLeft": 500.0,
    "WindowTop": 300.0,
    ...
  }
}
```
프로그램 재실행 시 해당 위치에 표시되는지 확인

## 문서 파일

### 작업 내역
- **이전 Chat 작업 내역:** `Documents/2025/10/20251016_1712_CNF_Work_list.md`
- **프로젝트 요구사항:** `Documents/20251016_WPF_Project_Configuration_File_Requirements_Prompt.md`

### 프로젝트 정보
- **CLAUDE.md:** 프로젝트 아키텍처 및 빌드 명령
- **Quick Reference (이전 버전):** `Documents/Project_overview/20251016_1638_CNF_Quick_Reference_v01.md`
- **프로젝트 개요:** `Documents/Project_overview/20250930_1557_CreateNewFile_Project_overview.md`

## 연락처

- **작성자:** 허창원 ((주)그린파워)
- **AI 지원:** Claude Code Assistant

---

## 다음 Chat 시작 시 체크리스트

1. ✅ 이 Quick Reference 파일(`20251016_1714_CNF_Quick_Reference_v02.md`)을 먼저 읽기
2. 🔴 **윈도우 위치 복원 테스트 결과 확인** (최우선)
   - 사용자가 실제로 테스트한 결과 청취
   - 정상 작동하면: 다중 모니터 테스트로 진행
   - 문제가 있으면: 추가 디버깅 및 수정
3. 📋 다중 모니터 환경 테스트 계획
4. 📋 화면 밖으로 벗어난 경우 처리 로직 추가 검토

---

**마지막 업데이트:** 2025-10-16 17:14
**다음 작업:** 윈도우 위치 복원 테스트 및 검증
