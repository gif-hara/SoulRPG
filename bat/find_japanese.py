import os
import re
import chardet
import csv

# 日本語を含む行を検出する正規表現パターン
pattern = re.compile(r'^(?!/).*(?:[ぁ-んァ-ン一-龯]|\\u[3040-9FAF]{4}).*')
# ダブルクオーテーション内の文字列を抽出するパターン
quote_pattern = re.compile(r'"(.*?)"')

# 対象とする拡張子のリスト
target_extensions = ['.txt', '.asset', '.scene', '.prefab', '.cs']
# デコード対象の拡張子リスト
decode_extensions = ['.asset', '.scene', '.prefab']

def detect_file_encoding(file_path):
    """ファイルのエンコーディングを検出する"""
    with open(file_path, 'rb') as f:
        result = chardet.detect(f.read())
    return result['encoding']

def decode_unicode_escape_if_needed(line):
    """Unicodeエスケープ文字列を適切にデコードし、人間が読みやすい形式に変換"""
    try:
        return line.encode('utf-8').decode('unicode-escape')
    except UnicodeDecodeError:
        return line

def find_japanese_in_assets_folder():
    """Assetsフォルダ内のファイルから日本語を含む行を列挙し、CSV形式で保存する"""
    # このスクリプトが配置されているbatフォルダのパスを取得
    script_directory = os.path.dirname(os.path.abspath(__file__))
    parent_directory = os.path.dirname(script_directory)

    # batフォルダと同じディレクトリにあるAssetsフォルダのパスを取得
    assets_directory = os.path.join(parent_directory, 'Assets')

    # 出力ファイルのパスを設定（CSVファイルとして保存）
    output_file = os.path.join(script_directory, 'japanese.csv')

    results = []
    seen_strings = set()  # 重複防止用のセット
    
    # Assetsフォルダ内のファイルを再帰的に探索
    for root, _, files in os.walk(assets_directory):
        for file in files:
            # ファイル名に「SDF」が含まれている場合はスキップ
            if 'SDF' in file:
                continue
            
            # ファイルの拡張子をチェック
            _, ext = os.path.splitext(file)
            if ext.lower() in target_extensions:
                file_path = os.path.join(root, file)
                try:
                    # ファイルのエンコーディングを検出
                    encoding = detect_file_encoding(file_path)
                    with open(file_path, 'r', encoding=encoding) as f:
                        for line_number, line in enumerate(f, start=1):
                            # 日本語を含む行を検索
                            if pattern.search(line):
                                # ダブルクオーテーション内の文字列を抽出
                                matches = quote_pattern.findall(line)
                                if matches:
                                    # デコード対象の拡張子の場合はデコード処理を適用
                                    if ext.lower() in decode_extensions:
                                        matches = [decode_unicode_escape_if_needed(m) for m in matches]
                                    # 重複を避けるため、文字列が既に出力されていないか確認
                                    for match in matches:
                                        if match not in seen_strings:
                                            # 重複していない場合のみ追加
                                            relative_path = os.path.relpath(file_path, assets_directory)
                                            results.append([relative_path, match])
                                            seen_strings.add(match)
                except Exception as e:
                    relative_path = os.path.relpath(file_path, assets_directory)
                    results.append([relative_path, f"エラー: {e}"])
    
    # CSVファイルに結果を保存（batフォルダ内）
    with open(output_file, 'w', newline='', encoding='utf-8') as csvfile:
        csvwriter = csv.writer(csvfile)
        # ヘッダー行を追加
        csvwriter.writerow(['FileName', 'JapaneseString'])
        # 結果をCSVに書き込み
        csvwriter.writerows(results)
    
    print(f"検索結果を {output_file} に保存しました。")

# 使用例
find_japanese_in_assets_folder()
