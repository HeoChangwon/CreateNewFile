using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CreateNewFile.Models;
using CreateNewFile.ViewModels;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 프로젝트 설정 파일 관리 서비스 구현
    /// </summary>
    public class ProjectConfigService : IProjectConfigService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// 프로젝트 설정 파일(.cnfjson)을 저장합니다.
        /// </summary>
        /// <param name="config">저장할 프로젝트 설정</param>
        /// <param name="filePath">저장할 파일 경로</param>
        /// <returns>저장 성공 여부</returns>
        public async Task<bool> SaveProjectConfigAsync(ProjectConfig config, string filePath)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("파일 경로가 필요합니다.", nameof(filePath));

                // 유효성 검사
                var validation = config.Validate();
                if (!validation.IsValid)
                    throw new InvalidOperationException($"프로젝트 설정이 유효하지 않습니다: {validation.ErrorMessage}");

                // 파일 확장자 확인
                if (!filePath.EndsWith(".cnfjson", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = Path.ChangeExtension(filePath, ".cnfjson");
                }

                // 디렉토리가 없으면 생성
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 수정 시간 업데이트
                config.ModifiedAt = DateTime.Now;

                // JSON으로 직렬화
                var json = JsonSerializer.Serialize(config, _jsonOptions);

                // UTF-8 BOM과 함께 저장
                await File.WriteAllTextAsync(filePath, json, new UTF8Encoding(true));

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"프로젝트 설정 저장 오류: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 프로젝트 설정 파일(.cnfjson)을 로드합니다.
        /// </summary>
        /// <param name="filePath">로드할 파일 경로</param>
        /// <returns>로드된 프로젝트 설정</returns>
        public async Task<ProjectConfig?> LoadProjectConfigAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("파일 경로가 필요합니다.", nameof(filePath));

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"프로젝트 설정 파일을 찾을 수 없습니다: {filePath}");

                // 파일 확장자 확인
                if (!filePath.EndsWith(".cnfjson", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("올바른 프로젝트 설정 파일 형식이 아닙니다. (.cnfjson 파일이 필요합니다)");

                // JSON 파일 읽기
                var json = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidOperationException("프로젝트 설정 파일이 비어 있습니다.");

                // JSON 역직렬화
                var config = JsonSerializer.Deserialize<ProjectConfig>(json, _jsonOptions);

                if (config == null)
                    throw new InvalidOperationException("프로젝트 설정을 로드할 수 없습니다.");

                // 유효성 검사
                var validation = config.Validate();
                if (!validation.IsValid)
                    throw new InvalidOperationException($"로드된 프로젝트 설정이 유효하지 않습니다: {validation.ErrorMessage}");

                return config;
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON 파싱 오류: {ex.Message}");
                throw new InvalidOperationException($"프로젝트 설정 파일 형식이 올바르지 않습니다: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"프로젝트 설정 로드 오류: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 프로젝트 설정 파일의 유효성을 검증합니다.
        /// </summary>
        /// <param name="filePath">검증할 파일 경로</param>
        /// <returns>유효성 검사 결과와 오류 메시지</returns>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateProjectConfigFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return (false, "파일 경로가 필요합니다.");

                if (!File.Exists(filePath))
                    return (false, $"파일을 찾을 수 없습니다: {filePath}");

                if (!filePath.EndsWith(".cnfjson", StringComparison.OrdinalIgnoreCase))
                    return (false, "올바른 프로젝트 설정 파일 형식이 아닙니다. (.cnfjson 파일이 필요합니다)");

                var config = await LoadProjectConfigAsync(filePath);
                if (config == null)
                    return (false, "프로젝트 설정을 로드할 수 없습니다.");

                return config.Validate();
            }
            catch (Exception ex)
            {
                return (false, $"유효성 검사 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        /// <summary>
        /// 프로젝트 설정을 현재 MainViewModel 상태로부터 생성합니다.
        /// </summary>
        /// <param name="viewModel">MainViewModel 인스턴스</param>
        /// <returns>생성된 프로젝트 설정</returns>
        public ProjectConfig CreateProjectConfigFromViewModel(MainViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var config = new ProjectConfig
            {
                Name = string.IsNullOrWhiteSpace(viewModel.SelectedTitle)
                    ? "Unnamed_Project"
                    : viewModel.SelectedTitle,
                Description = string.Empty,
                FileInfo = new ProjectFileInfo
                {
                    DateTime = viewModel.SelectedDateTime,
                    Abbreviation = viewModel.SelectedAbbreviation ?? string.Empty,
                    Title = viewModel.SelectedTitle ?? string.Empty,
                    Suffix = viewModel.SelectedSuffix ?? string.Empty,
                    Extension = viewModel.SelectedExtension ?? string.Empty,
                    IsDateTimeEnabled = viewModel.IsDateTimeEnabled,
                    IsAbbreviationEnabled = viewModel.IsAbbreviationEnabled,
                    IsTitleEnabled = viewModel.IsTitleEnabled,
                    IsSuffixEnabled = viewModel.IsSuffixEnabled
                },
                OutputPath = viewModel.SelectedOutputPath ?? string.Empty,
                TemplatePath = viewModel.SelectedTemplatePath ?? string.Empty,
                StringReplacements = viewModel.StringReplacements
                    .Select(r => (StringReplacementRule)r.Clone())
                    .ToList(),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            return config;
        }
    }
}
