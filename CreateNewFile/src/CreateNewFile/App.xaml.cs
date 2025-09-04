using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CreateNewFile.ViewModels;
using CreateNewFile.Views;
using CreateNewFile.Services;
using CreateNewFile.Utils;

namespace CreateNewFile;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// 애플리케이션 시작 시 호출됩니다.
    /// </summary>
    /// <param name="e">시작 이벤트 인수</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // .NET 8 런타임 설치 여부 확인
        if (!RuntimeChecker.IsNet8RuntimeInstalled())
        {
            RuntimeChecker.ShowRuntimeInstallGuide();
            Shutdown(1);
            return;
        }

        // 의존성 주입 컨테이너 설정
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ViewModels 등록
                services.AddSingleton<MainViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views 등록
                services.AddSingleton<MainWindow>();
                services.AddTransient<SettingsWindow>();

                // Services 등록
                services.AddSingleton<IFileGeneratorService, FileGeneratorService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IFileInfoService, FileInfoService>();
            })
            .Build();

        // 메인 윈도우 표시
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        
        // 윈도우를 먼저 표시
        mainWindow.Show();
        
        // 윈도우가 표시된 후 잠시 대기 후 초기화
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(100); // 100ms 후 실행
        timer.Tick += async (sender, e) =>
        {
            timer.Stop();
            try
            {
                await mainViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                // 오류가 발생해도 사용자가 알 수 있도록 상태 메시지 표시
                mainViewModel.StatusMessage = $"초기화 오류: {ex.Message}";
            }
        };
        timer.Start();

        base.OnStartup(e);
    }

    /// <summary>
    /// 애플리케이션 종료 시 호출됩니다.
    /// </summary>
    /// <param name="e">종료 이벤트 인수</param>
    protected override void OnExit(ExitEventArgs e)
    {
        // 프로그램 종료 시 마지막 설정 자동 저장
        try
        {
            var mainViewModel = _host?.Services.GetService<MainViewModel>();
            if (mainViewModel != null)
            {
                // 현재 설정 저장 (동기식으로 처리)
                var saveTask = mainViewModel.SaveCurrentStateAsync();
                saveTask.Wait(5000); // 최대 5초 대기
            }
        }
        catch (Exception ex)
        {
            // 종료 시에는 오류를 표시하지 않음
        }
        
        _host?.Dispose();
        base.OnExit(e);
    }
}

