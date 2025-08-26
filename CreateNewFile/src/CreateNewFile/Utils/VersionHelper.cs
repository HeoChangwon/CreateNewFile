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
                // 프로젝트 파일에서 BuildDate 읽기 시도
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    
                    // BuildDate 메타데이터 확인
                    var metadataAttributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
                    var buildDateAttribute = metadataAttributes?.FirstOrDefault(x => x.Key == "BuildDate");
                    if (buildDateAttribute != null && !string.IsNullOrEmpty(buildDateAttribute.Value))
                    {
                        return buildDateAttribute.Value;
                    }

                    // 프로젝트 파일에서 직접 읽기 시도
                    var assemblyLocation = assembly.Location;
                    if (!string.IsNullOrEmpty(assemblyLocation))
                    {
                        var projectPath = FindProjectFile(assemblyLocation);
                        if (!string.IsNullOrEmpty(projectPath) && System.IO.File.Exists(projectPath))
                        {
                            var projectContent = System.IO.File.ReadAllText(projectPath);
                            var buildDateMatch = System.Text.RegularExpressions.Regex.Match(
                                projectContent, @"<BuildDate>(.+?)</BuildDate>");
                            if (buildDateMatch.Success)
                            {
                                return buildDateMatch.Groups[1].Value;
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }

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
        /// 어셈블리 위치에서 프로젝트 파일 경로를 찾습니다.
        /// </summary>
        private static string FindProjectFile(string assemblyLocation)
        {
            try
            {
                var directory = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(assemblyLocation) ?? "");
                
                // bin 폴더에서 시작해서 src 폴더까지 올라가면서 .csproj 파일 찾기
                while (directory != null && directory.Exists)
                {
                    var projectFiles = directory.GetFiles("*.csproj", System.IO.SearchOption.TopDirectoryOnly);
                    if (projectFiles.Length > 0)
                    {
                        return projectFiles[0].FullName;
                    }

                    // CreateNewFile 폴더를 찾으면 그 안에서 .csproj 찾기
                    var createNewFileDir = directory.GetDirectories("CreateNewFile").FirstOrDefault();
                    if (createNewFileDir != null)
                    {
                        var projectFilesInSubdir = createNewFileDir.GetFiles("*.csproj", System.IO.SearchOption.TopDirectoryOnly);
                        if (projectFilesInSubdir.Length > 0)
                        {
                            return projectFilesInSubdir[0].FullName;
                        }
                    }

                    directory = directory.Parent;
                }
            }
            catch
            {
                // ignore
            }

            return string.Empty;
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

        /// <summary>
        /// 개발자 정보를 가져옵니다.
        /// </summary>
        public static string DeveloperInfo
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var companyAttribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
                var copyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
                
                if (companyAttribute != null && !string.IsNullOrEmpty(companyAttribute.Company))
                {
                    return companyAttribute.Company;
                }
                
                if (copyrightAttribute != null && !string.IsNullOrEmpty(copyrightAttribute.Copyright))
                {
                    // "Copyright © 2025 HeoChangwon" -> "2025 HeoChangwon"
                    var copyright = copyrightAttribute.Copyright;
                    var match = System.Text.RegularExpressions.Regex.Match(copyright, @"©\s*(\d{4})\s*(.+)");
                    if (match.Success)
                    {
                        return $"{match.Groups[1].Value} {match.Groups[2].Value}";
                    }
                    return copyright;
                }
                
                return "허창원 (Green Power)";
            }
        }

        /// <summary>
        /// 개발자 정보를 포함한 전체 버전 문자열을 가져옵니다.
        /// 예: "CreateNewFile v1.0.001 (Build: 2025-08-25 11:48) - 허창원"
        /// </summary>
        public static string FullVersionStringWithDeveloper
        {
            get
            {
                return $"{ProductName} v{Version} (Build: {BuildDate}) - {DeveloperInfo}";
            }
        }
    }
}