# WPF 애플리케이션 설정 파일 구조 요구사항

> 작성일: 2025-10-16
> 작성자: Changwon Heo ((주)그린파워)
> AI 지원: Claude Code Assistant

## 1. 설정 파일 관리 방식

- **Properties.Settings 사용 금지**: .NET의 기본 Properties.Settings는 사용하지 않음
- **JSON 기반 통합 설정**: 모든 설정을 하나의 JSON 파일에 저장
- **위치**: `%LOCALAPPDATA%\[AppName]\config\appsettings.json`

## 2. 설정 파일 구조

### AppSettings 모델 구조

```csharp
public class AppSettings
{
    // 애플리케이션별 설정
    public string DefaultOutputPath { get; set; }
    public List<PresetItem> Items { get; set; }

    // UI 설정 (윈도우 위치/크기 포함)
    public UiSettings UI { get; set; } = new();

    // 고급 설정
    public AdvancedSettings Advanced { get; set; } = new();
}

public class UiSettings
{
    // 윈도우 위치 및 크기
    public double WindowWidth { get; set; } = 800;
    public double WindowHeight { get; set; } = 600;
    public double WindowLeft { get; set; } = 100;
    public double WindowTop { get; set; } = 100;
    public string WindowState { get; set; } = "Normal";

    // 기타 UI 설정
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "ko-KR";
}
```

## 3. SettingsService 구현

### 인터페이스

```csharp
public interface ISettingsService
{
    Task<AppSettings> LoadSettingsAsync();
    Task<bool> SaveSettingsAsync(AppSettings settings);
    string GetSettingsFolderPath();
}
```

### 구현

```csharp
public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        // %LOCALAPPDATA%\[AppName]\config\appsettings.json
        var localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        var configDir = Path.Combine(localAppData, "AppName", "config");
        Directory.CreateDirectory(configDir);
        _settingsFilePath = Path.Combine(configDir, "appsettings.json");
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath))
            return new AppSettings();

        var json = await File.ReadAllTextAsync(_settingsFilePath);
        return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
    }

    public async Task<bool> SaveSettingsAsync(AppSettings settings)
    {
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        await File.WriteAllTextAsync(_settingsFilePath, json);
        return true;
    }
}
```

## 4. MainWindow에서 윈도우 위치 저장/복원

### 초기화

```csharp
public partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService();

        // 윈도우 표시 전 위치 복원
        this.SourceInitialized += MainWindow_SourceInitialized;
        this.Closing += MainWindow_Closing;
    }
}
```

### 위치 복원

```csharp
private void MainWindow_SourceInitialized(object sender, EventArgs e)
{
    RestoreWindowPosition();
}

private void RestoreWindowPosition()
{
    try
    {
        var settings = _settingsService.LoadSettingsAsync()
            .GetAwaiter().GetResult().UI;

        // 화면 경계 확인
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        // 저장된 설정이 유효한지 확인
        if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
        {
            // 화면 내부인지 확인
            if (settings.WindowLeft < screenWidth &&
                settings.WindowTop < screenHeight)
            {
                Left = settings.WindowLeft;
                Top = settings.WindowTop;
            }
            else
            {
                // 화면 중앙에 배치
                Left = (screenWidth - Width) / 2;
                Top = (screenHeight - Height) / 2;
            }

            Width = settings.WindowWidth;
            Height = settings.WindowHeight;

            if (Enum.TryParse<WindowState>(settings.WindowState, out var state))
                WindowState = state;
        }
        else
        {
            // 첫 실행 - 화면 중앙
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }
    catch (Exception ex)
    {
        // 실패 시 화면 중앙에 배치
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = (screenWidth - Width) / 2;
        Top = (screenHeight - Height) / 2;
    }
}
```

### 위치 저장

```csharp
private void MainWindow_Closing(object sender, CancelEventArgs e)
{
    SaveWindowPosition();
}

private void SaveWindowPosition()
{
    try
    {
        // 최소화 상태는 저장하지 않음
        if (WindowState == WindowState.Minimized)
            return;

        var appSettings = _settingsService.LoadSettingsAsync()
            .GetAwaiter().GetResult();
        var settings = appSettings.UI;

        // 최대화 상태일 때는 복원 시 크기 저장
        if (WindowState == WindowState.Maximized && RestoreBounds != Rect.Empty)
        {
            settings.WindowLeft = RestoreBounds.Left;
            settings.WindowTop = RestoreBounds.Top;
            settings.WindowWidth = RestoreBounds.Width;
            settings.WindowHeight = RestoreBounds.Height;
            settings.WindowState = "Normal";
        }
        else
        {
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = ActualWidth;
            settings.WindowHeight = ActualHeight;
            settings.WindowState = WindowState.ToString();
        }

        _settingsService.SaveSettingsAsync(appSettings)
            .GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        // 저장 실패는 무시
    }
}
```

## 5. 주의사항

### ✅ 해야 할 것

- 모든 설정을 하나의 JSON 파일에 저장
- `%LOCALAPPDATA%` 사용 (로밍하지 않는 설정)
- 윈도우 위치/크기도 JSON에 저장
- 최소화 상태는 저장하지 않음
- 최대화 상태일 때는 RestoreBounds 저장

### ❌ 하지 말아야 할 것

- `Properties.Settings` 사용
- `%APPDATA%` (Roaming) 사용
- Settings.settings 파일 생성
- user.config 파일 생성
- 해시가 포함된 난수 폴더 생성

## 6. 결과

### 설정 파일 위치

```
C:\Users\[Username]\AppData\Local\[AppName]\config\appsettings.json
```

### JSON 파일 예시

```json
{
  "DefaultOutputPath": "C:\\Users\\User\\Documents",
  "UI": {
    "WindowWidth": 800,
    "WindowHeight": 600,
    "WindowLeft": 100,
    "WindowTop": 100,
    "WindowState": "Normal",
    "Theme": "Light",
    "Language": "ko-KR"
  },
  "Advanced": {
    "LogLevel": "Information"
  }
}
```

## 7. 장점

- ✅ 설정 파일이 하나로 통합됨
- ✅ 설정 위치를 완전히 제어 가능
- ✅ 백업/복원이 간단함
- ✅ 해시 폴더가 생성되지 않음
- ✅ 버전 변경 시 자동 마이그레이션 가능
- ✅ JSON 파일이므로 수동 편집 가능

## 8. 프롬프트 사용 방법

향후 새로운 WPF 프로젝트를 시작할 때 다음과 같이 프롬프트를 제공하면 됩니다:

```
WPF 애플리케이션을 개발할 때 위의 "WPF 애플리케이션 설정 파일 구조 요구사항"을
정확히 따라 구현해주세요. Properties.Settings는 사용하지 말고,
모든 설정을 JSON 파일 하나에 저장하는 방식으로 구현해주세요.
```

## 9. 참고 프로젝트

이 구조는 다음 프로젝트에서 성공적으로 적용되었습니다:

- **CreateYMDFolder**: 날짜 기반 폴더 생성 유틸리티
- **CreateNewFile**: 파일명 생성 및 파일 생성 유틸리티

---

## 변경 이력

| 날짜 | 버전 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 2025-10-16 | 1.0 | 초기 문서 작성 | Changwon Heo |
