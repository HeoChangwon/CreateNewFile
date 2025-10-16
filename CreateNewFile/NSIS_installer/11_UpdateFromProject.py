#!/usr/bin/env python3
"""
CreateNewFile 프로젝트 업데이트 스크립트
프로젝트를 빌드하고 NSIS 설치파일 생성을 위한 publish 폴더를 생성합니다.
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path

# ==========================================
# 설정 (필요시 수정)
# ==========================================
PRODUCT_NAME = "CreateNewFile"
PRODUCT_VERSION = "1.0.003"
BUILD_DATE = "20251016_1422"
PROJECT_FILE = "CreateNewFile.csproj"

def print_header():
    """헤더 출력"""
    print("=" * 50)
    print("CreateNewFile 프로젝트 업데이트")
    print("=" * 50)
    print()

def clean_publish_folder():
    """기존 publish 폴더 정리"""
    print("1. 이전 빌드 정리 중...")
    
    publish_dir = Path("publish")
    if publish_dir.exists():
        shutil.rmtree(publish_dir)
        print("   기존 publish 폴더를 삭제했습니다.")
    
    # 디렉터리 생성
    publish_dir.mkdir()
    (publish_dir / "framework-dependent").mkdir()
    print("   새로운 publish 폴더를 생성했습니다.")
    print()

def build_project():
    """프로젝트 빌드 및 퍼블리시"""
    print("2. 프로젝트 빌드 및 퍼블리시 중...")
    
    project_dir = Path("../CreateNewFile")
    if not project_dir.exists():
        print("   오류: 프로젝트 디렉터리를 찾을 수 없습니다.")
        return False
    
    # 현재 디렉터리 변경
    os.chdir(project_dir)
    
    try:
        # Clean
        print("   - Clean 수행 중...")
        result = subprocess.run([
            "dotnet", "clean", PROJECT_FILE, "-c", "Release"
        ], capture_output=True, text=True, encoding='utf-8', check=True)
        
        # Publish
        print("   - Publish 수행 중...")
        result = subprocess.run([
            "dotnet", "publish", PROJECT_FILE, 
            "-c", "Release", 
            "-o", "../NSIS_installer/publish/framework-dependent"
        ], capture_output=True, text=True, encoding='utf-8', check=True)
        
        print("   빌드가 성공적으로 완료되었습니다.")
        
    except subprocess.CalledProcessError as e:
        print(f"   오류: 빌드 실패!")
        print(f"   {e}")
        return False
    
    finally:
        # 원래 디렉터리로 복귀
        os.chdir("../NSIS_installer")
    
    print()
    return True

def copy_icon_file():
    """아이콘 파일 복사"""
    print("3. 아이콘 파일 복사 중...")
    
    source_icon = Path("../CreateNewFile/Resources/CreateNewFile.ico")
    dest_dir = Path("publish/framework-dependent/Resources")
    dest_icon = dest_dir / "CreateNewFile.ico"
    
    if not source_icon.exists():
        print(f"   오류: 소스 아이콘 파일을 찾을 수 없습니다: {source_icon}")
        return False
    
    # Resources 디렉터리 생성
    dest_dir.mkdir(parents=True, exist_ok=True)
    
    # 아이콘 파일 복사
    shutil.copy2(source_icon, dest_icon)
    print(f"   아이콘 파일을 복사했습니다: {dest_icon}")
    print()
    return True

def verify_files():
    """생성된 파일들 확인"""
    print("4. 생성된 파일들 확인 중...")
    
    required_files = [
        "publish/framework-dependent/CreateNewFile.exe",
        "publish/framework-dependent/Resources/CreateNewFile.ico"
    ]
    
    for file_path in required_files:
        if not Path(file_path).exists():
            print(f"   오류: 필수 파일을 찾을 수 없습니다: {file_path}")
            return False
    
    print("   모든 필수 파일이 정상적으로 생성되었습니다.")
    print()
    return True

def main():
    """메인 함수"""
    print_header()
    
    # 1. 이전 빌드 정리
    clean_publish_folder()
    
    # 2. 프로젝트 빌드
    if not build_project():
        print("빌드 실패!")
        input("계속하려면 Enter 키를 누르세요...")
        sys.exit(1)
    
    # 3. 아이콘 파일 복사
    if not copy_icon_file():
        print("아이콘 파일 복사 실패!")
        input("계속하려면 Enter 키를 누르세요...")
        sys.exit(1)
    
    # 4. 파일 확인
    if not verify_files():
        print("파일 확인 실패!")
        input("계속하려면 Enter 키를 누르세요...")
        sys.exit(1)
    
    # 완료 메시지
    print("=" * 50)
    print("업데이트가 성공적으로 완료되었습니다!")
    print("=" * 50)
    print()
    print("다음 단계:")
    print("- 설치파일 빌드: 12_BuildInstaller.py 실행")
    print("- 전체 빌드: 10_BuildAll.py 실행")
    print()
    
    input("계속하려면 Enter 키를 누르세요...")

if __name__ == "__main__":
    main()