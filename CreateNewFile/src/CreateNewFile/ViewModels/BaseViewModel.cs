using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CreateNewFile.ViewModels
{
    /// <summary>
    /// MVVM 패턴의 기본 ViewModel 클래스
    /// INotifyPropertyChanged 인터페이스 구현
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 속성 변경 알림을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">변경된 속성 이름 (자동으로 설정됨)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 속성 값을 설정하고 변경 알림을 발생시킵니다.
        /// </summary>
        /// <typeparam name="T">속성 타입</typeparam>
        /// <param name="field">백킹 필드</param>
        /// <param name="value">새 값</param>
        /// <param name="propertyName">속성 이름 (자동으로 설정됨)</param>
        /// <returns>값이 변경되었으면 true, 아니면 false</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}