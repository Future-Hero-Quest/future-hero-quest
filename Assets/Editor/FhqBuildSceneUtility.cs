using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FutureHeroQuest.EditorTools
{
    internal static class FhqBuildSceneUtility
    {
        private static readonly string[] FinalScenePaths =
        {
            "Assets/Scenes/Launcher.unity",
            "Assets/Scenes/Level01_Bridge.unity",
            "Assets/Scenes/Level02_Archive.unity",
            "Assets/Scenes/Level03_ClubRoom.unity"
        };

        public static bool TryGetFinalScenePathsForBuild(out string[] scenePaths)
        {
            scenePaths = GetExistingFinalScenePaths(logMissingAsError: true);
            if (scenePaths.Length == FinalScenePaths.Length) return true;

            Debug.LogError("[FHQ] Windows build aborted: final build scene list is incomplete.");
            return false;
        }

        public static void ApplyFinalBuildSettings()
        {
            string[] scenePaths = GetExistingFinalScenePaths(logMissingAsError: false);
            var scenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            EditorBuildSettings.scenes = scenes;
        }

        private static string[] GetExistingFinalScenePaths(bool logMissingAsError)
        {
            var scenePaths = new List<string>(FinalScenePaths.Length);
            foreach (string scenePath in FinalScenePaths)
            {
                if (File.Exists(scenePath))
                {
                    scenePaths.Add(scenePath);
                    continue;
                }

                string message = $"[FHQ] Final build scene missing: {scenePath}";
                if (logMissingAsError) Debug.LogError(message);
                else Debug.LogWarning(message);
            }

            return scenePaths.ToArray();
        }
    }
}
