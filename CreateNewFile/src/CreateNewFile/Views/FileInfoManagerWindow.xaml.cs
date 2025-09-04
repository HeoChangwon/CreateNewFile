using System.Windows;
using CreateNewFile.ViewModels;

namespace CreateNewFile.Views
{
    /// <summary>
    /// FileInfoManagerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FileInfoManagerWindow : Window
    {
        public FileInfoManagerWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 닫기 버튼 클릭 이벤트
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}