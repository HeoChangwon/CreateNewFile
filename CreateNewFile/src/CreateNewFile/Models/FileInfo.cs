using System.ComponentModel;
using Newtonsoft.Json;

namespace CreateNewFile.Models
{
    /// <summary>
    /// 저장/불러오기 가능한 파일 정보를 나타내는 모델 클래스
    /// </summary>
    public class FileInfoModel : INotifyPropertyChanged, ICloneable
    {
        private string _name = string.Empty;
        private DateTime _dateTime = DateTime.Now;
        private string _abbreviation = string.Empty;
        private string _title = string.Empty;
        private string _suffix = string.Empty;
        private string _extension = string.Empty;
        private string _outputPath = string.Empty;
        private string _templatePath = string.Empty;
        private bool _isDateTimeEnabled = true;
        private bool _isAbbreviationEnabled = true;
        private bool _isTitleEnabled = true;
        private bool _isSuffixEnabled = true;

        /// <summary>
        /// 파일 정보의 고유 식별자
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 파일 정보의 이름 (사용자가 지정하는 구분용 이름)
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 생성 날짜/시간
        /// </summary>
        public DateTime DateTime
        {
            get => _dateTime;
            set
            {
                if (_dateTime != value)
                {
                    _dateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 약어
        /// </summary>
        public string Abbreviation
        {
            get => _abbreviation;
            set
            {
                if (_abbreviation != value)
                {
                    _abbreviation = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 제목
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 접미어
        /// </summary>
        public string Suffix
        {
            get => _suffix;
            set
            {
                if (_suffix != value)
                {
                    _suffix = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 확장자
        /// </summary>
        public string Extension
        {
            get => _extension;
            set
            {
                if (_extension != value)
                {
                    _extension = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 출력 폴더 경로
        /// </summary>
        public string OutputPath
        {
            get => _outputPath;
            set
            {
                if (_outputPath != value)
                {
                    _outputPath = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 템플릿 파일 경로
        /// </summary>
        public string TemplatePath
        {
            get => _templatePath;
            set
            {
                if (_templatePath != value)
                {
                    _templatePath = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 날짜/시간 항목 활성화 상태
        /// </summary>
        public bool IsDateTimeEnabled
        {
            get => _isDateTimeEnabled;
            set
            {
                if (_isDateTimeEnabled != value)
                {
                    _isDateTimeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 약어 항목 활성화 상태
        /// </summary>
        public bool IsAbbreviationEnabled
        {
            get => _isAbbreviationEnabled;
            set
            {
                if (_isAbbreviationEnabled != value)
                {
                    _isAbbreviationEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 제목 항목 활성화 상태
        /// </summary>
        public bool IsTitleEnabled
        {
            get => _isTitleEnabled;
            set
            {
                if (_isTitleEnabled != value)
                {
                    _isTitleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 접미어 항목 활성화 상태
        /// </summary>
        public bool IsSuffixEnabled
        {
            get => _isSuffixEnabled;
            set
            {
                if (_isSuffixEnabled != value)
                {
                    _isSuffixEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 템플릿 내 문자열 교체 규칙 목록
        /// </summary>
        public List<StringReplacementRule> StringReplacements { get; set; } = new();

        /// <summary>
        /// 생성된 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 마지막 수정 시간
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 사용 횟수
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// 마지막 사용 시간
        /// </summary>
        public DateTime LastUsed { get; set; } = DateTime.Now;

        /// <summary>
        /// 즐겨찾기 여부
        /// </summary>
        public bool IsFavorite { get; set; } = false;

        /// <summary>
        /// 태그 목록
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 설명
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// FileCreationRequest로 변환합니다.
        /// </summary>
        /// <returns>FileCreationRequest 객체</returns>
        public FileCreationRequest ToFileCreationRequest()
        {
            return new FileCreationRequest
            {
                DateTime = this.DateTime,
                Abbreviation = this.Abbreviation,
                Title = this.Title,
                Suffix = this.Suffix,
                Extension = this.Extension,
                OutputPath = this.OutputPath,
                TemplatePath = this.TemplatePath
            };
        }

        /// <summary>
        /// FileCreationRequest에서 데이터를 로드합니다.
        /// </summary>
        /// <param name="request">FileCreationRequest 객체</param>
        public void LoadFromFileCreationRequest(FileCreationRequest request)
        {
            DateTime = request.DateTime;
            Abbreviation = request.Abbreviation;
            Title = request.Title;
            Suffix = request.Suffix;
            Extension = request.Extension;
            OutputPath = request.OutputPath;
            TemplatePath = request.TemplatePath;
            ModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효성 검사 결과</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return (false, "파일정보 이름이 필요합니다.");

            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Abbreviation))
                return (false, "제목 또는 약어 중 하나는 필요합니다.");

            if (string.IsNullOrWhiteSpace(Extension))
                return (false, "확장자가 필요합니다.");

            if (string.IsNullOrWhiteSpace(OutputPath))
                return (false, "출력 경로가 필요합니다.");

            return (true, string.Empty);
        }

        /// <summary>
        /// 객체를 복사합니다.
        /// </summary>
        /// <returns>복사된 객체</returns>
        public object Clone()
        {
            var cloned = new FileInfoModel
            {
                Id = Guid.NewGuid().ToString(), // 새로운 ID 생성
                Name = Name,
                DateTime = DateTime,
                Abbreviation = Abbreviation,
                Title = Title,
                Suffix = Suffix,
                Extension = Extension,
                OutputPath = OutputPath,
                TemplatePath = TemplatePath,
                IsDateTimeEnabled = IsDateTimeEnabled,
                IsAbbreviationEnabled = IsAbbreviationEnabled,
                IsTitleEnabled = IsTitleEnabled,
                IsSuffixEnabled = IsSuffixEnabled,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                UsageCount = 0,
                LastUsed = DateTime.Now,
                IsFavorite = IsFavorite,
                Description = Description,
                Tags = new List<string>(Tags),
                StringReplacements = StringReplacements.Select(r => r.Clone()).Cast<StringReplacementRule>().ToList()
            };

            return cloned;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 사용 통계를 업데이트합니다.
        /// </summary>
        public void MarkAsUsed()
        {
            UsageCount++;
            LastUsed = DateTime.Now;
            ModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// 파일정보 요약 정보를 반환합니다.
        /// </summary>
        /// <returns>요약 정보</returns>
        public override string ToString()
        {
            var parts = new List<string>();
            if (IsAbbreviationEnabled && !string.IsNullOrWhiteSpace(Abbreviation))
                parts.Add(Abbreviation);
            if (IsTitleEnabled && !string.IsNullOrWhiteSpace(Title))
                parts.Add(Title);
            if (IsSuffixEnabled && !string.IsNullOrWhiteSpace(Suffix))
                parts.Add(Suffix);

            var fileName = string.Join("_", parts) + Extension;
            return $"{Name}: {fileName}";
        }
    }

    /// <summary>
    /// 템플릿 내 문자열 교체 규칙을 나타내는 클래스
    /// </summary>
    public class StringReplacementRule : INotifyPropertyChanged, ICloneable
    {
        private string _searchText = string.Empty;
        private string _replaceText = string.Empty;
        private bool _isEnabled = true;
        private bool _isCaseSensitive = false;
        private bool _useRegex = false;
        private bool _useDynamicReplacement = false;

        /// <summary>
        /// 고유 식별자
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 검색할 문자열
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 교체할 문자열
        /// </summary>
        public string ReplaceText
        {
            get => _replaceText;
            set
            {
                if (_replaceText != value)
                {
                    _replaceText = value ?? string.Empty;
                    OnPropertyChanged();
                    
                    // 동적 패턴이 포함된 경우 자동으로 동적교체 활성화
                    CheckAndEnableDynamicReplacement();
                }
            }
        }

        /// <summary>
        /// 규칙 활성화 여부
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 대소문자 구분 여부
        /// </summary>
        public bool IsCaseSensitive
        {
            get => _isCaseSensitive;
            set
            {
                if (_isCaseSensitive != value)
                {
                    _isCaseSensitive = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 정규식 사용 여부
        /// </summary>
        public bool UseRegex
        {
            get => _useRegex;
            set
            {
                if (_useRegex != value)
                {
                    _useRegex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 동적 교체 사용 여부 (파일정보 날짜/시간 기반)
        /// </summary>
        public bool UseDynamicReplacement
        {
            get => _useDynamicReplacement;
            set
            {
                if (_useDynamicReplacement != value)
                {
                    _useDynamicReplacement = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 설명
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 생성 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 교체 문자열에 동적 패턴이 포함되어 있는지 확인하고 자동으로 동적교체를 활성화합니다.
        /// </summary>
        private void CheckAndEnableDynamicReplacement()
        {
            if (string.IsNullOrEmpty(_replaceText))
            {
                // 교체 문자열이 비어있으면 동적교체 비활성화
                UseDynamicReplacement = false;
                return;
            }

            // 동적 패턴 감지 (YYYYMMDD_HHMMSS 또는 YYYYMMDD_HHMM 패턴)
            var dynamicPatterns = new[]
            {
                @"YYYYMMDD_HHMMSS([+\-]\d+)?",
                @"YYYYMMDD_HHMM([+\-]\d+)?"
            };

            bool hasDynamicPattern = false;
            foreach (var pattern in dynamicPatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(_replaceText, pattern))
                {
                    hasDynamicPattern = true;
                    break;
                }
            }

            // 동적 패턴이 발견되면 자동으로 동적교체 활성화
            if (hasDynamicPattern && !_useDynamicReplacement)
            {
                _useDynamicReplacement = true;
                OnPropertyChanged(nameof(UseDynamicReplacement));
            }
        }

        /// <summary>
        /// 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효성 검사 결과</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return (false, "검색할 문자열이 필요합니다.");

            if (UseRegex)
            {
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex(SearchText);
                }
                catch (System.ArgumentException ex)
                {
                    return (false, $"유효하지 않은 정규식입니다: {ex.Message}");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 객체를 복사합니다.
        /// </summary>
        /// <returns>복사된 객체</returns>
        public object Clone()
        {
            return new StringReplacementRule
            {
                Id = Guid.NewGuid().ToString(), // 새로운 ID 생성
                SearchText = SearchText,
                ReplaceText = ReplaceText,
                IsEnabled = IsEnabled,
                IsCaseSensitive = IsCaseSensitive,
                UseRegex = UseRegex,
                UseDynamicReplacement = UseDynamicReplacement,
                Description = Description,
                CreatedAt = DateTime.Now
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>문자열 표현</returns>
        public override string ToString()
        {
            return $"'{SearchText}' → '{ReplaceText}'" + (IsEnabled ? "" : " (비활성)");
        }
    }
}