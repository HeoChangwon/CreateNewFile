using System;
using System.Linq;
using System.Threading.Tasks;

namespace CreateNewFile.Utils
{
    /// <summary>
    /// 사용자 대화상자 관련 유틸리티 클래스
    /// </summary>
    public static class DialogHelper
    {
        #region 정보 대화상자

        /// <summary>
        /// 정보 메시지를 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        public static void ShowInfo(string message, string title = "정보")
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// 정보 메시지를 비동기로 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        public static Task ShowInfoAsync(string message, string title = "정보")
        {
            return Task.Run(() => ShowInfo(message, title));
        }

        #endregion

        #region 경고 대화상자

        /// <summary>
        /// 경고 메시지를 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        public static void ShowWarning(string message, string title = "경고")
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            });
        }

        /// <summary>
        /// 경고 메시지를 비동기로 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        public static Task ShowWarningAsync(string message, string title = "경고")
        {
            return Task.Run(() => ShowWarning(message, title));
        }

        #endregion

        #region 오류 대화상자

        /// <summary>
        /// 오류 메시지를 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        public static void ShowError(string message, string title = "오류")
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            });
        }

        /// <summary>
        /// 오류 메시지를 비동기로 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        public static Task ShowErrorAsync(string message, string title = "오류")
        {
            return Task.Run(() => ShowError(message, title));
        }

        /// <summary>
        /// 예외 정보를 포함한 오류 메시지를 표시합니다
        /// </summary>
        /// <param name="ex">예외 객체</param>
        /// <param name="userMessage">사용자에게 표시할 메시지</param>
        /// <param name="title">창 제목</param>
        /// <param name="showDetails">상세 정보 표시 여부</param>
        public static void ShowError(Exception ex, string userMessage = null, string title = "오류", bool showDetails = false)
        {
            var message = userMessage ?? "처리 중 오류가 발생했습니다.";
            
            if (showDetails && ex != null)
            {
                message += $"\n\n상세 정보:\n{ex.Message}";
                
                if (ex.InnerException != null)
                {
                    message += $"\n\n내부 오류:\n{ex.InnerException.Message}";
                }
            }

            ShowError(message, title);
        }

        #endregion

        #region 확인 대화상자

        /// <summary>
        /// 예/아니오 확인 대화상자를 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        /// <returns>사용자가 '예'를 선택했으면 true</returns>
        public static bool ShowConfirm(string message, string title = "확인")
        {
            var result = System.Windows.MessageBoxResult.No;
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                result = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            });
            
            return result == System.Windows.MessageBoxResult.Yes;
        }

        /// <summary>
        /// 예/아니오 확인 대화상자를 비동기로 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        /// <returns>사용자가 '예'를 선택했으면 true</returns>
        public static Task<bool> ShowConfirmAsync(string message, string title = "확인")
        {
            return Task.Run(() => ShowConfirm(message, title));
        }

        /// <summary>
        /// 예/아니오/취소 확인 대화상자를 표시합니다
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="title">창 제목</param>
        /// <returns>사용자의 선택 결과</returns>
        public static System.Windows.MessageBoxResult ShowConfirmCancel(string message, string title = "확인")
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBoxResult.Cancel;
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                result = System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Question);
            });
            
            return result;
        }

        #endregion

        #region 파일 덮어쓰기 확인

        /// <summary>
        /// 파일 덮어쓰기 확인 대화상자를 표시합니다
        /// </summary>
        /// <param name="fileName">덮어쓸 파일명</param>
        /// <param name="filePath">덮어쓸 파일 경로</param>
        /// <returns>사용자가 덮어쓰기를 허용했으면 true</returns>
        public static bool ShowFileOverwriteConfirm(string fileName, string filePath = null)
        {
            var message = $"파일 '{fileName}'이(가) 이미 존재합니다.";
            
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                message += $"\n\n경로: {filePath}";
            }
            
            message += "\n\n덮어쓰시겠습니까?";
            
            return ShowConfirm(message, "파일 덮어쓰기 확인");
        }

        /// <summary>
        /// 파일 덮어쓰기 확인 대화상자를 비동기로 표시합니다
        /// </summary>
        /// <param name="fileName">덮어쓸 파일명</param>
        /// <param name="filePath">덮어쓸 파일 경로</param>
        /// <returns>사용자가 덮어쓰기를 허용했으면 true</returns>
        public static Task<bool> ShowFileOverwriteConfirmAsync(string fileName, string filePath = null)
        {
            return Task.Run(() => ShowFileOverwriteConfirm(fileName, filePath));
        }

        #endregion

        #region 작업 완료 확인

        /// <summary>
        /// 파일 생성 완료 대화상자를 표시합니다
        /// </summary>
        /// <param name="fileName">생성된 파일명</param>
        /// <param name="filePath">생성된 파일 경로</param>
        /// <param name="fileSize">파일 크기 (바이트)</param>
        /// <param name="usedTemplate">템플릿 사용 여부</param>
        /// <param name="showOpenOptions">열기 옵션 표시 여부</param>
        /// <returns>사용자의 선택 (Yes: 파일 열기, No: 폴더 열기, Cancel: 닫기)</returns>
        public static System.Windows.MessageBoxResult ShowFileCreationSuccess(
            string fileName, 
            string filePath, 
            long fileSize, 
            bool usedTemplate, 
            bool showOpenOptions = true)
        {
            var message = $"파일이 성공적으로 생성되었습니다.\n\n";
            message += $"📄 파일명: {fileName}\n";
            message += $"📁 경로: {filePath}\n";
            message += $"📏 크기: {FormatFileSize(fileSize)}\n";
            message += $"📋 템플릿 사용: {(usedTemplate ? "예" : "아니오")}";

            if (showOpenOptions)
            {
                message += "\n\n파일을 열거나 폴더를 열까요?";
                
                System.Windows.Application.Current.Dispatcher.Invoke(() => { });
                var customResult = ShowCustomDialog(
                    message, 
                    "파일 생성 완료", 
                    "파일 열기", 
                    "폴더 열기", 
                    "닫기");
                
                return customResult;
            }
            else
            {
                ShowInfo(message, "파일 생성 완료");
                return System.Windows.MessageBoxResult.OK;
            }
        }

        /// <summary>
        /// 사용자 정의 3개 버튼 대화상자를 표시합니다
        /// </summary>
        /// <param name="message">메시지</param>
        /// <param name="title">제목</param>
        /// <param name="button1Text">첫 번째 버튼 텍스트</param>
        /// <param name="button2Text">두 번째 버튼 텍스트</param>
        /// <param name="button3Text">세 번째 버튼 텍스트</param>
        /// <returns>사용자 선택</returns>
        public static System.Windows.MessageBoxResult ShowCustomDialog(
            string message, 
            string title, 
            string button1Text, 
            string button2Text, 
            string button3Text)
        {
            // 기본 MessageBox로 구현 (추후 Custom Dialog Window로 확장 가능)
            var result = System.Windows.MessageBoxResult.Cancel;
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var dialogMessage = $"{message}\n\n[예: {button1Text}] [아니오: {button2Text}] [취소: {button3Text}]";
                result = System.Windows.MessageBox.Show(dialogMessage, title, System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Information);
            });
            
            return result;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 파일 크기를 사람이 읽기 쉬운 형태로 포맷합니다
        /// </summary>
        /// <param name="bytes">바이트 크기</param>
        /// <returns>포맷된 크기 문자열</returns>
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} 바이트";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F1} MB";
            else
                return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
        }

        /// <summary>
        /// 유효성 검사 에러를 사용자 친화적인 형태로 표시합니다
        /// </summary>
        /// <param name="validationResult">유효성 검사 결과</param>
        /// <param name="title">대화상자 제목</param>
        public static void ShowValidationErrors(ValidationResult validationResult, string title = "입력 확인")
        {
            if (validationResult.IsValid)
                return;

            var message = "다음 문제를 해결해주세요:\n\n";
            message += string.Join("\n• ", validationResult.ErrorMessages.Select(m => $"• {m}"));

            ShowWarning(message, title);
        }

        /// <summary>
        /// 설정 변경 확인 대화상자를 표시합니다
        /// </summary>
        /// <param name="changeCount">변경된 항목 수</param>
        /// <returns>저장 여부</returns>
        public static bool ShowSettingsChangeConfirm(int changeCount)
        {
            var message = $"{changeCount}개의 설정이 변경되었습니다.\n\n변경사항을 저장하시겠습니까?";
            return ShowConfirm(message, "설정 변경 확인");
        }

        /// <summary>
        /// 데이터 손실 경고 대화상자를 표시합니다
        /// </summary>
        /// <param name="action">수행할 작업</param>
        /// <returns>계속 진행 여부</returns>
        public static bool ShowDataLossWarning(string action = "이 작업")
        {
            var message = $"{action}을(를) 진행하면 저장되지 않은 변경사항이 손실됩니다.\n\n계속 진행하시겠습니까?";
            return ShowConfirm(message, "데이터 손실 경고");
        }

        #endregion

        #region 진행률 관련 (확장 가능)

        /// <summary>
        /// 장시간 작업에 대한 진행률 대화상자 표시 준비
        /// (향후 ProgressDialog 구현 시 사용)
        /// </summary>
        /// <param name="title">작업 제목</param>
        /// <param name="message">작업 메시지</param>
        /// <param name="cancellable">취소 가능 여부</param>
        public static void ShowProgressDialog(string title, string message, bool cancellable = false)
        {
            // 향후 구현 예정
            // 현재는 상태 메시지로 대체
        }

        #endregion
    }
}