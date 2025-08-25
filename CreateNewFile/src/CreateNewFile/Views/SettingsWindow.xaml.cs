using System.Windows;
using System.Windows.Controls;
using CreateNewFile.ViewModels;
using CreateNewFile.Models;

namespace CreateNewFile.Views
{
    /// <summary>
    /// SettingsWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            
            // DataContext가 설정된 후 이벤트 구독
            Loaded += (s, e) =>
            {
                if (DataContext is SettingsViewModel viewModel)
                {
                    viewModel.CloseRequested += OnCloseRequested;
                }
                
                // ListView 선택 변경 이벤트 구독
                ItemsListView.SelectionChanged += OnListViewSelectionChanged;
            };
            
            // 창이 닫힐 때 이벤트 구독 해제
            Unloaded += (s, e) =>
            {
                if (DataContext is SettingsViewModel viewModel)
                {
                    viewModel.CloseRequested -= OnCloseRequested;
                }
                
                ItemsListView.SelectionChanged -= OnListViewSelectionChanged;
            };
        }
        
        private void OnCloseRequested(object? sender, System.EventArgs e)
        {
            Close();
        }
        
        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                var selectedItems = ItemsListView.SelectedItems.Cast<PresetItem>();
                viewModel.SetSelectedItems(selectedItems);
            }
        }
    }
}