using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CreateNewFile.ViewModels;
using CreateNewFile.Services;

namespace CreateNewFile.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();
        
        // 윈도우 제목에 버전 및 빌드 날짜 설정
        Title = CreateNewFile.Utils.VersionHelper.FullVersionString;
        
        // SettingsService 인스턴스 생성
        _settingsService = new SettingsService();
        
        // Settings 업그레이드 (버전이 변경된 경우)
        if (Properties.Settings.Default.CallUpgrade)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.CallUpgrade = false;
            Properties.Settings.Default.Save();
        }
        
        // 이벤트 핸들러 등록
        this.SourceInitialized += MainWindow_SourceInitialized;
        this.Loaded += MainWindow_Loaded;
        this.Closing += MainWindow_Closing;
    }

    private void MainWindow_SourceInitialized(object sender, EventArgs e)
    {
        // 윈도우 위치 및 크기 복원 (화면 표시 전에 실행)
        RestoreWindowPosition();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 드래그앤드롭 영역 찾기 및 이벤트 핸들러 등록
        RegisterDragDropHandlers();
        
        // TabControl의 SelectionChanged 이벤트 핸들러 등록
        var tabControl = FindName("MainTabControl") as System.Windows.Controls.TabControl;
        if (tabControl != null)
        {
            tabControl.SelectionChanged += TabControl_SelectionChanged;
        }
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 탭이 변경될 때 드래그 앤 드롭 핸들러를 재등록
        // 새로 선택된 탭의 콘텐츠가 완전히 로드된 후에 실행
        Dispatcher.BeginInvoke(new Action(() =>
        {
            RegisterDragDropHandlers();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // 윈도우 위치 및 크기 저장
        SaveWindowPosition();
        
        // MainViewModel의 현재 상태 저장
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.SaveCurrentStateAsync();
        }
    }


    /// <summary>
    /// Properties.Settings을 사용하여 윈도우 위치 및 크기를 복원합니다.
    /// </summary>
    private void RestoreWindowPosition()
    {
        try
        {
            var settings = Properties.Settings.Default;
            
            System.Diagnostics.Debug.WriteLine($"Properties.Settings 로드: Left={settings.WindowLeft}, Top={settings.WindowTop}, Width={settings.WindowWidth}, Height={settings.WindowHeight}, State={settings.WindowState}");

            // 화면 경계 확인 (안전한 기본값 설정)
            var screenWidth = Math.Max(SystemParameters.PrimaryScreenWidth, 1024);
            var screenHeight = Math.Max(SystemParameters.PrimaryScreenHeight, 768);

            // 저장된 설정이 유효한지 확인 (-1은 초기값)
            bool hasValidSavedSettings = settings.WindowLeft != -1 && settings.WindowTop != -1;

            System.Diagnostics.Debug.WriteLine($"유효한 저장된 설정 존재: {hasValidSavedSettings}");
            System.Diagnostics.Debug.WriteLine($"화면 크기: {screenWidth}x{screenHeight}");

            if (hasValidSavedSettings)
            {
                // 저장된 설정이 있는 경우
                // 유효한 위치인지 확인
                bool isValidPosition = settings.WindowLeft >= 0 && settings.WindowTop >= 0 &&
                                     settings.WindowLeft < screenWidth && settings.WindowTop < screenHeight;

                if (isValidPosition)
                {
                    Left = settings.WindowLeft;
                    Top = settings.WindowTop;
                    System.Diagnostics.Debug.WriteLine($"저장된 위치 적용: {Left}, {Top}");
                }
                else
                {
                    // 유효하지 않은 위치인 경우 화면 중앙에 배치
                    Left = (screenWidth - settings.WindowWidth) / 2;
                    Top = (screenHeight - settings.WindowHeight) / 2;
                    System.Diagnostics.Debug.WriteLine($"유효하지 않은 위치 - 중앙 배치: {Left}, {Top}");
                }

                // 유효한 크기인지 확인하고 적용
                if (settings.WindowWidth >= MinWidth && settings.WindowHeight >= MinHeight &&
                    settings.WindowWidth <= screenWidth + 100 && settings.WindowHeight <= screenHeight + 100)
                {
                    Width = settings.WindowWidth;
                    Height = settings.WindowHeight;
                    System.Diagnostics.Debug.WriteLine($"저장된 크기 적용: {Width}x{Height}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"유효하지 않은 크기 - XAML 기본값 유지: {Width}x{Height}");
                }

                // 윈도우 상태 복원
                if (Enum.TryParse<WindowState>(settings.WindowState, out var windowState) && 
                    windowState != WindowState.Minimized)
                {
                    WindowState = windowState;
                    System.Diagnostics.Debug.WriteLine($"윈도우 상태 복원: {WindowState}");
                }
            }
            else
            {
                // 처음 실행인 경우 - 화면 중앙에 배치
                Left = (screenWidth - Width) / 2;
                Top = (screenHeight - Height) / 2;
                System.Diagnostics.Debug.WriteLine($"처음 실행 - 중앙 배치: {Left}, {Top}, 크기: {Width}x{Height}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"윈도우 위치 복원 실패: {ex.Message}");
            // 실패한 경우 화면 중앙에 배치 (안전한 기본값 사용)
            var screenWidth = Math.Max(SystemParameters.PrimaryScreenWidth, 1024);
            var screenHeight = Math.Max(SystemParameters.PrimaryScreenHeight, 768);
            Left = (screenWidth - Width) / 2;
            Top = (screenHeight - Height) / 2;
        }
    }

    /// <summary>
    /// Properties.Settings을 사용하여 윈도우 위치 및 크기를 저장합니다.
    /// </summary>
    private void SaveWindowPosition()
    {
        try
        {
            // 최소화 상태일 때는 저장하지 않음
            if (WindowState == WindowState.Minimized)
            {
                System.Diagnostics.Debug.WriteLine("최소화 상태 - 저장 생략");
                return;
            }

            // 유효하지 않은 값들 확인
            if (double.IsNaN(Left) || double.IsNaN(Top) || 
                double.IsNaN(ActualWidth) || double.IsNaN(ActualHeight) ||
                ActualWidth <= 0 || ActualHeight <= 0)
            {
                System.Diagnostics.Debug.WriteLine("유효하지 않은 윈도우 값 - 저장 생략");
                return;
            }

            var settings = Properties.Settings.Default;

            // 현재 윈도우 정보 저장
            if (WindowState == WindowState.Maximized && RestoreBounds != Rect.Empty)
            {
                // 최대화 상태일 때는 복원될 때의 크기와 위치 저장
                settings.WindowLeft = RestoreBounds.Left;
                settings.WindowTop = RestoreBounds.Top;
                settings.WindowWidth = RestoreBounds.Width;
                settings.WindowHeight = RestoreBounds.Height;
                settings.WindowState = "Normal"; // 다음 실행 시 일반 상태로 시작
                System.Diagnostics.Debug.WriteLine($"최대화 상태 - RestoreBounds 저장: {settings.WindowLeft}, {settings.WindowTop}, {settings.WindowWidth}x{settings.WindowHeight}");
            }
            else
            {
                // 일반 상태일 때는 현재 크기와 위치 저장
                settings.WindowLeft = Left;
                settings.WindowTop = Top;
                settings.WindowWidth = ActualWidth;
                settings.WindowHeight = ActualHeight;
                settings.WindowState = WindowState.ToString();
                System.Diagnostics.Debug.WriteLine($"일반 상태 - 현재 위치/크기 저장: {settings.WindowLeft}, {settings.WindowTop}, {settings.WindowWidth}x{settings.WindowHeight}");
            }

            // Properties.Settings 저장
            settings.Save();
            System.Diagnostics.Debug.WriteLine("Properties.Settings 저장 완료");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"윈도우 위치 저장 실패: {ex.Message}");
        }
    }

    private void RegisterDragDropHandlers()
    {
        // 탭 컨트롤이 로드된 후에 드래그 앤 드롭 핸들러를 등록하기 위해 지연 실행
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // 모든 Border 컨트롤 중에서 AllowDrop이 true인 것들을 찾아서 이벤트 핸들러 등록
            RegisterDragDropForElement(this);
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void RegisterDragDropForElement(DependencyObject parent)
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is Border border && border.AllowDrop)
            {
                // Border가 드롭 영역인지 확인하고 적절한 이벤트 핸들러 등록
                var textBlock = FindChild<TextBlock>(border);
                if (textBlock != null)
                {
                    var text = textBlock.Text;
                    System.Diagnostics.Debug.WriteLine($"드래그 앤 드롭 영역 발견: '{text}'");
                    
                    // 기존 이벤트 핸들러 제거 (중복 등록 방지)
                    RemoveDropHandlers(border);
                    
                    if (text.Contains("폴더"))
                    {
                        // 폴더 드롭 영역
                        border.DragEnter += FolderDropArea_DragEnter;
                        border.DragOver += FolderDropArea_DragOver;
                        border.DragLeave += DropArea_DragLeave;
                        border.Drop += FolderDropArea_Drop;
                        System.Diagnostics.Debug.WriteLine("폴더 드롭 이벤트 핸들러 등록됨");
                    }
                    else if (text.Contains("템플릿"))
                    {
                        // 파일 드롭 영역
                        border.DragEnter += FileDropArea_DragEnter;
                        border.DragOver += FileDropArea_DragOver;
                        border.DragLeave += DropArea_DragLeave;
                        border.Drop += FileDropArea_Drop;
                        System.Diagnostics.Debug.WriteLine("템플릿 파일 드롭 이벤트 핸들러 등록됨");
                    }
                }
            }
            
            RegisterDragDropForElement(child);
        }
    }

    private void RemoveDropHandlers(Border border)
    {
        // 모든 드래그 앤 드롭 이벤트 핸들러 제거
        border.DragEnter -= FolderDropArea_DragEnter;
        border.DragOver -= FolderDropArea_DragOver;
        border.DragLeave -= DropArea_DragLeave;
        border.Drop -= FolderDropArea_Drop;
        border.DragEnter -= FileDropArea_DragEnter;
        border.DragOver -= FileDropArea_DragOver;
        border.Drop -= FileDropArea_Drop;
    }

    private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T directChild)
                return directChild;
            
            var foundChild = FindChild<T>(child);
            if (foundChild != null)
                return foundChild;
        }
        
        return null;
    }

    #region Folder Drop Events

    private void FolderDropArea_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        if (HasValidFolders(e.Data))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
            SetDropAreaHighlight(sender as Border, true);
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void FolderDropArea_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (HasValidFolders(e.Data))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void FolderDropArea_Drop(object sender, System.Windows.DragEventArgs e)
    {
        try
        {
            if (HasValidFolders(e.Data))
            {
                var folders = GetDroppedFolders(e.Data);
                if (folders.Count > 0)
                {
                    var folderPath = folders[0]; // 첫 번째 폴더만 사용
                    
                    // ViewModel에 폴더 경로 설정
                    if (DataContext is MainViewModel viewModel)
                    {
                        viewModel.SelectedOutputPath = folderPath;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"폴더 드롭 처리 중 오류가 발생했습니다: {ex.Message}", 
                          "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetDropAreaHighlight(sender as Border, false);
            e.Handled = true;
        }
    }

    #endregion

    #region File Drop Events

    private void FileDropArea_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        if (HasValidFiles(e.Data))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
            SetDropAreaHighlight(sender as Border, true);
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void FileDropArea_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (HasValidFiles(e.Data))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void FileDropArea_Drop(object sender, System.Windows.DragEventArgs e)
    {
        try
        {
            if (HasValidFiles(e.Data))
            {
                var files = GetDroppedFiles(e.Data);
                if (files.Count > 0)
                {
                    var filePath = files[0]; // 첫 번째 파일만 사용
                    
                    // ViewModel에 템플릿 파일 경로 설정
                    if (DataContext is MainViewModel viewModel)
                    {
                        viewModel.SelectedTemplatePath = filePath;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"파일 드롭 처리 중 오류가 발생했습니다: {ex.Message}", 
                          "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetDropAreaHighlight(sender as Border, false);
            e.Handled = true;
        }
    }

    #endregion

    #region Common Drop Events

    private void DropArea_DragLeave(object sender, System.Windows.DragEventArgs e)
    {
        SetDropAreaHighlight(sender as Border, false);
        e.Handled = true;
    }

    #endregion

    #region Helper Methods

    private bool HasValidFolders(System.Windows.IDataObject dataObject)
    {
        if (!dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
            return false;

        var files = dataObject.GetData(System.Windows.DataFormats.FileDrop) as string[];
        return files?.Any(Directory.Exists) == true;
    }

    private bool HasValidFiles(System.Windows.IDataObject dataObject)
    {
        if (!dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
            return false;

        var files = dataObject.GetData(System.Windows.DataFormats.FileDrop) as string[];
        return files?.Any(File.Exists) == true;
    }

    private List<string> GetDroppedFolders(System.Windows.IDataObject dataObject)
    {
        var result = new List<string>();
        
        if (dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            var files = dataObject.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (files != null)
            {
                result.AddRange(files.Where(Directory.Exists));
            }
        }
        
        return result;
    }

    private List<string> GetDroppedFiles(System.Windows.IDataObject dataObject)
    {
        var result = new List<string>();
        
        if (dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            var files = dataObject.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (files != null)
            {
                result.AddRange(files.Where(File.Exists));
            }
        }
        
        return result;
    }

    private void SetDropAreaHighlight(Border? border, bool highlight)
    {
        if (border == null) return;

        if (highlight)
        {
            border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204)); // #FF007ACC
            border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 248, 255)); // #FFF0F8FF
        }
        else
        {
            // 원래 스타일로 복원
            border.BorderBrush = System.Windows.Media.Brushes.LightGray;
            border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(249, 249, 249)); // #FFF9F9F9
        }
    }

    #endregion
}