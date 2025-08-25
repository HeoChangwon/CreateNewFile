using System.Collections.Generic;
using System.Threading.Tasks;
using CreateNewFile.Models;
using CreateNewFile.Utils;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 설정 관리 서비스 인터페이스
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 애플리케이션 설정을 로드합니다.
        /// </summary>
        /// <returns>애플리케이션 설정</returns>
        Task<AppSettings> LoadSettingsAsync();

        /// <summary>
        /// 애플리케이션 설정을 저장합니다.
        /// </summary>
        /// <param name="settings">저장할 설정</param>
        /// <returns>저장 성공 여부</returns>
        Task<bool> SaveSettingsAsync(AppSettings settings);

        /// <summary>
        /// 기본 설정으로 초기화합니다.
        /// </summary>
        /// <returns>초기화된 설정</returns>
        Task<AppSettings> InitializeDefaultSettingsAsync();

        /// <summary>
        /// 지정된 타입의 미리 정의된 항목 목록을 가져옵니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <returns>미리 정의된 항목 목록</returns>
        Task<List<PresetItem>> GetPresetItemsAsync(PresetType type);

        /// <summary>
        /// 미리 정의된 항목을 추가합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="item">추가할 항목</param>
        /// <returns>추가 성공 여부</returns>
        Task<bool> AddPresetItemAsync(PresetType type, PresetItem item);

        /// <summary>
        /// 미리 정의된 항목을 업데이트합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="item">업데이트할 항목</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdatePresetItemAsync(PresetType type, PresetItem item);

        /// <summary>
        /// 미리 정의된 항목을 삭제합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="itemId">삭제할 항목 ID</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeletePresetItemAsync(PresetType type, string itemId);

        /// <summary>
        /// 미리 정의된 항목의 사용 정보를 업데이트합니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <param name="itemId">항목 ID</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> MarkItemAsUsedAsync(PresetType type, string itemId);

        /// <summary>
        /// 설정 파일을 백업합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <returns>백업 성공 여부</returns>
        Task<bool> BackupSettingsAsync(string backupPath);

        /// <summary>
        /// 설정 파일을 복원합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <returns>복원 성공 여부</returns>
        Task<bool> RestoreSettingsAsync(string backupPath);

        /// <summary>
        /// 설정 파일이 존재하는지 확인합니다.
        /// </summary>
        /// <returns>설정 파일 존재 여부</returns>
        bool SettingsFileExists();

        /// <summary>
        /// 설정의 유효성을 검사합니다.
        /// </summary>
        /// <param name="settings">검사할 설정</param>
        /// <returns>유효성 검사 결과</returns>
        ValidationResult ValidateSettings(AppSettings settings);

        /// <summary>
        /// 체크박스 활성화 상태를 저장합니다.
        /// </summary>
        /// <param name="isDateTimeEnabled">날짜/시간 활성화 상태</param>
        /// <param name="isAbbreviationEnabled">약어 활성화 상태</param>
        /// <param name="isTitleEnabled">제목 활성화 상태</param>
        /// <param name="isSuffixEnabled">접미어 활성화 상태</param>
        /// <returns>저장 성공 여부</returns>
        Task<bool> SaveCheckboxStatesAsync(bool isDateTimeEnabled, bool isAbbreviationEnabled, bool isTitleEnabled, bool isSuffixEnabled);

        /// <summary>
        /// 체크박스 활성화 상태를 로드합니다.
        /// </summary>
        /// <returns>체크박스 상태 튜플</returns>
        Task<(bool isDateTime, bool isAbbreviation, bool isTitle, bool isSuffix)> LoadCheckboxStatesAsync();
    }

    /// <summary>
    /// 미리 정의된 항목의 타입을 나타내는 열거형
    /// </summary>
    public enum PresetType
    {
        /// <summary>
        /// 약어
        /// </summary>
        Abbreviation,

        /// <summary>
        /// 제목
        /// </summary>
        Title,

        /// <summary>
        /// 접미어
        /// </summary>
        Suffix,

        /// <summary>
        /// 확장자
        /// </summary>
        Extension,

        /// <summary>
        /// 출력 경로
        /// </summary>
        OutputPath,

        /// <summary>
        /// 템플릿 경로
        /// </summary>
        TemplatePath
    }
}