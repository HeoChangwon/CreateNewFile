# CreateNewFile - Quick Reference

> 버전: v01
> 작성일: 2025-10-16 16:38
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

## 현재 상태 (2025-10-16)

### ✅ 정상 작동 기능
- 템플릿 파일명 전체 경로 하단에 파일명만 별도 표시
- Stylus/터치 예외 해결 (App.xaml.cs에서 비활성화)
- 파일 생성 기능
- 설정 저장/로드 (`appsettings.json`)
- 프리셋 관리

### ⚠️ 미해결 문제
**윈도우 위치 복원 실패**
- 설정 파일에는 위치가 저장됨
- 복원 코드에서 위치를 설정하지만 실제로는 적용되지 않음
- WPF 윈도우 생명주기 또는 DPI/다중 모니터 문제로 추정

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
│   ├── MainWindow.xaml(.cs)    # 메인 윈도우
│   └── SettingsWindow.xaml(.cs)# 설정 윈도우
├── Services\
│   ├── ISettingsService        # 설정 관리 인터페이스
│   ├── SettingsService         # JSON 설정 저장/로드
│   ├── IFileGeneratorService   # 파일 생성 인터페이스
│   ├── FileGeneratorService    # 파일 생성 로직
│   └── IFileInfoService        # 파일 정보 조회
└── Utils\
    ├── FileNameBuilder.cs      # 파일명 패턴 생성
    ├── ValidationHelper.cs     # 유효성 검증
    ├── DialogHelper.cs         # 대화상자 헬퍼
    ├── VersionHelper.cs        # 버전 정보
    └── RuntimeChecker.cs       # 런타임 확인
```

## 설정 파일 위치

```
C:\Users\[Username]\AppData\Local\CreateNewFile\config\appsettings.json
```

## 핵심 코드 위치

### 윈도우 위치 관련 (미해결 문제)

**App.xaml.cs (라인 21-25):**
```csharp
public App()
{
    AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", true);
}
```

**App.xaml.cs (라인 56-77):**
```csharp
Dispatcher.BeginInvoke(new Action(async () =>
{
    mainWindow.PrepareWindow();  // 위치 복원
    mainWindow.Show();
    // ...
}), DispatcherPriority.ApplicationIdle);
```

**MainWindow.xaml.cs (라인 42-64):**
```csharp
public void PrepareWindow()
{
    // 설정 파일에서 윈도우 위치 읽어서 Left, Top, Width, Height 설정
    RestoreWindowPositionSync();
}
```

**MainWindow.xaml.cs (라인 103-180):**
```csharp
private void SaveWindowPositionSync()
{
    // WindowState 확인, RestoreBounds 사용, appsettings.json에 저장
}
```

### 템플릿 파일명 표시

**MainWindow.xaml (라인 407-413):**
```xaml
<TextBlock Grid.Row="1" Grid.Column="1" Grid.ColumnSpan="2"
           Text="{Binding TemplateFileName}"
           FontSize="11"
           Foreground="DarkBlue"
           Margin="2,2,0,5"
           TextTrimming="CharacterEllipsis"
           ToolTip="{Binding TemplateFileName}"/>
```

**MainViewModel.cs (라인 158-175):**
```csharp
public string SelectedTemplatePath
{
    get => _selectedTemplatePath;
    set
    {
        if (SetProperty(ref _selectedTemplatePath, value))
        {
            TemplateFileName = !string.IsNullOrWhiteSpace(value)
                ? Path.GetFileName(value)
                : string.Empty;
            ValidateInput();
        }
    }
}

public string TemplateFileName
{
    get => _templateFileName;
    private set => SetProperty(ref _templateFileName, value);
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

## 다음 작업 우선순위

### 1. 윈도우 위치 복원 해결 🔴 (최우선)

**시도해볼 방법:**

**옵션 A: Win32 API 사용**
```csharp
[DllImport("user32.dll")]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
    int X, int Y, int cx, int cy, uint uFlags);

private void MainWindow_SourceInitialized(object sender, EventArgs e)
{
    var hwnd = new WindowInteropHelper(this).Handle;
    SetWindowPos(hwnd, IntPtr.Zero, (int)Left, (int)Top,
        (int)Width, (int)Height, 0);
}
```

**옵션 B: XAML에서 WindowStartupLocation 제거**
```xaml
<!-- WindowStartupLocation="Manual" 제거하고 코드에서만 제어 -->
```

**옵션 C: ContentRendered 이벤트 사용**
```csharp
this.ContentRendered += (s, e) =>
{
    Left = savedLeft;
    Top = savedTop;
};
```

**옵션 D: DPI 스케일링 고려**
```csharp
var dpiScale = VisualTreeHelper.GetDpi(this);
Left = savedLeft / dpiScale.DpiScaleX;
Top = savedTop / dpiScale.DpiScaleY;
```

### 2. 추가 개선 사항

- 다중 모니터 환경 테스트
- DPI 스케일링 처리
- 윈도우 상태(최대화/일반) 복원 검증

## 디버깅 팁

### 윈도우 위치 디버그

설정 파일 확인:
```powershell
notepad "$env:LOCALAPPDATA\CreateNewFile\config\appsettings.json"
```

디버그 출력 보기:
- Visual Studio에서 F5로 디버깅 시작
- View → Output (Ctrl+Alt+O)
- "Show output from" → "Debug" 선택
- `PrepareWindow` 관련 메시지 확인

## 알려진 제약사항

1. **터치/펜 입력 비활성화됨**
   - Stylus 예외 해결을 위해 비활성화
   - 마우스/키보드는 정상 작동
   - 대부분의 데스크톱 환경에서 문제없음

2. **윈도우 위치 복원 미작동**
   - 현재 미해결 상태
   - 다음 Chat에서 해결 예정

## 문서 파일

- **프로젝트 요구사항:** `Documents/20251016_WPF_Project_Configuration_File_Requirements_Prompt.md`
- **CLAUDE.md:** 프로젝트 아키텍처 및 빌드 명령
- **Work_list:** `Documents/2025/10/20251016_1637_CNF_Work_list.md`

## 연락처

- **작성자:** Changwon Heo (Green Power Co., Ltd.)
- **AI 지원:** Claude Code Assistant

---

**다음 Chat 시작 시:**
1. 이 Quick Reference 파일을 먼저 읽어주세요
2. 윈도우 위치 복원 문제부터 해결해주세요
3. 위의 "옵션 A~D" 중 하나를 시도해주세요
