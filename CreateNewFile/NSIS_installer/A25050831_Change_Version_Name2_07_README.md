# Change_Version_Name2 스크립트 사용법

## 간단한 사용법

### 1. 기본 실행 (권장)
스크립트 파일을 더블클릭하거나 다음 명령으로 실행:
```
python A25050831_Change_Version_Name2_07.py
```
- 처리 완료 후 Enter 키를 눌러 종료

### 2. 자동 종료 실행
```
python A25050831_Change_Version_Name2_07.py --no-wait-exit
```
- 처리 완료 후 자동으로 로그 파일이 열리고 프로그램 종료

## 스크립트 개요
파일 List에 나열된 파일을 찾아 문자열을 교체하는 Python 스크립트입니다.

**작성자:** 허창원

---

## 세부 사용 방법

### 명령행 옵션

#### 기본 옵션
```
--help                    : 사용방법 표시
--no-wait-exit           : 스크립트 종료 전 대기하지 않음
```

#### 파일 지정 옵션
```
--file-list [파일명]              : File List JSON 입력파일명 지정
--replace-string-list [파일명]    : Replace String List JSON 입력파일명 지정
--log [파일명]                    : Log 파일명 지정
```

### 입력 파일

#### 1. File List JSON 입력파일
**기본 파일명:** `A25050831_Change_Version_Name2_INPUT_FileList.json`

**파일 형식:**
```json
[
  {
    "base_directory": "sandbox",
    "files": [
      "AAD10_DefProg_06_Bat_copy2Output.bat",
      "AAD10_DefProg_06_Bat_copy2Output_C.bat",
      "AAD11_DefProg_06_Bat_ClearOutput.bat",
      "02_Config\\forNSIS_Common\\IncludePart.nsi"
    ]
  }
]
```

- `base_directory`: 현재 스크립트 폴더 기준 상대경로 또는 절대경로 (C:, D: 등으로 시작)
- `files`: base_directory 기준 파일들의 상대경로

#### 2. Replace String List JSON 입력파일
**기본 파일명:** `A25050831_Change_Version_Name2_INPUT_ReplaceStringList.json`

**파일 형식:**
```json
[
  {"from": "2.6.004", "to": "2.6.004a"},
  {"from": "_20250221_1603", "to": "_20250508_1426"}
]
```

- `from`: 교체될 문자열
- `to`: 교체할 문자열

### 사용 예시

#### 기본 설정으로 실행
```bash
python A25050831_Change_Version_Name2_07.py
```

#### 사용자 정의 파일로 실행
```bash
python A25050831_Change_Version_Name2_07.py --file-list my_files.json --replace-string-list my_replacements.json
```

#### 자동 종료 모드로 실행
```bash
python A25050831_Change_Version_Name2_07.py --no-wait-exit
```

#### 사용자 정의 로그 파일 지정
```bash
python A25050831_Change_Version_Name2_07.py --log my_custom_log.txt
```

### 출력 및 로그

#### Log 파일
- **저장 경로:** `Output_Result/` 폴더
- **파일명 형식:** `OUTPUT_YYYYMMDD_HHMMSS_CVN2_Log.txt`
- **내용:** 실행 정보, 처리 결과, 처리 시간 등

#### 동작 방식
- **기본 모드:** 처리 완료 후 Enter 키 대기
- **--no-wait-exit 모드:** 처리 완료 후 로그 파일 자동 열기 및 종료

### 지원 기능

#### 인코딩 지원
- UTF-8
- CP949 (한글 Windows 인코딩)
- EUC-KR
- ASCII
- Latin1

#### 경로 지원
- 상대 경로
- 절대 경로
- 한글이 포함된 경로
- 공백이 포함된 경로

### 주의사항
- 모든 입력 파일은 UTF-8 인코딩으로 저장
- 처리 대상 파일은 자동으로 인코딩 감지
- 변경사항이 있는 파일만 저장됨
- 스크립트와 같은 폴더에 입력 파일 위치 권장