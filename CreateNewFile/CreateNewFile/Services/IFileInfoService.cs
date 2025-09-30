using CreateNewFile.Models;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 파일 정보 관리 서비스 인터페이스
    /// </summary>
    public interface IFileInfoService
    {
        /// <summary>
        /// 모든 파일 정보 목록을 가져옵니다.
        /// </summary>
        /// <returns>파일 정보 목록</returns>
        Task<List<FileInfoModel>> GetAllFileInfosAsync();

        /// <summary>
        /// 특정 ID의 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="id">파일 정보 ID</param>
        /// <returns>파일 정보 객체, 없으면 null</returns>
        Task<FileInfoModel?> GetFileInfoByIdAsync(string id);

        /// <summary>
        /// 파일 정보를 저장합니다.
        /// </summary>
        /// <param name="fileInfo">저장할 파일 정보</param>
        /// <returns>저장 성공 여부</returns>
        Task<bool> SaveFileInfoAsync(FileInfoModel fileInfo);

        /// <summary>
        /// 파일 정보를 업데이트합니다.
        /// </summary>
        /// <param name="fileInfo">업데이트할 파일 정보</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateFileInfoAsync(FileInfoModel fileInfo);

        /// <summary>
        /// 파일 정보를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 파일 정보 ID</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteFileInfoAsync(string id);

        /// <summary>
        /// 파일 정보를 파일로 내보냅니다.
        /// </summary>
        /// <param name="fileInfo">내보낼 파일 정보</param>
        /// <param name="filePath">내보낼 파일 경로</param>
        /// <returns>내보내기 성공 여부</returns>
        Task<bool> ExportFileInfoAsync(FileInfoModel fileInfo, string filePath);

        /// <summary>
        /// 파일에서 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="filePath">가져올 파일 경로</param>
        /// <returns>가져온 파일 정보</returns>
        Task<FileInfoModel?> ImportFileInfoAsync(string filePath);

        /// <summary>
        /// 여러 파일 정보를 파일로 내보냅니다.
        /// </summary>
        /// <param name="fileInfos">내보낼 파일 정보 목록</param>
        /// <param name="filePath">내보낼 파일 경로</param>
        /// <returns>내보내기 성공 여부</returns>
        Task<bool> ExportMultipleFileInfosAsync(List<FileInfoModel> fileInfos, string filePath);

        /// <summary>
        /// 파일에서 여러 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="filePath">가져올 파일 경로</param>
        /// <returns>가져온 파일 정보 목록</returns>
        Task<List<FileInfoModel>> ImportMultipleFileInfosAsync(string filePath);

        /// <summary>
        /// 검색 조건에 맞는 파일 정보를 찾습니다.
        /// </summary>
        /// <param name="searchText">검색할 텍스트</param>
        /// <param name="searchInTags">태그에서 검색할지 여부</param>
        /// <param name="onlyFavorites">즐겨찾기만 검색할지 여부</param>
        /// <returns>검색된 파일 정보 목록</returns>
        Task<List<FileInfoModel>> SearchFileInfosAsync(string searchText, bool searchInTags = true, bool onlyFavorites = false);

        /// <summary>
        /// 파일 정보의 사용 통계를 업데이트합니다.
        /// </summary>
        /// <param name="id">파일 정보 ID</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> MarkFileInfoModelAsUsedAsync(string id);

        /// <summary>
        /// 즐겨찾기 상태를 토글합니다.
        /// </summary>
        /// <param name="id">파일 정보 ID</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> ToggleFavoriteAsync(string id);

        /// <summary>
        /// 데이터를 백업합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <returns>백업 성공 여부</returns>
        Task<bool> BackupDataAsync(string backupPath);

        /// <summary>
        /// 백업 데이터를 복원합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <param name="replaceExisting">기존 데이터를 덮어쓸지 여부</param>
        /// <returns>복원 성공 여부</returns>
        Task<bool> RestoreDataAsync(string backupPath, bool replaceExisting = false);

        /// <summary>
        /// 데이터 파일이 존재하는지 확인합니다.
        /// </summary>
        /// <returns>데이터 파일 존재 여부</returns>
        bool DataFileExists();

        /// <summary>
        /// 데이터 저장소를 초기화합니다.
        /// </summary>
        /// <returns>초기화 성공 여부</returns>
        Task<bool> InitializeDataStoreAsync();

        /// <summary>
        /// 중복된 이름이 있는지 확인합니다.
        /// </summary>
        /// <param name="name">확인할 이름</param>
        /// <param name="excludeId">제외할 ID (수정 시 사용)</param>
        /// <returns>중복 여부</returns>
        Task<bool> IsDuplicateNameAsync(string name, string? excludeId = null);

        /// <summary>
        /// 사용 빈도 순으로 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="count">가져올 개수</param>
        /// <returns>사용 빈도 순 파일 정보 목록</returns>
        Task<List<FileInfoModel>> GetMostUsedFileInfosAsync(int count = 10);

        /// <summary>
        /// 최근에 사용한 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="count">가져올 개수</param>
        /// <returns>최근 사용 순 파일 정보 목록</returns>
        Task<List<FileInfoModel>> GetRecentlyUsedFileInfosAsync(int count = 10);

        /// <summary>
        /// 즐겨찾기 파일 정보를 가져옵니다.
        /// </summary>
        /// <returns>즐겨찾기 파일 정보 목록</returns>
        Task<List<FileInfoModel>> GetFavoriteFileInfosAsync();

        /// <summary>
        /// 파일 정보를 정렬합니다.
        /// </summary>
        /// <param name="sortBy">정렬 기준</param>
        /// <param name="ascending">오름차순 여부</param>
        /// <returns>정렬된 파일 정보 목록</returns>
        Task<List<FileInfoModel>> GetSortedFileInfosAsync(FileInfoModelSortBy sortBy, bool ascending = true);
    }

    /// <summary>
    /// 파일 정보 정렬 기준
    /// </summary>
    public enum FileInfoModelSortBy
    {
        Name,
        CreatedAt,
        ModifiedAt,
        UsageCount,
        LastUsed
    }
}