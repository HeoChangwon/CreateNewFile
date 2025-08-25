using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CreateNewFile.Models;
using CreateNewFile.Utils;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 설정 관리 서비스 구현 클래스
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings? _cachedSettings;
        private readonly object _lockObject = new object();

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="settingsFilePath">설정 파일 경로 (선택사항)</param>
        public SettingsService(string? settingsFilePath = null)
        {
            _settingsFilePath = settingsFilePath ?? GetDefaultSettingsFilePath();
        }

        /// <summary>
        /// 기본 설정 파일 경로를 가져옵니다.
        /// </summary>
        /// <returns>기본 설정 파일 경로</returns>
        private static string GetDefaultSettingsFilePath()
        {
            var configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            return Path.Combine(configDirectory, "appsettings.json");
        }

        /// <summary>
        /// 기본 템플릿 파일 경로를 가져옵니다.
        /// </summary>
        /// <returns>기본 템플릿 파일 경로</returns>
        private static string GetDefaultTemplateFilePath()
        {
            var configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            return Path.Combine(configDirectory, "appsettings.default.json");
        }

        /// <summary>
        /// 애플리케이션 설정을 로드합니다.
        /// </summary>
        /// <returns>애플리케이션 설정</returns>
        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    if (_cachedSettings != null)
                        return _cachedSettings;
                }

                if (!File.Exists(_settingsFilePath))
                {
                    return await InitializeDefaultSettingsAsync();
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json);

                if (settings == null)
                {
                    return await InitializeDefaultSettingsAsync();
                }

                // 설정 유효성 검사
                var validationResult = ValidateSettings(settings);
                if (!validationResult.IsValid)
                {
                    // 유효하지 않은 설정인 경우 기본값으로 병합
                    var defaultSettings = new AppSettings();
                    defaultSettings.LoadDefaults();
                    MergeWithDefaults(settings, defaultSettings);
                }

                lock (_lockObject)
                {
                    _cachedSettings = settings;
                }

                return settings;
            }
            catch (Exception ex)
            {
                // 로깅이 구현되면 여기에 로그 추가
                System.Diagnostics.Debug.WriteLine($"설정 로드 실패: {ex.Message}");
                return await InitializeDefaultSettingsAsync();
            }
        }

        /// <summary>
        /// 애플리케이션 설정을 저장합니다.
        /// </summary>
        /// <param name="settings">저장할 설정</param>
        /// <returns>저장 성공 여부</returns>
        public async Task<bool> SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                if (settings == null)
                    return false;

                // 설정 유효성 검사
                var validationResult = ValidateSettings(settings);
                if (!validationResult.IsValid)
                    return false;

                // 디렉토리 생성
                var directory = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // JSON으로 직렬화
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(_settingsFilePath, json);

                // 캐시 업데이트
                lock (_lockObject)
                {
                    _cachedSettings = settings.Clone();
                }

                return true;
            }
            catch (Exception ex)
            {
                // 로깅이 구현되면 여기에 로그 추가
                System.Diagnostics.Debug.WriteLine($"설정 저장 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 기본 설정으로 초기화합니다.
        /// </summary>
        /// <returns>초기화된 설정</returns>
        public async Task<AppSettings> InitializeDefaultSettingsAsync()
        {
            try
            {
                // 기본 템플릿 파일에서 설정을 로드 시도
                var defaultSettingsPath = GetDefaultTemplateFilePath();
                if (File.Exists(defaultSettingsPath))
                {
                    System.Diagnostics.Debug.WriteLine($"기본 템플릿 파일에서 설정 로드: {defaultSettingsPath}");
                    var defaultJson = await File.ReadAllTextAsync(defaultSettingsPath);
                    var defaultSettings = JsonConvert.DeserializeObject<AppSettings>(defaultJson);
                    
                    if (defaultSettings != null)
                    {
                        // 기본 출력 경로를 사용자 문서 폴더로 설정
                        if (string.IsNullOrEmpty(defaultSettings.DefaultOutputPath))
                        {
                            defaultSettings.DefaultOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        }

                        await SaveSettingsAsync(defaultSettings);
                        System.Diagnostics.Debug.WriteLine("기본 템플릿에서 설정을 성공적으로 로드했습니다.");
                        return defaultSettings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"기본 템플릿 파일 로드 실패: {ex.Message}");
            }

            // 기본 템플릿 파일 로드에 실패한 경우 코드에서 기본값 생성
            System.Diagnostics.Debug.WriteLine("코드에서 기본 설정 생성");
            var settings = new AppSettings();
            settings.LoadDefaults();

            await SaveSettingsAsync(settings);
            return settings;
        }

        /// <summary>
        /// 지정된 타입의 미리 정의된 항목 목록을 가져옵니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <returns>미리 정의된 항목 목록</returns>
        public async Task<List<PresetItem>> GetPresetItemsAsync(PresetType type)
        {
            var settings = await LoadSettingsAsync();
            
            return type switch
            {
                PresetType.Abbreviation => settings.Abbreviations,
                PresetType.Title => settings.Titles,
                PresetType.Suffix => settings.Suffixes,
                PresetType.Extension => settings.Extensions,
                _ => new List<PresetItem>()
            };
        }

        /// <summary>
        /// 미리 정의된 항목을 추가합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="item">추가할 항목</param>
        /// <returns>추가 성공 여부</returns>
        public async Task<bool> AddPresetItemAsync(PresetType type, PresetItem item)
        {
            try
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Value))
                    return false;

                var settings = await LoadSettingsAsync();
                var targetList = GetPresetListByType(settings, type);

                // 중복 확인
                if (targetList.Any(x => x.Value.Equals(item.Value, StringComparison.OrdinalIgnoreCase)))
                    return false;

                // ID 생성 (없는 경우)
                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    item.Id = Guid.NewGuid().ToString();
                }


                targetList.Add(item);
                return await SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"항목 추가 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 미리 정의된 항목을 업데이트합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="item">업데이트할 항목</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdatePresetItemAsync(PresetType type, PresetItem item)
        {
            try
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                    return false;

                var settings = await LoadSettingsAsync();
                var targetList = GetPresetListByType(settings, type);

                var existingItem = targetList.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem == null)
                    return false;

                // 값 업데이트
                existingItem.Value = item.Value;
                existingItem.IsEnabled = item.IsEnabled;

                return await SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"항목 업데이트 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 미리 정의된 항목을 삭제합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="itemId">삭제할 항목 ID</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeletePresetItemAsync(PresetType type, string itemId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemId))
                    return false;

                var settings = await LoadSettingsAsync();
                var targetList = GetPresetListByType(settings, type);

                var itemToRemove = targetList.FirstOrDefault(x => x.Id == itemId);
                if (itemToRemove == null)
                    return false;

                targetList.Remove(itemToRemove);
                return await SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"항목 삭제 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 미리 정의된 항목의 사용 정보를 업데이트합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="itemId">항목 ID</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> MarkItemAsUsedAsync(PresetType type, string itemId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(itemId))
                    return false;

                var settings = await LoadSettingsAsync();
                var targetList = GetPresetListByType(settings, type);

                var item = targetList.FirstOrDefault(x => x.Id == itemId);
                if (item == null)
                    return false;

                return await SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"사용 정보 업데이트 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 설정 파일을 백업합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <returns>백업 성공 여부</returns>
        public async Task<bool> BackupSettingsAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return false;

                var directory = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var sourceStream = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read);
                using var destinationStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write);
                
                await sourceStream.CopyToAsync(destinationStream);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"설정 백업 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 설정 파일을 복원합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <returns>복원 성공 여부</returns>
        public async Task<bool> RestoreSettingsAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                // 백업 파일의 유효성 검사
                var backupJson = await File.ReadAllTextAsync(backupPath);
                var backupSettings = JsonConvert.DeserializeObject<AppSettings>(backupJson);
                
                if (backupSettings == null)
                    return false;

                var validationResult = ValidateSettings(backupSettings);
                if (!validationResult.IsValid)
                    return false;

                // 현재 설정 파일 백업 (복원 실패 시 롤백용)
                var rollbackPath = _settingsFilePath + ".rollback";
                if (File.Exists(_settingsFilePath))
                {
                    File.Copy(_settingsFilePath, rollbackPath, true);
                }

                try
                {
                    File.Copy(backupPath, _settingsFilePath, true);
                    
                    // 캐시 무효화
                    lock (_lockObject)
                    {
                        _cachedSettings = null;
                    }

                    // 롤백 파일 삭제
                    if (File.Exists(rollbackPath))
                    {
                        File.Delete(rollbackPath);
                    }

                    return true;
                }
                catch
                {
                    // 복원 실패 시 롤백
                    if (File.Exists(rollbackPath))
                    {
                        File.Copy(rollbackPath, _settingsFilePath, true);
                        File.Delete(rollbackPath);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"설정 복원 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 설정 파일이 존재하는지 확인합니다.
        /// </summary>
        /// <returns>설정 파일 존재 여부</returns>
        public bool SettingsFileExists()
        {
            return File.Exists(_settingsFilePath);
        }

        /// <summary>
        /// 설정의 유효성을 검사합니다.
        /// </summary>
        /// <param name="settings">검사할 설정</param>
        /// <returns>유효성 검사 결과</returns>
        public ValidationResult ValidateSettings(AppSettings settings)
        {
            var result = new ValidationResult();

            if (settings == null)
            {
                result.AddError("설정 정보가 없습니다.");
                return result;
            }

            // 기본 경로 검증
            if (!string.IsNullOrWhiteSpace(settings.DefaultOutputPath))
            {
                if (!FileNameBuilder.IsValidPath(settings.DefaultOutputPath))
                {
                    result.AddWarning("기본 출력 경로가 유효하지 않습니다.");
                }
                else if (!Directory.Exists(settings.DefaultOutputPath))
                {
                    result.AddWarning("기본 출력 경로가 존재하지 않습니다.");
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.DefaultTemplatePath))
            {
                if (!FileNameBuilder.IsValidPath(settings.DefaultTemplatePath))
                {
                    result.AddWarning("기본 템플릿 경로가 유효하지 않습니다.");
                }
                else if (!File.Exists(settings.DefaultTemplatePath))
                {
                    result.AddWarning("기본 템플릿 파일이 존재하지 않습니다.");
                }
            }

            // 컬렉션 null 체크 및 초기화
            settings.Abbreviations ??= new List<PresetItem>();
            settings.Titles ??= new List<PresetItem>();
            settings.Suffixes ??= new List<PresetItem>();
            settings.Extensions ??= new List<PresetItem>();
            settings.OutputPaths ??= new List<PresetItem>();
            settings.TemplatePaths ??= new List<PresetItem>();

            return result;
        }

        /// <summary>
        /// 타입에 따른 미리 정의된 항목 목록을 가져옵니다.
        /// </summary>
        /// <param name="settings">설정 객체</param>
        /// <param name="type">항목 타입</param>
        /// <returns>미리 정의된 항목 목록</returns>
        private static List<PresetItem> GetPresetListByType(AppSettings settings, PresetType type)
        {
            return type switch
            {
                PresetType.Abbreviation => settings.Abbreviations,
                PresetType.Title => settings.Titles,
                PresetType.Suffix => settings.Suffixes,
                PresetType.Extension => settings.Extensions,
                _ => throw new ArgumentException($"지원하지 않는 미리 정의된 항목 타입: {type}")
            };
        }

        /// <summary>
        /// 설정을 기본값과 병합합니다.
        /// </summary>
        /// <param name="settings">현재 설정</param>
        /// <param name="defaultSettings">기본 설정</param>
        private static void MergeWithDefaults(AppSettings settings, AppSettings defaultSettings)
        {
            settings.DefaultOutputPath ??= defaultSettings.DefaultOutputPath;
            settings.DefaultTemplatePath ??= defaultSettings.DefaultTemplatePath;
            
            settings.Abbreviations ??= defaultSettings.Abbreviations;
            settings.Titles ??= defaultSettings.Titles;
            settings.Suffixes ??= defaultSettings.Suffixes;
            settings.Extensions ??= defaultSettings.Extensions;
            settings.OutputPaths ??= defaultSettings.OutputPaths;
            settings.TemplatePaths ??= defaultSettings.TemplatePaths;
        }

        /// <summary>
        /// 체크박스 활성화 상태를 저장합니다.
        /// </summary>
        /// <param name="isDateTimeEnabled">날짜/시간 활성화 상태</param>
        /// <param name="isAbbreviationEnabled">약어 활성화 상태</param>
        /// <param name="isTitleEnabled">제목 활성화 상태</param>
        /// <param name="isSuffixEnabled">접미어 활성화 상태</param>
        /// <returns>저장 성공 여부</returns>
        public async Task<bool> SaveCheckboxStatesAsync(bool isDateTimeEnabled, bool isAbbreviationEnabled, bool isTitleEnabled, bool isSuffixEnabled)
        {
            try
            {
                var settings = await LoadSettingsAsync();
                settings.IsDateTimeEnabled = isDateTimeEnabled;
                settings.IsAbbreviationEnabled = isAbbreviationEnabled;
                settings.IsTitleEnabled = isTitleEnabled;
                settings.IsSuffixEnabled = isSuffixEnabled;

                return await SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"체크박스 상태 저장 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 체크박스 활성화 상태를 로드합니다.
        /// </summary>
        /// <returns>체크박스 상태 튜플</returns>
        public async Task<(bool isDateTime, bool isAbbreviation, bool isTitle, bool isSuffix)> LoadCheckboxStatesAsync()
        {
            try
            {
                var settings = await LoadSettingsAsync();
                return (settings.IsDateTimeEnabled, settings.IsAbbreviationEnabled, settings.IsTitleEnabled, settings.IsSuffixEnabled);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"체크박스 상태 로드 실패: {ex.Message}");
                // 기본값 반환
                return (true, true, true, true);
            }
        }
    }
}