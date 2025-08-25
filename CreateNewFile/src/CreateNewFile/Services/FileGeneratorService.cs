using System;
using System.IO;
using System.Threading.Tasks;
using CreateNewFile.Models;
using CreateNewFile.Utils;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 파일 생성 서비스 구현 클래스
    /// </summary>
    public class FileGeneratorService : IFileGeneratorService
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
        public string GenerateFileName(FileCreationRequest request, bool isDateTimeEnabled = true, bool isAbbreviationEnabled = true, bool isTitleEnabled = true, bool isSuffixEnabled = true)
        {
            return FileNameBuilder.GenerateFileName(request, isDateTimeEnabled, isAbbreviationEnabled, isTitleEnabled, isSuffixEnabled);
        }

        /// <summary>
        /// 전체 파일 경로를 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <param name="isDateTimeEnabled">날짜/시간 포함 여부</param>
        /// <param name="isAbbreviationEnabled">약어 포함 여부</param>
        /// <param name="isTitleEnabled">제목 포함 여부</param>
        /// <param name="isSuffixEnabled">접미어 포함 여부</param>
        /// <returns>전체 파일 경로</returns>
        public string GetFullFilePath(FileCreationRequest request, bool isDateTimeEnabled = true, bool isAbbreviationEnabled = true, bool isTitleEnabled = true, bool isSuffixEnabled = true)
        {
            return FileNameBuilder.GenerateFullPath(request, isDateTimeEnabled, isAbbreviationEnabled, isTitleEnabled, isSuffixEnabled);
        }

        /// <summary>
        /// 파일 생성 요청의 유효성을 검사합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <returns>유효성 검사 결과</returns>
        public ValidationResult ValidateRequest(FileCreationRequest request)
        {
            return FileNameBuilder.ValidateRequest(request);
        }

        /// <summary>
        /// 파일이 이미 존재하는지 확인합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <returns>파일 존재 여부</returns>
        public bool FileExists(FileCreationRequest request)
        {
            try
            {
                var fullPath = GetFullFilePath(request);
                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 파일을 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <returns>생성 결과</returns>
        public async Task<FileCreationResult> CreateFileAsync(FileCreationRequest request)
        {
            try
            {
                // 유효성 검사
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    return FileCreationResult.Failure($"유효성 검사 실패: {validationResult}");
                }

                var fileName = GenerateFileName(request);
                var fullPath = GetFullFilePath(request);

                // 파일 존재 확인
                if (File.Exists(fullPath))
                {
                    return FileCreationResult.Failure($"파일이 이미 존재합니다: {fileName}");
                }

                // 출력 디렉토리 확인 및 생성
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                bool usedTemplate = false;
                long fileSize = 0;

                // 템플릿 파일이 지정되고 존재하는 경우 복사
                if (!string.IsNullOrWhiteSpace(request.TemplatePath) && File.Exists(request.TemplatePath))
                {
                    var copySuccess = await CopyTemplateFileAsync(request.TemplatePath, fullPath);
                    if (!copySuccess)
                    {
                        return FileCreationResult.Failure("템플릿 파일 복사에 실패했습니다.");
                    }
                    usedTemplate = true;
                    
                    var templateInfo = new FileInfo(request.TemplatePath);
                    fileSize = templateInfo.Length;
                }
                else
                {
                    // 빈 파일 생성
                    var createSuccess = await CreateEmptyFileAsync(fullPath, request.Extension);
                    if (!createSuccess)
                    {
                        return FileCreationResult.Failure("빈 파일 생성에 실패했습니다.");
                    }

                    var fileInfo = new FileInfo(fullPath);
                    fileSize = fileInfo.Length;
                }

                return FileCreationResult.CreateSuccess(fullPath, fileName, usedTemplate, fileSize);
            }
            catch (UnauthorizedAccessException ex)
            {
                return FileCreationResult.Failure($"접근 권한이 없습니다: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                return FileCreationResult.Failure($"디렉토리를 찾을 수 없습니다: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                return FileCreationResult.Failure($"경로가 너무 깁니다: {ex.Message}");
            }
            catch (IOException ex)
            {
                return FileCreationResult.Failure($"IO 오류가 발생했습니다: {ex.Message}");
            }
            catch (Exception ex)
            {
                return FileCreationResult.Failure($"파일 생성 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        /// <summary>
        /// 빈 파일을 생성합니다.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="extension">파일 확장자</param>
        /// <returns>생성 성공 여부</returns>
        public async Task<bool> CreateEmptyFileAsync(string filePath, string extension)
        {
            try
            {
                // .txt 파일의 경우 공백 문자 하나 추가 (빈 파일 방지)
                var content = string.Empty;
                var normalizedExtension = FileNameBuilder.NormalizeExtension(extension);
                
                if (normalizedExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    content = " ";
                }

                await File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch (Exception ex)
            {
                // 로깅이 구현되면 여기에 로그 추가
                System.Diagnostics.Debug.WriteLine($"빈 파일 생성 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 템플릿 파일을 복사하여 새 파일을 생성합니다.
        /// </summary>
        /// <param name="templatePath">템플릿 파일 경로</param>
        /// <param name="destinationPath">대상 파일 경로</param>
        /// <returns>복사 성공 여부</returns>
        public async Task<bool> CopyTemplateFileAsync(string templatePath, string destinationPath)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    return false;
                }

                // 비동기 파일 복사
                using var sourceStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                
                await sourceStream.CopyToAsync(destinationStream);
                await destinationStream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                // 로깅이 구현되면 여기에 로그 추가
                System.Diagnostics.Debug.WriteLine($"템플릿 파일 복사 실패: {ex.Message}");
                return false;
            }
        }
    }
}