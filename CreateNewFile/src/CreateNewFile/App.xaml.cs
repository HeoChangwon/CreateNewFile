using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CreateNewFile.ViewModels;
using CreateNewFile.Views;
using CreateNewFile.Services;

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
            })
            .Build();

        // 메인 윈도우 표시
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// 애플리케이션 종료 시 호출됩니다.
    /// </summary>
    /// <param name="e">종료 이벤트 인수</param>
    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}

