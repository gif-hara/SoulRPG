using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Localization.Editor;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace SoulRPG
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CheckLocalization
    {
        [MenuItem("SoulRPG/Check Localization")]
        public static void Check()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Please play the game to check localization.");
                return;
            }
            const string filePath = "Assets/SoulRPG/Editor/japanese_all.txt";
            if (!File.Exists(filePath))
            {
                Debug.LogError("File not found: " + filePath);
                return;
            }

            var table = LocalizationSettings.StringDatabase.GetTable("TextData");
            var file = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            var lines = file.text.Split('\n');
            var sb = new System.Text.StringBuilder();
            var addedLines = new HashSet<string>();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var entry = table.GetEntry(line);
                if (entry == null)
                {
                    if (addedLines.Contains(line))
                    {
                        continue;
                    }
                    sb.AppendLine(line);
                    addedLines.Add(line);
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}
