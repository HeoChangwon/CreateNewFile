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
        private PresetItem? _selectedItem;
        private PresetType _selectedPresetType = PresetType.Abbreviation;
        private PresetTypeInfo? _selectedPresetTypeInfo;
        private bool _isWorking = false;
        private string _statusMessage = string.Empty;
        private AppSettings? _originalSettings;
        private List<PresetItem> _selectedItems = new List<PresetItem>();
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
        /// 선택된 항목
        /// </summary>
        public PresetItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    // 선택된 항목의 값을 텍스트박스에 표시
                    if (value != null)
                    {
                        NewItemValue = value.Value;
                    }
                }
            }
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
                    OnPropertyChanged(nameof(SelectedPresetTypeDisplayName));
                    _ = LoadCurrentTypeItemsAsync();
                }
            }
        }

        /// <summary>
        /// 선택된 미리 정의된 항목 타입 정보
        /// </summary>
        public PresetTypeInfo? SelectedPresetTypeInfo
        {
            get => _selectedPresetTypeInfo;
            set
            {
                if (SetProperty(ref _selectedPresetTypeInfo, value))
                {
                    if (value != null)
                    {
                        _selectedPresetType = value.Type;
                        OnPropertyChanged(nameof(SelectedPresetType));
                        OnPropertyChanged(nameof(SelectedPresetTypeDisplayName));
                        _ = LoadCurrentTypeItemsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 선택된 미리 정의된 항목 타입의 표시 이름
        /// </summary>
        public string SelectedPresetTypeDisplayName => GetPresetTypeDisplayName(SelectedPresetType);

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
        /// 선택된 여러 항목 삭제 명령
        /// </summary>
        public ICommand DeleteSelectedItemsCommand { get; }

        /// <summary>
        /// 설정 창 닫기 명령
        /// </summary>
        public ICommand CancelSettingsCommand { get; }

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
            DeleteSelectedItemsCommand = new RelayCommand(async () => await DeleteSelectedItemsAsync());
            CancelSettingsCommand = new RelayCommand(CancelSettings);

            // 미리 정의된 항목 타입 초기화
            InitializePresetTypes();
            
            // 첫 번째 항목을 기본 선택
            if (PresetTypes.Count > 0)
            {
                _selectedPresetTypeInfo = PresetTypes[0];
                OnPropertyChanged(nameof(SelectedPresetTypeInfo));
            }
            
            StatusMessage = "설정 관리 초기화됨";

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
                StatusMessage = $"설정 로드됨. 약어: {_originalSettings.Abbreviations.Count}개, 제목: {_originalSettings.Titles.Count}개, 접미어: {_originalSettings.Suffixes.Count}개, 확장자: {_originalSettings.Extensions.Count}개";
                
                await LoadCurrentTypeItemsAsync();
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
                foreach (var item in items.OrderBy(i => i.Value))
                {
                    CurrentItems.Add(item);
                }
                
                StatusMessage = $"{items.Count}개의 {GetPresetTypeDisplayName(SelectedPresetType)} 항목을 로드했습니다.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"항목 로드 오류: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 미리 정의된 항목 타입의 표시 이름을 가져옵니다.
        /// </summary>
        /// <param name="type">항목 타입</param>
        /// <returns>표시 이름</returns>
        private static string GetPresetTypeDisplayName(PresetType type)
        {
            return type switch
            {
                PresetType.Abbreviation => "약어",
                PresetType.Title => "제목",
                PresetType.Suffix => "접미어",
                PresetType.Extension => "확장자",
                _ => "항목"
            };
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
                    Description = string.Empty,
                    CreatedAt = DateTime.Now,
                    IsEnabled = true,
                    IsFavorite = false
                };

                var success = await _settingsService.AddPresetItemAsync(SelectedPresetType, newItem);
                if (success)
                {
                    await LoadCurrentTypeItemsAsync();
                    NewItemValue = string.Empty;
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
            return SelectedItem != null && !IsWorking && !string.IsNullOrWhiteSpace(NewItemValue);
        }

        /// <summary>
        /// 선택된 항목을 수정합니다.
        /// </summary>
        private async Task EditItemAsync()
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(NewItemValue)) return;

            try
            {
                IsWorking = true;
                StatusMessage = "항목을 수정하는 중...";

                // 선택된 항목의 값을 새 값으로 변경
                SelectedItem.Value = NewItemValue.Trim();
                
                var success = await _settingsService.UpdatePresetItemAsync(SelectedPresetType, SelectedItem);
                if (success)
                {
                    await LoadCurrentTypeItemsAsync();
                    NewItemValue = string.Empty;
                    SelectedItem = null;
                    StatusMessage = "항목이 수정되었습니다.";
                }
                else
                {
                    StatusMessage = "항목 수정에 실패했습니다.";
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
        /// 선택된 여러 항목을 삭제합니다.
        /// </summary>
        private async Task DeleteSelectedItemsAsync()
        {
            if (_selectedItems.Count == 0) 
            {
                StatusMessage = "삭제할 항목을 선택해주세요.";
                return;
            }

            try
            {
                IsWorking = true;
                StatusMessage = $"{_selectedItems.Count}개 항목을 삭제하는 중...";

                var deletedCount = 0;
                foreach (var item in _selectedItems.ToList())
                {
                    var success = await _settingsService.DeletePresetItemAsync(SelectedPresetType, item.Id);
                    if (success)
                        deletedCount++;
                }

                if (deletedCount > 0)
                {
                    await LoadCurrentTypeItemsAsync();
                    SelectedItem = null;
                    _selectedItems.Clear();
                    StatusMessage = $"{deletedCount}개 항목이 삭제되었습니다.";
                }
                else
                {
                    StatusMessage = "항목 삭제에 실패했습니다.";
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
        /// 선택된 여러 항목 목록을 설정합니다.
        /// </summary>
        /// <param name="selectedItems">선택된 항목 목록</param>
        public void SetSelectedItems(IEnumerable<PresetItem> selectedItems)
        {
            _selectedItems = selectedItems?.ToList() ?? new List<PresetItem>();
        }

        /// <summary>
        /// 창 닫기 요청 이벤트
        /// </summary>
        public event EventHandler? CloseRequested;

        /// <summary>
        /// 설정 창을 닫습니다.
        /// </summary>
        private void CancelSettings()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
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