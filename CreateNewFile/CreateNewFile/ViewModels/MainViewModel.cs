using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using CreateNewFile.Utils;
using CreateNewFile.Models;
using CreateNewFile.Services;

namespace CreateNewFile.ViewModels
{
    /// <summary>
    /// 메인 윈도우의 ViewModel
    /// 파일 생성 관련 로직 처리
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly IFileGeneratorService _fileGeneratorService;
        private readonly ISettingsService _settingsService;
        private readonly IFileInfoService _fileInfoService;
        private readonly IProjectConfigService _projectConfigService;
        
        private DateTime _selectedDateTime;
        private string _selectedAbbreviation = string.Empty;
        private string _selectedTitle = string.Empty;
        private string _selectedSuffix = string.Empty;
        private string _selectedExtension = string.Empty;
        private string _selectedOutputPath = string.Empty;
        private string _selectedTemplatePath = string.Empty;
        private string _templateFileName = string.Empty;
        private string _generatedFileName = string.Empty;
        private string _fullFilePath = string.Empty;
        private bool _isWorking = false;
        private string _statusMessage = string.Empty;
        private string _validationError = string.Empty;
        private bool _hasValidationErrors = false;
        
        private bool _isDateTimeEnabled = true;
        private bool _isAbbreviationEnabled = true;
        private bool _isTitleEnabled = true;
        private bool _isSuffixEnabled = true;
        private bool _isLoadingCheckboxStates = false;
        
        // 파일정보 관련 필드
        private string _fileInfoName = string.Empty;
        private bool _isEditingFileInfoName = false;
        private FileInfoModel? _selectedFileInfoModel = null;
        private StringReplacementRule? _selectedStringReplacement = null;
        #endregion

        #region Properties
        /// <summary>
        /// 선택된 날짜와 시간
        /// </summary>
        public DateTime SelectedDateTime
        {
            get => _selectedDateTime;
            set
            {
                if (SetProperty(ref _selectedDateTime, value))
                {
                    UpdateGeneratedFileName();
                }
            }
        }

        /// <summary>
        /// 선택된 약어
        /// </summary>
        public string SelectedAbbreviation
        {
            get => _selectedAbbreviation;
            set
            {
                if (SetProperty(ref _selectedAbbreviation, value))
                {
                    ValidateInput();
                    UpdateGeneratedFileName();
                }
            }
        }

        /// <summary>
        /// 선택된 제목
        /// </summary>
        public string SelectedTitle
        {
            get => _selectedTitle;
            set
            {
                if (SetProperty(ref _selectedTitle, value))
                {
                    ValidateInput();
                    UpdateGeneratedFileName();
                }
            }
        }

        /// <summary>
        /// 선택된 접미어
        /// </summary>
        public string SelectedSuffix
        {
            get => _selectedSuffix;
            set
            {
                if (SetProperty(ref _selectedSuffix, value))
                {
                    ValidateInput();
                    UpdateGeneratedFileName();
                }
            }
        }

        /// <summary>
        /// 선택된 확장자
        /// </summary>
        public string SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                if (SetProperty(ref _selectedExtension, value))
                {
                    ValidateInput();
                    UpdateGeneratedFileName();
                }
            }
        }

        /// <summary>
        /// 선택된 출력 경로
        /// </summary>
        public string SelectedOutputPath
        {
            get => _selectedOutputPath;
            set
            {
                if (SetProperty(ref _selectedOutputPath, value))
                {
                    ValidateInput();
                    UpdateFullFilePath();
                }
            }
        }

        /// <summary>
        /// 선택된 템플릿 경로
        /// </summary>
        public string SelectedTemplatePath
        {
            get => _selectedTemplatePath;
            set
            {
                if (SetProperty(ref _selectedTemplatePath, value))
                {
                    TemplateFileName = !string.IsNullOrWhiteSpace(value)
                        ? Path.GetFileName(value)
                        : string.Empty;
                    ValidateInput();
                }
            }
        }

        /// <summary>
        /// 템플릿 파일명 (경로에서 추출)
        /// </summary>
        public string TemplateFileName
        {
            get => _templateFileName;
            private set => SetProperty(ref _templateFileName, value);
        }

        /// <summary>
        /// 생성될 파일명
        /// </summary>
        public string GeneratedFileName
        {
            get => _generatedFileName;
            private set
            {
                if (SetProperty(ref _generatedFileName, value))
                {
                    UpdateFullFilePath();
                }
            }
        }

        /// <summary>
        /// 전체 파일 경로
        /// </summary>
        public string FullFilePath
        {
            get => _fullFilePath;
            private set => SetProperty(ref _fullFilePath, value);
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
        /// 유효성 검사 에러 메시지
        /// </summary>
        public string ValidationError
        {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        /// <summary>
        /// 유효성 검사 에러 존재 여부
        /// </summary>
        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            set => SetProperty(ref _hasValidationErrors, value);
        }

        /// <summary>
        /// 약어 목록
        /// </summary>
        public ObservableCollection<PresetItem> Abbreviations { get; } = new();

        /// <summary>
        /// 제목 목록
        /// </summary>
        public ObservableCollection<PresetItem> Titles { get; } = new();

        /// <summary>
        /// 접미어 목록
        /// </summary>
        public ObservableCollection<PresetItem> Suffixes { get; } = new();

        /// <summary>
        /// 확장자 목록
        /// </summary>
        public ObservableCollection<PresetItem> Extensions { get; } = new();

        /// <summary>
        /// 출력 경로 목록
        /// </summary>
        public ObservableCollection<PresetItem> OutputPaths { get; } = new();

        /// <summary>
        /// 템플릿 경로 목록
        /// </summary>
        public ObservableCollection<PresetItem> TemplatePaths { get; } = new();

        /// <summary>
        /// 저장된 파일정보 목록
        /// </summary>
        public ObservableCollection<FileInfoModel> SavedFileInfos { get; } = new();

        /// <summary>
        /// 문자열 교체 규칙 목록
        /// </summary>
        public ObservableCollection<StringReplacementRule> StringReplacements { get; } = new();
        
        /// <summary>
        /// 날짜/시간 항목 활성화 여부
        /// </summary>
        public bool IsDateTimeEnabled
        {
            get => _isDateTimeEnabled;
            set
            {
                if (SetProperty(ref _isDateTimeEnabled, value))
                {
                    UpdateGeneratedFileName();
                    if (!_isLoadingCheckboxStates)
                    {
                        _ = SaveCheckboxStatesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 약어 항목 활성화 여부
        /// </summary>
        public bool IsAbbreviationEnabled
        {
            get => _isAbbreviationEnabled;
            set
            {
                if (SetProperty(ref _isAbbreviationEnabled, value))
                {
                    UpdateGeneratedFileName();
                    if (!_isLoadingCheckboxStates)
                    {
                        _ = SaveCheckboxStatesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 제목 항목 활성화 여부
        /// </summary>
        public bool IsTitleEnabled
        {
            get => _isTitleEnabled;
            set
            {
                if (SetProperty(ref _isTitleEnabled, value))
                {
                    UpdateGeneratedFileName();
                    if (!_isLoadingCheckboxStates)
                    {
                        _ = SaveCheckboxStatesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 접미어 항목 활성화 여부
        /// </summary>
        public bool IsSuffixEnabled
        {
            get => _isSuffixEnabled;
            set
            {
                if (SetProperty(ref _isSuffixEnabled, value))
                {
                    UpdateGeneratedFileName();
                    if (!_isLoadingCheckboxStates)
                    {
                        _ = SaveCheckboxStatesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 파일정보 이름
        /// </summary>
        public string FileInfoName
        {
            get => _fileInfoName;
            set => SetProperty(ref _fileInfoName, value);
        }

        /// <summary>
        /// 파일정보 이름 편집 중 여부
        /// </summary>
        public bool IsEditingFileInfoName
        {
            get => _isEditingFileInfoName;
            set => SetProperty(ref _isEditingFileInfoName, value);
        }

        /// <summary>
        /// 선택된 파일정보
        /// </summary>
        public FileInfoModel? SelectedFileInfo
        {
            get => _selectedFileInfoModel;
            set => SetProperty(ref _selectedFileInfoModel, value);
        }

        /// <summary>
        /// 선택된 문자열 교체 규칙
        /// </summary>
        public StringReplacementRule? SelectedStringReplacement
        {
            get => _selectedStringReplacement;
            set => SetProperty(ref _selectedStringReplacement, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// 현재 날짜/시간 설정 명령
        /// </summary>
        public ICommand SetCurrentDateTimeCommand { get; }

        /// <summary>
        /// 파일 생성 명령
        /// </summary>
        public ICommand CreateFileCommand { get; }

        /// <summary>
        /// 설정 열기 명령
        /// </summary>
        public ICommand OpenSettingsCommand { get; }

        /// <summary>
        /// 출력 경로 찾아보기 명령
        /// </summary>
        public ICommand BrowseOutputPathCommand { get; }

        /// <summary>
        /// 템플릿 경로 찾아보기 명령
        /// </summary>
        public ICommand BrowseTemplatePathCommand { get; }

        /// <summary>
        /// 템플릿 파일 편집기로 열기 명령
        /// </summary>
        public ICommand OpenTemplateFileCommand { get; }

        /// <summary>
        /// 설정 폴더 열기 명령
        /// </summary>
        public ICommand OpenSettingsFolderCommand { get; }

        /// <summary>
        /// 출력 폴더 열기 명령
        /// </summary>
        public ICommand OpenOutputFolderCommand { get; }

        /// <summary>
        /// 템플릿 폴더 열기 명령
        /// </summary>
        public ICommand OpenTemplateFolderCommand { get; }

        // 파일정보 관련 명령들
        /// <summary>
        /// 파일정보 불러오기 명령
        /// </summary>
        public ICommand LoadFileInfoCommand { get; }

        /// <summary>
        /// 파일정보 저장 명령
        /// </summary>
        public ICommand SaveFileInfoCommand { get; }

        /// <summary>
        /// 파일정보 관리 명령
        /// </summary>
        public ICommand ManageFileInfosCommand { get; }

        // 문자열 교체 관련 명령들
        /// <summary>
        /// 문자열 교체 규칙 추가 명령
        /// </summary>
        public ICommand AddStringReplacementCommand { get; }

        /// <summary>
        /// 문자열 교체 규칙 제거 명령
        /// </summary>
        public ICommand RemoveStringReplacementCommand { get; }

        /// <summary>
        /// 문자열 교체 규칙 위로 이동 명령
        /// </summary>
        public ICommand MoveUpStringReplacementCommand { get; }

        /// <summary>
        /// 문자열 교체 규칙 아래로 이동 명령
        /// </summary>
        public ICommand MoveDownStringReplacementCommand { get; }

        // 프로젝트 설정 관련 명령들
        /// <summary>
        /// 프로젝트 설정 저장 명령
        /// </summary>
        public ICommand SaveProjectConfigCommand { get; }

        /// <summary>
        /// 프로젝트 설정 열기 명령
        /// </summary>
        public ICommand OpenProjectConfigCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// MainViewModel의 새 인스턴스를 초기화합니다.
        /// </summary>
        public MainViewModel(IFileGeneratorService fileGeneratorService, ISettingsService settingsService, IFileInfoService fileInfoService, IProjectConfigService projectConfigService)
        {
            _fileGeneratorService = fileGeneratorService ?? throw new ArgumentNullException(nameof(fileGeneratorService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _fileInfoService = fileInfoService ?? throw new ArgumentNullException(nameof(fileInfoService));
            _projectConfigService = projectConfigService ?? throw new ArgumentNullException(nameof(projectConfigService));

            // 기본값 설정 (날짜/시간은 설정 로드 후 적용)
            StatusMessage = "준비";

            // 명령 초기화
            SetCurrentDateTimeCommand = new RelayCommand(SetCurrentDateTime);
            CreateFileCommand = new RelayCommand(async () => await CreateFileAsync(), CanCreateFile);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            BrowseOutputPathCommand = new RelayCommand(BrowseOutputPath);
            BrowseTemplatePathCommand = new RelayCommand(BrowseTemplatePath);
            OpenTemplateFileCommand = new RelayCommand(OpenTemplateFile, CanOpenTemplateFile);
            OpenSettingsFolderCommand = new RelayCommand(OpenSettingsFolder);
            OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder, CanOpenOutputFolder);
            OpenTemplateFolderCommand = new RelayCommand(OpenTemplateFolder, CanOpenTemplateFolder);

            // 파일정보 관련 명령 초기화
            LoadFileInfoCommand = new RelayCommand(async () => await LoadFileInfoAsync());
            SaveFileInfoCommand = new RelayCommand(async () => await SaveFileInfoAsync());
            ManageFileInfosCommand = new RelayCommand(ManageFileInfos);

            // 문자열 교체 관련 명령 초기화
            AddStringReplacementCommand = new RelayCommand(AddStringReplacement);
            RemoveStringReplacementCommand = new RelayCommand(RemoveStringReplacement);
            MoveUpStringReplacementCommand = new RelayCommand(MoveUpStringReplacement);
            MoveDownStringReplacementCommand = new RelayCommand(MoveDownStringReplacement);

            // 프로젝트 설정 관련 명령 초기화
            SaveProjectConfigCommand = new RelayCommand(async () => await SaveProjectConfigAsync());
            OpenProjectConfigCommand = new RelayCommand(async () => await OpenProjectConfigAsync());

            // 데이터 로드는 별도로 호출하도록 변경 (화면 표시 전에 완료하기 위해)
        }

        /// <summary>
        /// 초기 데이터를 로드합니다 (화면 표시 전에 호출).
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 설정 창에서 변경된 내용을 반영하여 콤보박스만 갱신합니다.
        /// </summary>
        public async Task RefreshComboBoxesAsync()
        {
            try
            {
                
                // 현재 선택된 값들 임시 저장
                var selectedAbbreviation = SelectedAbbreviation;
                var selectedTitle = SelectedTitle;
                var selectedSuffix = SelectedSuffix;
                var selectedExtension = SelectedExtension;
                var selectedOutputPath = SelectedOutputPath;
                var selectedTemplatePath = SelectedTemplatePath;
                
                // 콤보박스 아이템 갱신 (출력 폴더와 템플릿 파일은 제외)
                await LoadPresetItems(PresetType.Abbreviation, Abbreviations);
                await LoadPresetItems(PresetType.Title, Titles);
                await LoadPresetItems(PresetType.Suffix, Suffixes);
                await LoadPresetItems(PresetType.Extension, Extensions);
                // OutputPath와 TemplatePath는 갱신하지 않음 (현재 선택된 경로 유지)
                
                // 이전에 선택되었던 값들이 여전히 존재하면 복원, 없으면 첫 번째 항목 선택
                RestoreOrSetFirstSelection(Abbreviations, selectedAbbreviation, value => SelectedAbbreviation = value);
                RestoreOrSetFirstSelection(Titles, selectedTitle, value => SelectedTitle = value);
                RestoreOrSetFirstSelection(Suffixes, selectedSuffix, value => SelectedSuffix = value);
                RestoreOrSetFirstSelection(Extensions, selectedExtension, value => SelectedExtension = value);
                // OutputPath와 TemplatePath는 복원하지 않음 (현재 값 유지)
                
            }
            catch (Exception ex)
            {
                StatusMessage = $"콤보박스 갱신 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 선택된 값을 복원하거나 첫 번째 항목을 선택합니다.
        /// </summary>
        private void RestoreOrSetFirstSelection<T>(ObservableCollection<T> collection, string previousValue, Action<string> setSelection) 
            where T : PresetItem
        {
            if (collection.Count == 0)
            {
                setSelection("");
                return;
            }

            // 이전 선택값이 여전히 존재하는지 확인
            if (!string.IsNullOrEmpty(previousValue) && collection.Any(item => item.Value == previousValue))
            {
                setSelection(previousValue);
            }
            else
            {
                // 없으면 첫 번째 항목 선택
                setSelection(collection[0].Value);
            }
        }

        /// <summary>
        /// 필수 설정만 동기적으로 빠르게 로드합니다 (윈도우 표시 전용)
        /// </summary>
        public void InitializeEssentialSync()
        {
            try
            {
                // 동기적으로 기본 설정값만 로드
                var settings = _settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
                
                // 체크박스 상태 설정
                IsDateTimeEnabled = settings.IsDateTimeEnabled;
                IsAbbreviationEnabled = settings.IsAbbreviationEnabled;
                IsTitleEnabled = settings.IsTitleEnabled;
                IsSuffixEnabled = settings.IsSuffixEnabled;
                
                // 체크박스 상태만 빠르게 설정 (선택된 값들은 백그라운드에서 로드)
                
                StatusMessage = "준비 완료";
            }
            catch (Exception ex)
            {
                StatusMessage = "기본 설정으로 시작";
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 파일명을 업데이트합니다.
        /// </summary>
        private void UpdateGeneratedFileName()
        {
            try
            {
                var request = CreateFileRequest();
                if (request != null)
                {
                    GeneratedFileName = _fileGeneratorService.GenerateFileName(request, IsDateTimeEnabled, IsAbbreviationEnabled, IsTitleEnabled, IsSuffixEnabled);
                }
                else
                {
                    GeneratedFileName = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일명 생성 오류: {ex.Message}";
                GeneratedFileName = string.Empty;
            }
        }

        /// <summary>
        /// 전체 파일 경로를 업데이트합니다.
        /// </summary>
        private void UpdateFullFilePath()
        {
            if (!string.IsNullOrWhiteSpace(SelectedOutputPath) && !string.IsNullOrWhiteSpace(GeneratedFileName))
            {
                FullFilePath = Path.Combine(SelectedOutputPath, GeneratedFileName);
            }
            else
            {
                FullFilePath = string.Empty;
            }
        }

        /// <summary>
        /// 데이터를 비동기로 로드합니다.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                IsWorking = true;
                StatusMessage = "설정을 로드하는 중...";

                var settings = await _settingsService.LoadSettingsAsync();

                // 컬렉션 업데이트
                await LoadPresetItems(PresetType.Abbreviation, Abbreviations);
                await LoadPresetItems(PresetType.Title, Titles);
                await LoadPresetItems(PresetType.Suffix, Suffixes);
                await LoadPresetItems(PresetType.Extension, Extensions);
                await LoadPresetItems(PresetType.OutputPath, OutputPaths);
                await LoadPresetItems(PresetType.TemplatePath, TemplatePaths);

                // 파일정보 목록 로드
                await RefreshSavedFileInfosAsync();

                // 기본값 설정
                await SetDefaultValues(settings);

                StatusMessage = "준비 완료";
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
        /// 미리 정의된 항목을 로드합니다.
        /// </summary>
        private async Task LoadPresetItems(PresetType type, ObservableCollection<PresetItem> collection)
        {
            try
            {
                var items = await _settingsService.GetPresetItemsAsync(type);
                collection.Clear();
                foreach (var item in items.Where(i => i.IsEnabled).OrderBy(i => i.Value))
                {
                    collection.Add(item);
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 기본값을 설정합니다.
        /// </summary>
        private async Task SetDefaultValues(AppSettings settings)
        {
            // 마지막 설정 정보 로드 우선, 없으면 기본값
            
            // 날짜/시간 (마지막 설정값 적용, 없으면 현재 시간)
            if (settings.LastSelectedDateTime != default(DateTime))
                SelectedDateTime = settings.LastSelectedDateTime;
            else
                SelectedDateTime = DateTime.Now;
            
            // 출력 경로 (마지막 설정 -> 기본 -> 첫 번째 항목)
            if (!string.IsNullOrWhiteSpace(settings.LastSelectedOutputPath))
            {
                SelectedOutputPath = settings.LastSelectedOutputPath;
            }
            else if (!string.IsNullOrWhiteSpace(settings.DefaultOutputPath))
            {
                SelectedOutputPath = settings.DefaultOutputPath;
            }
            else if (OutputPaths.Count > 0)
            {
                SelectedOutputPath = OutputPaths[0].Value;
            }

            // 템플릿 경로 (마지막 설정 -> 기본)
            if (!string.IsNullOrWhiteSpace(settings.LastSelectedTemplatePath))
            {
                SelectedTemplatePath = settings.LastSelectedTemplatePath;
            }
            else if (!string.IsNullOrWhiteSpace(settings.DefaultTemplatePath))
            {
                SelectedTemplatePath = settings.DefaultTemplatePath;
            }

            // 각 항목별 마지막 설정값 적용
            if (!string.IsNullOrWhiteSpace(settings.LastSelectedAbbreviation))
                SelectedAbbreviation = settings.LastSelectedAbbreviation;
            else if (Abbreviations.Count > 0) 
                SelectedAbbreviation = Abbreviations[0].Value;

            if (!string.IsNullOrWhiteSpace(settings.LastSelectedTitle))
                SelectedTitle = settings.LastSelectedTitle;

            if (!string.IsNullOrWhiteSpace(settings.LastSelectedSuffix))
                SelectedSuffix = settings.LastSelectedSuffix;

            if (!string.IsNullOrWhiteSpace(settings.LastSelectedExtension))
                SelectedExtension = settings.LastSelectedExtension;
            else if (Extensions.Count > 0) 
                SelectedExtension = Extensions[0].Value;

            // 문자열 교체 규칙 복원
            StringReplacements.Clear();
            if (settings.LastStringReplacements != null && settings.LastStringReplacements.Count > 0)
            {
                foreach (var rule in settings.LastStringReplacements)
                {
                    StringReplacements.Add((StringReplacementRule)rule.Clone());
                }
            }

            // 체크박스 상태 복원
            await LoadCheckboxStatesAsync();
        }

        /// <summary>
        /// 체크박스 상태를 저장합니다.
        /// </summary>
        private async Task SaveCheckboxStatesAsync()
        {
            try
            {
                await _settingsService.SaveCheckboxStatesAsync(
                    IsDateTimeEnabled, 
                    IsAbbreviationEnabled, 
                    IsTitleEnabled, 
                    IsSuffixEnabled);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 체크박스 상태를 로드합니다.
        /// </summary>
        private async Task LoadCheckboxStatesAsync()
        {
            try
            {
                _isLoadingCheckboxStates = true;
                
                var (isDateTime, isAbbreviation, isTitle, isSuffix) = await _settingsService.LoadCheckboxStatesAsync();
                
                
                // 속성을 통해 설정 (UI 업데이트를 위해)
                IsDateTimeEnabled = isDateTime;
                IsAbbreviationEnabled = isAbbreviation;
                IsTitleEnabled = isTitle;
                IsSuffixEnabled = isSuffix;
                
                // 파일명 업데이트
                UpdateGeneratedFileName();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _isLoadingCheckboxStates = false;
            }
        }
        #endregion

        #region Command Methods
        /// <summary>
        /// 현재 날짜/시간을 설정합니다.
        /// </summary>
        private void SetCurrentDateTime()
        {
            SelectedDateTime = DateTime.Now;
        }

        /// <summary>
        /// 입력값의 유효성을 검사합니다.
        /// </summary>
        private void ValidateInput()
        {
            try
            {
                var validationResults = new List<ValidationResult>();

                // 기본 유효성 검사
                validationResults.Add(ValidationHelper.ValidateFileCreationRequest(
                    SelectedAbbreviation,
                    SelectedTitle,
                    SelectedSuffix,
                    SelectedExtension,
                    SelectedOutputPath,
                    SelectedTemplatePath));

                // 추가적인 강화된 검사
                if (!string.IsNullOrWhiteSpace(SelectedOutputPath))
                {
                    // 경로 유효성 및 쓰기 권한 검사
                    validationResults.Add(ValidationHelper.ValidateFolderExists(SelectedOutputPath));
                    validationResults.Add(ValidationHelper.ValidateWritePermission(SelectedOutputPath));
                }

                // 파일명 길이 검사 (생성될 파일명 기준)
                if (!string.IsNullOrWhiteSpace(GeneratedFileName))
                {
                    validationResults.Add(ValidationHelper.ValidateFileName(GeneratedFileName));
                    
                    // 전체 경로 길이 검사
                    if (!string.IsNullOrWhiteSpace(SelectedOutputPath))
                    {
                        var fullPath = Path.Combine(SelectedOutputPath, GeneratedFileName);
                        if (fullPath.Length > ValidationHelper.MaxPathLength)
                        {
                            validationResults.Add(ValidationResult.CreateFailure(
                                $"전체 경로가 너무 깁니다. 최대 {ValidationHelper.MaxPathLength}자까지 허용됩니다."));
                        }
                    }
                }

                // 템플릿 파일 추가 검사
                if (!string.IsNullOrWhiteSpace(SelectedTemplatePath))
                {
                    validationResults.Add(ValidationHelper.ValidateFileExists(SelectedTemplatePath));
                    
                    // 템플릿 파일 읽기 권한 검사
                    try
                    {
                        if (File.Exists(SelectedTemplatePath))
                        {
                            using (var stream = File.OpenRead(SelectedTemplatePath))
                            {
                                // 읽기 권한 테스트
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        validationResults.Add(ValidationResult.CreateFailure("템플릿 파일에 읽기 권한이 없습니다."));
                    }
                    catch (Exception ex)
                    {
                        validationResults.Add(ValidationResult.CreateFailure($"템플릿 파일 접근 오류: {ex.Message}"));
                    }
                }

                // 결과 통합
                var combinedResult = ValidationHelper.CombineValidationResults(validationResults.ToArray());

                if (combinedResult.IsValid)
                {
                    ValidationError = string.Empty;
                    HasValidationErrors = false;
                }
                else
                {
                    ValidationError = string.Join("\n", combinedResult.ErrorMessages);
                    HasValidationErrors = true;
                }
            }
            catch (Exception ex)
            {
                ValidationError = $"유효성 검사 중 오류가 발생했습니다: {ex.Message}";
                HasValidationErrors = true;
            }
        }

        /// <summary>
        /// 파일을 생성할 수 있는지 확인합니다.
        /// </summary>
        /// <returns>생성 가능하면 true</returns>
        private bool CanCreateFile()
        {
            return !HasValidationErrors &&
                   !string.IsNullOrWhiteSpace(GeneratedFileName) &&
                   !string.IsNullOrWhiteSpace(SelectedOutputPath) &&
                   !string.IsNullOrWhiteSpace(SelectedTitle) &&
                   !string.IsNullOrWhiteSpace(SelectedExtension);
        }

        /// <summary>
        /// 파일을 생성합니다.
        /// </summary>
        private async Task CreateFileAsync()
        {
            try
            {
                IsWorking = true;
                StatusMessage = "파일을 생성하는 중...";

                var request = CreateFileRequest();
                if (request == null)
                {
                    StatusMessage = "파일 생성 요청을 만들 수 없습니다.";
                    return;
                }

                // 파일 덮어쓰기 확인 (체크박스 상태를 고려한 경로 사용)
                var fullPath = _fileGeneratorService.GetFullFilePath(request, IsDateTimeEnabled, IsAbbreviationEnabled, IsTitleEnabled, IsSuffixEnabled);
                if (System.IO.File.Exists(fullPath))
                {
                    var overwriteConfirmed = await DialogHelper.ShowFileOverwriteConfirmAsync(
                        System.IO.Path.GetFileName(fullPath),
                        fullPath);
                    
                    if (!overwriteConfirmed)
                    {
                        StatusMessage = "파일 생성이 취소되었습니다.";
                        return;
                    }
                }

                // 문자열 교체 규칙이 있으면 교체 기능이 포함된 메서드 사용
                var result = StringReplacements.Any(r => r.IsEnabled) 
                    ? await _fileGeneratorService.CreateFileWithStringReplacementAsync(
                        request, StringReplacements.ToList(), SelectedDateTime,
                        IsDateTimeEnabled, IsAbbreviationEnabled, IsTitleEnabled, IsSuffixEnabled)
                    : await _fileGeneratorService.CreateFileAsync(
                        request, IsDateTimeEnabled, IsAbbreviationEnabled, IsTitleEnabled, IsSuffixEnabled);
                if (result.Success)
                {
                    StatusMessage = $"파일 생성 완료: {result.FileName}";
                    
                    // 성공 대화상자 표시 및 사용자 선택 처리
                    var userChoice = DialogHelper.ShowFileCreationSuccess(
                        result.FileName, 
                        result.FilePath, 
                        result.FileSize, 
                        result.UsedTemplate);
                    
                    // 사용자 선택에 따른 추가 작업
                    await HandleFileCreationChoice(userChoice, result.FilePath);
                    
                    // 사용 통계 업데이트
                    await UpdateUsageStatistics();
                }
                else
                {
                    StatusMessage = $"파일 생성 실패: {result.ErrorMessage}";
                    DialogHelper.ShowError($"파일 생성에 실패했습니다.\n\n오류: {result.ErrorMessage}", "파일 생성 실패");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일 생성 중 오류 발생: {ex.Message}";
                DialogHelper.ShowError(ex, "파일 생성 중 오류가 발생했습니다.", "파일 생성 오류", true);
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 설정 창을 엽니다.
        /// </summary>
        private void OpenSettings()
        {
            try
            {
                var settingsWindow = new CreateNewFile.Views.SettingsWindow();
                var settingsViewModel = new SettingsViewModel(_settingsService);
                settingsWindow.DataContext = settingsViewModel;
                
                // 메인 윈도우 중앙에 위치시키기
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    settingsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                    
                    // 메인 윈도우 중앙에 설정 윈도우 배치
                    var mainLeft = mainWindow.Left;
                    var mainTop = mainWindow.Top;
                    var mainWidth = mainWindow.ActualWidth;
                    var mainHeight = mainWindow.ActualHeight;
                    
                    settingsWindow.Left = mainLeft + (mainWidth - settingsWindow.Width) / 2;
                    settingsWindow.Top = mainTop + (mainHeight - settingsWindow.Height) / 2;
                    
                    // 설정 윈도우가 화면을 벗어나지 않도록 조정
                    var screenWidth = SystemParameters.PrimaryScreenWidth;
                    var screenHeight = SystemParameters.PrimaryScreenHeight;
                    
                    if (settingsWindow.Left < 0) settingsWindow.Left = 0;
                    if (settingsWindow.Top < 0) settingsWindow.Top = 0;
                    if (settingsWindow.Left + settingsWindow.Width > screenWidth)
                        settingsWindow.Left = screenWidth - settingsWindow.Width;
                    if (settingsWindow.Top + settingsWindow.Height > screenHeight)
                        settingsWindow.Top = screenHeight - settingsWindow.Height;
                }
                
                var result = settingsWindow.ShowDialog();
                
                // 설정 창이 닫혔으므로 콤보박스를 갱신 (OK/Cancel 상관없이 변경사항이 있을 수 있음)
                _ = RefreshComboBoxesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"설정 창 열기 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "설정 창을 열 수 없습니다.", "설정 오류");
            }
        }

        /// <summary>
        /// 출력 경로를 선택합니다.
        /// </summary>
        private void BrowseOutputPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "파일을 생성할 폴더를 선택하세요.",
                ShowNewFolderButton = true,
                SelectedPath = SelectedOutputPath
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedOutputPath = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 템플릿 경로를 선택합니다.
        /// </summary>
        private void BrowseTemplatePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "템플릿 파일을 선택하세요.",
                Filter = "모든 파일 (*.*)|*.*|텍스트 파일 (*.txt)|*.txt|마크다운 파일 (*.md)|*.md",
                FilterIndex = 1,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (!string.IsNullOrWhiteSpace(SelectedTemplatePath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(SelectedTemplatePath);
                dialog.FileName = Path.GetFileName(SelectedTemplatePath);
            }

            if (dialog.ShowDialog() == true)
            {
                SelectedTemplatePath = dialog.FileName;
            }
        }

        /// <summary>
        /// 템플릿 파일을 편집 가능한지 확인합니다.
        /// </summary>
        private bool CanOpenTemplateFile()
        {
            return !string.IsNullOrWhiteSpace(SelectedTemplatePath)
                   && File.Exists(SelectedTemplatePath);
        }

        /// <summary>
        /// 템플릿 파일을 기본 연결 프로그램으로 엽니다.
        /// </summary>
        private void OpenTemplateFile()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedTemplatePath))
                {
                    DialogHelper.ShowError("템플릿 파일이 선택되지 않았습니다.");
                    return;
                }

                if (!File.Exists(SelectedTemplatePath))
                {
                    DialogHelper.ShowError("템플릿 파일을 찾을 수 없습니다.\n\n경로: " + SelectedTemplatePath);
                    return;
                }

                // 기본 연결 프로그램으로 파일 열기
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = SelectedTemplatePath,
                    UseShellExecute = true
                });

                StatusMessage = $"템플릿 파일을 열었습니다: {Path.GetFileName(SelectedTemplatePath)}";
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"템플릿 파일을 열 수 없습니다.\n\n{ex.Message}", "파일 열기 오류");
                StatusMessage = "템플릿 파일 열기 실패";
            }
        }

        /// <summary>
        /// 설정 폴더를 윈도우 탐색기로 엽니다.
        /// </summary>
        private void OpenSettingsFolder()
        {
            try
            {
                string settingsFolderPath = _settingsService.GetSettingsFolderPath();

                // Windows 탐색기로 폴더 열기
                System.Diagnostics.Process.Start("explorer.exe", settingsFolderPath);

                StatusMessage = "설정 폴더를 열었습니다.";
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"설정 폴더를 열 수 없습니다: {ex.Message}");
                StatusMessage = "설정 폴더 열기 실패";
            }
        }

        /// <summary>
        /// 출력 폴더를 열 수 있는지 확인합니다.
        /// </summary>
        private bool CanOpenOutputFolder()
        {
            return !string.IsNullOrWhiteSpace(SelectedOutputPath) && Directory.Exists(SelectedOutputPath);
        }

        /// <summary>
        /// 출력 폴더를 윈도우 탐색기로 엽니다.
        /// </summary>
        private void OpenOutputFolder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedOutputPath))
                {
                    DialogHelper.ShowError("출력 폴더가 선택되지 않았습니다.");
                    return;
                }

                if (!Directory.Exists(SelectedOutputPath))
                {
                    DialogHelper.ShowError($"출력 폴더를 찾을 수 없습니다.\n\n경로: {SelectedOutputPath}");
                    return;
                }

                // Windows 탐색기로 폴더 열기
                System.Diagnostics.Process.Start("explorer.exe", SelectedOutputPath);

                StatusMessage = "출력 폴더를 열었습니다.";
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"출력 폴더를 열 수 없습니다.\n\n{ex.Message}", "폴더 열기 오류");
                StatusMessage = "출력 폴더 열기 실패";
            }
        }

        /// <summary>
        /// 템플릿 폴더를 열 수 있는지 확인합니다.
        /// </summary>
        private bool CanOpenTemplateFolder()
        {
            if (string.IsNullOrWhiteSpace(SelectedTemplatePath))
                return false;

            var templateFolder = Path.GetDirectoryName(SelectedTemplatePath);
            return !string.IsNullOrWhiteSpace(templateFolder) && Directory.Exists(templateFolder);
        }

        /// <summary>
        /// 템플릿 파일이 있는 폴더를 윈도우 탐색기로 엽니다.
        /// </summary>
        private void OpenTemplateFolder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SelectedTemplatePath))
                {
                    DialogHelper.ShowError("템플릿 파일 경로가 선택되지 않았습니다.");
                    return;
                }

                var templateFolder = Path.GetDirectoryName(SelectedTemplatePath);
                if (string.IsNullOrWhiteSpace(templateFolder))
                {
                    DialogHelper.ShowError("템플릿 파일 경로가 올바르지 않습니다.");
                    return;
                }

                if (!Directory.Exists(templateFolder))
                {
                    DialogHelper.ShowError($"템플릿 폴더를 찾을 수 없습니다.\n\n경로: {templateFolder}");
                    return;
                }

                // Windows 탐색기로 폴더 열기
                System.Diagnostics.Process.Start("explorer.exe", templateFolder);

                StatusMessage = "템플릿 폴더를 열었습니다.";
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"템플릿 폴더를 열 수 없습니다.\n\n{ex.Message}", "폴더 열기 오류");
                StatusMessage = "템플릿 폴더 열기 실패";
            }
        }

        /// <summary>
        /// FileCreationRequest 객체를 생성합니다.
        /// </summary>
        private FileCreationRequest? CreateFileRequest()
        {
            try
            {
                return new FileCreationRequest
                {
                    DateTime = SelectedDateTime,
                    Abbreviation = SelectedAbbreviation?.Trim() ?? string.Empty,
                    Title = SelectedTitle?.Trim() ?? string.Empty,
                    Suffix = SelectedSuffix?.Trim() ?? string.Empty,
                    Extension = SelectedExtension?.Trim() ?? string.Empty,
                    OutputPath = SelectedOutputPath?.Trim() ?? string.Empty,
                    TemplatePath = SelectedTemplatePath?.Trim() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 파일 생성 완료 후 사용자 선택을 처리합니다.
        /// </summary>
        /// <param name="choice">사용자 선택</param>
        /// <param name="filePath">생성된 파일 경로</param>
        private async Task HandleFileCreationChoice(System.Windows.MessageBoxResult choice, string filePath)
        {
            try
            {
                switch (choice)
                {
                    case System.Windows.MessageBoxResult.Yes: // 파일 열기
                        await Task.Run(() =>
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = filePath,
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    DialogHelper.ShowError($"파일을 열 수 없습니다.\n\n{ex.Message}", "파일 열기 오류");
                                });
                            }
                        });
                        break;

                    case System.Windows.MessageBoxResult.No: // 폴더 열기
                        await Task.Run(() =>
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "explorer.exe",
                                    Arguments = $"/select,\"{filePath}\"",
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    DialogHelper.ShowError($"폴더를 열 수 없습니다.\n\n{ex.Message}", "폴더 열기 오류");
                                });
                            }
                        });
                        break;

                    default: // 취소 또는 닫기
                        // 아무 작업 안함
                        break;
                }
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError(ex, "파일 또는 폴더를 여는 중 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 사용 통계를 업데이트합니다.
        /// </summary>
        private async Task UpdateUsageStatistics()
        {
            try
            {
                // 사용된 항목들의 통계 업데이트
                var tasks = new List<Task>();

                var abbreviation = Abbreviations.FirstOrDefault(a => a.Value == SelectedAbbreviation);
                if (abbreviation != null)
                    tasks.Add(_settingsService.MarkItemAsUsedAsync(PresetType.Abbreviation, abbreviation.Id));

                var title = Titles.FirstOrDefault(t => t.Value == SelectedTitle);
                if (title != null)
                    tasks.Add(_settingsService.MarkItemAsUsedAsync(PresetType.Title, title.Id));

                if (!string.IsNullOrWhiteSpace(SelectedSuffix))
                {
                    var suffix = Suffixes.FirstOrDefault(s => s.Value == SelectedSuffix);
                    if (suffix != null)
                        tasks.Add(_settingsService.MarkItemAsUsedAsync(PresetType.Suffix, suffix.Id));
                }

                var extension = Extensions.FirstOrDefault(e => e.Value == SelectedExtension);
                if (extension != null)
                    tasks.Add(_settingsService.MarkItemAsUsedAsync(PresetType.Extension, extension.Id));

                var outputPath = OutputPaths.FirstOrDefault(p => p.Value == SelectedOutputPath);
                if (outputPath != null)
                    tasks.Add(_settingsService.MarkItemAsUsedAsync(PresetType.OutputPath, outputPath.Id));

                if (!string.IsNullOrWhiteSpace(SelectedTemplatePath))
                {
                    var templatePath = TemplatePaths.FirstOrDefault(t => t.Value == SelectedTemplatePath);
                    if (templatePath != null)
                        tasks.Add(_settingsService.MarkItemAsUsedAsync(PresetType.TemplatePath, templatePath.Id));
                }

                await Task.WhenAll(tasks);
                
                // 마지막 선택 항목들 저장
                await SaveLastSelectedItemsAsync();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 마지막 선택된 항목들을 저장합니다.
        /// </summary>
        private async Task SaveLastSelectedItemsAsync()
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();

                // 마지막 선택 정보 업데이트
                settings.LastSelectedDateTime = SelectedDateTime;
                settings.LastSelectedAbbreviation = SelectedAbbreviation;
                settings.LastSelectedTitle = SelectedTitle;
                settings.LastSelectedSuffix = SelectedSuffix;
                settings.LastSelectedExtension = SelectedExtension;
                settings.LastSelectedOutputPath = SelectedOutputPath;
                settings.LastSelectedTemplatePath = SelectedTemplatePath;

                // 문자열 교체 규칙 저장
                settings.LastStringReplacements.Clear();
                settings.LastStringReplacements.AddRange(
                    StringReplacements.Select(x => (StringReplacementRule)x.Clone()).ToList());

                await _settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception)
            {
                // 저장 실패 시 무시
            }
        }

        /// <summary>
        /// 애플리케이션 종료 시 현재 설정을 저장합니다.
        /// </summary>
        public async Task SaveCurrentStateAsync()
        {
            await SaveLastSelectedItemsAsync();
        }

        /// <summary>
        /// SettingsService의 캐시를 무효화합니다.
        /// 외부에서 설정 파일을 직접 수정한 경우 호출하여 최신 설정을 다시 로드하도록 합니다.
        /// </summary>
        public void ClearSettingsCache()
        {
            _settingsService.ClearCache();
        }

        #region 파일정보 관련 메서드

        /// <summary>
        /// 파일정보를 불러옵니다.
        /// </summary>
        private async Task LoadFileInfoAsync()
        {
            try
            {
                if (SelectedFileInfo == null)
                {
                    StatusMessage = "선택된 파일정보가 없습니다.";
                    return;
                }

                var fileInfo = SelectedFileInfo;
                
                // 현재 UI에 파일정보 데이터 적용
                SelectedDateTime = fileInfo.DateTime;
                SelectedAbbreviation = fileInfo.Abbreviation;
                SelectedTitle = fileInfo.Title;
                SelectedSuffix = fileInfo.Suffix;
                SelectedExtension = fileInfo.Extension;
                SelectedOutputPath = fileInfo.OutputPath;
                SelectedTemplatePath = fileInfo.TemplatePath;

                // 체크박스 상태 적용
                IsDateTimeEnabled = fileInfo.IsDateTimeEnabled;
                IsAbbreviationEnabled = fileInfo.IsAbbreviationEnabled;
                IsTitleEnabled = fileInfo.IsTitleEnabled;
                IsSuffixEnabled = fileInfo.IsSuffixEnabled;

                // 문자열 교체 규칙 적용
                StringReplacements.Clear();
                foreach (var rule in fileInfo.StringReplacements)
                {
                    StringReplacements.Add(rule);
                }

                // 사용 통계 업데이트
                await _fileInfoService.MarkFileInfoModelAsUsedAsync(fileInfo.Id);

                StatusMessage = $"파일정보 '{fileInfo.Name}' 불러오기 완료";
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일정보 불러오기 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "파일정보를 불러올 수 없습니다.");
            }
        }

        /// <summary>
        /// 현재 상태를 파일정보로 저장합니다.
        /// </summary>
        private async Task SaveFileInfoAsync()
        {
            try
            {
                // 파일명 미리보기에서 확장자를 제외한 이름을 기본값으로 사용
                string defaultName;
                if (!string.IsNullOrWhiteSpace(GeneratedFileName))
                {
                    // 확장자 제거
                    defaultName = Path.GetFileNameWithoutExtension(GeneratedFileName);
                }
                else
                {
                    defaultName = GenerateDefaultFileInfoName();
                }

                IsEditingFileInfoName = true;
                
                // 사용자 입력 대기 후 실제 저장 수행
                var result = DialogHelper.ShowInputDialog("파일정보 저장", "파일정보 이름을 입력하세요:", defaultName);
                IsEditingFileInfoName = false;

                if (result.IsConfirmed && !string.IsNullOrWhiteSpace(result.InputText))
                {
                    var fileInfo = new Models.FileInfoModel
                    {
                        Name = result.InputText.Trim(),
                        DateTime = SelectedDateTime,
                        Abbreviation = SelectedAbbreviation,
                        Title = SelectedTitle,
                        Suffix = SelectedSuffix,
                        Extension = SelectedExtension,
                        OutputPath = SelectedOutputPath,
                        TemplatePath = SelectedTemplatePath,
                        IsDateTimeEnabled = IsDateTimeEnabled,
                        IsAbbreviationEnabled = IsAbbreviationEnabled,
                        IsTitleEnabled = IsTitleEnabled,
                        IsSuffixEnabled = IsSuffixEnabled,
                        StringReplacements = StringReplacements.ToList()
                    };

                    var success = await _fileInfoService.SaveFileInfoAsync(fileInfo);
                    if (success)
                    {
                        await RefreshSavedFileInfosAsync();
                        StatusMessage = $"파일정보 '{fileInfo.Name}' 저장 완료";
                    }
                    else
                    {
                        StatusMessage = "파일정보 저장 실패";
                        DialogHelper.ShowError("파일정보를 저장할 수 없습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일정보 저장 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "파일정보를 저장할 수 없습니다.");
            }
        }

        /// <summary>
        /// 파일정보 관리 창을 엽니다.
        /// </summary>
        private void ManageFileInfos()
        {
            try
            {
                var managerWindow = new CreateNewFile.Views.FileInfoManagerWindow();
                var managerViewModel = new FileInfoManagerViewModel(_fileInfoService);
                managerWindow.DataContext = managerViewModel;
                
                // 메인 윈도우 중앙에 위치시키기
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    managerWindow.Owner = mainWindow;
                    managerWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                }
                
                var result = managerWindow.ShowDialog();
                
                // 관리 창이 닫혔으므로 파일정보 목록을 갱신
                _ = RefreshSavedFileInfosAsync();
                
                StatusMessage = "파일정보 관리가 완료되었습니다.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일정보 관리 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "파일정보 관리창을 열 수 없습니다.");
            }
        }

        /// <summary>
        /// 기본 파일정보 이름을 생성합니다.
        /// </summary>
        private string GenerateDefaultFileInfoName()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(SelectedAbbreviation))
                parts.Add(SelectedAbbreviation);
            if (!string.IsNullOrWhiteSpace(SelectedTitle))
                parts.Add(SelectedTitle);
            if (!string.IsNullOrWhiteSpace(SelectedSuffix))
                parts.Add(SelectedSuffix);

            var baseName = parts.Any() ? string.Join("_", parts) : "새파일정보";
            return $"{baseName}_{DateTime.Now:yyyyMMdd_HHmm}";
        }

        /// <summary>
        /// 저장된 파일정보 목록을 새로고침합니다.
        /// </summary>
        private async Task RefreshSavedFileInfosAsync()
        {
            try
            {
                var fileInfos = await _fileInfoService.GetAllFileInfosAsync();
                SavedFileInfos.Clear();
                foreach (var info in fileInfos.OrderBy(f => f.Name))
                {
                    SavedFileInfos.Add(info);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"파일정보 목록 새로고침 오류: {ex.Message}";
            }
        }

        #endregion

        #region 문자열 교체 관련 메서드

        /// <summary>
        /// 새로운 문자열 교체 규칙을 추가합니다.
        /// </summary>
        private void AddStringReplacement()
        {
            try
            {
                var newRule = new StringReplacementRule
                {
                    SearchText = "",
                    ReplaceText = "",
                    Description = "새 규칙"
                };

                StringReplacements.Add(newRule);
                SelectedStringReplacement = newRule;
                StatusMessage = "새 문자열 교체 규칙이 추가되었습니다.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"규칙 추가 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 선택된 문자열 교체 규칙을 제거합니다.
        /// </summary>
        private void RemoveStringReplacement()
        {
            try
            {
                if (SelectedStringReplacement == null)
                {
                    StatusMessage = "제거할 규칙을 선택하세요.";
                    return;
                }

                var result = System.Windows.MessageBox.Show(
                    $"선택된 규칙을 제거하시겠습니까?\n\n'{SelectedStringReplacement.SearchText}' → '{SelectedStringReplacement.ReplaceText}'",
                    "규칙 제거 확인",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    StringReplacements.Remove(SelectedStringReplacement);
                    SelectedStringReplacement = null;
                    StatusMessage = "문자열 교체 규칙이 제거되었습니다.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"규칙 제거 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 선택된 문자열 교체 규칙을 위로 이동합니다.
        /// </summary>
        private void MoveUpStringReplacement()
        {
            try
            {
                if (SelectedStringReplacement == null)
                {
                    StatusMessage = "이동할 규칙을 선택하세요.";
                    return;
                }

                var index = StringReplacements.IndexOf(SelectedStringReplacement);
                if (index > 0)
                {
                    StringReplacements.Move(index, index - 1);
                    StatusMessage = "규칙이 위로 이동되었습니다.";
                }
                else
                {
                    StatusMessage = "이미 첫 번째 규칙입니다.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"규칙 이동 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 선택된 문자열 교체 규칙을 아래로 이동합니다.
        /// </summary>
        private void MoveDownStringReplacement()
        {
            try
            {
                if (SelectedStringReplacement == null)
                {
                    StatusMessage = "이동할 규칙을 선택하세요.";
                    return;
                }

                var index = StringReplacements.IndexOf(SelectedStringReplacement);
                if (index < StringReplacements.Count - 1)
                {
                    StringReplacements.Move(index, index + 1);
                    StatusMessage = "규칙이 아래로 이동되었습니다.";
                }
                else
                {
                    StatusMessage = "이미 마지막 규칙입니다.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"규칙 이동 오류: {ex.Message}";
            }
        }

        /// <summary>
        /// 문자열 교체를 수행합니다.
        /// </summary>
        /// <param name="content">원본 내용</param>
        /// <param name="fileInfoDateTime">파일정보 날짜/시간 (동적 교체용)</param>
        /// <returns>교체된 내용</returns>
        public string ApplyStringReplacements(string content, DateTime? fileInfoDateTime = null)
        {
            if (string.IsNullOrEmpty(content) || !StringReplacements.Any())
                return content;

            var result = content;
            var baseDateTime = fileInfoDateTime ?? SelectedDateTime;
            
            foreach (var rule in StringReplacements.Where(r => r.IsEnabled))
            {
                try
                {
                    if (string.IsNullOrEmpty(rule.SearchText))
                        continue;

                    if (rule.UseDynamicReplacement)
                    {
                        // 동적 플레이스홀더 처리
                        result = ProcessDynamicPlaceholders(result, rule, baseDateTime);
                    }
                    else if (rule.UseRegex)
                    {
                        var options = rule.IsCaseSensitive 
                            ? System.Text.RegularExpressions.RegexOptions.None
                            : System.Text.RegularExpressions.RegexOptions.IgnoreCase;
                        
                        var regex = new System.Text.RegularExpressions.Regex(rule.SearchText, options);
                        result = regex.Replace(result, rule.ReplaceText);
                    }
                    else
                    {
                        var comparison = rule.IsCaseSensitive 
                            ? StringComparison.Ordinal
                            : StringComparison.OrdinalIgnoreCase;
                        
                        result = result.Replace(rule.SearchText, rule.ReplaceText, comparison);
                    }
                }
                catch (Exception ex)
                {
                    // 정규식 오류 등이 발생해도 계속 진행
                    System.Diagnostics.Debug.WriteLine($"문자열 교체 규칙 적용 오류: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// 동적 플레이스홀더를 처리합니다.
        /// </summary>
        /// <param name="content">원본 내용</param>
        /// <param name="rule">교체 규칙</param>
        /// <param name="baseDateTime">기준 날짜/시간</param>
        /// <returns>처리된 내용</returns>
        private string ProcessDynamicPlaceholders(string content, StringReplacementRule rule, DateTime baseDateTime)
        {
            var searchText = rule.SearchText.Trim();
            var replaceText = rule.ReplaceText.Trim();
            
            // 교체 문자열에서 동적 패턴을 찾아서 처리
            var processedReplaceText = ProcessDynamicPatterns(replaceText, baseDateTime);
            
            // 일반적인 문자열 교체 수행
            var comparison = rule.IsCaseSensitive 
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            
            return content.Replace(searchText, processedReplaceText, comparison);
        }

        /// <summary>
        /// 문자열에서 동적 패턴을 찾아 처리합니다.
        /// </summary>
        /// <param name="text">처리할 텍스트</param>
        /// <param name="baseDateTime">기준 날짜/시간</param>
        /// <returns>동적 패턴이 처리된 텍스트</returns>
        private string ProcessDynamicPatterns(string text, DateTime baseDateTime)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            
            var result = text;
            
            // YYYYMMDD_HHMMSS 패턴 처리
            var pattern1 = @"YYYYMMDD_HHMMSS([+-]\d+)?";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern1, match =>
            {
                var dateTime = baseDateTime;
                var offsetText = match.Groups[1].Value;
                
                if (!string.IsNullOrEmpty(offsetText))
                {
                    var offset = int.Parse(offsetText);
                    dateTime = dateTime.AddMinutes(offset);
                }
                
                return dateTime.ToString("yyyyMMdd_HHmmss");
            });
            
            // YYYYMMDD_HHMM 패턴 처리
            var pattern2 = @"YYYYMMDD_HHMM([+-]\d+)?";
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern2, match =>
            {
                var dateTime = baseDateTime;
                var offsetText = match.Groups[1].Value;
                
                if (!string.IsNullOrEmpty(offsetText))
                {
                    var offset = int.Parse(offsetText);
                    dateTime = dateTime.AddMinutes(offset);
                }
                
                return dateTime.ToString("yyyyMMdd_HHmm");
            });
            
            return result;
        }

        #endregion

        #region 프로젝트 설정 관련 메서드

        /// <summary>
        /// 프로젝트 설정을 로드하여 UI에 적용합니다.
        /// </summary>
        /// <param name="config">로드할 프로젝트 설정</param>
        public async Task LoadProjectConfigAsync(ProjectConfig config)
        {
            try
            {
                IsWorking = true;
                StatusMessage = "프로젝트 설정을 로드하는 중...";

                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                // 유효성 검사
                var validation = config.Validate();
                if (!validation.IsValid)
                    throw new InvalidOperationException($"프로젝트 설정이 유효하지 않습니다: {validation.ErrorMessage}");

                // 먼저 일반 설정 로드 (Preset 항목들)
                await LoadDataAsync();

                // 프로젝트 설정 적용
                var fileInfo = config.FileInfo;

                SelectedDateTime = fileInfo.DateTime;
                SelectedAbbreviation = fileInfo.Abbreviation;
                SelectedTitle = fileInfo.Title;
                SelectedSuffix = fileInfo.Suffix;
                SelectedExtension = fileInfo.Extension;
                SelectedOutputPath = config.OutputPath;
                SelectedTemplatePath = config.TemplatePath;

                // 체크박스 상태 적용
                IsDateTimeEnabled = fileInfo.IsDateTimeEnabled;
                IsAbbreviationEnabled = fileInfo.IsAbbreviationEnabled;
                IsTitleEnabled = fileInfo.IsTitleEnabled;
                IsSuffixEnabled = fileInfo.IsSuffixEnabled;

                // 문자열 교체 규칙 적용
                StringReplacements.Clear();
                foreach (var rule in config.StringReplacements)
                {
                    StringReplacements.Add((StringReplacementRule)rule.Clone());
                }

                StatusMessage = $"프로젝트 '{config.Name}' 로드 완료";
                // 성공 시 대화상자 표시 안 함
            }
            catch (Exception ex)
            {
                StatusMessage = $"프로젝트 설정 로드 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "프로젝트 설정을 로드할 수 없습니다.");
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 현재 상태를 프로젝트 설정 파일로 저장합니다.
        /// 템플릿 폴더에 템플릿 파일명.cnfjson 형식으로 자동 저장합니다.
        /// </summary>
        private async Task SaveProjectConfigAsync()
        {
            try
            {
                // 템플릿 파일이 선택되어 있는지 확인
                if (string.IsNullOrWhiteSpace(SelectedTemplatePath) || !File.Exists(SelectedTemplatePath))
                {
                    StatusMessage = "템플릿 파일을 먼저 선택해주세요.";
                    DialogHelper.ShowError("템플릿 파일이 선택되지 않았습니다.\n프로젝트 설정을 저장하려면 템플릿 파일을 선택해주세요.");
                    return;
                }

                IsWorking = true;
                StatusMessage = "프로젝트 설정을 저장하는 중...";

                // 템플릿 파일명과 폴더 경로 추출
                var templateFileName = Path.GetFileNameWithoutExtension(SelectedTemplatePath);
                var templateFolder = Path.GetDirectoryName(SelectedTemplatePath);

                if (string.IsNullOrWhiteSpace(templateFolder))
                {
                    throw new InvalidOperationException("템플릿 폴더 경로를 확인할 수 없습니다.");
                }

                // .cnfjson 파일 경로 생성
                var configFilePath = Path.Combine(templateFolder, $"{templateFileName}.cnfjson");

                // 파일이 이미 존재하는 경우 덮어쓰기 확인
                if (File.Exists(configFilePath))
                {
                    var overwrite = System.Windows.MessageBox.Show(
                        $"'{Path.GetFileName(configFilePath)}' 파일이 이미 존재합니다.\n\n덮어쓰시겠습니까?",
                        "파일 덮어쓰기 확인",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);

                    if (overwrite != System.Windows.MessageBoxResult.Yes)
                    {
                        StatusMessage = "프로젝트 설정 저장이 취소되었습니다.";
                        return;
                    }
                }

                // 현재 상태를 프로젝트 설정으로 변환
                var config = _projectConfigService.CreateProjectConfigFromViewModel(this);
                config.Name = templateFileName;
                config.Description = templateFileName; // 템플릿 파일명을 설명으로 사용

                // 프로젝트 설정 저장
                var success = await _projectConfigService.SaveProjectConfigAsync(config, configFilePath);

                if (success)
                {
                    StatusMessage = $"프로젝트 설정 저장 완료: {Path.GetFileName(configFilePath)}";
                    // 성공 시 대화상자 표시 안 함
                }
                else
                {
                    StatusMessage = "프로젝트 설정 저장 실패";
                    DialogHelper.ShowError("프로젝트 설정을 저장할 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"프로젝트 설정 저장 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "프로젝트 설정을 저장할 수 없습니다.");
            }
            finally
            {
                IsWorking = false;
            }
        }

        /// <summary>
        /// 프로젝트 설정 파일을 열어서 로드합니다.
        /// </summary>
        private async Task OpenProjectConfigAsync()
        {
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "프로젝트 설정 열기",
                    Filter = "CreateNewFile 프로젝트 파일 (*.cnfjson)|*.cnfjson",
                    DefaultExt = ".cnfjson",
                    CheckFileExists = true,
                    CheckPathExists = true
                };

                // 템플릿 파일이 선택되어 있으면 템플릿 폴더를 초기 폴더로 설정
                if (!string.IsNullOrWhiteSpace(SelectedTemplatePath) && File.Exists(SelectedTemplatePath))
                {
                    var templateFolder = Path.GetDirectoryName(SelectedTemplatePath);
                    if (!string.IsNullOrWhiteSpace(templateFolder) && Directory.Exists(templateFolder))
                    {
                        openDialog.InitialDirectory = templateFolder;
                    }
                }

                if (openDialog.ShowDialog() != true)
                    return;

                IsWorking = true;
                StatusMessage = "프로젝트 설정을 로드하는 중...";

                // 프로젝트 설정 로드
                var config = await _projectConfigService.LoadProjectConfigAsync(openDialog.FileName);

                if (config != null)
                {
                    await LoadProjectConfigAsync(config);
                }
                else
                {
                    StatusMessage = "프로젝트 설정 로드 실패";
                    DialogHelper.ShowError("프로젝트 설정을 로드할 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"프로젝트 설정 열기 오류: {ex.Message}";
                DialogHelper.ShowError(ex, "프로젝트 설정을 열 수 없습니다.");
            }
            finally
            {
                IsWorking = false;
            }
        }

        #endregion
        #endregion
    }
}