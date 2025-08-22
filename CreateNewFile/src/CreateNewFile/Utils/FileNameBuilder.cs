using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CreateNewFile.Models;

namespace CreateNewFile.Utils
{
    /// <summary>
    /// 파일명 생성을 위한 유틸리티 클래스
    /// </summary>
    public static class FileNameBuilder
    {
        /// <summary>
        /// 파일명에 사용할 수 없는 문자들
        /// </summary>
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// 파일 경로에 사용할 수 없는 문자들
        /// </summary>
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        /// <summary>
        /// 최대 파일명 길이 (Windows 기준)
        /// </summary>
        private const int MaxFileNameLength = 255;

        /// <summary>
        /// 최대 경로 길이 (Windows 기준)
        /// </summary>
        private const int MaxPathLength = 260;

        /// <summary>
        /// FileCreationRequest를 기반으로 파일명을 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청 정보</param>
        /// <returns>생성된 파일명</returns>
        /// <exception cref="ArgumentNullException">request가 null인 경우</exception>
        /// <exception cref="ArgumentException">필수 값이 누락된 경우</exception>
        public static string GenerateFileName(FileCreationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var components = new List<string>();

            // 1. 날짜/시간 (YYYYMMDD_HHMM)
            var dateTimeStr = request.DateTime.ToString("yyyyMMdd_HHmm");
            components.Add(dateTimeStr);

            // 2. 약어
            if (!string.IsNullOrWhiteSpace(request.Abbreviation))
            {
                var cleanAbbreviation = CleanStringForFileName(request.Abbreviation.Trim());
                if (!string.IsNullOrWhiteSpace(cleanAbbreviation))
                    components.Add(cleanAbbreviation);
            }

            // 3. 제목
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var cleanTitle = CleanStringForFileName(request.Title.Trim());
                if (!string.IsNullOrWhiteSpace(cleanTitle))
                    components.Add(cleanTitle);
            }

            // 4. 접미어 (선택사항)
            if (!string.IsNullOrWhiteSpace(request.Suffix))
            {
                var cleanSuffix = CleanStringForFileName(request.Suffix.Trim());
                if (!string.IsNullOrWhiteSpace(cleanSuffix))
                    components.Add(cleanSuffix);
            }

            // 언더스코어로 연결
            var fileName = string.Join("_", components.Where(c => !string.IsNullOrWhiteSpace(c)));

            // 5. 확장자 추가
            if (!string.IsNullOrWhiteSpace(request.Extension))
            {
                var extension = NormalizeExtension(request.Extension);
                fileName += extension;
            }

            // 파일명 길이 제한 (잘라내기)
            if (fileName.Length > MaxFileNameLength)
            {
                var extension = Path.GetExtension(fileName);
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var maxNameLength = MaxFileNameLength - extension.Length;
                
                if (maxNameLength > 0)
                {
                    fileName = nameWithoutExtension.Substring(0, Math.Min(nameWithoutExtension.Length, maxNameLength)) + extension;
                }
                else
                {
                    throw new ArgumentException($"확장자가 너무 깁니다. (최대 {MaxFileNameLength}자)");
                }
            }

            return fileName;
        }

        /// <summary>
        /// 전체 파일 경로를 생성합니다.
        /// </summary>
        /// <param name="request">파일 생성 요청 정보</param>
        /// <returns>전체 파일 경로</returns>
        /// <exception cref="ArgumentNullException">request가 null인 경우</exception>
        /// <exception cref="ArgumentException">경로가 유효하지 않은 경우</exception>
        public static string GenerateFullPath(FileCreationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.OutputPath))
                throw new ArgumentException("출력 경로가 지정되지 않았습니다.", nameof(request));

            var fileName = GenerateFileName(request);
            var fullPath = Path.Combine(request.OutputPath, fileName);

            // 경로 길이 검증 및 자동 조정
            if (fullPath.Length > MaxPathLength)
            {
                // 경로 부분의 길이를 계산
                var pathPart = request.OutputPath + Path.DirectorySeparatorChar;
                var extension = Path.GetExtension(fileName);
                var availableLength = MaxPathLength - pathPart.Length - extension.Length;
                
                if (availableLength <= 0)
                {
                    throw new ArgumentException($"출력 경로가 너무 깁니다. 파일명을 생성할 수 없습니다.");
                }
                
                // 파일명 부분을 줄여서 다시 생성
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var truncatedName = nameWithoutExtension.Substring(0, Math.Min(nameWithoutExtension.Length, availableLength));
                fileName = truncatedName + extension;
                fullPath = Path.Combine(request.OutputPath, fileName);
            }

            return fullPath;
        }

        /// <summary>
        /// 날짜/시간 형식을 파일명에 사용할 수 있는 형태로 변환합니다.
        /// </summary>
        /// <param name="dateTime">변환할 날짜/시간</param>
        /// <param name="format">날짜/시간 형식 (기본값: "yyyyMMdd_HHmm")</param>
        /// <returns>변환된 날짜/시간 문자열</returns>
        public static string FormatDateTime(DateTime dateTime, string format = "yyyyMMdd_HHmm")
        {
            if (string.IsNullOrWhiteSpace(format))
                format = "yyyyMMdd_HHmm";

            return dateTime.ToString(format);
        }

        /// <summary>
        /// 문자열을 파일명에 사용할 수 있도록 정리합니다.
        /// </summary>
        /// <param name="input">정리할 문자열</param>
        /// <returns>정리된 문자열</returns>
        public static string CleanStringForFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var cleaned = input.Trim();

            // 유효하지 않은 문자 제거 (언더스코어로 치환하지 않고 제거)
            foreach (var invalidChar in InvalidFileNameChars)
            {
                cleaned = cleaned.Replace(invalidChar.ToString(), "");
            }

            // 공백을 언더스코어로 변경
            cleaned = Regex.Replace(cleaned, @"\s+", "_");

            // 연속된 언더스코어를 하나로 줄임
            cleaned = Regex.Replace(cleaned, @"_{2,}", "_");

            // 앞뒤 언더스코어 제거
            cleaned = cleaned.Trim('_');

            return cleaned;
        }

        /// <summary>
        /// 확장자를 정규화합니다.
        /// </summary>
        /// <param name="extension">정규화할 확장자</param>
        /// <returns>정규화된 확장자</returns>
        public static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return string.Empty;

            var normalized = extension.Trim().ToLowerInvariant();

            // 점이 없으면 추가
            if (!normalized.StartsWith("."))
                normalized = "." + normalized;

            // 유효하지 않은 문자 제거
            foreach (var invalidChar in InvalidFileNameChars)
            {
                normalized = normalized.Replace(invalidChar.ToString(), "");
            }

            return normalized;
        }

        /// <summary>
        /// 파일명이 유효한지 검사합니다.
        /// </summary>
        /// <param name="fileName">검사할 파일명</param>
        /// <returns>유효한 경우 true, 그렇지 않으면 false</returns>
        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (fileName.Length > MaxFileNameLength)
                return false;

            // 유효하지 않은 문자 확인
            if (fileName.IndexOfAny(InvalidFileNameChars) >= 0)
                return false;

            // 예약된 이름 확인 (Windows)
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
            if (reservedNames.Contains(nameWithoutExtension))
                return false;

            return true;
        }

        /// <summary>
        /// 경로가 유효한지 검사합니다.
        /// </summary>
        /// <param name="path">검사할 경로</param>
        /// <returns>유효한 경우 true, 그렇지 않으면 false</returns>
        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.Length > MaxPathLength)
                return false;

            // 유효하지 않은 문자 확인
            if (path.IndexOfAny(InvalidPathChars) >= 0)
                return false;

            // 특별한 경우만 체크 (콜론은 드라이브 문자 뒤에 올 수 있으므로 제외)
            var invalidCharsForPath = new[] { '<', '>', '"', '|', '?', '*' };
            if (path.IndexOfAny(invalidCharsForPath) >= 0)
                return false;

            try
            {
                var fullPath = Path.GetFullPath(path);
                
                // 드라이브 존재 확인
                var drive = Path.GetPathRoot(fullPath);
                if (!string.IsNullOrEmpty(drive) && !DriveInfo.GetDrives().Any(d => d.Name.Equals(drive, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// FileCreationRequest의 유효성을 검사합니다.
        /// </summary>
        /// <param name="request">검사할 요청</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateRequest(FileCreationRequest request)
        {
            var result = new ValidationResult();

            if (request == null)
            {
                result.AddError("요청 정보가 없습니다.");
                return result;
            }

            // 출력 경로 검증
            if (string.IsNullOrWhiteSpace(request.OutputPath))
            {
                result.AddError("출력 경로가 지정되지 않았습니다.");
            }
            else if (!IsValidPath(request.OutputPath))
            {
                result.AddError("출력 경로가 유효하지 않습니다.");
            }
            else if (!Directory.Exists(request.OutputPath))
            {
                result.AddError("출력 경로가 존재하지 않습니다.");
            }

            // 템플릿 파일 검증 (선택사항)
            if (!string.IsNullOrWhiteSpace(request.TemplatePath))
            {
                if (!IsValidPath(request.TemplatePath))
                {
                    result.AddError("템플릿 파일 경로가 유효하지 않습니다.");
                }
                else if (!File.Exists(request.TemplatePath))
                {
                    result.AddError("템플릿 파일이 존재하지 않습니다.");
                }
            }

            // 파일명 생성 시도
            try
            {
                var fileName = GenerateFileName(request);
                if (!IsValidFileName(fileName))
                {
                    result.AddError("생성된 파일명이 유효하지 않습니다.");
                }

                var fullPath = GenerateFullPath(request);
                if (File.Exists(fullPath))
                {
                    result.AddError($"파일이 이미 존재합니다: {fileName}");
                }
            }
            catch (Exception ex)
            {
                result.AddError($"파일명 생성 중 오류 발생: {ex.Message}");
            }

            return result;
        }
    }

    /// <summary>
    /// 유효성 검사 결과를 나타내는 클래스
    /// </summary>
    public class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        /// <summary>
        /// 유효한지 여부
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// 오류 목록
        /// </summary>
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        /// <summary>
        /// 오류 메시지 목록 (ValidationHelper 호환용)
        /// </summary>
        public IReadOnlyList<string> ErrorMessages => _errors.AsReadOnly();

        /// <summary>
        /// 경고 목록
        /// </summary>
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

        /// <summary>
        /// 오류를 추가합니다.
        /// </summary>
        /// <param name="error">오류 메시지</param>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                _errors.Add(error);
        }

        /// <summary>
        /// 경고를 추가합니다.
        /// </summary>
        /// <param name="warning">경고 메시지</param>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
                _warnings.Add(warning);
        }

        /// <summary>
        /// 모든 오류와 경고를 하나의 문자열로 반환합니다.
        /// </summary>
        /// <returns>오류 및 경고 메시지</returns>
        public override string ToString()
        {
            var messages = new List<string>();
            
            if (_errors.Count > 0)
            {
                messages.Add("오류:");
                messages.AddRange(_errors.Select(e => "- " + e));
            }

            if (_warnings.Count > 0)
            {
                messages.Add("경고:");
                messages.AddRange(_warnings.Select(w => "- " + w));
            }

            return string.Join(Environment.NewLine, messages);
        }

        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        /// <returns>성공 ValidationResult</returns>
        public static ValidationResult CreateSuccess()
        {
            return new ValidationResult();
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        /// <param name="errorMessage">오류 메시지</param>
        /// <returns>실패 ValidationResult</returns>
        public static ValidationResult CreateFailure(string errorMessage)
        {
            var result = new ValidationResult();
            result.AddError(errorMessage);
            return result;
        }

        /// <summary>
        /// 여러 오류 메시지로 실패 결과를 생성합니다.
        /// </summary>
        /// <param name="errorMessages">오류 메시지 목록</param>
        /// <returns>실패 ValidationResult</returns>
        public static ValidationResult CreateFailure(IEnumerable<string> errorMessages)
        {
            var result = new ValidationResult();
            foreach (var message in errorMessages.Where(m => !string.IsNullOrWhiteSpace(m)))
            {
                result.AddError(message);
            }
            return result;
        }
    }
}