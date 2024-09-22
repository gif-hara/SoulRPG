import os
import re
import chardet

# スクリプトが配置されているディレクトリをカレントディレクトリに設定
script_directory = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_directory)

# Assetsフォルダのパスを指定
assets_folder = '../Assets'

# 出力ファイルのパス (Assets/SoulRPG/Editor/japanese_all.txt に変更)
output_file = '../Assets/SoulRPG/Editor/japanese_all.txt'

# 日本語文字を検出する正規表現パターン
japanese_pattern = re.compile(r'[\u3040-\u30FF\u4E00-\u9FFF\uFF66-\uFF9F]+')

# 検索対象とする拡張子のリスト
target_extensions = ['.cs', '.asset', '.prefab', '.scene']

def detect_encoding(file_path):
    """ファイルのエンコーディングを検出する"""
    with open(file_path, 'rb') as f:
        result = chardet.detect(f.read())
        return result['encoding']

def find_japanese_in_file(file_path):
    """ファイル内で日本語を含む行を探す"""
    encoding = detect_encoding(file_path)
    
    try:
        with open(file_path, 'r', encoding=encoding) as f:
            lines = f.readlines()
    except Exception:
        # エンコーディング検出や読み込みが失敗した場合、UTF-8 で再試行
        with open(file_path, 'r', encoding='utf-8', errors='replace') as f:
            lines = f.readlines()

    extracted_strings = []
    for line in lines:
        # Unicode エスケープシーケンスをデコード
        decoded_line = line.encode('utf-8').decode('unicode-escape')

        # 日本語が含まれている行のみチェック
        if japanese_pattern.search(decoded_line):
            # ダブルクォートで囲まれた部分を抽出
            matches = re.findall(r'"(.*?)"', decoded_line)
            extracted_strings.extend(matches)  # 抽出された文字列をリストに追加

    return extracted_strings

def is_text_file(file_path):
    """ファイルがテキストファイルかどうかをチェックする"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            f.read()
        return True
    except:
        return False

def find_japanese_in_assets(folder_path, output_file):
    """Assetsフォルダ内の全てのファイルをチェックし、日本語を含む行を出力ファイルに記録する"""
    # 出力先ディレクトリが存在しない場合、ディレクトリを作成
    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    with open(output_file, 'w', encoding='utf-8') as out_file:
        for root, dirs, files in os.walk(folder_path):
            for file in files:
                file_path = os.path.join(root, file)
                
                # "Font" および "Localization" がパスに含まれている場合は無視
                if 'Font' in file_path or 'Localization' in file_path:
                    continue
                
                # 指定された拡張子のみを対象にする
                if not any(file.endswith(ext) for ext in target_extensions):
                    continue
                
                if is_text_file(file_path):  # テキストファイルのみを対象にする
                    try:
                        extracted_strings = find_japanese_in_file(file_path)
                        if extracted_strings:
                            for string in extracted_strings:
                                out_file.write(f'{string}\n')  # 抽出した文字列のみを書き込む
                    except Exception as e:
                        out_file.write(f'エラー: {file_path} の処理中に問題が発生しました: {e}\n')

if __name__ == "__main__":
    find_japanese_in_assets(assets_folder, output_file)
