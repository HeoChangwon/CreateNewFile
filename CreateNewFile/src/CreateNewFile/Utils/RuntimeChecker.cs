using System;
using System.Diagnostics;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using Clipboard = System.Windows.Clipboard;

namespace CreateNewFile.Utils
{
    /// <summary>
    /// .NET 런타임 설치 상태를 확인하고 설치 가이드를 제공하는 유틸리티 클래스
    /// </summary>
    public static class RuntimeChecker
    {
        /// <summary>
        /// .NET 8 런타임이 설치되어 있는지 확인합니다.
        /// </summary>
        /// <returns>.NET 8 런타임이 설치되어 있으면 true, 그렇지 않으면 false</returns>
        public static bool IsNet8RuntimeInstalled()
        {
            try
            {
                // 현재 실행 중인 .NET 버전 확인
                var runtimeVersion = Environment.Version;
                
                // .NET 8.0 이상인지 확인
                if (runtimeVersion.Major >= 8)
                {
                    return true;
                }

                // 레지스트리에서 추가 확인 (Windows)
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    return CheckDotNetFromRegistry();
                }

                return false;
            }
            catch (Exception)
            {
                // 예외 발생 시 false 반환 (보수적 접근)
                return false;
            }
        }

        /// <summary>
        /// Windows 레지스트리에서 .NET 8 런타임 설치 여부를 확인합니다.
        /// </summary>
        /// <returns>.NET 8이 설치되어 있으면 true, 그렇지 않으면 false</returns>
        private static bool CheckDotNetFromRegistry()
        {
            try
            {
                // .NET Core/.NET 5+ 설치 경로들
                string[] registryPaths = 
                {
                    @"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedhost",
                    @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost",
                    @"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x86\sharedhost",
                    @"SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedhost"
                };

                foreach (var path in registryPaths)
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(path))
                    {
                        if (key != null)
                        {
                            var version = key.GetValue("Version")?.ToString();
                            if (!string.IsNullOrEmpty(version) && 
                                Version.TryParse(version, out var parsedVersion) &&
                                parsedVersion.Major >= 8)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// .NET 8 런타임 설치 가이드를 표시합니다.
        /// </summary>
        /// <returns>사용자가 다운로드 페이지로 이동하기로 선택했으면 true</returns>
        public static bool ShowRuntimeInstallGuide()
        {
            try
            {
                var result = MessageBox.Show(
                    ".NET 8 런타임이 설치되어 있지 않습니다.\n\n" +
                    "이 프로그램을 실행하려면 .NET 8 Desktop Runtime이 필요합니다.\n\n" +
                    "Microsoft 공식 다운로드 페이지로 이동하시겠습니까?\n\n" +
                    "다운로드 후 설치하시면 프로그램을 정상적으로 사용할 수 있습니다.",
                    "런타임 설치 필요 - CreateNewFile",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    OpenDotNetDownloadPage();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// .NET 8 다운로드 페이지를 브라우저에서 엽니다.
        /// </summary>
        private static void OpenDotNetDownloadPage()
        {
            try
            {
                // .NET 8 Desktop Runtime 직접 다운로드 링크
                var downloadUrl = "https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0";
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // URL 열기에 실패한 경우 클립보드에 복사
                try
                {
                    Clipboard.SetText("https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0");
                    MessageBox.Show(
                        "브라우저를 열 수 없습니다.\n\n" +
                        "다운로드 링크가 클립보드에 복사되었습니다.\n" +
                        "브라우저에 붙여넣기하여 .NET 8을 다운로드하세요.\n\n" +
                        "링크: https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0",
                        "다운로드 링크 복사됨",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show(
                        "브라우저를 열 수 없습니다.\n\n" +
                        "다음 링크에서 .NET 8 Desktop Runtime을 다운로드하세요:\n" +
                        "https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0",
                        "수동 다운로드 필요",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// 현재 설치된 .NET 런타임 버전 정보를 가져옵니다.
        /// </summary>
        /// <returns>런타임 버전 정보</returns>
        public static string GetInstalledRuntimeVersion()
        {
            try
            {
                var version = Environment.Version;
                return $".NET {version.Major}.{version.Minor}.{version.Build}";
            }
            catch
            {
                return "알 수 없음";
            }
        }

        /// <summary>
        /// 런타임 호환성을 확인합니다.
        /// </summary>
        /// <returns>호환성 정보</returns>
        public static RuntimeCompatibilityInfo CheckCompatibility()
        {
            try
            {
                var runtimeVersion = Environment.Version;
                var isCompatible = runtimeVersion.Major >= 8;
                var currentVersion = GetInstalledRuntimeVersion();

                return new RuntimeCompatibilityInfo
                {
                    IsCompatible = isCompatible,
                    CurrentVersion = currentVersion,
                    RequiredVersion = ".NET 8.0",
                    Message = isCompatible 
                        ? $"호환됨: {currentVersion}" 
                        : $"호환되지 않음: 현재 {currentVersion}, 필요 .NET 8.0+"
                };
            }
            catch
            {
                return new RuntimeCompatibilityInfo
                {
                    IsCompatible = false,
                    CurrentVersion = "알 수 없음",
                    RequiredVersion = ".NET 8.0",
                    Message = "런타임 버전을 확인할 수 없습니다."
                };
            }
        }
    }

    /// <summary>
    /// 런타임 호환성 정보를 나타내는 클래스
    /// </summary>
    public class RuntimeCompatibilityInfo
    {
        /// <summary>
        /// 런타임이 호환되는지 여부
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// 현재 설치된 런타임 버전
        /// </summary>
        public string CurrentVersion { get; set; } = string.Empty;

        /// <summary>
        /// 필요한 런타임 버전
        /// </summary>
        public string RequiredVersion { get; set; } = string.Empty;

        /// <summary>
        /// 호환성 메시지
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}