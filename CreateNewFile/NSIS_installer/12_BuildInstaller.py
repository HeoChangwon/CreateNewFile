#!/usr/bin/env python3
"""
CreateNewFile NSIS 설치파일 빌드 스크립트
NSIS 컴파일러를 사용하여 Windows 설치파일을 생성합니다.
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path
from datetime import datetime

# ==========================================
# 설정 (필요시 수정)
# ==========================================
PRODUCT_NAME = "CreateNewFile"
PRODUCT_VERSION = "1.0.002"
BUILD_DATE = "20250930_1807"
NSIS_SCRIPT = "CreateNewFile_Installer.nsi"
NSIS_PATH = r"C:\Program Files (x86)\NSIS\makensis.exe"

def print_header():
    """헤더 출력"""
    print("=" * 60)
    print("CreateNewFile NSIS 설치파일 빌드")
    print("NSIS_installer 폴더 버전")
    print("=" * 60)
    print()
    print(f"현재 디렉터리: {os.getcwd()}")
    print()

def check_nsis():
    """NSIS 설치 확인"""
    print("1. NSIS 설치 확인 중...")
    
    if not Path(NSIS_PATH).exists():
        print("   오류: NSIS를 찾을 수 없습니다!")
        print("   기본 경로에서 NSIS를 찾을 수 없습니다.")
        print("   다음 사이트에서 NSIS를 다운로드하세요: https://nsis.sourceforge.io/Download")
        print("   또는 스크립트에서 NSIS_PATH를 올바른 경로로 수정하세요.")
        return False
    
    print("   NSIS를 찾았습니다!")
    print()
    return True

def check_required_files():
    """필수 파일들 확인"""
    print("2. 필수 파일들 확인 중...")
    
    required_files = [
        "publish/framework-dependent/CreateNewFile.exe",
        "publish/framework-dependent/Resources/CreateNewFile.ico",
        "LICENSE.txt"
    ]
    
    for file_path in required_files:
        if not Path(file_path).exists():
            print(f"   오류: 필수 파일을 찾을 수 없습니다: {file_path}")
            if file_path.startswith("publish/"):
                print("   11_UpdateFromProject.py를 먼저 실행하세요.")
            return False
    
    print("   모든 필수 파일을 찾았습니다!")
    print()
    return True

def convert_version_format(version_str):
    """버전을 NSIS VIProductVersion 형식(X.X.X.X)으로 변환"""
    # "1.0.001" -> "1.0.1.0"
    parts = version_str.split('.')
    if len(parts) == 3:
        # 세 번째 파트가 "001" 같은 형식이면 숫자로 변환
        try:
            third_part = str(int(parts[2]))
            return f"{parts[0]}.{parts[1]}.{third_part}.0"
        except ValueError:
            return f"{parts[0]}.{parts[1]}.{parts[2]}.0"
    elif len(parts) == 4:
        return version_str
    else:
        return "1.0.0.0"

def create_dynamic_nsi():
    """동적 NSIS 스크립트 생성 (파일명 포함)"""
    print("3. 동적 NSIS 스크립트 생성 중...")
    
    # 원본 NSI 파일 읽기
    original_nsi = Path(NSIS_SCRIPT)
    if not original_nsi.exists():
        print(f"   오류: NSIS 스크립트를 찾을 수 없습니다: {NSIS_SCRIPT}")
        return False, None
    
    with open(original_nsi, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 새로운 파일명 형식으로 변경
    setup_filename = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe"
    nsis_version = convert_version_format(PRODUCT_VERSION)
    
    # OutFile 라인과 버전 라인 교체
    lines = content.split('\n')
    for i, line in enumerate(lines):
        if line.startswith('OutFile '):
            lines[i] = f'OutFile "{setup_filename}"'
        elif line.startswith('!define PRODUCT_VERSION '):
            lines[i] = f'!define PRODUCT_VERSION "{nsis_version}"'
    
    # 임시 NSI 파일 생성
    temp_nsi = Path("temp_installer.nsi")
    with open(temp_nsi, 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines))
    
    print(f"   설치파일명: {setup_filename}")
    print(f"   NSIS 버전: {nsis_version}")
    print()
    return True, temp_nsi

def build_installer(nsi_file):
    """NSIS 설치파일 빌드"""
    print("4. NSIS 설치파일 빌드 중...")
    
    try:
        result = subprocess.run([
            NSIS_PATH, str(nsi_file)
        ], capture_output=True, text=True, encoding='utf-8', check=True)
        
        print("   NSIS 빌드가 성공적으로 완료되었습니다.")
        print()
        return True
        
    except subprocess.CalledProcessError as e:
        print("   오류: NSIS 빌드 실패!")
        print(f"   오류 메시지: {e.stderr}")
        return False

def cleanup_temp_files(temp_nsi):
    """임시 파일 정리"""
    if temp_nsi and temp_nsi.exists():
        temp_nsi.unlink()

def print_completion_info():
    """완료 정보 출력"""
    setup_filename = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe"
    
    print("=" * 60)
    print("빌드가 성공적으로 완료되었습니다!")
    print("=" * 60)
    print()
    print("생성된 파일들:")
    print(f"- {setup_filename} (NSIS 설치파일)")
    print("- publish\\framework-dependent\\        (애플리케이션 파일들)")
    print()
    print("설치 기능:")
    print("- C:\\Program Files\\CreateNewFile에 설치")
    print("- 데스크톱 및 시작 메뉴 바로가기 생성")
    print("- 선택적 자동 시작 (기본 비활성화)")
    print("- 깨끗한 제거 옵션")
    print()
    print("시스템 요구사항:")
    print("- Windows 10 이상")
    print("- .NET 8.0 Runtime (사용자에게 안내됨)")
    print()

def main():
    """메인 함수"""
    print_header()
    
    # 1. NSIS 설치 확인
    if not check_nsis():
        input("계속하려면 Enter 키를 누르세요...")
        sys.exit(1)
    
    # 2. 필수 파일들 확인
    if not check_required_files():
        input("계속하려면 Enter 키를 누르세요...")
        sys.exit(1)
    
    # 3. 동적 NSIS 스크립트 생성
    success, temp_nsi = create_dynamic_nsi()
    if not success:
        input("계속하려면 Enter 키를 누르세요...")
        sys.exit(1)
    
    try:
        # 4. NSIS 빌드
        if not build_installer(temp_nsi):
            input("계속하려면 Enter 키를 누르세요...")
            sys.exit(1)
        
        # 완료 정보 출력
        print_completion_info()
        
    finally:
        # 임시 파일 정리
        cleanup_temp_files(temp_nsi)
    
    input("계속하려면 Enter 키를 누르세요...")

if __name__ == "__main__":
    main()