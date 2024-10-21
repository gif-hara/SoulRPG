using UnityEditor;
using UnityEngine;

namespace SoulRPG.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BuildSwitcher
    {
        [MenuItem("SoulRPG/Build/UnityRoom")]
        private static void SwitchToUnityRoom()
        {
            string[] symbols = new[]
            {
                "UNITY_POST_PROCESSING_STACK_V2",
            };
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.WebGL, symbols);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            Debug.Log("Switched to UnityRoom");
        }

        [MenuItem("SoulRPG/Build/itch.io")]
        private static void SwitchToItchIo()
        {
            string[] symbols = new[]
            {
                "UNITY_POST_PROCESSING_STACK_V2",
                "USE_LOCALIZATION",
            };
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone, symbols);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            Debug.Log("Switched to itch.io");
        }
    }
}
