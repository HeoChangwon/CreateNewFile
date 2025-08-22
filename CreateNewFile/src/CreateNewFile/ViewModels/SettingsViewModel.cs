using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using CreateNewFile.Utils;
using CreateNewFile.Models;
using CreateNewFile.Services;

namespace CreateNewFile.ViewModels
{
    /// <summary>
    /// 설정 윈도우의 ViewModel
    /// 미리 정의된 항목 관리 로직 처리
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly ISettingsService _settingsService;
        private string _newItemValue = string.Empty;
        private string _newItemDescription = string.Empty;
        private PresetItem? _selectedItem;
        private PresetType _selectedPresetType = PresetType.Abbreviation;
        private bool _isWorking = false;
        private string _statusMessage = string.Empty;
        private AppSettings? _originalSettings;
        #endregion

        #region Properties
        /// <summary>
        /// 새 항목 값
        /// </summary>
        public string NewItemValue
        {
            get => _newItemValue;
            set => SetProperty(ref _newItemValue, value);
        }

        /// <summary>
        /// 새 항목 설명
        /// </summary>
        public string NewItemDescription
        {
            get => _newItemDescription;
            set => SetProperty(ref _newItemDescription, value);
        }

        /// <summary>
        /// 선택된 항목
        /// </summary>
        public PresetItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        /// <summary>
        /// 선택된 미리 정의된 항목 타입
        /// </summary>
        public PresetType SelectedPresetType
        {
            get => _selectedPresetType;
            set
            {
                if (SetProperty(ref _selectedPresetType, value))
                {
                    _ = LoadCurrentTypeItemsAsync();
                }
            }
        }

        /// <summary>
        /// 작업 중 여부
        /// </summary>
        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty(ref _isWorking, value);
        }

        /// <summary>
        /// 상태 메시지
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 현재 타입의 항목 목록
        /// </summary>
        public ObservableCollection<PresetItem> CurrentItems { get; } = new();

        /// <summary>
        /// 미리 정의된 항목 타입 목록
        /// </summary>
        public ObservableCollection<PresetTypeInfo> PresetTypes { get; } = new();
        #endregion

        #region Commands
        /// <summary>
        /// 항목 추가 명령
        /// </summary>
        public ICommand AddItemCommand { get; }

        /// <summary>
        /// 항목 수정 명령
        /// </summary>
        public ICommand EditItemCommand { get; }

        /// <summary>
        /// 항목 삭제 명령
        /// </summary>
        public ICommand DeleteItemCommand { get; }

        /// <summary>
        /// 설정 저장 명령
        /// </summary>
        public ICommand SaveSettingsCommand { get; }

        /// <summary>
        /// 설정 취소 명령
        /// </summary>
        public ICommand CancelSettingsCommand { get; }

        /// <summary>
        /// 즐겨찾기 토글 명령
        /// </summary>
        public ICommand ToggleFavoriteCommand { get; }

        /// <summary>
        /// 항목 활성화 토글 명령
        /// </summary>
        public ICommand ToggleEnabledCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// SettingsViewModel의 새 인스턴스를 초기화합니다.
        /// </summary>
        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // 명령 초기화
            AddItemCommand = new RelayCommand(async () => await AddItemAsync(), CanAddItem);
            EditItemCommand = new RelayCommand(async () => await EditItemAsync(), CanEditItem);
            DeleteItemCommand = new RelayCommand(async () => await DeleteItemAsync(), CanDeleteItem);
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            CancelSettingsCommand = new RelayCommand(CancelSettings);
            ToggleFavoriteCommand = new RelayCommand<PresetItem>(async item => await ToggleFavoriteAsync(item!));
            ToggleEnabledCommand = new RelayCommand<PresetItem>(async item => await ToggleEnabledAsync(item!));

            // 미리 정의된 항목 타입 초기화
            InitializePresetTypes();

            // 설정 로드
            _ = LoadSettingsAsync();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 미리 정의된 항목 타입을 초기화합니다.
        /// </summary>
        private void InitializePresetTypes()
        {
            PresetTypes.Add(new PresetTypeInfo(PresetType.Abbreviation, "약어", "파일명에 사용할 약어 목록"));
            PresetTypes.Add(new PresetTypeInfo(PresetType.Title, "제목", "파일명에 사용할 제목 목록"));
            PresetTypes.Add(new PresetTypeInfo(PresetType.Suffix, "접미어", "파일명에 사용할 접미어 목록"));
            PresetTypes.Add(new PresetTypeInfo(PresetType.Extension, "확장자", "파일 확장자 목록"));
            PresetTypes.Add(new PresetTypeInfo(PresetType.OutputPath, "출력 경로", "파일을 생성할 경로 목록"));
            PresetTypes.Add(new PresetTypeInfo(PresetType.TemplatePath, "템플릿 경로", "템플릿 파일 경로 목록"));
        }

        /// <summary>
        /// 설정을 비동기로 로드합니다.
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            try
            {
                IsWorking = true;
                StatusMessage = "설정을 로드하는 중...";

                _originalSettings = await _settingsService.LoadSettingsAsync();
                await LoadCurrentTypeItemsAsync();

                StatusMessage = "설정 로드 완료";
            }
            catch (Exception ex)
            {
                StatusMessage = $"설정 로드 오류: {ex.Message}";
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 현재 선택된 타입의 항목들을 로드합니다.
        /// </summary>
        private async Task LoadCurrentTypeItemsAsync()
        {
            try
            {
                var items = await _settingsService.GetPresetItemsAsync(SelectedPresetType);
                
                CurrentItems.Clear();
                foreach (var item in items.OrderByDescending(i => i.IsFavorite)
                                        .ThenByDescending(i => i.UsageCount)
                                        .ThenBy(i => i.Value))
                {
                    CurrentItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"항목 로드 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 항목을 추가할 수 있는지 확인합니다.
        /// </summary>
        /// <returns>추가 가능하면 true</returns>
        private bool CanAddItem()
        {
            return !string.IsNullOrWhiteSpace(NewItemValue) && !IsWorking;
        }

        /// <summary>
        /// 새 항목을 추가합니다.
        /// </summary>
        private async Task AddItemAsync()
        {
            try
            {
                IsWorking = true;
                StatusMessage = "항목을 추가하는 중...";

                var newItem = new PresetItem
                {
                    Value = NewItemValue.Trim(),
                    Description = NewItemDescription.Trim(),
                    CreatedAt = DateTime.Now,
                    LastUsed = DateTime.Now,
                    IsEnabled = true,
                    IsFavorite = false
                };

                var success = await _settingsService.AddPresetItemAsync(SelectedPresetType, newItem);
                if (success)
                {
                    await LoadCurrentTypeItemsAsync();
                    NewItemValue = string.Empty;
                    NewItemDescription = string.Empty;
                    StatusMessage = "항목이 추가되었습니다.";
                }
                else
                {
                    StatusMessage = "항목 추가에 실패했습니다. (중복된 값이거나 오류 발생)";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"항목 추가 오류: {ex.Message}";
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 항목을 수정할 수 있는지 확인합니다.
        /// </summary>
        /// <returns>수정 가능하면 true</returns>
        private bool CanEditItem()
        {
            return SelectedItem != null && !IsWorking;
        }

        /// <summary>
        /// 선택된 항목을 수정합니다.
        /// </summary>
        private async Task EditItemAsync()
        {
            if (SelectedItem == null) return;

            try
            {
                IsWorking = true;
                StatusMessage = "항목을 수정하는 중...";

                // 간단한 편집 다이얼로그 (실제 구현에서는 별도 윈도우 사용)
                var result = System.Windows.MessageBox.Show(
                    $"항목을 수정하시겠습니까?\n\n현재 값: {SelectedItem.Value}\n현재 설명: {SelectedItem.Description}",
                    "항목 수정",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var success = await _settingsService.UpdatePresetItemAsync(SelectedPresetType, SelectedItem);
                    if (success)
                    {
                        await LoadCurrentTypeItemsAsync();
                        StatusMessage = "항목이 수정되었습니다.";
                    }
                    else
                    {
                        StatusMessage = "항목 수정에 실패했습니다.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"항목 수정 오류: {ex.Message}";
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 항목을 삭제할 수 있는지 확인합니다.
        /// </summary>
        /// <returns>삭제 가능하면 true</returns>
        private bool CanDeleteItem()
        {
            return SelectedItem != null && !IsWorking;
        }

        /// <summary>
        /// 선택된 항목을 삭제합니다.
        /// </summary>
        private async Task DeleteItemAsync()
        {
            if (SelectedItem == null) return;

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"다음 항목을 삭제하시겠습니까?\n\n값: {SelectedItem.Value}\n설명: {SelectedItem.Description}",
                    "항목 삭제 확인",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    IsWorking = true;
                    StatusMessage = "항목을 삭제하는 중...";

                    var success = await _settingsService.DeletePresetItemAsync(SelectedPresetType, SelectedItem.Id);
                    if (success)
                    {
                        await LoadCurrentTypeItemsAsync();
                        SelectedItem = null;
                        StatusMessage = "항목이 삭제되었습니다.";
                    }
                    else
                    {
                        StatusMessage = "항목 삭제에 실패했습니다.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"항목 삭제 오류: {ex.Message}";
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 즐겨찾기 상태를 토글합니다.
        /// </summary>
        private async Task ToggleFavoriteAsync(PresetItem item)
        {
            if (item == null) return;

            try
            {
                item.IsFavorite = !item.IsFavorite;
                await _settingsService.UpdatePresetItemAsync(SelectedPresetType, item);
                await LoadCurrentTypeItemsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"즐겨찾기 설정 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 활성화 상태를 토글합니다.
        /// </summary>
        private async Task ToggleEnabledAsync(PresetItem item)
        {
            if (item == null) return;

            try
            {
                item.IsEnabled = !item.IsEnabled;
                await _settingsService.UpdatePresetItemAsync(SelectedPresetType, item);
                await LoadCurrentTypeItemsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"활성화 설정 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 설정을 저장합니다.
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            try
            {
                IsWorking = true;
                StatusMessage = "설정을 저장하는 중...";

                // 현재 설정이 이미 서비스를 통해 개별적으로 저장되므로
                // 여기서는 추가적인 저장 작업 없이 완료 메시지만 표시
                StatusMessage = "모든 설정이 저장되었습니다.";
                
                System.Windows.MessageBox.Show(
                    "설정이 저장되었습니다.",
                    "저장 완료",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"설정 저장 오류: {ex.Message}";
                System.Windows.MessageBox.Show(
                    $"설정 저장 중 오류가 발생했습니다.\n\n{ex.Message}",
                    "저장 오류",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 설정을 취소합니다.
        /// </summary>
        private void CancelSettings()
        {
            var result = System.Windows.MessageBox.Show(
                "변경사항을 취소하고 창을 닫으시겠습니까?",
                "취소 확인",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // 원본 설정으로 복원하는 로직은 실제 UI에서 창을 닫는 처리와 함께 구현
                StatusMessage = "설정이 취소되었습니다.";
            }
        }
        #endregion
    }

    /// <summary>
    /// 미리 정의된 항목 타입 정보
    /// </summary>
    public class PresetTypeInfo
    {
        public PresetType Type { get; }
        public string DisplayName { get; }
        public string Description { get; }

        public PresetTypeInfo(PresetType type, string displayName, string description)
        {
            Type = type;
            DisplayName = displayName;
            Description = description;
        }

        public override string ToString() => DisplayName;
    }
}