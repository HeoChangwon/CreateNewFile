namespace CreateNewFile.Models
{
    /// <summary>
    /// 미리 정의된 항목을 나타내는 모델 클래스
    /// </summary>
    public class PresetItem
    {
        /// <summary>
        /// 항목의 고유 식별자
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 항목 값
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 항목 설명
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        /// <summary>
        /// 항목이 활성화되어 있는지 여부
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 항목이 즐겨찾기에 추가되어 있는지 여부
        /// </summary>
        public bool IsFavorite { get; set; } = false;

        /// <summary>
        /// 항목의 유효성을 검사합니다.
        /// </summary>
        /// <returns>유효하면 true, 아니면 false</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Value);
        }


        /// <summary>
        /// 항목을 복사하여 새 인스턴스를 생성합니다.
        /// </summary>
        /// <returns>복사된 새 인스턴스</returns>
        public PresetItem Clone()
        {
            return new PresetItem
            {
                Id = this.Id,
                Value = this.Value,
                Description = this.Description,
                CreatedAt = this.CreatedAt,
                IsEnabled = this.IsEnabled,
                IsFavorite = this.IsFavorite
            };
        }

        /// <summary>
        /// 두 PresetItem이 같은지 비교합니다.
        /// </summary>
        /// <param name="obj">비교할 객체</param>
        /// <returns>같으면 true, 다르면 false</returns>
        public override bool Equals(object? obj)
        {
            if (obj is PresetItem other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// 해시 코드를 반환합니다.
        /// </summary>
        /// <returns>해시 코드</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// 객체의 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>값과 설명</returns>
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Description))
                return $"{Value} - {Description}";
            return Value;
        }

        /// <summary>
        /// 표시용 문자열을 반환합니다.
        /// </summary>
        public string DisplayText => ToString();

    }
}