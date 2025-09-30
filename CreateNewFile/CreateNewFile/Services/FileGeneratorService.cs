using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
        /// <param name="isDateTimeEnabled">날짜/시간 포함 여부</param>
        /// <param name="isAbbreviationEnabled">약어 포함 여부</param>
        /// <param name="isTitleEnabled">제목 포함 여부</param>
        /// <param name="isSuffixEnabled">접미어 포함 여부</param>
        /// <returns>생성 결과</returns>
        public async Task<FileCreationResult> CreateFileAsync(FileCreationRequest request, bool isDateTimeEnabled = true, bool isAbbreviationEnabled = true, bool isTitleEnabled = true, bool isSuffixEnabled = true)
        {
            try
            {
                // 유효성 검사
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    return FileCreationResult.Failure($"유효성 검사 실패: {validationResult}");
                }

                var fileName = GenerateFileName(request, isDateTimeEnabled, isAbbreviationEnabled, isTitleEnabled, isSuffixEnabled);
                var fullPath = GetFullFilePath(request, isDateTimeEnabled, isAbbreviationEnabled, isTitleEnabled, isSuffixEnabled);

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
                    
                    var templateInfo = new System.IO.FileInfo(request.TemplatePath);
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

                    var fileInfo = new System.IO.FileInfo(fullPath);
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
                // 빈 파일 생성 실패 로그 제거
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
                // 템플릿 파일 복사 실패 로그 제거
                return false;
            }
        }

        /// <summary>
        /// 문자열 교체를 적용하여 파일을 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청</param>
        /// <param name="stringReplacements">문자열 교체 규칙 목록</param>
        /// <param name="fileInfoDateTime">파일정보 날짜/시간 (동적 교체용)</param>
        /// <param name="isDateTimeEnabled">날짜/시간 포함 여부</param>
        /// <param name="isAbbreviationEnabled">약어 포함 여부</param>
        /// <param name="isTitleEnabled">제목 포함 여부</param>
        /// <param name="isSuffixEnabled">접미어 포함 여부</param>
        /// <returns>생성 결과</returns>
        public async Task<FileCreationResult> CreateFileWithStringReplacementAsync(
            FileCreationRequest request, 
            IList<StringReplacementRule> stringReplacements,
            DateTime? fileInfoDateTime = null,
            bool isDateTimeEnabled = true, 
            bool isAbbreviationEnabled = true, 
            bool isTitleEnabled = true, 
            bool isSuffixEnabled = true)
        {
            try
            {
                // 유효성 검사
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    return FileCreationResult.Failure($"유효성 검사 실패: {validationResult}");
                }

                var fileName = GenerateFileName(request, isDateTimeEnabled, isAbbreviationEnabled, isTitleEnabled, isSuffixEnabled);
                var fullPath = GetFullFilePath(request, isDateTimeEnabled, isAbbreviationEnabled, isTitleEnabled, isSuffixEnabled);

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

                // 템플릿 파일이 지정되고 존재하는 경우 문자열 교체와 함께 복사
                if (!string.IsNullOrWhiteSpace(request.TemplatePath) && File.Exists(request.TemplatePath))
                {
                    var copySuccess = await CopyTemplateFileWithReplacementAsync(request.TemplatePath, fullPath, stringReplacements, fileInfoDateTime);
                    if (!copySuccess)
                    {
                        return FileCreationResult.Failure("템플릿 파일 복사 또는 문자열 교체에 실패했습니다.");
                    }
                    usedTemplate = true;
                    
                    var templateInfo = new System.IO.FileInfo(fullPath);
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

                    var fileInfo = new System.IO.FileInfo(fullPath);
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
        /// 템플릿 파일에 문자열 교체를 적용합니다.
        /// </summary>
        /// <param name="templatePath">템플릿 파일 경로</param>
        /// <param name="destinationPath">대상 파일 경로</param>
        /// <param name="stringReplacements">문자열 교체 규칙 목록</param>
        /// <param name="fileInfoDateTime">파일정보 날짜/시간 (동적 교체용)</param>
        /// <returns>복사 및 교체 성공 여부</returns>
        public async Task<bool> CopyTemplateFileWithReplacementAsync(
            string templatePath, 
            string destinationPath, 
            IList<StringReplacementRule> stringReplacements,
            DateTime? fileInfoDateTime = null)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    return false;
                }

                // 템플릿 파일 읽기
                var templateContent = await File.ReadAllTextAsync(templatePath);
                
                // 문자열 교체 적용
                var processedContent = ApplyStringReplacements(templateContent, stringReplacements, fileInfoDateTime);
                
                // 처리된 내용으로 새 파일 생성
                await File.WriteAllTextAsync(destinationPath, processedContent);

                return true;
            }
            catch (Exception ex)
            {
                // 로깅이 구현되면 여기에 로그 추가
                System.Diagnostics.Debug.WriteLine($"템플릿 파일 복사 및 문자열 교체 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 문자열 교체를 수행합니다.
        /// </summary>
        /// <param name="content">원본 내용</param>
        /// <param name="stringReplacements">문자열 교체 규칙 목록</param>
        /// <param name="fileInfoDateTime">파일정보 날짜/시간 (동적 교체용)</param>
        /// <returns>교체된 내용</returns>
        private string ApplyStringReplacements(string content, IList<StringReplacementRule> stringReplacements, DateTime? fileInfoDateTime = null)
        {
            if (string.IsNullOrEmpty(content) || stringReplacements == null || !stringReplacements.Any())
                return content;

            var result = content;
            var baseDateTime = fileInfoDateTime ?? DateTime.Now;
            
            foreach (var rule in stringReplacements.Where(r => r.IsEnabled))
            {
                try
                {
                    if (string.IsNullOrEmpty(rule.SearchText))
                        continue;

                    if (rule.UseDynamicReplacement)
                    {
                        // 동적 플레이스홀더 처리
                        result = ProcessDynamicPlaceholders(result, rule, baseDateTime);
                    }
                    else if (rule.UseRegex)
                    {
                        var options = rule.IsCaseSensitive 
                            ? System.Text.RegularExpressions.RegexOptions.None
                            : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                        
                        var regex = new System.Text.RegularExpressions.Regex(rule.SearchText, options);
                        result = regex.Replace(result, rule.ReplaceText);
                    }
                    else
                    {
                        var comparison = rule.IsCaseSensitive 
                            ? StringComparison.Ordinal
                            : StringComparison.OrdinalIgnoreCase;
                        
                        result = result.Replace(rule.SearchText, rule.ReplaceText, comparison);
                    }
                }
                catch (Exception ex)
                {
                    // 정규식 오류 등이 발생해도 계속 진행
                    System.Diagnostics.Debug.WriteLine($"문자열 교체 규칙 적용 오류: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// 동적 플레이스홀더를 처리합니다.
        /// </summary>
        /// <param name="content">원본 내용</param>
        /// <param name="rule">교체 규칙</param>
        /// <param name="baseDateTime">기준 날짜/시간</param>
        /// <returns>처리된 내용</returns>
        private string ProcessDynamicPlaceholders(string content, StringReplacementRule rule, DateTime baseDateTime)
        {
            var searchText = rule.SearchText.Trim();
            var replaceText = rule.ReplaceText.Trim();
            
            // 교체 문자열에서 동적 패턴을 찾아서 처리
            var processedReplaceText = ProcessDynamicPatterns(replaceText, baseDateTime);
            
            // 일반적인 문자열 교체 수행
            var comparison = rule.IsCaseSensitive 
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            
            return content.Replace(searchText, processedReplaceText, comparison);
        }

        /// <summary>
        /// 문자열에서 동적 패턴을 찾아 처리합니다.
        /// </summary>
        /// <param name="text">처리할 텍스트</param>
        /// <param name="baseDateTime">기준 날짜/시간</param>
        /// <returns>동적 패턴이 처리된 텍스트</returns>
        private string ProcessDynamicPatterns(string text, DateTime baseDateTime)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            
            var result = text;
            
            // YYYYMMDD_HHMMSS 패턴 처리 (+ 기호 이스케이프)
            var pattern1 = @"YYYYMMDD_HHMMSS([+\-]\d+)?";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern1, match =>
            {
                var dateTime = baseDateTime;
                var offsetText = match.Groups[1].Value;
                
                if (!string.IsNullOrEmpty(offsetText))
                {
                    var offset = int.Parse(offsetText);
                    dateTime = dateTime.AddMinutes(offset);
                }
                
                return dateTime.ToString("yyyyMMdd_HHmmss");
            });
            
            // YYYYMMDD_HHMM 패턴 처리 (+ 기호 이스케이프)
            var pattern2 = @"YYYYMMDD_HHMM([+\-]\d+)?";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern2, match =>
            {
                var dateTime = baseDateTime;
                var offsetText = match.Groups[1].Value;
                
                if (!string.IsNullOrEmpty(offsetText))
                {
                    var offset = int.Parse(offsetText);
                    dateTime = dateTime.AddMinutes(offset);
                }
                
                return dateTime.ToString("yyyyMMdd_HHmm");
            });
            
            return result;
        }
    }
}