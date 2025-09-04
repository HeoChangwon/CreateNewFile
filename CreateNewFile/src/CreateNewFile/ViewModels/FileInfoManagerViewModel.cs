using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using CreateNewFile.Models;
using CreateNewFile.Services;
using CreateNewFile.Utils;

namespace CreateNewFile.ViewModels
{
    /// <summary>
    /// 파일정보 관리 창의 ViewModel
    /// </summary>
    public class FileInfoManagerViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly IFileInfoService _fileInfoService;
        private FileInfoModel? _selectedFileInfo;
        private bool _hasSelectedItem = false;
        #endregion

        #region Properties
        /// <summary>
        /// 파일정보 목록
        /// </summary>
        public ObservableCollection<FileInfoModel> FileInfos { get; } = new();

        /// <summary>
        /// 선택된 파일정보
        /// </summary>
        public FileInfoModel? SelectedFileInfo
        {
            get => _selectedFileInfo;
            set
            {
                if (SetProperty(ref _selectedFileInfo, value))
                {
                    HasSelectedItem = value != null;
                    OnPropertyChanged(nameof(StringReplacementRulesText));
                }
            }
        }

        /// <summary>
        /// 선택된 항목이 있는지 여부
        /// </summary>
        public bool HasSelectedItem
        {
            get => _hasSelectedItem;
            private set => SetProperty(ref _hasSelectedItem, value);
        }

        /// <summary>
        /// 전체 파일정보 개수
        /// </summary>
        public int TotalCount => FileInfos.Count;

        /// <summary>
        /// 문자열 교체 규칙 텍스트 (미리보기용)
        /// </summary>
        public string StringReplacementRulesText
        {
            get
            {
                if (SelectedFileInfo?.StringReplacements == null || !SelectedFileInfo.StringReplacements.Any())
                    return "문자열 교체 규칙이 없습니다.";

                var activeRules = SelectedFileInfo.StringReplacements.Where(r => r.IsEnabled).ToList();
                if (!activeRules.Any())
                    return "활성화된 문자열 교체 규칙이 없습니다.";

                return string.Join("\n", activeRules.Select((rule, index) => 
                    $"{index + 1}. '{rule.SearchText}' → '{rule.ReplaceText}'" + 
                    (string.IsNullOrWhiteSpace(rule.Description) ? "" : $" ({rule.Description})")));
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// 새로고침 명령
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// 이름 변경 명령
        /// </summary>
        public ICommand RenameCommand { get; }

        /// <summary>
        /// 복사 명령
        /// </summary>
        public ICommand DuplicateCommand { get; }

        /// <summary>
        /// 삭제 명령
        /// </summary>
        public ICommand DeleteCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// FileInfoManagerViewModel의 새 인스턴스를 초기화합니다.
        /// </summary>
        public FileInfoManagerViewModel(IFileInfoService fileInfoService)
        {
            _fileInfoService = fileInfoService ?? throw new ArgumentNullException(nameof(fileInfoService));

            // 명령 초기화
            RefreshCommand = new RelayCommand(async () => await RefreshAsync());
            RenameCommand = new RelayCommand(async () => await RenameAsync(), CanModifySelectedItem);
            DuplicateCommand = new RelayCommand(async () => await DuplicateAsync(), CanModifySelectedItem);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), CanModifySelectedItem);

            // 초기 데이터 로드
            _ = RefreshAsync();
        }
        #endregion

        #region Command Methods
        /// <summary>
        /// 파일정보 목록을 새로고침합니다.
        /// </summary>
        private async Task RefreshAsync()
        {
            try
            {
                var fileInfos = await _fileInfoService.GetAllFileInfosAsync();
                
                FileInfos.Clear();
                foreach (var info in fileInfos.OrderByDescending(f => f.LastUsed))
                {
                    FileInfos.Add(info);
                }
                
                OnPropertyChanged(nameof(TotalCount));
                
                // 선택된 항목이 없어졌다면 첫 번째 항목 선택
                if (SelectedFileInfo == null && FileInfos.Any())
                {
                    SelectedFileInfo = FileInfos.First();
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"파일정보 목록을 불러오는 중 오류가 발생했습니다.\n\n{ex.Message}", "새로고침 오류");
            }
        }

        /// <summary>
        /// 선택된 파일정보의 이름을 변경합니다.
        /// </summary>
        private async Task RenameAsync()
        {
            if (SelectedFileInfo == null) return;

            try
            {
                var result = DialogHelper.ShowInputDialog(
                    "파일정보 이름 변경", 
                    "새 이름을 입력하세요:", 
                    SelectedFileInfo.Name);

                if (result.IsConfirmed && !string.IsNullOrWhiteSpace(result.InputText))
                {
                    var newName = result.InputText.Trim();
                    
                    // 중복 이름 확인
                    if (FileInfos.Any(f => f.Id != SelectedFileInfo.Id && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        DialogHelper.ShowError("이미 같은 이름의 파일정보가 존재합니다.", "이름 변경 실패");
                        return;
                    }

                    var originalName = SelectedFileInfo.Name;
                    SelectedFileInfo.Name = newName;
                    
                    var success = await _fileInfoService.UpdateFileInfoAsync(SelectedFileInfo);
                    if (success)
                    {
                        DialogHelper.ShowInfo($"파일정보 이름이 '{originalName}'에서 '{newName}'으로 변경되었습니다.", "이름 변경 완료");
                        await RefreshAsync();
                    }
                    else
                    {
                        SelectedFileInfo.Name = originalName; // 롤백
                        DialogHelper.ShowError("파일정보 이름 변경에 실패했습니다.", "이름 변경 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"이름 변경 중 오류가 발생했습니다.\n\n{ex.Message}", "이름 변경 오류");
            }
        }

        /// <summary>
        /// 선택된 파일정보를 복사합니다.
        /// </summary>
        private async Task DuplicateAsync()
        {
            if (SelectedFileInfo == null) return;

            try
            {
                var originalName = SelectedFileInfo.Name;
                var copyName = GenerateCopyName(originalName);
                
                var result = DialogHelper.ShowInputDialog(
                    "파일정보 복사", 
                    "복사본의 이름을 입력하세요:", 
                    copyName);

                if (result.IsConfirmed && !string.IsNullOrWhiteSpace(result.InputText))
                {
                    var newName = result.InputText.Trim();
                    
                    // 중복 이름 확인
                    if (FileInfos.Any(f => f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        DialogHelper.ShowError("이미 같은 이름의 파일정보가 존재합니다.", "복사 실패");
                        return;
                    }

                    var duplicateInfo = new FileInfoModel
                    {
                        Name = newName,
                        DateTime = SelectedFileInfo.DateTime,
                        Abbreviation = SelectedFileInfo.Abbreviation,
                        Title = SelectedFileInfo.Title,
                        Suffix = SelectedFileInfo.Suffix,
                        Extension = SelectedFileInfo.Extension,
                        OutputPath = SelectedFileInfo.OutputPath,
                        TemplatePath = SelectedFileInfo.TemplatePath,
                        IsDateTimeEnabled = SelectedFileInfo.IsDateTimeEnabled,
                        IsAbbreviationEnabled = SelectedFileInfo.IsAbbreviationEnabled,
                        IsTitleEnabled = SelectedFileInfo.IsTitleEnabled,
                        IsSuffixEnabled = SelectedFileInfo.IsSuffixEnabled,
                        StringReplacements = SelectedFileInfo.StringReplacements?.ToList() ?? new List<StringReplacementRule>(),
                        CreatedAt = DateTime.Now,
                        UsageCount = 0,
                        LastUsed = DateTime.Now
                    };
                    
                    var success = await _fileInfoService.SaveFileInfoAsync(duplicateInfo);
                    if (success)
                    {
                        DialogHelper.ShowInfo($"파일정보가 '{newName}'으로 복사되었습니다.", "복사 완료");
                        await RefreshAsync();
                        
                        // 새로 생성된 항목 선택
                        SelectedFileInfo = FileInfos.FirstOrDefault(f => f.Name == newName);
                    }
                    else
                    {
                        DialogHelper.ShowError("파일정보 복사에 실패했습니다.", "복사 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"복사 중 오류가 발생했습니다.\n\n{ex.Message}", "복사 오류");
            }
        }

        /// <summary>
        /// 선택된 파일정보를 삭제합니다.
        /// </summary>
        private async Task DeleteAsync()
        {
            if (SelectedFileInfo == null) return;

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"'{SelectedFileInfo.Name}' 파일정보를 삭제하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다.",
                    "파일정보 삭제 확인",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var deletedName = SelectedFileInfo.Name;
                    var success = await _fileInfoService.DeleteFileInfoAsync(SelectedFileInfo.Id);
                    
                    if (success)
                    {
                        DialogHelper.ShowInfo($"파일정보 '{deletedName}'가 삭제되었습니다.", "삭제 완료");
                        await RefreshAsync();
                    }
                    else
                    {
                        DialogHelper.ShowError("파일정보 삭제에 실패했습니다.", "삭제 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"삭제 중 오류가 발생했습니다.\n\n{ex.Message}", "삭제 오류");
            }
        }

        /// <summary>
        /// 선택된 항목을 수정할 수 있는지 확인합니다.
        /// </summary>
        private bool CanModifySelectedItem()
        {
            return SelectedFileInfo != null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 복사본 이름을 생성합니다.
        /// </summary>
        private string GenerateCopyName(string originalName)
        {
            var baseName = originalName;
            var copyNumber = 1;
            
            // "원본이름_복사본N" 형태로 이름 생성
            string copyName;
            do
            {
                copyName = $"{baseName}_복사본{copyNumber}";
                copyNumber++;
            } 
            while (FileInfos.Any(f => f.Name.Equals(copyName, StringComparison.OrdinalIgnoreCase)));
            
            return copyName;
        }
        #endregion
    }
}