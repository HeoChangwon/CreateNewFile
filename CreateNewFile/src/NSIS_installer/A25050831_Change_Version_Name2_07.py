#!/usr/bin/env python
# -*- coding: utf-8 -*-

import os
import sys
import json
import time
import argparse
import datetime
import subprocess
from pathlib import Path

# 기본 문자열 설정 (이 부분을 수정하여 기본 문자열을 변경할 수 있습니다)
BASE_FILENAME_PREFIX = "A25050831_Change_Version_Name2"
BASE_FILENAME_ABBR = "CVN2"

# 현재 시간 정보 가져오기
current_time = datetime.datetime.now()
timestamp = current_time.strftime("%Y%m%d_%H%M%S")

# 기본 파일명 설정
DEFAULT_FILE_LIST = f"{BASE_FILENAME_PREFIX}_INPUT_FileList.json"
DEFAULT_REPLACE_STRING_LIST = f"{BASE_FILENAME_PREFIX}_INPUT_ReplaceStringList.json"
DEFAULT_LOG_FILE = f"OUTPUT_{timestamp}_{BASE_FILENAME_ABBR}_Log.txt"

# 로그 파일 경로 설정
OUTPUT_DIR = "Output_Result"
if not os.path.exists(OUTPUT_DIR):
    os.makedirs(OUTPUT_DIR)
DEFAULT_LOG_PATH = os.path.join(OUTPUT_DIR, DEFAULT_LOG_FILE)

def setup_argparse():
    """명령행 인자 설정"""
    parser = argparse.ArgumentParser(description=f'파일 내의 문자열을 교체하는 스크립트 - {BASE_FILENAME_PREFIX}')
    parser.add_argument('--no-wait-exit', action='store_true', help='스크립트 종료 전 대기하지 않는 Flag')
    parser.add_argument('--file-list', type=str, default=DEFAULT_FILE_LIST, 
                        help=f'File List JSON 입력파일명 (기본값: {DEFAULT_FILE_LIST})')
    parser.add_argument('--replace-string-list', type=str, default=DEFAULT_REPLACE_STRING_LIST,
                        help=f'Replace String List JSON 입력파일명 (기본값: {DEFAULT_REPLACE_STRING_LIST})')
    parser.add_argument('--log', type=str, default=DEFAULT_LOG_PATH,
                        help=f'Log 파일명 (기본값: {DEFAULT_LOG_PATH})')
    return parser

def load_json_file(filename):
    """JSON 파일 로딩"""
    try:
        with open(filename, 'r', encoding='utf-8') as file:
            return json.load(file)
    except Exception as e:
        print(f"오류: {filename} 파일을 로드하는 데 실패했습니다. - {str(e)}")
        return None

def write_log(log_file, message):
    """로그 파일에 메시지 기록"""
    log_dir = os.path.dirname(log_file)
    if not os.path.exists(log_dir):
        os.makedirs(log_dir)
    
    with open(log_file, 'a', encoding='utf-8') as f:
        f.write(f"{message}\n")
    
    print(message)

def detect_encoding(file_path):
    """파일의 인코딩 감지"""
    encodings = ['utf-8', 'cp949', 'euc-kr', 'ascii', 'latin1']
    
    for encoding in encodings:
        try:
            with open(file_path, 'r', encoding=encoding) as file:
                file.read()
                return encoding
        except UnicodeDecodeError:
            continue
    
    return None

def replace_strings_in_file(file_path, replace_strings, log_file):
    """파일 내의 문자열 교체"""
    try:
        # 파일 인코딩 감지
        encoding = detect_encoding(file_path)
        
        if encoding is None:
            write_log(log_file, f"  경고: {file_path} 파일의 인코딩을 감지할 수 없습니다.")
            return False, []
        
        # 파일 내용 읽기
        with open(file_path, 'r', encoding=encoding) as file:
            content = file.read()
        
        # 문자열 교체 및 교체된 문자열 추적
        original_content = content
        replaced_items = []
        
        for replace_item in replace_strings:
            from_str = replace_item['from']
            to_str = replace_item['to']
            
            if from_str in content:
                content = content.replace(from_str, to_str)
                replaced_items.append(replace_item)
        
        # 변경사항이 있는 경우에만 파일 저장
        if content != original_content:
            with open(file_path, 'w', encoding=encoding) as file:
                file.write(content)
            return True, replaced_items
        else:
            return False, []
    
    except Exception as e:
        write_log(log_file, f"  오류: 파일 처리 중 오류 발생 - {str(e)}")
        return False, []

def process_files(file_list, replace_strings, log_file):
    """파일 리스트 처리"""
    success_count = 0
    fail_count = 0
    
    for item in file_list:
        base_dir = item['base_directory']
        files = item['files']
        
        # 상대 경로 또는 절대 경로 처리
        if base_dir.startswith(('C:', 'D:', 'E:', 'F:', 'G:')):
            base_path = Path(base_dir)
        else:
            base_path = Path(os.path.join(os.path.dirname(os.path.abspath(__file__)), base_dir))
        
        write_log(log_file, f"기준 디렉토리: {base_dir}")
        
        for file_rel_path in files:
            file_path = base_path / file_rel_path
            
            if not file_path.exists():
                write_log(log_file, f"  오류: {file_rel_path} 파일을 찾을 수 없습니다.")
                fail_count += 1
                continue
            
            write_log(log_file, f"  처리: {file_rel_path}")
            
            result, replaced_items = replace_strings_in_file(str(file_path), replace_strings, log_file)
            if result:
                # 교체된 문자열 정보 표시
                for item in replaced_items:
                    write_log(log_file, f"    교체: \"{item['from']}\" → \"{item['to']}\"")
                write_log(log_file, f"    결과: 성공")
                success_count += 1
            else:
                write_log(log_file, f"    결과: 변경사항 없음")
                fail_count += 1
    
    return success_count, fail_count

def main():
    """메인 함수"""
    start_time = time.time()
    
    # 명령행 인자 파싱
    parser = setup_argparse()
    args = parser.parse_args()
    
    # 로그 파일 경로 설정
    log_file = args.log
    log_dir = os.path.dirname(log_file)
    if log_dir and not os.path.exists(log_dir):
        os.makedirs(log_dir)
    
    # 로그 시작
    write_log(log_file, f"스크립트: {os.path.basename(__file__)}")
    write_log(log_file, f"실행 시간: {datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    write_log(log_file, f"파일 리스트: {args.file_list}")
    write_log(log_file, f"문자열 교체 리스트: {args.replace_string_list}")
    write_log(log_file, f"로그 파일: {log_file}")
    write_log(log_file, "-" * 80)
    
    # JSON 파일 로드
    file_list = load_json_file(args.file_list)
    if file_list is None:
        write_log(log_file, f"오류: 파일 리스트를 로드할 수 없습니다: {args.file_list}")
        input("엔터 키를 눌러 종료하세요...")
        sys.exit(1)
    
    replace_strings = load_json_file(args.replace_string_list)
    if replace_strings is None:
        write_log(log_file, f"오류: 문자열 교체 리스트를 로드할 수 없습니다: {args.replace_string_list}")
        input("엔터 키를 눌러 종료하세요...")
        sys.exit(1)
    
    # 파일 처리
    write_log(log_file, "문자열 교체 작업 시작...")
    success_count, fail_count = process_files(file_list, replace_strings, log_file)
    
    # 결과 요약
    end_time = time.time()
    elapsed_time = end_time - start_time
    
    write_log(log_file, "-" * 80)
    write_log(log_file, f"처리 완료: 성공 {success_count}개, 실패 {fail_count}개")
    write_log(log_file, f"처리 시간: {elapsed_time:.2f}초")
    
    # --no-wait-exit 옵션이 사용된 경우에만 로그 파일 열기
    if args.no_wait_exit:
        try:
            if sys.platform == 'win32':
                os.startfile(log_file)
            elif sys.platform == 'darwin':  # macOS
                subprocess.call(['open', log_file])
            else:  # Linux/Unix
                subprocess.call(['xdg-open', log_file])
        except Exception as e:
            print(f"로그 파일을 여는 데 실패했습니다: {str(e)}")
    
    # 기본적으로 종료 전 대기 (--no-wait-exit 옵션 없을 경우)
    if not args.no_wait_exit:
        input("엔터 키를 눌러 종료하세요...")

if __name__ == "__main__":
    main()