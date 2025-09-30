using System.IO;
using Newtonsoft.Json;
using CreateNewFile.Models;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 파일 정보 관리 서비스 구현 클래스
    /// </summary>
    public class FileInfoService : IFileInfoService
    {
        private readonly string _dataFilePath;
        private List<FileInfoModel>? _cachedFileInfos;
        private readonly object _lockObject = new object();

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="dataFilePath">데이터 파일 경로 (선택사항)</param>
        public FileInfoService(string? dataFilePath = null)
        {
            _dataFilePath = dataFilePath ?? GetDefaultDataFilePath();
        }

        /// <summary>
        /// 기본 데이터 파일 경로를 가져옵니다.
        /// </summary>
        /// <returns>기본 데이터 파일 경로</returns>
        private static string GetDefaultDataFilePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dataDirectory = Path.Combine(appDataPath, "CreateNewFile", "data");
            
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
            
            return Path.Combine(dataDirectory, "fileinfos.json");
        }

        /// <summary>
        /// 모든 파일 정보 목록을 가져옵니다.
        /// </summary>
        /// <returns>파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> GetAllFileInfosAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    if (_cachedFileInfos != null)
                        return new List<FileInfoModel>(_cachedFileInfos);
                }

                if (!File.Exists(_dataFilePath))
                {
                    await InitializeDataStoreAsync();
                    return new List<FileInfoModel>();
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                var fileInfos = JsonConvert.DeserializeObject<List<FileInfoModel>>(json) ?? new List<FileInfoModel>();

                lock (_lockObject)
                {
                    _cachedFileInfos = new List<FileInfoModel>(fileInfos);
                }

                return fileInfos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"파일 정보 로드 실패: {ex.Message}");
                return new List<FileInfoModel>();
            }
        }

        /// <summary>
        /// 특정 ID의 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="id">파일 정보 ID</param>
        /// <returns>파일 정보 객체, 없으면 null</returns>
        public async Task<FileInfoModel?> GetFileInfoByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var fileInfos = await GetAllFileInfosAsync();
            return fileInfos.FirstOrDefault(f => f.Id == id);
        }

        /// <summary>
        /// 파일 정보를 저장합니다.
        /// </summary>
        /// <param name="fileInfo">저장할 파일 정보</param>
        /// <returns>저장 성공 여부</returns>
        public async Task<bool> SaveFileInfoAsync(FileInfoModel fileInfo)
        {
            try
            {
                if (fileInfo == null)
                    return false;

                var validation = fileInfo.Validate();
                if (!validation.IsValid)
                    return false;

                // 중복 이름 체크
                if (await IsDuplicateNameAsync(fileInfo.Name, fileInfo.Id))
                    return false;

                var fileInfos = await GetAllFileInfosAsync();

                // 기존 항목이 있는지 확인
                var existingIndex = fileInfos.FindIndex(f => f.Id == fileInfo.Id);
                if (existingIndex >= 0)
                {
                    // 업데이트
                    fileInfo.ModifiedAt = DateTime.Now;
                    fileInfos[existingIndex] = fileInfo;
                }
                else
                {
                    // 새로 추가
                    fileInfo.CreatedAt = DateTime.Now;
                    fileInfo.ModifiedAt = DateTime.Now;
                    fileInfos.Add(fileInfo);
                }

                return await SaveAllFileInfosAsync(fileInfos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"파일 정보 저장 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일 정보를 업데이트합니다.
        /// </summary>
        /// <param name="fileInfo">업데이트할 파일 정보</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdateFileInfoAsync(FileInfoModel fileInfo)
        {
            return await SaveFileInfoAsync(fileInfo);
        }

        /// <summary>
        /// 파일 정보를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 파일 정보 ID</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeleteFileInfoAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return false;

                var fileInfos = await GetAllFileInfosAsync();
                var itemToRemove = fileInfos.FirstOrDefault(f => f.Id == id);
                
                if (itemToRemove == null)
                    return false;

                fileInfos.Remove(itemToRemove);
                return await SaveAllFileInfosAsync(fileInfos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"파일 정보 삭제 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일 정보를 파일로 내보냅니다.
        /// </summary>
        /// <param name="fileInfo">내보낼 파일 정보</param>
        /// <param name="filePath">내보낼 파일 경로</param>
        /// <returns>내보내기 성공 여부</returns>
        public async Task<bool> ExportFileInfoAsync(FileInfoModel fileInfo, string filePath)
        {
            try
            {
                if (fileInfo == null || string.IsNullOrWhiteSpace(filePath))
                    return false;

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(fileInfo, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"파일 정보 내보내기 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일에서 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="filePath">가져올 파일 경로</param>
        /// <returns>가져온 파일 정보</returns>
        public async Task<FileInfoModel?> ImportFileInfoAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                var fileInfo = JsonConvert.DeserializeObject<FileInfoModel>(json);

                if (fileInfo != null)
                {
                    // 새로운 ID 생성 (중복 방지)
                    fileInfo.Id = Guid.NewGuid().ToString();
                    fileInfo.CreatedAt = DateTime.Now;
                    fileInfo.ModifiedAt = DateTime.Now;
                    fileInfo.UsageCount = 0;
                    fileInfo.LastUsed = DateTime.Now;

                    // 이름 중복 처리
                    var baseName = fileInfo.Name;
                    var counter = 1;
                    while (await IsDuplicateNameAsync(fileInfo.Name))
                    {
                        fileInfo.Name = $"{baseName} ({counter})";
                        counter++;
                    }
                }

                return fileInfo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"파일 정보 가져오기 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 여러 파일 정보를 파일로 내보냅니다.
        /// </summary>
        /// <param name="fileInfos">내보낼 파일 정보 목록</param>
        /// <param name="filePath">내보낼 파일 경로</param>
        /// <returns>내보내기 성공 여부</returns>
        public async Task<bool> ExportMultipleFileInfosAsync(List<FileInfoModel> fileInfos, string filePath)
        {
            try
            {
                if (fileInfos == null || !fileInfos.Any() || string.IsNullOrWhiteSpace(filePath))
                    return false;

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(fileInfos, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"다중 파일 정보 내보내기 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 파일에서 여러 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="filePath">가져올 파일 경로</param>
        /// <returns>가져온 파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> ImportMultipleFileInfosAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return new List<FileInfoModel>();

                var json = await File.ReadAllTextAsync(filePath);
                var fileInfos = JsonConvert.DeserializeObject<List<FileInfoModel>>(json) ?? new List<FileInfoModel>();

                // 각 파일정보의 ID와 이름을 새로 설정
                foreach (var fileInfo in fileInfos)
                {
                    fileInfo.Id = Guid.NewGuid().ToString();
                    fileInfo.CreatedAt = DateTime.Now;
                    fileInfo.ModifiedAt = DateTime.Now;
                    fileInfo.UsageCount = 0;
                    fileInfo.LastUsed = DateTime.Now;

                    // 이름 중복 처리
                    var baseName = fileInfo.Name;
                    var counter = 1;
                    while (await IsDuplicateNameAsync(fileInfo.Name))
                    {
                        fileInfo.Name = $"{baseName} ({counter})";
                        counter++;
                    }
                }

                return fileInfos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"다중 파일 정보 가져오기 실패: {ex.Message}");
                return new List<FileInfoModel>();
            }
        }

        /// <summary>
        /// 검색 조건에 맞는 파일 정보를 찾습니다.
        /// </summary>
        /// <param name="searchText">검색할 텍스트</param>
        /// <param name="searchInTags">태그에서 검색할지 여부</param>
        /// <param name="onlyFavorites">즐겨찾기만 검색할지 여부</param>
        /// <returns>검색된 파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> SearchFileInfosAsync(string searchText, bool searchInTags = true, bool onlyFavorites = false)
        {
            var fileInfos = await GetAllFileInfosAsync();

            if (onlyFavorites)
            {
                fileInfos = fileInfos.Where(f => f.IsFavorite).ToList();
            }

            if (string.IsNullOrWhiteSpace(searchText))
                return fileInfos;

            var searchLower = searchText.ToLower();
            
            return fileInfos.Where(f =>
                f.Name.ToLower().Contains(searchLower) ||
                f.Abbreviation.ToLower().Contains(searchLower) ||
                f.Title.ToLower().Contains(searchLower) ||
                f.Suffix.ToLower().Contains(searchLower) ||
                f.Description.ToLower().Contains(searchLower) ||
                (searchInTags && f.Tags.Any(tag => tag.ToLower().Contains(searchLower)))
            ).ToList();
        }

        /// <summary>
        /// 파일 정보의 사용 통계를 업데이트합니다.
        /// </summary>
        /// <param name="id">파일 정보 ID</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> MarkFileInfoModelAsUsedAsync(string id)
        {
            try
            {
                var fileInfo = await GetFileInfoByIdAsync(id);
                if (fileInfo == null)
                    return false;

                fileInfo.MarkAsUsed();
                return await UpdateFileInfoAsync(fileInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"사용 통계 업데이트 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 즐겨찾기 상태를 토글합니다.
        /// </summary>
        /// <param name="id">파일 정보 ID</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> ToggleFavoriteAsync(string id)
        {
            try
            {
                var fileInfo = await GetFileInfoByIdAsync(id);
                if (fileInfo == null)
                    return false;

                fileInfo.IsFavorite = !fileInfo.IsFavorite;
                fileInfo.ModifiedAt = DateTime.Now;
                return await UpdateFileInfoAsync(fileInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"즐겨찾기 토글 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 데이터를 백업합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <returns>백업 성공 여부</returns>
        public async Task<bool> BackupDataAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                    return false;

                var directory = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var sourceStream = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read);
                using var destinationStream = new FileStream(backupPath, FileMode.Create, FileAccess.Write);
                
                await sourceStream.CopyToAsync(destinationStream);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"데이터 백업 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 백업 데이터를 복원합니다.
        /// </summary>
        /// <param name="backupPath">백업 파일 경로</param>
        /// <param name="replaceExisting">기존 데이터를 덮어쓸지 여부</param>
        /// <returns>복원 성공 여부</returns>
        public async Task<bool> RestoreDataAsync(string backupPath, bool replaceExisting = false)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                // 백업 파일의 유효성 검사
                var backupJson = await File.ReadAllTextAsync(backupPath);
                var backupFileInfos = JsonConvert.DeserializeObject<List<FileInfoModel>>(backupJson);
                
                if (backupFileInfos == null)
                    return false;

                if (replaceExisting)
                {
                    // 기존 데이터 완전 교체
                    File.Copy(backupPath, _dataFilePath, true);
                    
                    // 캐시 무효화
                    lock (_lockObject)
                    {
                        _cachedFileInfos = null;
                    }
                }
                else
                {
                    // 기존 데이터와 병합
                    var existingFileInfos = await GetAllFileInfosAsync();
                    
                    foreach (var backupFileInfo in backupFileInfos)
                    {
                        // 새로운 ID 생성
                        backupFileInfo.Id = Guid.NewGuid().ToString();
                        backupFileInfo.CreatedAt = DateTime.Now;
                        backupFileInfo.ModifiedAt = DateTime.Now;

                        // 이름 중복 처리
                        var baseName = backupFileInfo.Name;
                        var counter = 1;
                        while (existingFileInfos.Any(f => f.Name == backupFileInfo.Name))
                        {
                            backupFileInfo.Name = $"{baseName} ({counter})";
                            counter++;
                        }

                        existingFileInfos.Add(backupFileInfo);
                    }

                    await SaveAllFileInfosAsync(existingFileInfos);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"데이터 복원 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 데이터 파일이 존재하는지 확인합니다.
        /// </summary>
        /// <returns>데이터 파일 존재 여부</returns>
        public bool DataFileExists()
        {
            return File.Exists(_dataFilePath);
        }

        /// <summary>
        /// 데이터 저장소를 초기화합니다.
        /// </summary>
        /// <returns>초기화 성공 여부</returns>
        public async Task<bool> InitializeDataStoreAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var emptyList = new List<FileInfoModel>();
                await SaveAllFileInfosAsync(emptyList);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"데이터 저장소 초기화 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 중복된 이름이 있는지 확인합니다.
        /// </summary>
        /// <param name="name">확인할 이름</param>
        /// <param name="excludeId">제외할 ID (수정 시 사용)</param>
        /// <returns>중복 여부</returns>
        public async Task<bool> IsDuplicateNameAsync(string name, string? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var fileInfos = await GetAllFileInfosAsync();
            return fileInfos.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && f.Id != excludeId);
        }

        /// <summary>
        /// 사용 빈도 순으로 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="count">가져올 개수</param>
        /// <returns>사용 빈도 순 파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> GetMostUsedFileInfosAsync(int count = 10)
        {
            var fileInfos = await GetAllFileInfosAsync();
            return fileInfos
                .OrderByDescending(f => f.UsageCount)
                .ThenByDescending(f => f.LastUsed)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 최근에 사용한 파일 정보를 가져옵니다.
        /// </summary>
        /// <param name="count">가져올 개수</param>
        /// <returns>최근 사용 순 파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> GetRecentlyUsedFileInfosAsync(int count = 10)
        {
            var fileInfos = await GetAllFileInfosAsync();
            return fileInfos
                .OrderByDescending(f => f.LastUsed)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 즐겨찾기 파일 정보를 가져옵니다.
        /// </summary>
        /// <returns>즐겨찾기 파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> GetFavoriteFileInfosAsync()
        {
            var fileInfos = await GetAllFileInfosAsync();
            return fileInfos
                .Where(f => f.IsFavorite)
                .OrderBy(f => f.Name)
                .ToList();
        }

        /// <summary>
        /// 파일 정보를 정렬합니다.
        /// </summary>
        /// <param name="sortBy">정렬 기준</param>
        /// <param name="ascending">오름차순 여부</param>
        /// <returns>정렬된 파일 정보 목록</returns>
        public async Task<List<FileInfoModel>> GetSortedFileInfosAsync(FileInfoModelSortBy sortBy, bool ascending = true)
        {
            var fileInfos = await GetAllFileInfosAsync();

            return sortBy switch
            {
                FileInfoModelSortBy.Name => ascending 
                    ? fileInfos.OrderBy(f => f.Name).ToList()
                    : fileInfos.OrderByDescending(f => f.Name).ToList(),
                FileInfoModelSortBy.CreatedAt => ascending
                    ? fileInfos.OrderBy(f => f.CreatedAt).ToList()
                    : fileInfos.OrderByDescending(f => f.CreatedAt).ToList(),
                FileInfoModelSortBy.ModifiedAt => ascending
                    ? fileInfos.OrderBy(f => f.ModifiedAt).ToList()
                    : fileInfos.OrderByDescending(f => f.ModifiedAt).ToList(),
                FileInfoModelSortBy.UsageCount => ascending
                    ? fileInfos.OrderBy(f => f.UsageCount).ToList()
                    : fileInfos.OrderByDescending(f => f.UsageCount).ToList(),
                FileInfoModelSortBy.LastUsed => ascending
                    ? fileInfos.OrderBy(f => f.LastUsed).ToList()
                    : fileInfos.OrderByDescending(f => f.LastUsed).ToList(),
                _ => fileInfos
            };
        }

        /// <summary>
        /// 모든 파일 정보를 저장합니다.
        /// </summary>
        /// <param name="fileInfos">저장할 파일 정보 목록</param>
        /// <returns>저장 성공 여부</returns>
        private async Task<bool> SaveAllFileInfosAsync(List<FileInfoModel> fileInfos)
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(fileInfos, Formatting.Indented);
                await File.WriteAllTextAsync(_dataFilePath, json);

                // 캐시 업데이트
                lock (_lockObject)
                {
                    _cachedFileInfos = new List<FileInfoModel>(fileInfos);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"파일 정보 목록 저장 실패: {ex.Message}");
                return false;
            }
        }
    }
}