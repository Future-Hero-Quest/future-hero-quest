using Photon.Pun;
using UnityEditor;
using UnityEngine;

namespace FutureHeroQuest.EditorTools
{
    internal static class FhqPhotonSetupChecker
    {
        private const string ServerSettingsPath = "Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset";

        [MenuItem("FHQ/Check Photon Setup")]
        public static void CheckPhotonSetup()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ServerSettings>(ServerSettingsPath);
            if (settings == null)
            {
                Debug.LogError($"[FHQ] Photon setup missing. Create local settings with PUN Wizard, then follow docs/PHOTON_SETUP.md. Expected local file: {ServerSettingsPath}");
                return;
            }

            string appId = settings.AppSettings != null ? settings.AppSettings.AppIdRealtime : null;
            if (string.IsNullOrWhiteSpace(appId))
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
                Debug.LogError("[FHQ] Photon App Id Realtime is empty. Fill the AppID sent privately by the project owner. Do not commit PhotonServerSettings.asset.");
                return;
            }

            if (!ServerSettings.IsAppId(appId))
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
                Debug.LogWarning("[FHQ] Photon App Id Realtime is present but does not look like a GUID. Check that it was copied exactly. The value is intentionally not printed.");
                return;
            }

            Debug.Log("[FHQ] Photon setup looks ready. AppID is present locally and was not printed.");
        }

        [MenuItem("FHQ/Select Photon Server Settings")]
        public static void SelectPhotonServerSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ServerSettings>(ServerSettingsPath);
            if (settings == null)
            {
                Debug.LogWarning($"[FHQ] PhotonServerSettings.asset not found locally. Follow docs/PHOTON_SETUP.md and use PUN Wizard to create it.");
                return;
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
