#!/usr/bin/env python3
"""
CreateNewFile 전체 빌드 스크립트
프로젝트 업데이트부터 NSIS 설치파일 생성까지 전체 과정을 자동으로 수행합니다.
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
BUILD_DATE = "20250930_1613"
PROJECT_FILE = "CreateNewFile.csproj"
NSIS_SCRIPT = "CreateNewFile_Installer.nsi"
NSIS_PATH = r"C:\Program Files (x86)\NSIS\makensis.exe"

def print_header():
    """헤더 출력"""
    print("=" * 70)
    print("CreateNewFile 전체 빌드 프로세스")
    print("프로젝트 업데이트 → NSIS 설치파일 생성")
    print("=" * 70)
    print()
    print(f"제품명: {PRODUCT_NAME}")
    print(f"버전: {PRODUCT_VERSION}")
    print(f"빌드 일시: {BUILD_DATE}")
    print(f"현재 디렉터리: {os.getcwd()}")
    print()

def step_1_clean_publish():
    """단계 1: 이전 빌드 정리"""
    print("=" * 50)
    print("단계 1: 이전 빌드 정리")
    print("=" * 50)
    
    publish_dir = Path("publish")
    if publish_dir.exists():
        shutil.rmtree(publish_dir)
        print("기존 publish 폴더를 삭제했습니다.")
    
    # 디렉터리 생성
    publish_dir.mkdir()
    (publish_dir / "framework-dependent").mkdir()
    print("새로운 publish 폴더를 생성했습니다.")
    print()
    return True

def step_2_build_project():
    """단계 2: 프로젝트 빌드"""
    print("=" * 50)
    print("단계 2: 프로젝트 빌드 및 퍼블리시")
    print("=" * 50)
    
    project_dir = Path("../CreateNewFile")
    if not project_dir.exists():
        print("오류: 프로젝트 디렉터리를 찾을 수 없습니다.")
        return False
    
    # 현재 디렉터리 변경
    os.chdir(project_dir)
    
    try:
        # Clean
        print("Clean 수행 중...")
        result = subprocess.run([
            "dotnet", "clean", PROJECT_FILE, "-c", "Release"
        ], capture_output=True, text=True, encoding='utf-8', check=True)
        
        # Publish
        print("Publish 수행 중...")
        result = subprocess.run([
            "dotnet", "publish", PROJECT_FILE, 
            "-c", "Release", 
            "-o", "../NSIS_installer/publish/framework-dependent"
        ], capture_output=True, text=True, encoding='utf-8', check=True)
        
        print("프로젝트 빌드가 성공적으로 완료되었습니다.")
        
    except subprocess.CalledProcessError as e:
        print(f"오류: 프로젝트 빌드 실패!")
        print(f"{e}")
        return False
    
    finally:
        # 원래 디렉터리로 복귀
        os.chdir("../NSIS_installer")
    
    print()
    return True

def step_3_copy_icon():
    """단계 3: 아이콘 파일 복사"""
    print("=" * 50)
    print("단계 3: 아이콘 파일 복사")
    print("=" * 50)
    
    source_icon = Path("../CreateNewFile/Resources/CreateNewFile.ico")
    dest_dir = Path("publish/framework-dependent/Resources")
    dest_icon = dest_dir / "CreateNewFile.ico"
    
    if not source_icon.exists():
        print(f"오류: 소스 아이콘 파일을 찾을 수 없습니다: {source_icon}")
        return False
    
    # Resources 디렉터리 생성
    dest_dir.mkdir(parents=True, exist_ok=True)
    
    # 아이콘 파일 복사
    shutil.copy2(source_icon, dest_icon)
    print(f"✓ 아이콘 파일을 복사했습니다: {dest_icon}")
    print()
    return True

def step_4_verify_files():
    """단계 4: 파일 확인"""
    print("=" * 50)
    print("단계 4: 생성된 파일들 확인")
    print("=" * 50)
    
    required_files = [
        ("publish/framework-dependent/CreateNewFile.exe", "실행파일"),
        ("publish/framework-dependent/Resources/CreateNewFile.ico", "아이콘 파일"),
        ("LICENSE.txt", "라이센스 파일")
    ]
    
    for file_path, description in required_files:
        if not Path(file_path).exists():
            print(f"오류: {description}을 찾을 수 없습니다: {file_path}")
            return False
        print(f"✓ {description}: {file_path}")
    
    print("모든 필수 파일이 정상적으로 생성되었습니다.")
    print()
    return True

def step_5_check_nsis():
    """단계 5: NSIS 확인"""
    print("=" * 50)
    print("단계 5: NSIS 설치 확인")
    print("=" * 50)
    
    if not Path(NSIS_PATH).exists():
        print("오류: NSIS를 찾을 수 없습니다!")
        print("기본 경로에서 NSIS를 찾을 수 없습니다.")
        print("다음 사이트에서 NSIS를 다운로드하세요: https://nsis.sourceforge.io/Download")
        print("또는 스크립트에서 NSIS_PATH를 올바른 경로로 수정하세요.")
        return False
    
    print("✓ NSIS를 찾았습니다!")
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

def step_6_create_dynamic_nsi():
    """단계 6: 동적 NSIS 스크립트 생성"""
    print("=" * 50)
    print("단계 6: 동적 NSIS 스크립트 생성")
    print("=" * 50)
    
    # 원본 NSI 파일 읽기
    original_nsi = Path(NSIS_SCRIPT)
    if not original_nsi.exists():
        print(f"오류: NSIS 스크립트를 찾을 수 없습니다: {NSIS_SCRIPT}")
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
    
    print(f"✓ 설치파일명: {setup_filename}")
    print(f"✓ NSIS 버전: {nsis_version}")
    print("✓ 임시 NSIS 스크립트를 생성했습니다.")
    print()
    return True, temp_nsi

def step_7_build_installer(nsi_file):
    """단계 7: NSIS 설치파일 빌드"""
    print("=" * 50)
    print("단계 7: NSIS 설치파일 빌드")
    print("=" * 50)
    
    try:
        result = subprocess.run([
            NSIS_PATH, str(nsi_file)
        ], capture_output=True, text=True, encoding='utf-8', check=True)
        
        print("✓ NSIS 설치파일 빌드가 성공적으로 완료되었습니다.")
        print()
        return True
        
    except subprocess.CalledProcessError as e:
        print("오류: NSIS 빌드 실패!")
        print(f"오류 메시지: {e.stderr}")
        return False

def cleanup_temp_files(temp_nsi):
    """임시 파일 정리"""
    if temp_nsi and temp_nsi.exists():
        temp_nsi.unlink()

def print_final_summary():
    """최종 완료 요약"""
    setup_filename = f"{PRODUCT_NAME}_v{PRODUCT_VERSION}_Build_{BUILD_DATE}_Setup.exe"
    
    print("=" * 70)
    print("🎉 전체 빌드가 성공적으로 완료되었습니다! 🎉")
    print("=" * 70)
    print()
    print("생성된 파일들:")
    print(f"📦 {setup_filename}")
    print("   → Windows 설치 파일 (NSIS)")
    print("📁 publish\\framework-dependent\\")
    print("   → .NET Framework-dependent 애플리케이션 파일들")
    print()
    print("설치 파일 정보:")
    print(f"• 제품명: {PRODUCT_NAME}")
    print(f"• 버전: {PRODUCT_VERSION}")
    print(f"• 빌드 일시: {BUILD_DATE}")
    print("• 설치 경로: C:\\Program Files\\CreateNewFile")
    print("• 바로가기: 데스크톱 및 시작 메뉴")
    print("• 자동 시작: 선택사항 (기본 비활성화)")
    print()
    print("시스템 요구사항:")
    print("• Windows 10 이상")
    print("• .NET 8.0 Runtime")
    print()

def main():
    """메인 함수"""
    print_header()
    
    temp_nsi = None
    
    try:
        # 단계별 실행
        steps = [
            ("이전 빌드 정리", step_1_clean_publish),
            ("프로젝트 빌드", step_2_build_project),
            ("아이콘 복사", step_3_copy_icon),
            ("파일 확인", step_4_verify_files),
            ("NSIS 확인", step_5_check_nsis)
        ]
        
        for step_name, step_func in steps:
            if not step_func():
                print(f"❌ {step_name} 단계에서 실패했습니다.")
                input("계속하려면 Enter 키를 누르세요...")
                sys.exit(1)
        
        # 동적 NSI 생성
        success, temp_nsi = step_6_create_dynamic_nsi()
        if not success:
            print("❌ 동적 NSIS 스크립트 생성에 실패했습니다.")
            input("계속하려면 Enter 키를 누르세요...")
            sys.exit(1)
        
        # NSIS 빌드
        if not step_7_build_installer(temp_nsi):
            print("❌ NSIS 설치파일 빌드에 실패했습니다.")
            input("계속하려면 Enter 키를 누르세요...")
            sys.exit(1)
        
        # 최종 완료 요약
        print_final_summary()
        
    except KeyboardInterrupt:
        print("\n\n⚠️  사용자에 의해 중단되었습니다.")
    except Exception as e:
        print(f"\n\n❌ 예상치 못한 오류가 발생했습니다: {e}")
    finally:
        # 임시 파일 정리
        cleanup_temp_files(temp_nsi)
    
    input("계속하려면 Enter 키를 누르세요...")

if __name__ == "__main__":
    main()