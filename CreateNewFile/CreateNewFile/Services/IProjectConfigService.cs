using CreateNewFile.Models;

namespace CreateNewFile.Services
{
    /// <summary>
    /// 프로젝트 설정 파일 관리 서비스 인터페이스
    /// </summary>
    public interface IProjectConfigService
    {
        /// <summary>
        /// 프로젝트 설정 파일(.cnfjson)을 저장합니다.
        /// </summary>
        /// <param name="config">저장할 프로젝트 설정</param>
        /// <param name="filePath">저장할 파일 경로</param>
        /// <returns>저장 성공 여부</returns>
        Task<bool> SaveProjectConfigAsync(ProjectConfig config, string filePath);

        /// <summary>
        /// 프로젝트 설정 파일(.cnfjson)을 로드합니다.
        /// </summary>
        /// <param name="filePath">로드할 파일 경로</param>
        /// <returns>로드된 프로젝트 설정</returns>
        Task<ProjectConfig?> LoadProjectConfigAsync(string filePath);

        /// <summary>
        /// 프로젝트 설정 파일의 유효성을 검증합니다.
        /// </summary>
        /// <param name="filePath">검증할 파일 경로</param>
        /// <returns>유효성 검사 결과와 오류 메시지</returns>
        Task<(bool IsValid, string ErrorMessage)> ValidateProjectConfigFileAsync(string filePath);

        /// <summary>
        /// 프로젝트 설정을 현재 MainViewModel 상태로부터 생성합니다.
        /// </summary>
        /// <param name="viewModel">MainViewModel 인스턴스</param>
        /// <returns>생성된 프로젝트 설정</returns>
        ProjectConfig CreateProjectConfigFromViewModel(ViewModels.MainViewModel viewModel);
    }
}
