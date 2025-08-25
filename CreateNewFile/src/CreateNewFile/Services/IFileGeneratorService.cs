using System.Threading.Tasks;
using CreateNewFile.Models;
using CreateNewFile.Utils;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 파일 생성 서비스 인터페이스
    /// </summary>
    public interface IFileGeneratorService
    {
        /// <summary>
        /// 파일명을 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <param name="isDateTimeEnabled">날짜/시간 포함 여부</param>
        /// <param name="isAbbreviationEnabled">약어 포함 여부</param>
        /// <param name="isTitleEnabled">제목 포함 여부</param>
        /// <param name="isSuffixEnabled">접미어 포함 여부</param>
        /// <returns>생성된 파일명</returns>
        string GenerateFileName(FileCreationRequest request, bool isDateTimeEnabled = true, bool isAbbreviationEnabled = true, bool isTitleEnabled = true, bool isSuffixEnabled = true);

        /// <summary>
        /// 전체 파일 경로를 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <param name="isDateTimeEnabled">날짜/시간 포함 여부</param>
        /// <param name="isAbbreviationEnabled">약어 포함 여부</param>
        /// <param name="isTitleEnabled">제목 포함 여부</param>
        /// <param name="isSuffixEnabled">접미어 포함 여부</param>
        /// <returns>전체 파일 경로</returns>
        string GetFullFilePath(FileCreationRequest request, bool isDateTimeEnabled = true, bool isAbbreviationEnabled = true, bool isTitleEnabled = true, bool isSuffixEnabled = true);

        /// <summary>
        /// 파일 생성 요청의 유효성을 검사합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <returns>유효성 검사 결과</returns>
        ValidationResult ValidateRequest(FileCreationRequest request);

        /// <summary>
        /// 파일을 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <param name="isDateTimeEnabled">날짜/시간 포함 여부</param>
        /// <param name="isAbbreviationEnabled">약어 포함 여부</param>
        /// <param name="isTitleEnabled">제목 포함 여부</param>
        /// <param name="isSuffixEnabled">접미어 포함 여부</param>
        /// <returns>생성 성공 여부</returns>
        Task<FileCreationResult> CreateFileAsync(FileCreationRequest request, bool isDateTimeEnabled = true, bool isAbbreviationEnabled = true, bool isTitleEnabled = true, bool isSuffixEnabled = true);

        /// <summary>
        /// 파일이 이미 존재하는지 확인합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <returns>파일 존재 여부</returns>
        bool FileExists(FileCreationRequest request);

        /// <summary>
        /// 빈 파일을 생성합니다.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="extension">파일 확장자</param>
        /// <returns>생성 성공 여부</returns>
        Task<bool> CreateEmptyFileAsync(string filePath, string extension);

        /// <summary>
        /// 템플릿 파일을 복사하여 새 파일을 생성합니다.
        /// </summary>
        /// <param name="templatePath">템플릿 파일 경로</param>
        /// <param name="destinationPath">대상 파일 경로</param>
        /// <returns>복사 성공 여부</returns>
        Task<bool> CopyTemplateFileAsync(string templatePath, string destinationPath);
    }

    /// <summary>
    /// 파일 생성 결과를 나타내는 클래스
    /// </summary>
    public class FileCreationResult
    {
        /// <summary>
        /// 생성 성공 여부
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 생성된 파일의 전체 경로
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 생성된 파일명
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 오류 메시지
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 템플릿 사용 여부
        /// </summary>
        public bool UsedTemplate { get; set; }

        /// <summary>
        /// 생성된 파일 크기 (바이트)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 성공적인 결과를 생성합니다.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="fileName">파일명</param>
        /// <param name="usedTemplate">템플릿 사용 여부</param>
        /// <param name="fileSize">파일 크기</param>
        /// <returns>성공 결과</returns>
        public static FileCreationResult CreateSuccess(string filePath, string fileName, bool usedTemplate = false, long fileSize = 0)
        {
            return new FileCreationResult
            {
                Success = true,
                FilePath = filePath,
                FileName = fileName,
                UsedTemplate = usedTemplate,
                FileSize = fileSize
            };
        }

        /// <summary>
        /// 실패한 결과를 생성합니다.
        /// </summary>
        /// <param name="errorMessage">오류 메시지</param>
        /// <returns>실패 결과</returns>
        public static FileCreationResult Failure(string errorMessage)
        {
            return new FileCreationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}