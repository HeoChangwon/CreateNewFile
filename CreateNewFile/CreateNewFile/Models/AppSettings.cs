using System.IO;

namespace CreateNewFile.Models
{
    /// <summary>
    /// 애플리케이션 설정을 나타내는 모델 클래스
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 기본 출력 폴더 경로
        /// </summary>
        public string DefaultOutputPath { get; set; } = string.Empty;

        /// <summary>
        /// 기본 템플릿 파일 경로
        /// </summary>
        public string DefaultTemplatePath { get; set; } = string.Empty;

        /// <summary>
        /// 미리 정의된 약어 목록
        /// </summary>
        public List<PresetItem> Abbreviations { get; set; } = new();

        /// <summary>
        /// 미리 정의된 제목 목록
        /// </summary>
        public List<PresetItem> Titles { get; set; } = new();

        /// <summary>
        /// 미리 정의된 접미어 목록
        /// </summary>
        public List<PresetItem> Suffixes { get; set; } = new();

        /// <summary>
        /// 미리 정의된 확장자 목록
        /// </summary>
        public List<PresetItem> Extensions { get; set; } = new();

        /// <summary>
        /// 미리 정의된 출력 폴더 경로 목록
        /// </summary>
        public List<PresetItem> OutputPaths { get; set; } = new();

        /// <summary>
        /// 미리 정의된 템플릿 파일 경로 목록
        /// </summary>
        public List<PresetItem> TemplatePaths { get; set; } = new();

        /// <summary>
        /// UI 설정
        /// </summary>
        public UiSettings UI { get; set; } = new();

        /// <summary>
        /// 고급 설정
        /// </summary>
        public AdvancedSettings Advanced { get; set; } = new();

        /// <summary>
        /// 마지막 선택된 약어
        /// </summary>
        public string LastSelectedAbbreviation { get; set; } = string.Empty;

        /// <summary>
        /// 마지막 선택된 제목
        /// </summary>
        public string LastSelectedTitle { get; set; } = string.Empty;

        /// <summary>
        /// 마지막 선택된 접미어
        /// </summary>
        public string LastSelectedSuffix { get; set; } = string.Empty;

        /// <summary>
        /// 마지막 선택된 확장자
        /// </summary>
        public string LastSelectedExtension { get; set; } = string.Empty;

        /// <summary>
        /// 마지막 선택된 출력 경로
        /// </summary>
        public string LastSelectedOutputPath { get; set; } = string.Empty;

        /// <summary>
        /// 마지막 선택된 템플릿 경로
        /// </summary>
        public string LastSelectedTemplatePath { get; set; } = string.Empty;

        /// <summary>
        /// 마지막 선택된 날짜/시간
        /// </summary>
        public DateTime LastSelectedDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 날짜/시간 항목 활성화 상태
        /// </summary>
        public bool IsDateTimeEnabled { get; set; } = true;

        /// <summary>
        /// 약어 항목 활성화 상태
        /// </summary>
        public bool IsAbbreviationEnabled { get; set; } = true;

        /// <summary>
        /// 제목 항목 활성화 상태
        /// </summary>
        public bool IsTitleEnabled { get; set; } = true;

        /// <summary>
        /// 접미어 항목 활성화 상태
        /// </summary>
        public bool IsSuffixEnabled { get; set; } = true;

        /// <summary>
        /// 마지막으로 사용된 문자열 교체 규칙 목록
        /// </summary>
        public List<StringReplacementRule> LastStringReplacements { get; set; } = new();

        /// <summary>
        /// 기본 설정값으로 초기화합니다.
        /// </summary>
        public void LoadDefaults()
        {
            // 기본 출력 경로
            DefaultOutputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CreateNewFile");

            // 기본 약어
            Abbreviations.Clear();
            Abbreviations.AddRange(new[]
            {
                new PresetItem { Value = "CNF" },
                new PresetItem { Value = "DOC" },
                new PresetItem { Value = "LOG" },
                new PresetItem { Value = "TEST" },
                new PresetItem { Value = "TEMP" }
            });

            // 기본 제목
            Titles.Clear();
            Titles.AddRange(new[]
            {
                new PresetItem { Value = "Development_note" },
                new PresetItem { Value = "User_manual" },
                new PresetItem { Value = "Technical_spec" },
                new PresetItem { Value = "Meeting_notes" },
                new PresetItem { Value = "Project_plan" }
            });

            // 기본 접미어
            Suffixes.Clear();
            Suffixes.AddRange(new[]
            {
                new PresetItem { Value = "v1.0" },
                new PresetItem { Value = "draft" },
                new PresetItem { Value = "final" },
                new PresetItem { Value = "review" },
                new PresetItem { Value = "backup" }
            });

            // 기본 확장자
            Extensions.Clear();
            Extensions.AddRange(new[]
            {
                new PresetItem { Value = ".txt" },
                new PresetItem { Value = ".md" },
                new PresetItem { Value = ".docx" },
                new PresetItem { Value = ".pdf" },
                new PresetItem { Value = ".xlsx" },
                new PresetItem { Value = ".log" }
            });

            // 기본 출력 경로
            OutputPaths.Clear();
            OutputPaths.Add(new PresetItem { Value = DefaultOutputPath });

            // 체크박스 기본값 설정 (모든 항목 활성화)
            IsDateTimeEnabled = true;
            IsAbbreviationEnabled = true;
            IsTitleEnabled = true;
            IsSuffixEnabled = true;
        }

        /// <summary>
        /// 설정의 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효성 검사 결과와 오류 메시지</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            // 기본 출력 경로 검사
            if (!string.IsNullOrWhiteSpace(DefaultOutputPath) && !Directory.Exists(DefaultOutputPath))
            {
                return (false, $"기본 출력 폴더가 존재하지 않습니다: {DefaultOutputPath}");
            }

            // 기본 템플릿 파일 검사
            if (!string.IsNullOrWhiteSpace(DefaultTemplatePath) && !File.Exists(DefaultTemplatePath))
            {
                return (false, $"기본 템플릿 파일이 존재하지 않습니다: {DefaultTemplatePath}");
            }

            // 미리 정의된 항목들의 유효성 검사
            var invalidAbbreviations = Abbreviations.Where(x => !x.IsValid()).ToList();
            if (invalidAbbreviations.Any())
            {
                return (false, "유효하지 않은 약어 항목이 있습니다.");
            }

            var invalidTitles = Titles.Where(x => !x.IsValid()).ToList();
            if (invalidTitles.Any())
            {
                return (false, "유효하지 않은 제목 항목이 있습니다.");
            }

            var invalidExtensions = Extensions.Where(x => !x.IsValid()).ToList();
            if (invalidExtensions.Any())
            {
                return (false, "유효하지 않은 확장자 항목이 있습니다.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 설정을 복사하여 새 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>복사된 새 인스턴스</returns>
        public AppSettings Clone()
        {
            return new AppSettings
            {
                DefaultOutputPath = this.DefaultOutputPath,
                DefaultTemplatePath = this.DefaultTemplatePath,
                Abbreviations = this.Abbreviations.Select(x => x.Clone()).ToList(),
                Titles = this.Titles.Select(x => x.Clone()).ToList(),
                Suffixes = this.Suffixes.Select(x => x.Clone()).ToList(),
                Extensions = this.Extensions.Select(x => x.Clone()).ToList(),
                OutputPaths = this.OutputPaths.Select(x => x.Clone()).ToList(),
                TemplatePaths = this.TemplatePaths.Select(x => x.Clone()).ToList(),
                UI = this.UI.Clone(),
                Advanced = this.Advanced.Clone(),
                LastSelectedAbbreviation = this.LastSelectedAbbreviation,
                LastSelectedTitle = this.LastSelectedTitle,
                LastSelectedSuffix = this.LastSelectedSuffix,
                LastSelectedExtension = this.LastSelectedExtension,
                LastSelectedOutputPath = this.LastSelectedOutputPath,
                LastSelectedTemplatePath = this.LastSelectedTemplatePath,
                LastSelectedDateTime = this.LastSelectedDateTime,
                IsDateTimeEnabled = this.IsDateTimeEnabled,
                IsAbbreviationEnabled = this.IsAbbreviationEnabled,
                IsTitleEnabled = this.IsTitleEnabled,
                IsSuffixEnabled = this.IsSuffixEnabled,
                LastStringReplacements = this.LastStringReplacements.Select(x => (StringReplacementRule)x.Clone()).ToList()
            };
        }
    }

    /// <summary>
    /// UI 관련 설정
    /// </summary>
    public class UiSettings
    {
        /// <summary>
        /// 윈도우 너비
        /// </summary>
        public double WindowWidth { get; set; } = 600;

        /// <summary>
        /// 윈도우 높이
        /// </summary>
        public double WindowHeight { get; set; } = 1065;

        /// <summary>
        /// 윈도우 위치 X
        /// </summary>
        public double WindowLeft { get; set; } = 100;

        /// <summary>
        /// 윈도우 위치 Y
        /// </summary>
        public double WindowTop { get; set; } = 100;

        /// <summary>
        /// 윈도우 상태 (최대화, 최소화 등)
        /// </summary>
        public string WindowState { get; set; } = "Normal";

        /// <summary>
        /// 테마 설정
        /// </summary>
        public string Theme { get; set; } = "Light";

        /// <summary>
        /// 언어 설정
        /// </summary>
        public string Language { get; set; } = "ko-KR";

        /// <summary>
        /// 자동 저장 활성화 여부
        /// </summary>
        public bool AutoSave { get; set; } = true;

        /// <summary>
        /// 시작 시 마지막 설정 복원 여부
        /// </summary>
        public bool RestoreLastSettings { get; set; } = true;

        /// <summary>
        /// UI 설정을 복사합니다.
        /// </summary>
        public UiSettings Clone()
        {
            return new UiSettings
            {
                WindowWidth = this.WindowWidth,
                WindowHeight = this.WindowHeight,
                WindowLeft = this.WindowLeft,
                WindowTop = this.WindowTop,
                WindowState = this.WindowState,
                Theme = this.Theme,
                Language = this.Language,
                AutoSave = this.AutoSave,
                RestoreLastSettings = this.RestoreLastSettings
            };
        }
    }

    /// <summary>
    /// 고급 설정
    /// </summary>
    public class AdvancedSettings
    {
        /// <summary>
        /// 파일명 최대 길이
        /// </summary>
        public int MaxFileNameLength { get; set; } = 200;

        /// <summary>
        /// 백업 파일 생성 여부
        /// </summary>
        public bool CreateBackup { get; set; } = false;

        /// <summary>
        /// 로그 레벨
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// 로그 파일 경로
        /// </summary>
        public string LogFilePath { get; set; } = string.Empty;

        /// <summary>
        /// 성능 모니터링 활성화 여부
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = false;

        /// <summary>
        /// 자동 업데이트 확인 여부
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// 사용 통계 수집 여부
        /// </summary>
        public bool CollectUsageStatistics { get; set; } = true;

        /// <summary>
        /// 고급 설정을 복사합니다.
        /// </summary>
        public AdvancedSettings Clone()
        {
            return new AdvancedSettings
            {
                MaxFileNameLength = this.MaxFileNameLength,
                CreateBackup = this.CreateBackup,
                LogLevel = this.LogLevel,
                LogFilePath = this.LogFilePath,
                EnablePerformanceMonitoring = this.EnablePerformanceMonitoring,
                CheckForUpdates = this.CheckForUpdates,
                CollectUsageStatistics = this.CollectUsageStatistics
            };
        }
    }
}