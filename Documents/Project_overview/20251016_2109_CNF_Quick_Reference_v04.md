# CreateNewFile - Quick Reference v04

**최종 업데이트**: 2025년 10월 16일 21:09

## 프로젝트 개요

### 기본 정보
- **프로젝트명**: CreateNewFile
- **플랫폼**: .NET 8.0 WPF (Windows)
- **아키텍처**: MVVM 패턴
- **목적**: 템플릿 기반 파일 자동 생성 도구

### 핵심 기능
1. 날짜/시간 기반 파일명 생성
2. 약어, 제목, 접미어를 조합한 파일명 생성
3. 템플릿 파일의 문자열 자동 교체
4. 프로젝트 설정 저장/로드 (.cnfjson)
5. 파일 정보 프리셋 관리

## 프로젝트 구조

### 주요 디렉토리
```
CreateNewFile/
├── Models/              # 데이터 모델
├── ViewModels/          # MVVM ViewModel
├── Views/               # XAML UI
├── Services/            # 비즈니스 로직
├── Utils/               # 유틸리티 클래스
├── Resources/           # 아이콘 등 리소스
└── NSIS_installer/      # 설치 패키지
```

### 핵심 파일

#### Models
- `ProjectConfig.cs`: 프로젝트 설정 데이터 모델
- `ProjectFileInfo.cs`: 파일 정보 구조
- `StringReplacementRule.cs`: 문자열 교체 규칙
- `PresetItem.cs`: 프리셋 항목 모델

#### Services
- `IProjectConfigService.cs` / `ProjectConfigService.cs`: 프로젝트 설정 저장/로드
- `IFileGeneratorService.cs` / `FileGeneratorService.cs`: 파일 생성 로직
- `ISettingsService.cs` / `SettingsService.cs`: 앱 설정 관리

#### ViewModels
- `MainViewModel.cs`: 메인 화면 뷰모델 (2100+ 라인)
  - 파일명 생성 로직
  - 프리셋 관리
  - 프로젝트 설정 관리
  - 유효성 검증

#### Views
- `MainWindow.xaml`: 메인 UI
  - 탭 구조: 파일정보, 경로설정, 문자열 교체
  - 최소 높이: 980px
  - 탭 영역 최소 높이: 430px

## 주요 기능 상세

### 1. 프로젝트 설정 (.cnfjson)

#### 파일 형식
```json
{
  "version": "1.0",
  "name": "프로젝트명",
  "description": "설명",
  "fileInfo": {
    "isDateTimeEnabled": true,
    "dateTime": "2025-10-16T21:00:00",
    "isAbbreviationEnabled": true,
    "abbreviation": "CNF",
    "isTitleEnabled": true,
    "title": "작업명",
    "isSuffixEnabled": false,
    "suffix": "",
    "extension": ".md"
  },
  "outputPath": "D:\\Output",
  "templatePath": "D:\\Templates\\template.txt",
  "stringReplacements": [
    {
      "isEnabled": true,
      "searchText": "{DATE}",
      "replaceText": "2025-10-16",
      "isCaseSensitive": false,
      "useRegex": false,
      "useDynamicReplacement": true,
      "description": "날짜 자동 교체"
    }
  ],
  "createdAt": "2025-10-16T21:00:00",
  "modifiedAt": "2025-10-16T21:00:00"
}
```

#### 저장/로드 동작
- **저장**: 템플릿 파일명 기반 자동 명명 → 템플릿 폴더에 저장
- **로드**: 템플릿 폴더를 초기 위치로 파일 선택 대화상자 표시
- **자동 로드**: 명령줄 인자로 .cnfjson 파일 경로 전달 시 자동 로드

### 2. 파일명 생성 규칙

#### 형식
```
[날짜시간]_[약어]_[제목]_[접미어].[확장자]
```

#### 예시
```
20251016_2100_CNF_작업명.md
20251016_CNF_보고서_초안.docx
```

### 3. 문자열 교체 기능

#### 교체 규칙 속성
- `isEnabled`: 활성화 여부
- `searchText`: 검색 문자열
- `replaceText`: 교체 문자열
- `isCaseSensitive`: 대소문자 구분
- `useRegex`: 정규식 사용
- `useDynamicReplacement`: 동적 값 교체 (날짜/시간 등)

#### 동적 교체 키워드
- `{DATE}`: 현재 날짜
- `{TIME}`: 현재 시간
- `{DATETIME}`: 날짜+시간
- `{TITLE}`: 선택한 제목
- `{ABBREVIATION}`: 선택한 약어

## 개발 가이드

### 빌드 명령
```bash
cd D:\Work_Claude\2025\08\CreateNewFile\CreateNewFile\CreateNewFile
dotnet build
```

### 실행 명령
```bash
dotnet run
```

### 프로젝트 설정 파일로 실행
```bash
.\CreateNewFile.exe "D:\path\to\project.cnfjson"
```

## UI 레이아웃 가이드

### 창 크기
- 기본 크기: 620x1000 (너비x높이)
- 최소 크기: 586x980

### 탭 영역
- 각 탭 ScrollViewer MinHeight: 430px
- 탭 목록: 파일정보, 경로설정, 문자열 교체

### 버튼 위치
- **프로젝트 설정**: "저장된 파일정보" 그룹 내부
  - 📥 가져오기
  - 💾 내보내기
- **파일 생성**: 하단 중앙
- **설정 관리**: 하단 중앙

## 설정 파일 위치

### Windows
```
%APPDATA%\CreateNewFile\
├── settings.json          # 앱 설정
├── abbreviations.json     # 약어 프리셋
├── titles.json           # 제목 프리셋
├── suffixes.json         # 접미어 프리셋
├── extensions.json       # 확장자 프리셋
├── outputPaths.json      # 출력 경로 프리셋
└── templatePaths.json    # 템플릿 경로 프리셋
```

## NSIS 인스톨러

### 파일 연결 등록
```nsis
; .cnfjson 파일 연결
WriteRegStr HKCR ".cnfjson" "" "CreateNewFile.Project"
WriteRegStr HKCR "CreateNewFile.Project" "" "CreateNewFile Project File"
WriteRegStr HKCR "CreateNewFile.Project\DefaultIcon" "" "$INSTDIR\Resources\CreateNewFile.ico,0"
WriteRegStr HKCR "CreateNewFile.Project\shell\open\command" "" '"$INSTDIR\CreateNewFile.exe" "%1"'
```

### 인스톨러 빌드
```bash
makensis CreateNewFile_Installer.nsi
```

## 최근 업데이트 (v04)

### 2025-10-16 변경사항

1. **UI 레이아웃 최적화**
   - 탭 영역 최소 높이를 430px로 조정
   - 창 최소 높이를 980px로 조정
   - 파일 정보 입력 영역 가시성 개선

2. **사용자 경험 개선**
   - "가져오기" 버튼 클릭 시 템플릿 폴더를 초기 위치로 설정
   - 프로젝트 설정 파일 접근성 향상

3. **안정성**
   - 파일 경로 유효성 검사 강화
   - 템플릿 폴더 존재 여부 확인

## 알려진 이슈

### 경고 (빌드 시)
- Null 참조 관련 경고 (CS8625, CS8622): 기능에 영향 없음
- 미사용 변수 경고 (CS0168): 예외 처리 블록
- Assembly.Location 경고 (IL3000): Single-file 배포 시 주의

### 해결 방법
- Null 허용 참조 타입 명시적 선언
- 예외 변수 로깅 또는 제거
- AppContext.BaseDirectory 사용 권장

## 테스트 체크리스트

### 기능 테스트
- [ ] 파일명 생성 로직
- [ ] 템플릿 기반 파일 생성
- [ ] 문자열 교체 기능
- [ ] 프로젝트 설정 저장
- [ ] 프로젝트 설정 로드
- [ ] .cnfjson 파일 더블클릭 실행
- [ ] 프리셋 관리
- [ ] 설정 관리

### UI 테스트
- [x] 창 크기 조정
- [x] 탭 전환
- [x] 스크롤 동작
- [ ] 버튼 동작
- [ ] 대화상자 표시

### 통합 테스트
- [ ] 전체 워크플로우
- [ ] 설치 후 실행
- [ ] 파일 연결 동작

## 지원 및 문서

### 관련 문서
- 작업 내역: `Documents/2025/10/`
- 프로젝트 개요: `Documents/Project_overview/`

### 버전 정보
- 현재 버전: v1.0.003
- Quick Reference 버전: v04
- 최종 업데이트: 2025-10-16 21:09

## 다음 개발 계획

### 예정 기능
1. 다중 템플릿 선택 지원
2. 배치 파일 생성
3. 히스토리 기능
4. 템플릿 편집기 통합
5. 클라우드 동기화

### 개선 사항
1. 경고 메시지 제거
2. 단위 테스트 추가
3. 성능 최적화
4. 다국어 지원 (영어)

---

**참고**: 이 문서는 개발 중인 프로젝트의 현재 상태를 반영합니다. 기능 및 구조는 변경될 수 있습니다.
