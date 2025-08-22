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

namespace CreateNewFile.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // 드래그앤드롭 이벤트 핸들러 등록
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 드래그앤드롭 영역 찾기 및 이벤트 핸들러 등록
        RegisterDragDropHandlers();
    }

    private void RegisterDragDropHandlers()
    {
        // 모든 Border 컨트롤 중에서 AllowDrop이 true인 것들을 찾아서 이벤트 핸들러 등록
        RegisterDragDropForElement(this);
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
                    if (text.Contains("폴더"))
                    {
                        // 폴더 드롭 영역
                        border.DragEnter += FolderDropArea_DragEnter;
                        border.DragOver += FolderDropArea_DragOver;
                        border.DragLeave += DropArea_DragLeave;
                        border.Drop += FolderDropArea_Drop;
                    }
                    else if (text.Contains("템플릿"))
                    {
                        // 파일 드롭 영역
                        border.DragEnter += FileDropArea_DragEnter;
                        border.DragOver += FileDropArea_DragOver;
                        border.DragLeave += DropArea_DragLeave;
                        border.Drop += FileDropArea_Drop;
                    }
                }
            }
            
            RegisterDragDropForElement(child);
        }
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