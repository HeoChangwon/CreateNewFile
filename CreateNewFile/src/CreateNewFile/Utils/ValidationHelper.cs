using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CreateNewFile.Utils
{
    /// <summary>
    /// 입력값 유효성 검사를 위한 유틸리티 클래스
    /// </summary>
    public static class ValidationHelper
    {
        #region 상수 정의

        /// <summary>
        /// Windows에서 파일명에 사용할 수 없는 문자들
        /// </summary>
        private static readonly char[] InvalidFileNameChars = { '<', '>', ':', '"', '|', '?', '*' };

        /// <summary>
        /// Windows에서 경로에 사용할 수 없는 문자들
        /// </summary>
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        /// <summary>
        /// Windows에서 예약된 파일명들
        /// </summary>
        private static readonly string[] ReservedFileNames = {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// 최대 파일명 길이 (확장자 포함)
        /// </summary>
        public const int MaxFileNameLength = 255;

        /// <summary>
        /// 최대 경로 길이
        /// </summary>
        public const int MaxPathLength = 260;

        /// <summary>
        /// 최소 입력 길이
        /// </summary>
        public const int MinInputLength = 1;

        #endregion

        #region 파일명 유효성 검사

        /// <summary>
        /// 파일명의 유효성을 검사합니다
        /// </summary>
        /// <param name="fileName">검사할 파일명</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return ValidationResult.CreateFailure("파일명이 비어있습니다.");
            }

            // 길이 검사
            if (fileName.Length > MaxFileNameLength)
            {
                return ValidationResult.CreateFailure($"파일명이 너무 깁니다. 최대 {MaxFileNameLength}자까지 허용됩니다.");
            }

            // 유효하지 않은 문자 검사
            var invalidChars = fileName.Where(c => InvalidFileNameChars.Contains(c) || char.IsControl(c)).ToList();
            if (invalidChars.Any())
            {
                var invalidCharsString = string.Join(", ", invalidChars.Distinct().Select(c => $"'{c}'"));
                return ValidationResult.CreateFailure($"파일명에 사용할 수 없는 문자가 포함되어 있습니다: {invalidCharsString}");
            }

            // 예약된 파일명 검사
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (ReservedFileNames.Contains(fileNameWithoutExtension.ToUpper()))
            {
                return ValidationResult.CreateFailure($"'{fileNameWithoutExtension}'은(는) Windows에서 예약된 파일명입니다.");
            }

            // 파일명이 점으로 시작하거나 끝나는지 검사
            if (fileName.StartsWith(".") || fileName.EndsWith("."))
            {
                return ValidationResult.CreateFailure("파일명은 점(.)으로 시작하거나 끝날 수 없습니다.");
            }

            // 공백으로만 구성된 파일명 검사
            if (fileName.Trim().Length == 0)
            {
                return ValidationResult.CreateFailure("파일명은 공백으로만 구성될 수 없습니다.");
            }

            return ValidationResult.CreateSuccess();
        }

        /// <summary>
        /// 파일명에서 유효하지 않은 문자를 제거합니다
        /// </summary>
        /// <param name="fileName">정리할 파일명</param>
        /// <returns>정리된 파일명</returns>
        public static string CleanFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // 유효하지 않은 문자 제거
            var cleaned = new string(fileName.Where(c => !InvalidFileNameChars.Contains(c) && !char.IsControl(c)).ToArray());

            // 연속된 공백을 단일 공백으로 변환
            cleaned = Regex.Replace(cleaned, @"\s+", " ");

            // 앞뒤 공백 제거
            cleaned = cleaned.Trim();

            // 예약된 파일명 처리
            var nameWithoutExt = Path.GetFileNameWithoutExtension(cleaned);
            var extension = Path.GetExtension(cleaned);
            
            if (ReservedFileNames.Contains(nameWithoutExt.ToUpper()))
            {
                cleaned = nameWithoutExt + "_file" + extension;
            }

            return cleaned;
        }

        #endregion

        #region 경로 유효성 검사

        /// <summary>
        /// 폴더 경로의 유효성을 검사합니다
        /// </summary>
        /// <param name="path">검사할 경로</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ValidationResult.CreateFailure("경로가 비어있습니다.");
            }

            // 길이 검사
            if (path.Length > MaxPathLength)
            {
                return ValidationResult.CreateFailure($"경로가 너무 깁니다. 최대 {MaxPathLength}자까지 허용됩니다.");
            }

            // 유효하지 않은 문자 검사
            var invalidChars = path.Where(c => InvalidPathChars.Contains(c)).ToList();
            if (invalidChars.Any())
            {
                var invalidCharsString = string.Join(", ", invalidChars.Distinct().Select(c => $"'{c}'"));
                return ValidationResult.CreateFailure($"경로에 사용할 수 없는 문자가 포함되어 있습니다: {invalidCharsString}");
            }

            // 경로 형식 검사
            try
            {
                var fullPath = Path.GetFullPath(path);
                
                // 루트 경로 확인
                if (!Path.IsPathRooted(fullPath))
                {
                    return ValidationResult.CreateFailure("절대 경로를 입력해주세요.");
                }

                // 드라이브 존재 확인
                var drive = Path.GetPathRoot(fullPath);
                if (!string.IsNullOrEmpty(drive) && !DriveInfo.GetDrives().Any(d => d.Name.Equals(drive, StringComparison.OrdinalIgnoreCase)))
                {
                    return ValidationResult.CreateFailure($"드라이브 '{drive}'을(를) 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                return ValidationResult.CreateFailure($"경로 형식이 올바르지 않습니다: {ex.Message}");
            }

            return ValidationResult.CreateSuccess();
        }

        /// <summary>
        /// 파일 경로의 유효성을 검사합니다 (파일 존재 여부 포함)
        /// </summary>
        /// <param name="filePath">검사할 파일 경로</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return ValidationResult.CreateFailure("파일 경로가 비어있습니다.");
            }

            // 경로 부분 검사
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                var directoryValidation = ValidateFolderPath(directory);
                if (!directoryValidation.IsValid)
                {
                    return directoryValidation;
                }
            }

            // 파일명 부분 검사
            var fileName = Path.GetFileName(filePath);
            var fileNameValidation = ValidateFileName(fileName);
            if (!fileNameValidation.IsValid)
            {
                return fileNameValidation;
            }

            // 파일 존재 여부 확인
            if (!File.Exists(filePath))
            {
                return ValidationResult.CreateFailure("지정된 파일이 존재하지 않습니다.");
            }

            return ValidationResult.CreateSuccess();
        }

        /// <summary>
        /// 폴더가 존재하는지 확인합니다
        /// </summary>
        /// <param name="path">확인할 폴더 경로</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFolderExists(string path)
        {
            var pathValidation = ValidateFolderPath(path);
            if (!pathValidation.IsValid)
            {
                return pathValidation;
            }

            if (!Directory.Exists(path))
            {
                return ValidationResult.CreateFailure("지정된 폴더가 존재하지 않습니다.");
            }

            return ValidationResult.CreateSuccess();
        }

        /// <summary>
        /// 파일이 존재하는지 확인합니다
        /// </summary>
        /// <param name="filePath">확인할 파일 경로</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFileExists(string filePath)
        {
            var fileValidation = ValidateFilePath(filePath);
            if (!fileValidation.IsValid)
            {
                return fileValidation;
            }

            if (!File.Exists(filePath))
            {
                return ValidationResult.CreateFailure("지정된 파일이 존재하지 않습니다.");
            }

            return ValidationResult.CreateSuccess();
        }

        #endregion

        #region 텍스트 입력 유효성 검사

        /// <summary>
        /// 일반 텍스트 입력의 유효성을 검사합니다
        /// </summary>
        /// <param name="input">검사할 텍스트</param>
        /// <param name="fieldName">필드명 (에러 메시지용)</param>
        /// <param name="isRequired">필수 여부</param>
        /// <param name="minLength">최소 길이</param>
        /// <param name="maxLength">최대 길이</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateTextInput(string input, string fieldName, bool isRequired = true, int minLength = MinInputLength, int? maxLength = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                if (isRequired)
                {
                    return ValidationResult.CreateFailure($"{fieldName}을(를) 입력해주세요.");
                }
                return ValidationResult.CreateSuccess();
            }

            var trimmedInput = input.Trim();

            // 최소 길이 검사
            if (trimmedInput.Length < minLength)
            {
                return ValidationResult.CreateFailure($"{fieldName}은(는) 최소 {minLength}자 이상이어야 합니다.");
            }

            // 최대 길이 검사
            if (maxLength.HasValue && trimmedInput.Length > maxLength.Value)
            {
                return ValidationResult.CreateFailure($"{fieldName}은(는) 최대 {maxLength.Value}자까지 허용됩니다.");
            }

            return ValidationResult.CreateSuccess();
        }

        /// <summary>
        /// 파일 확장자의 유효성을 검사합니다
        /// </summary>
        /// <param name="extension">검사할 확장자</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFileExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return ValidationResult.CreateFailure("파일 확장자를 입력해주세요.");
            }

            var cleanExtension = extension.Trim();

            // 점으로 시작하지 않으면 점 추가
            if (!cleanExtension.StartsWith("."))
            {
                cleanExtension = "." + cleanExtension;
            }

            // 유효하지 않은 문자 검사
            var invalidChars = cleanExtension.Where(c => InvalidFileNameChars.Contains(c) || char.IsControl(c)).ToList();
            if (invalidChars.Any())
            {
                var invalidCharsString = string.Join(", ", invalidChars.Distinct().Select(c => $"'{c}'"));
                return ValidationResult.CreateFailure($"확장자에 사용할 수 없는 문자가 포함되어 있습니다: {invalidCharsString}");
            }

            // 확장자 길이 검사 (일반적으로 10자 이내)
            if (cleanExtension.Length > 10)
            {
                return ValidationResult.CreateFailure("확장자가 너무 깁니다. 최대 10자까지 허용됩니다.");
            }

            // 점만으로 구성된 확장자 검사
            if (cleanExtension.All(c => c == '.'))
            {
                return ValidationResult.CreateFailure("올바른 확장자를 입력해주세요.");
            }

            return ValidationResult.CreateSuccess();
        }

        #endregion

        #region 종합 유효성 검사

        /// <summary>
        /// 여러 유효성 검사 결과를 종합합니다
        /// </summary>
        /// <param name="validationResults">검사 결과 목록</param>
        /// <returns>종합 유효성 검사 결과</returns>
        public static ValidationResult CombineValidationResults(params ValidationResult[] validationResults)
        {
            var errors = validationResults.Where(r => !r.IsValid).SelectMany(r => r.ErrorMessages).ToList();
            
            if (errors.Any())
            {
                return ValidationResult.CreateFailure(errors);
            }

            return ValidationResult.CreateSuccess();
        }

        /// <summary>
        /// 파일 생성 요청의 전체 유효성을 검사합니다
        /// </summary>
        /// <param name="abbreviation">약어</param>
        /// <param name="title">제목</param>
        /// <param name="suffix">접미어</param>
        /// <param name="extension">확장자</param>
        /// <param name="outputPath">출력 경로</param>
        /// <param name="templatePath">템플릿 경로 (선택사항)</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateFileCreationRequest(
            string abbreviation, 
            string title, 
            string suffix, 
            string extension, 
            string outputPath, 
            string templatePath = null)
        {
            var validations = new List<ValidationResult>
            {
                ValidateTextInput(abbreviation, "약어", false, 1, 20),
                ValidateTextInput(title, "제목", true, 1, 100),
                ValidateTextInput(suffix, "접미어", false, 1, 50),
                ValidateFileExtension(extension),
                ValidateFolderExists(outputPath)
            };

            // 템플릿 파일이 지정된 경우 검사
            if (!string.IsNullOrWhiteSpace(templatePath))
            {
                validations.Add(ValidateFileExists(templatePath));
            }

            return CombineValidationResults(validations.ToArray());
        }

        #endregion

        #region 권한 검사

        /// <summary>
        /// 폴더에 쓰기 권한이 있는지 확인합니다
        /// </summary>
        /// <param name="folderPath">확인할 폴더 경로</param>
        /// <returns>유효성 검사 결과</returns>
        public static ValidationResult ValidateWritePermission(string folderPath)
        {
            try
            {
                var testFile = Path.Combine(folderPath, $"test_write_permission_{Guid.NewGuid()}.tmp");
                
                // 임시 파일 생성 및 삭제로 권한 확인
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                
                return ValidationResult.CreateSuccess();
            }
            catch (UnauthorizedAccessException)
            {
                return ValidationResult.CreateFailure("해당 폴더에 파일을 생성할 권한이 없습니다.");
            }
            catch (Exception ex)
            {
                return ValidationResult.CreateFailure($"폴더 접근 권한을 확인할 수 없습니다: {ex.Message}");
            }
        }

        #endregion
    }
}