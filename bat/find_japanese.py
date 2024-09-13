import os
import re
import chardet

# 日本語を含む行を検出する正規表現パターン
pattern = re.compile(r'^(?!/).*(?:[ぁ-んァ-ン一-龯]|\\u[3040-9FAF]{4}).*')

# 対象とする拡張子のリスト
target_extensions = ['.txt', '.asset', '.scene', '.prefab', '.cs']

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
    """Assetsフォルダ内のファイルから日本語を含む行を列挙し、utf-8ならjapanese_utf8.txtに、それ以外はjapanese.txtに保存する"""
    # このスクリプトが配置されているbatフォルダのパスを取得
    script_directory = os.path.dirname(os.path.abspath(__file__))
    parent_directory = os.path.dirname(script_directory)

    # batフォルダと同じディレクトリにあるAssetsフォルダのパスを取得
    assets_directory = os.path.join(parent_directory, 'Assets')

    # 出力ファイルのパスを設定
    output_file_utf8 = os.path.join(script_directory, 'japanese_utf8.txt')
    output_file_non_utf8 = os.path.join(script_directory, 'japanese.txt')

    utf8_results = []
    non_utf8_results = []
    
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
                                # Unicodeエスケープ文字列を見やすくする処理
                                decoded_line = decode_unicode_escape_if_needed(line.strip())
                                if encoding == 'utf-8':
                                    utf8_results.append(f"ファイル: {file_path}, 行番号: {line_number}, 内容: {decoded_line}")
                                else:
                                    non_utf8_results.append(f"ファイル: {file_path}, 行番号: {line_number}, 内容: {decoded_line}")
                except Exception as e:
                    non_utf8_results.append(f"ファイルの読み込み中にエラーが発生しました: {file_path}, エラー: {e}")
    
    # UTF-8の結果をファイルに保存（batフォルダ内）
    with open(output_file_utf8, 'w', encoding='utf-8') as output_utf8:
        for result in utf8_results:
            output_utf8.write(result + '\n')

    # UTF-8以外の結果をファイルに保存（batフォルダ内）
    with open(output_file_non_utf8, 'w', encoding='utf-8') as output_non_utf8:
        for result in non_utf8_results:
            output_non_utf8.write(result + '\n')
    
    print(f"UTF-8のファイルの結果を {output_file_utf8} に保存しました。")
    print(f"その他のファイルの結果を {output_file_non_utf8} に保存しました。")

# 使用例
find_japanese_in_assets_folder()
