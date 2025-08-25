using System.Reflection;
using System.Linq;

namespace CreateNewFile.Utils
{
    /// <summary>
    /// 애플리케이션 버전 정보를 관리하는 헬퍼 클래스
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// 제품 이름을 가져옵니다.
        /// </summary>
        public static string ProductName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var productAttribute = assembly.GetCustomAttribute<AssemblyProductAttribute>();
                return productAttribute?.Product ?? "CreateNewFile";
            }
        }

        /// <summary>
        /// 버전 번호를 가져옵니다 (예: "1.0.001")
        /// </summary>
        public static string Version
        {
            get
            {
                // AssemblyInformationalVersion에 1.0.001이 설정되어 있으므로 이것을 우선 사용
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    
                    // AssemblyInformationalVersion 확인 (csproj에서 1.0.001로 설정됨)
                    var informationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                    if (informationalVersionAttribute != null && !string.IsNullOrEmpty(informationalVersionAttribute.InformationalVersion))
                    {
                        return informationalVersionAttribute.InformationalVersion;
                    }
                    
                    // DisplayVersion 메타데이터 확인
                    var metadataAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
                    var displayVersionAttribute = metadataAttributes?.FirstOrDefault(x => x.Key == "DisplayVersion");
                    if (displayVersionAttribute != null && !string.IsNullOrEmpty(displayVersionAttribute.Value))
                    {
                        return displayVersionAttribute.Value;
                    }
                    
                    // AssemblyVersion에서 가져와서 변환 (1.0.1.0 -> 1.0.001)
                    var version = assembly.GetName().Version;
                    if (version != null)
                    {
                        // Build 번호가 0이면 Major.Minor만 사용하고 뒤에 001 추가
                        if (version.Build == 0)
                        {
                            return $"{version.Major}.{version.Minor}.001";
                        }
                        else
                        {
                            return $"{version.Major}.{version.Minor}.{version.Build:D3}";
                        }
                    }
                }
                catch
                {
                    // ignore
                }
                
                // 모든 방법이 실패하면 기본값
                return "1.0.001";
            }
        }

        /// <summary>
        /// 빌드 날짜를 가져옵니다.
        /// </summary>
        public static string BuildDate
        {
            get
            {
                // fallback: 어셈블리 파일의 최종 수정 시간 사용
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var assemblyLocation = assembly.Location;
                    if (System.IO.File.Exists(assemblyLocation))
                    {
                        var fileInfo = new System.IO.FileInfo(assemblyLocation);
                        return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                    }
                }
                catch
                {
                    // ignore
                }

                return DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            }
        }

        /// <summary>
        /// 전체 버전 문자열을 가져옵니다.
        /// 예: "CreateNewFile v1.0.001 (Build: 2025-08-25 11:48)"
        /// </summary>
        public static string FullVersionString
        {
            get
            {
                return $"{ProductName} v{Version} (Build: {BuildDate})";
            }
        }

        /// <summary>
        /// 짧은 버전 문자열을 가져옵니다.
        /// 예: "v1.0.001"
        /// </summary>
        public static string ShortVersionString
        {
            get
            {
                return $"v{Version}";
            }
        }
    }
}