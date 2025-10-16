using System.ComponentModel;
using System.IO;

namespace CreateNewFile.Models
{
    /// <summary>
    /// 프로젝트 설정을 나타내는 모델 클래스
    /// .cnfjson 파일로 저장/로드됩니다.
    /// </summary>
    public class ProjectConfig : ICloneable
    {
        /// <summary>
        /// 프로젝트 설정 파일 포맷 버전
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 프로젝트 이름
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 프로젝트 설명
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 파일 정보
        /// </summary>
        public ProjectFileInfo FileInfo { get; set; } = new();

        /// <summary>
        /// 출력 폴더 경로
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// 템플릿 파일 경로
        /// </summary>
        public string TemplatePath { get; set; } = string.Empty;

        /// <summary>
        /// 문자열 교체 규칙 목록
        /// </summary>
        public List<StringReplacementRule> StringReplacements { get; set; } = new();

        /// <summary>
        /// 생성 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 마지막 수정 시간
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효성 검사 결과와 오류 메시지</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return (false, "프로젝트 이름이 필요합니다.");

            if (FileInfo == null)
                return (false, "파일 정보가 필요합니다.");

            var fileInfoValidation = FileInfo.Validate();
            if (!fileInfoValidation.IsValid)
                return fileInfoValidation;

            if (string.IsNullOrWhiteSpace(OutputPath))
                return (false, "출력 폴더 경로가 필요합니다.");

            if (!string.IsNullOrWhiteSpace(OutputPath) && !Directory.Exists(OutputPath))
                return (false, $"출력 폴더가 존재하지 않습니다: {OutputPath}");

            if (!string.IsNullOrWhiteSpace(TemplatePath) && !File.Exists(TemplatePath))
                return (false, $"템플릿 파일이 존재하지 않습니다: {TemplatePath}");

            // 문자열 교체 규칙 유효성 검사
            foreach (var rule in StringReplacements.Where(r => r.IsEnabled))
            {
                var ruleValidation = rule.Validate();
                if (!ruleValidation.IsValid)
                    return (false, $"문자열 교체 규칙 오류: {ruleValidation.ErrorMessage}");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 객체를 복사합니다.
        /// </summary>
        /// <returns>복사된 객체</returns>
        public object Clone()
        {
            return new ProjectConfig
            {
                Version = Version,
                Name = Name,
                Description = Description,
                FileInfo = (ProjectFileInfo)FileInfo.Clone(),
                OutputPath = OutputPath,
                TemplatePath = TemplatePath,
                StringReplacements = StringReplacements.Select(r => (StringReplacementRule)r.Clone()).ToList(),
                CreatedAt = CreatedAt,
                ModifiedAt = DateTime.Now
            };
        }

        /// <summary>
        /// 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>문자열 표현</returns>
        public override string ToString()
        {
            return $"{Name} (v{Version})";
        }
    }

    /// <summary>
    /// 프로젝트의 파일 정보를 나타내는 클래스
    /// </summary>
    public class ProjectFileInfo : ICloneable
    {
        /// <summary>
        /// 생성 날짜/시간
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 약어
        /// </summary>
        public string Abbreviation { get; set; } = string.Empty;

        /// <summary>
        /// 제목
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 접미어
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        /// <summary>
        /// 확장자
        /// </summary>
        public string Extension { get; set; } = string.Empty;

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
        /// 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효성 검사 결과와 오류 메시지</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Abbreviation))
                return (false, "제목 또는 약어 중 하나는 필요합니다.");

            if (string.IsNullOrWhiteSpace(Extension))
                return (false, "확장자가 필요합니다.");

            return (true, string.Empty);
        }

        /// <summary>
        /// 객체를 복사합니다.
        /// </summary>
        /// <returns>복사된 객체</returns>
        public object Clone()
        {
            return new ProjectFileInfo
            {
                DateTime = DateTime,
                Abbreviation = Abbreviation,
                Title = Title,
                Suffix = Suffix,
                Extension = Extension,
                IsDateTimeEnabled = IsDateTimeEnabled,
                IsAbbreviationEnabled = IsAbbreviationEnabled,
                IsTitleEnabled = IsTitleEnabled,
                IsSuffixEnabled = IsSuffixEnabled
            };
        }

        /// <summary>
        /// FileCreationRequest로 변환합니다.
        /// </summary>
        /// <param name="outputPath">출력 경로</param>
        /// <param name="templatePath">템플릿 경로</param>
        /// <returns>FileCreationRequest 객체</returns>
        public FileCreationRequest ToFileCreationRequest(string outputPath, string templatePath)
        {
            return new FileCreationRequest
            {
                DateTime = DateTime,
                Abbreviation = Abbreviation,
                Title = Title,
                Suffix = Suffix,
                Extension = Extension,
                OutputPath = outputPath,
                TemplatePath = templatePath
            };
        }

        /// <summary>
        /// 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>문자열 표현</returns>
        public override string ToString()
        {
            var parts = new List<string>();
            if (IsDateTimeEnabled)
                parts.Add(DateTime.ToString("yyyyMMdd_HHmm"));
            if (IsAbbreviationEnabled && !string.IsNullOrWhiteSpace(Abbreviation))
                parts.Add(Abbreviation);
            if (IsTitleEnabled && !string.IsNullOrWhiteSpace(Title))
                parts.Add(Title);
            if (IsSuffixEnabled && !string.IsNullOrWhiteSpace(Suffix))
                parts.Add(Suffix);

            return string.Join("_", parts) + Extension;
        }
    }
}
