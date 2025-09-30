using System.IO;

namespace CreateNewFile.Models
{
    /// <summary>
    /// 파일 생성 요청 정보를 담는 모델 클래스
    /// </summary>
    public class FileCreationRequest
    {
        /// <summary>
        /// 파일명에 포함될 날짜와 시간
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 약어 문자열
        /// </summary>
        public string Abbreviation { get; set; } = string.Empty;

        /// <summary>
        /// 제목 문자열
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 접미어 문자열 (선택사항)
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        /// <summary>
        /// 파일 확장자
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// 출력 폴더 경로
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// 템플릿 파일 경로 (선택사항)
        /// </summary>
        public string TemplatePath { get; set; } = string.Empty;

        /// <summary>
        /// 요청 정보의 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효하면 true, 아니면 false</returns>
        public bool IsValid()
        {
            // 필수 필드 검사
            if (string.IsNullOrWhiteSpace(OutputPath))
                return false;

            // 약어나 제목 중 하나는 반드시 있어야 함
            if (string.IsNullOrWhiteSpace(Abbreviation) && string.IsNullOrWhiteSpace(Title))
                return false;

            // 확장자 검사
            if (string.IsNullOrWhiteSpace(Extension))
                return false;

            // 출력 경로가 존재하는지 확인
            if (!Directory.Exists(OutputPath))
                return false;

            // 템플릿 파일이 지정된 경우 존재 여부 확인
            if (!string.IsNullOrWhiteSpace(TemplatePath) && !File.Exists(TemplatePath))
                return false;

            return true;
        }

        /// <summary>
        /// 유효성 검사 결과와 오류 메시지를 반환합니다.
        /// </summary>
        /// <returns>유효성 검사 결과와 오류 메시지</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(OutputPath))
                return (false, "출력 폴더 경로를 지정해야 합니다.");

            if (string.IsNullOrWhiteSpace(Abbreviation) && string.IsNullOrWhiteSpace(Title))
                return (false, "약어 또는 제목 중 하나는 반드시 입력해야 합니다.");

            if (string.IsNullOrWhiteSpace(Extension))
                return (false, "파일 확장자를 지정해야 합니다.");

            if (!Directory.Exists(OutputPath))
                return (false, $"출력 폴더가 존재하지 않습니다: {OutputPath}");

            if (!string.IsNullOrWhiteSpace(TemplatePath) && !File.Exists(TemplatePath))
                return (false, $"템플릿 파일이 존재하지 않습니다: {TemplatePath}");

            return (true, string.Empty);
        }

        /// <summary>
        /// 요청 정보를 복사하여 새 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>복사된 새 인스턴스</returns>
        public FileCreationRequest Clone()
        {
            return new FileCreationRequest
            {
                DateTime = this.DateTime,
                Abbreviation = this.Abbreviation,
                Title = this.Title,
                Suffix = this.Suffix,
                Extension = this.Extension,
                OutputPath = this.OutputPath,
                TemplatePath = this.TemplatePath
            };
        }

        /// <summary>
        /// 생성될 파일의 전체 경로를 반환합니다.
        /// </summary>
        /// <returns>전체 파일 경로</returns>
        public string GetFullPath()
        {
            try
            {
                var fileName = CreateNewFile.Utils.FileNameBuilder.GenerateFileName(this);
                return Path.Combine(OutputPath, fileName);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 객체의 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>요청 정보의 요약</returns>
        public override string ToString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Abbreviation))
                parts.Add(Abbreviation);

            if (!string.IsNullOrWhiteSpace(Title))
                parts.Add(Title);

            if (!string.IsNullOrWhiteSpace(Suffix))
                parts.Add(Suffix);

            var summary = string.Join("_", parts);
            return $"{DateTime:yyyy-MM-dd HH:mm} - {summary}{Extension}";
        }
    }
}