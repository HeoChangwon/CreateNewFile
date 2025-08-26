# CreateNewFile - 간단 사용법

**문서 작성자**: 허창원 with Claude Code Assistant  
**프로그램 버전**: CreateNewFile v1.0.001  

---

## 🚀 3분만에 시작하기

### 1. 설치하기
1. `CreateNewFile_v1.0.001_Build_YYYYMMDD_HHMM_Setup.exe` 파일 실행
2. "Next" → "Install" → "Finish"
3. .NET 8.0 Runtime 설치 (필요시 자동 안내)

### 2. 프로그램 실행
- 시작 메뉴 → CreateNewFile → CreateNewFile 실행

### 3. 파일 만들기 (기본)
1. **날짜/시간**: "현재 일시" 버튼 클릭
2. **약어**: 드롭다운에서 선택 (예: CNF)
3. **제목**: 원하는 제목 입력 (예: Manual)
4. **확장자**: 드롭다운에서 선택 (예: .md)
5. **"파일 생성"** 버튼 클릭!

**결과**: `20250826_1600_CNF_Manual.md` 파일 생성됨

---

## ⚡ 핵심 기능 5개

### 1. 체크박스로 파일명 제어
```
☑ 날짜/시간  ☑ 약어  ☑ 제목  ☐ 접미어
결과: 20250826_1600_CNF_Manual.md

☐ 날짜/시간  ☑ 약어  ☑ 제목  ☐ 접미어  
결과: CNF_Manual.md
```

### 2. 실시간 미리보기
- 입력하는 대로 파일명이 실시간으로 보임
- 체크박스 변경 시 즉시 반영

### 3. 드래그앤드롭
- 폴더를 창에 끌어다 놓으면 출력 경로 자동 설정
- 파일을 끌어다 놓으면 템플릿으로 자동 설정

### 4. 자동 저장
- 프로그램 종료 후 다시 실행해도 마지막 설정 유지
- 체크박스 상태, 선택한 값들 모두 기억

### 5. 설정 관리
- "설정 관리" 버튼으로 프리셋 추가/삭제
- "설정 폴더 열기"로 설정 파일 직접 편집 가능

---

## 📁 파일명 형식

### 기본 형식
```
YYYYMMDD_HHMM_약어_제목_접미어.확장자
20250826_1600_CNF_User_manual_with_AI.md
```

### 자주 쓰이는 조합
```
✅ 날짜+약어+제목: 20250826_1600_CNF_Manual.md
✅ 약어+제목: CNF_Manual.md
✅ 날짜+제목: 20250826_1600_Manual.md
✅ 제목만: Manual.md
```

---

## 🎯 빠른 사용 팁

### 자주 사용하는 프리셋 추가
1. "설정 관리" 버튼 클릭
2. 항목 추가하기:
   - **약어**: DOC, NOTE, TEMP 등
   - **제목**: Report, Meeting, Manual 등  
   - **접미어**: draft, final, backup 등

### 템플릿 사용하기
1. 기존 파일을 창에 드래그앤드롭
2. 또는 "템플릿 선택" 버튼으로 파일 선택
3. 파일 생성 시 템플릿 내용이 자동으로 복사됨

### 키보드 단축키
```
Ctrl + N    파일 생성
F5         현재 일시로 자동 설정
Ctrl + S    설정 관리 창 열기
```

---

## 🔧 문제 해결

### ".NET 8.0이 필요합니다" 오류
1. https://dotnet.microsoft.com/ko-kr/download/dotnet/8.0 방문
2. "Run desktop apps" → "Download x64" 클릭
3. 설치 후 프로그램 재실행

### 파일이 생성되지 않음
- 출력 폴더 권한 확인
- 파일명에 특수문자(`<>:|?*`) 제거
- 디스크 공간 확보

### 설정이 저장되지 않음
1. "설정 폴더 열기" 버튼 클릭
2. `appsettings.json` 파일 삭제
3. 프로그램 재시작 (기본값으로 초기화)

---

## 💡 실제 사용 예시

### 회의록 작성
```
체크박스: ☑ 날짜/시간 ☑ 약어 ☑ 제목 ☐ 접미어
약어: MEETING
제목: Weekly_team_sync
확장자: .md
→ 20250826_1600_MEETING_Weekly_team_sync.md
```

### 개발 문서 작성
```
체크박스: ☑ 날짜/시간 ☑ 약어 ☑ 제목 ☑ 접미어
약어: DEV
제목: API_specification  
접미어: draft
확장자: .md
→ 20250826_1600_DEV_API_specification_draft.md
```

### 임시 메모
```
체크박스: ☐ 날짜/시간 ☑ 약어 ☑ 제목 ☐ 접미어
약어: NOTE
제목: Quick_memo
확장자: .txt
→ NOTE_Quick_memo.txt
```

---

## 📞 도움이 필요하면

**개발자 연락처**: changwon.heo@gmail.com  
**GitHub**: https://github.com/HeoChangwon/CreateNewFile

---

**간단 매뉴얼 버전**: v1.0  
**마지막 업데이트**: 2025년 8월 26일  

*CreateNewFile로 체계적인 파일 관리를 시작하세요! 🎉*