using System.Collections.Generic;
using System.IO;
using FutureHeroQuest.Core;
using FutureHeroQuest.Level;
using FutureHeroQuest.Players;
using FutureHeroQuest.UI;
using Photon.Pun;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FutureHeroQuest.EditorTools
{
    /// <summary>
    /// Creates the minimum playable Photon capsule test setup.
    /// Safe to rerun from the FHQ/Regenerate Network Demo menu.
    /// </summary>
    public static class FhqProjectBootstrapper
    {
        private const string LauncherScenePath = "Assets/Scenes/Launcher.unity";
        private const string LevelScenePath = "Assets/Scenes/Level01_Tree.unity";
        private const string BridgeLevelScenePath = "Assets/Scenes/Level01_Bridge.unity";
        private const string ArchiveLevelScenePath = "Assets/Scenes/Level02_Archive.unity";
        private const string ClubRoomLevelScenePath = "Assets/Scenes/Level03_ClubRoom.unity";
        private const string PastPrefabPath = "Assets/Prefabs/Resources/PastPlayer.prefab";
        private const string FuturePrefabPath = "Assets/Prefabs/Resources/FuturePlayer.prefab";

        private static bool _queued;

        [InitializeOnLoadMethod]
        private static void QueueAutoBootstrap()
        {
            if (_queued) return;
            _queued = true;
            EditorApplication.delayCall += AutoBootstrapIfNeeded;
        }

        [MenuItem("FHQ/Regenerate Network Demo")]
        public static void RegenerateNetworkDemo()
        {
            GenerateNetworkDemo();
        }

        [MenuItem("FHQ/Build Windows Network Demo")]
        public static void BuildWindowsNetworkDemo()
        {
            GenerateNetworkDemo();

            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../FHQ-Workspace/build/NetworkDemoWin/FutureHeroQuest.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var report = BuildPipeline.BuildPlayer(
                GetBuildScenePaths().ToArray(),
                output,
                BuildTarget.StandaloneWindows64,
                BuildOptions.Development);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[FHQ] Windows network demo build succeeded: {output}");
            }
            else
            {
                Debug.LogError($"[FHQ] Windows network demo build failed: {report.summary.result}");
            }
        }

        private static void AutoBootstrapIfNeeded()
        {
            if (!Directory.Exists("Assets/Photon")) return;
            if (File.Exists(LauncherScenePath) && File.Exists(LevelScenePath) && File.Exists(PastPrefabPath) && File.Exists(FuturePrefabPath)) return;
            GenerateNetworkDemo();
        }

        private static void GenerateNetworkDemo()
        {
            EnsureFolders();
            CreatePlayerPrefab(PastPrefabPath, "PastPlayer", new Color(0.35f, 0.58f, 1.0f));
            CreatePlayerPrefab(FuturePrefabPath, "FuturePlayer", new Color(0.2f, 0.95f, 0.75f));
            CreateLauncherScene();
            CreateLevelScene();
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string scenePath in GetBuildScenePaths())
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(LauncherScenePath);
            Debug.Log("[FHQ] Network demo bootstrap complete. Open Launcher, enter Play, then Create/Join room.");
        }

        private static List<string> GetBuildScenePaths()
        {
            var scenePaths = new List<string> { LauncherScenePath };
            scenePaths.Add(File.Exists(BridgeLevelScenePath) ? BridgeLevelScenePath : LevelScenePath);

            if (File.Exists(ArchiveLevelScenePath))
                scenePaths.Add(ArchiveLevelScenePath);
            else
                Debug.LogWarning($"[FHQ] Build scene missing: {ArchiveLevelScenePath}");

            if (File.Exists(ClubRoomLevelScenePath))
                scenePaths.Add(ClubRoomLevelScenePath);
            else
                Debug.LogWarning($"[FHQ] Build scene missing: {ClubRoomLevelScenePath}");

            return scenePaths;
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Prefabs/Resources");
            Directory.CreateDirectory("Assets/Materials");
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (shader != null && material.shader != shader)
            {
                material.shader = shader;
            }
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreatePlayerPrefab(string path, string prefabName, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = prefabName;
            go.transform.position = Vector3.zero;

            var capsuleCollider = go.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null) Object.DestroyImmediate(capsuleCollider);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = CreateMaterial($"{prefabName}_Mat", color);

            var controller = go.AddComponent<CharacterController>();
            controller.height = 2.0f;
            controller.radius = 0.35f;
            controller.center = Vector3.up;

            var photonView = go.AddComponent<PhotonView>();
            var transformView = go.AddComponent<PhotonTransformView>();
            go.AddComponent<PlayerController>();

            photonView.ObservedComponents = new List<Component> { transformView };
            photonView.Synchronization = ViewSynchronization.UnreliableOnChange;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateLauncherScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Launcher";

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
            camera.transform.position = new Vector3(0, 4, -8);
            camera.transform.rotation = Quaternion.Euler(25, 0, 0);

            var light = new GameObject("Directional Light");
            light.AddComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            var network = new GameObject("NetworkManager");
            network.AddComponent<NetworkManager>();

            var bus = new GameObject("TimelineEventBus");
            var busPhotonView = bus.AddComponent<PhotonView>();
            busPhotonView.sceneViewId = 1;
            bus.AddComponent<TimelineEventBus>();

            CreateLauncherCanvas();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, LauncherScenePath);
        }

        private static void CreateLauncherCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var title = CreateText(canvasGo.transform, "Title", "Future Hero Quest", 34, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(520, 60));

            var status = CreateText(canvasGo.transform, "StatusText", "State: starting", 18, TextAnchor.MiddleCenter);
            SetRect(status.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 68), new Vector2(520, 40));

            var create = CreateButton(canvasGo.transform, "CreateButton", "Create Room");
            SetRect(create.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-95, 0), new Vector2(170, 48));

            var join = CreateButton(canvasGo.transform, "JoinButton", "Join Room");
            SetRect(join.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(95, 0), new Vector2(170, 48));

            var hint = CreateText(canvasGo.transform, "Hint", "Run two clients. First clicks Create Room, second clicks Join Room.", 16, TextAnchor.MiddleCenter);
            SetRect(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(760, 44));

            var connectUi = canvasGo.AddComponent<ConnectUI>();
            var so = new SerializedObject(connectUi);
            so.FindProperty("createButton").objectReferenceValue = create;
            so.FindProperty("joinButton").objectReferenceValue = join;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.34f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText(go.transform, "Text", label, 18, TextAnchor.MiddleCenter);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.color = Color.white;
            return button;
        }

        private static Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void CreateLevelScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level01_Tree";

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 7f;
            camera.transform.position = new Vector3(0, 10, -8);
            camera.transform.rotation = Quaternion.Euler(55, 0, 0);

            var light = new GameObject("Directional Light");
            light.AddComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(55, -35, 0);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2.0f, 1, 2.0f);
            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = CreateMaterial("Ground_Mat", new Color(0.24f, 0.36f, 0.27f));

            var levelManager = new GameObject("LevelManager");
            var levelPhotonView = levelManager.AddComponent<PhotonView>();
            levelPhotonView.sceneViewId = 2;
            levelManager.AddComponent<LevelManager>();

            var spawner = new GameObject("PlayerSpawner");
            var pastSpawn = new GameObject("PastSpawnPoint");
            pastSpawn.transform.position = new Vector3(-2.5f, 1.1f, 0);
            var futureSpawn = new GameObject("FutureSpawnPoint");
            futureSpawn.transform.position = new Vector3(2.5f, 1.1f, 0);
            pastSpawn.transform.SetParent(spawner.transform);
            futureSpawn.transform.SetParent(spawner.transform);

            var playerSpawner = spawner.AddComponent<PlayerSpawner>();
            var so = new SerializedObject(playerSpawner);
            so.FindProperty("pastSpawnPoint").objectReferenceValue = pastSpawn.transform;
            so.FindProperty("futureSpawnPoint").objectReferenceValue = futureSpawn.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            CreateLevelHud();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, LevelScenePath);
        }

        private static void CreateLevelHud()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var text = CreateText(canvasGo.transform, "HudText", "WASD / Arrow Keys to move. R resets on host.", 16, TextAnchor.UpperLeft);
            SetRect(text.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(260, -26), new Vector2(500, 36));
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
