using System.Windows.Input;

namespace CreateNewFile.Utils
{
    /// <summary>
    /// ICommand 인터페이스의 기본 구현
    /// MVVM 패턴에서 명령(Command) 바인딩을 위해 사용
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// RelayCommand의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="execute">실행할 메서드</param>
        /// <param name="canExecute">실행 가능 여부를 확인하는 메서드 (선택사항)</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 명령이 실행 가능한지 확인합니다.
        /// </summary>
        /// <param name="parameter">명령 매개변수</param>
        /// <returns>실행 가능하면 true, 아니면 false</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// 명령을 실행합니다.
        /// </summary>
        /// <param name="parameter">명령 매개변수</param>
        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// CanExecute 상태가 변경되었을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// CanExecute 상태 변경을 강제로 알립니다.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 매개변수를 받는 RelayCommand의 제네릭 버전
    /// </summary>
    /// <typeparam name="T">매개변수 타입</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        /// <summary>
        /// RelayCommand<T>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="execute">실행할 메서드</param>
        /// <param name="canExecute">실행 가능 여부를 확인하는 메서드 (선택사항)</param>
        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 명령이 실행 가능한지 확인합니다.
        /// </summary>
        /// <param name="parameter">명령 매개변수</param>
        /// <returns>실행 가능하면 true, 아니면 false</returns>
        public bool CanExecute(object? parameter)
        {
            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;
            
            return _canExecute?.Invoke(default(T)!) ?? true;
        }

        /// <summary>
        /// 명령을 실행합니다.
        /// </summary>
        /// <param name="parameter">명령 매개변수</param>
        public void Execute(object? parameter)
        {
            if (parameter is T typedParameter)
                _execute(typedParameter);
            else
                _execute(default(T)!);
        }

        /// <summary>
        /// CanExecute 상태가 변경되었을 때 발생하는 이벤트
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// CanExecute 상태 변경을 강제로 알립니다.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}