# CreateNewFile (CNF) - Project Overview

**문서 작성자**: 허창원 with Claude Code Assistant
**작성일**: 2025-09-30
**버전**: 1.0.001
**프로젝트 유형**: .NET 8.0 WPF Desktop Application

---

## 1. 프로젝트 개요

### 1.1 프로젝트 소개
**CreateNewFile (CNF)** 은 일관된 명명 규칙에 따라 파일명을 자동으로 생성하고, 템플릿 기반으로 새로운 파일을 생성하는 Windows 데스크톱 애플리케이션입니다.

### 1.2 프로젝트 목표
- 일관된 파일 명명 규칙 적용을 통한 파일 관리 효율성 향상
- 템플릿 기반 파일 생성으로 반복 작업 자동화
- 직관적이고 사용자 친화적인 인터페이스 제공
- 효율적인 파일 생성 워크플로우 구현

### 1.3 파일명 형식
```
YYYYMMDD_HHMM_약어_제목_접미어.확장자
```
**예시**: `20250822_0944_CNF_Development_note_with_AI.txt`

---

## 2. 주요 기능

### 2.1 핵심 기능
- **자동 파일명 생성**: 날짜/시간, 약어, 제목, 접미어를 조합한 파일명 자동 생성
- **템플릿 시스템**: 템플릿 파일 기반 파일 생성 지원
- **미리 정의된 항목 관리**: 약어, 제목, 접미어, 확장자 등의 사전 등록 및 관리
- **드래그앤드롭 지원**: 출력 폴더 및 템플릿 파일 경로 설정 시 드래그앤드롭 지원
- **실시간 미리보기**: 생성될 파일명 실시간 확인
- **설정 영속화**: 사용자 설정 및 마지막 선택 값 자동 저장

### 2.2 파일 생성 옵션
- **컴포넌트 기반 파일명 구성**: 체크박스를 통한 선택적 파일명 컴포넌트 활성화
  - DateTime (날짜/시간)
  - Abbreviation (약어)
  - Title (제목)
  - Suffix (접미어)
- **빈 파일 생성**: 템플릿 없이 빈 파일 생성 (.txt 파일은 공백 문자 포함)
- **템플릿 복사**: 지정된 템플릿 파일을 복사하여 새 파일 생성

---

## 3. 기술 스택

### 3.1 개발 환경
- **프레임워크**: .NET 8.0
- **UI 프레임워크**: WPF (Windows Presentation Foundation) with Windows Forms Integration
- **언어**: C# 12
- **IDE**: Visual Studio 2022
- **운영체제**: Windows 10 이상

### 3.2 아키텍처 패턴
- **MVVM Pattern**: Model-View-ViewModel 아키텍처
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Async/Await**: 비동기 프로그래밍 패턴 적용

### 3.3 주요 라이브러리
```xml
- Microsoft.Extensions.DependencyInjection (9.0.8)
- Microsoft.Extensions.Configuration (9.0.8)
- Microsoft.Extensions.Configuration.Json (9.0.8)
- Microsoft.Extensions.Hosting (9.0.8)
- Newtonsoft.Json (13.0.3)
```

### 3.4 테스트 프레임워크
- **xUnit**: 단위 테스트 프레임워크
- **Moq**: Mocking 프레임워크

---

## 4. 시스템 아키텍처

### 4.1 계층 구조
```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  ┌─────────────┐  ┌──────────────────┐  │
│  │ MainWindow  │  │ SettingsWindow   │  │
│  │   (XAML)    │  │     (XAML)       │  │
│  └─────────────┘  └──────────────────┘  │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│          Business Layer                 │
│  ┌─────────────┐  ┌──────────────────┐  │
│  │FileGenerator│  │SettingsService   │  │
│  │   Service   │  │                  │  │
│  └─────────────┘  └──────────────────┘  │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│           Data Layer                    │
│  ┌─────────────┐  ┌──────────────────┐  │
│  │ File System │  │   JSON Config    │  │
│  │             │  │     Storage      │  │
│  └─────────────┘  └──────────────────┘  │
└─────────────────────────────────────────┘
```

### 4.2 핵심 컴포넌트

#### Models
- **`AppSettings`**: 전체 애플리케이션 설정을 담는 중앙 구성 모델
- **`PresetItem`**: 미리 정의된 항목 모델 (Id, Value, IsEnabled 속성)
- **`FileCreationRequest`**: 파일 생성 요청 모델
- **`FileInfo`**: 파일 정보 관리 모델

#### Services
- **`ISettingsService/SettingsService`**: JSON 기반 설정 관리 서비스
- **`IFileGeneratorService/FileGeneratorService`**: 파일 생성 및 템플릿 처리 서비스
- **`IFileInfoService/FileInfoService`**: 파일 정보 관리 서비스

#### ViewModels
- **`MainViewModel`**: 메인 윈도우 제어 및 파일명 구성 관리
- **`SettingsViewModel`**: Preset 항목 CRUD 작업 관리
- **`FileInfoManagerViewModel`**: 파일 정보 관리 기능
- **`BaseViewModel`**: INotifyPropertyChanged 구현 기반 클래스

#### Utilities
- **`FileNameBuilder`**: 파일명 생성 로직
- **`ValidationHelper`**: 파일 경로 및 이름 유효성 검사

---

## 5. 프로젝트 구조

```
D:\Work_Claude\CreateNewFile\
├── CreateNewFile/
│   ├── .git/                           ✅ Git 저장소
│   ├── .gitignore                      ✅ Git 무시 파일
│   ├── CLAUDE.md                       ✅ Claude Code 가이드
│   ├── src/
│   │   ├── CreateNewFile/
│   │   │   ├── CreateNewFile.csproj    ✅ 프로젝트 파일
│   │   │   ├── App.xaml                ✅ 애플리케이션 정의
│   │   │   ├── App.xaml.cs             ✅ DI 컨테이너 설정
│   │   │   ├── Views/                  ✅ XAML 뷰 파일
│   │   │   │   ├── MainWindow.xaml
│   │   │   │   ├── SettingsWindow.xaml
│   │   │   │   └── FileInfoManagerWindow.xaml
│   │   │   ├── ViewModels/             ✅ ViewModel 클래스
│   │   │   │   ├── BaseViewModel.cs
│   │   │   │   ├── MainViewModel.cs
│   │   │   │   ├── SettingsViewModel.cs
│   │   │   │   └── FileInfoManagerViewModel.cs
│   │   │   ├── Models/                 ✅ 모델 클래스
│   │   │   │   ├── AppSettings.cs
│   │   │   │   ├── PresetItem.cs
│   │   │   │   ├── FileCreationRequest.cs
│   │   │   │   └── FileInfo.cs
│   │   │   ├── Services/               ✅ 서비스 레이어
│   │   │   │   ├── ISettingsService.cs
│   │   │   │   ├── SettingsService.cs
│   │   │   │   ├── IFileGeneratorService.cs
│   │   │   │   ├── FileGeneratorService.cs
│   │   │   │   ├── IFileInfoService.cs
│   │   │   │   └── FileInfoService.cs
│   │   │   ├── Utils/                  ✅ 유틸리티 클래스
│   │   │   │   ├── FileNameBuilder.cs
│   │   │   │   └── ValidationHelper.cs
│   │   │   ├── Resources/              ✅ 리소스 파일
│   │   │   │   ├── CreateNewFile.ico
│   │   │   │   └── CreateNewFile.png
│   │   │   └── config/                 ✅ 설정 파일 폴더
│   │   │       └── appsettings.default.json
│   │   └── CreateNewFile.Tests/        ✅ 테스트 프로젝트
│   │       ├── Models/
│   │       ├── Services/
│   │       └── Utils/
│   └── config/                         ✅ 런타임 설정 폴더
│       └── appsettings.json            (런타임 생성)
└── Documents/                          ✅ 프로젝트 문서
    ├── 20250822_0944_CNF_Requirements.md
    ├── 20250822_0945_CNF_Design.md
    ├── 20250822_0946_CNF_Task_list.md
    ├── 20250825_1328_CNF_Work_list_total.md
    ├── 20250826_1600_CNF_User_manual.md
    └── 20250829_1719_CNF_NSIS_설치파일_시스템.md
```

---

## 6. 설정 관리

### 6.1 설정 파일
- **`appsettings.default.json`**: 기본 설정 템플릿 (빌드 시 복사)
- **`appsettings.json`**: 사용자별 런타임 설정 (첫 실행 시 생성)

### 6.2 설정 내용
```json
{
  "EnableDateTime": true,
  "EnableAbbreviation": true,
  "EnableTitle": true,
  "EnableSuffix": false,
  "LastSelectedDateTime": "2025-09-30T15:57:00",
  "LastSelectedAbbreviation": "CNF",
  "LastSelectedTitle": "Project_overview",
  "LastSelectedOutputPath": "D:\\Work_Claude\\CreateNewFile\\Documents",
  "PresetItems": {
    "Abbreviations": [...],
    "Titles": [...],
    "Suffixes": [...],
    "Extensions": [...]
  }
}
```

### 6.3 설정 영속화
- 체크박스 상태 (DateTime, Abbreviation, Title, Suffix)
- 마지막 선택 값 (각 필드별)
- Preset 항목 목록
- 출력 경로 및 템플릿 경로

---

## 7. 빌드 및 실행

### 7.1 빌드 명령
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\src\CreateNewFile"
dotnet build
```

### 7.2 실행 명령
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\src\CreateNewFile"
dotnet run
```

### 7.3 테스트 실행
```bash
cd "D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\src\CreateNewFile.Tests"
dotnet test
```

### 7.4 배포 빌드
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

**배포 설정**:
- Target Framework: net8.0-windows
- Platform: x64
- Self-contained: false
- Single File: true
- Ready to Run: true

---

## 8. 개발 현황

### 8.1 현재 버전
- **Assembly Version**: 1.0.1.0
- **File Version**: 1.0.1.0
- **Product Version**: 1.0.001
- **Build Date**: 2025-09-05 13:44

### 8.2 완료된 기능
✅ MVVM 아키텍처 구현
✅ 의존성 주입 시스템 구축
✅ 파일명 생성 엔진 (체크박스 기반 컴포넌트 선택)
✅ 템플릿 기반 파일 생성
✅ JSON 기반 설정 관리
✅ Preset 항목 관리 (CRUD)
✅ 드래그앤드롭 지원
✅ 실시간 파일명 미리보기
✅ 마지막 선택 값 복원
✅ 체크박스 상태 영속화
✅ 단위 테스트 (xUnit)
✅ 통합 테스트
✅ 사용자 매뉴얼
✅ NSIS 설치 파일 시스템

### 8.3 주요 특징
- **Simplified PresetItem Model**: Description, CreatedAt, IsFavorite 등 deprecated 속성 제거
- **Checkbox-based Filename Control**: 파일명 컴포넌트 선택적 활성화
- **DispatcherTimer Initialization**: UI 스레드 데드락 방지
- **Dynamic Version Display**: 윈도우 타이틀 및 상태바에 버전 정보 표시
- **Minimal JSON Configuration**: 필수 속성만 포함한 간결한 설정 구조

---

## 9. 테스트 전략

### 9.1 테스트 범위
- **단위 테스트**: 모든 핵심 컴포넌트 및 서비스
- **통합 테스트**: 서비스 간 상호작용
- **사용자 수용 테스트**: 전체 워크플로우 시나리오

### 9.2 테스트 케이스
- PresetItem 모델 테스트 (단순화된 모델 검증)
- FileGeneratorService 테스트
- SettingsService 테스트
- FileNameBuilder 유틸리티 테스트
- ValidationHelper 테스트

---

## 10. 비기능 요구사항

### 10.1 성능 목표
- 애플리케이션 시작 시간: **3초 이내**
- 메모리 사용량: **50MB 이하**
- 파일 생성 응답 시간: **1초 이내**

### 10.2 호환성
- 운영체제: **Windows 10 이상**
- .NET Runtime: **.NET 8.0 이상**
- 화면 해상도: **1024x768 이상**

### 10.3 신뢰성
- 잘못된 입력에 대한 검증
- 파일 덮어쓰기 방지
- 설정 데이터 백업 및 복구

---

## 11. 향후 계획

### 11.1 단기 목표
- 키보드 단축키 개선
- 추가 유효성 검사 강화
- 에러 메시지 개선

### 11.2 중기 목표
- 다국어 지원 (한국어/영어)
- 클라우드 스토리지 연동
- 파일 생성 이력 관리

### 11.3 장기 목표
- 플러그인 아키텍처 도입
- 사용자 정의 파일명 규칙
- 배치 파일 생성 기능

---

## 12. 참고 문서

- **Requirements**: `Documents/20250822_0944_CNF_Requirements.md`
- **Design**: `Documents/20250822_0945_CNF_Design.md`
- **Task List**: `Documents/20250822_0946_CNF_Task_list.md`
- **Work List**: `Documents/20250825_1328_CNF_Work_list_total.md`
- **User Manual**: `Documents/20250826_1600_CNF_User_manual.md`
- **NSIS Installer**: `Documents/20250829_1719_CNF_NSIS_설치파일_시스템.md`
- **Claude Code Guide**: `CLAUDE.md`

---

## 13. 연락처 및 지원

**프로젝트 관리자**: 허창원 (changwon.heo@gmail.com)
**개발 도구**: Claude Code Assistant
**저장소**: Git (Local Repository)
**문서 버전**: 1.0
**최종 업데이트**: 2025-09-30

---

## 부록 A: 파일명 생성 예시

| DateTime | Abbreviation | Title | Suffix | Extension | 결과 파일명 |
|----------|-------------|-------|--------|-----------|-----------|
| ✅ | ✅ | ✅ | ✅ | .md | 20250930_1557_CNF_Project_overview_v1.md |
| ✅ | ✅ | ✅ | ❌ | .txt | 20250930_1557_CNF_Document.txt |
| ✅ | ❌ | ✅ | ❌ | .cs | 20250930_1557_MainClass.cs |
| ❌ | ✅ | ✅ | ✅ | .log | CNF_Application_debug.log |

---

## 부록 B: 주요 클래스 다이어그램

```
┌─────────────────────┐
│   MainViewModel     │
├─────────────────────┤
│ + EnableDateTime    │
│ + EnableAbbreviation│
│ + EnableTitle       │
│ + EnableSuffix      │
│ + CreateFileAsync() │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ FileGeneratorService│
├─────────────────────┤
│ + GenerateFileName()│
│ + CreateFileAsync() │
│ + ValidateRequest() │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   FileNameBuilder   │
├─────────────────────┤
│ + BuildFileName()   │
│ + ValidateFileName()│
└─────────────────────┘
```

---

**문서 끝**