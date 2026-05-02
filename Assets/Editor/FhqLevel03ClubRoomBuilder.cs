using System.Collections.Generic;
using System.IO;
using FutureHeroQuest.Core;
using FutureHeroQuest.Level;
using FutureHeroQuest.Players;
using Photon.Pun;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FutureHeroQuest.EditorTools
{
    public static class FhqLevel03ClubRoomBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level03_ClubRoom.unity";
        private const string LevelDataPath = "Assets/Data/LevelData_Level03_ClubRoom.asset";
        private const string PrefracturedWallPath = "Assets/Prefabs/Level/PrefracturedTestWall.prefab";

        [MenuItem("FHQ/Generate Level 03 Club Room")]
        public static void GenerateLevel03ClubRoom()
        {
            EnsureFolders();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level03_ClubRoom";

            CreateLightingAndCamera();

            var root = new GameObject("Level03_ClubRoom");
            var materials = CreateMaterials();

            CreateRoom(root.transform, materials);
            GameObject futureLockRoot = CreateFutureLockAndDoor(root.transform, materials);
            CreateBilliardsPuzzle(root.transform, materials, futureLockRoot);
            CreateOptionalGlassBreak(root.transform);
            CreateOldClubProps(root.transform, materials);
            CreateSceneManagers();
            CreateHud();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            FhqBuildSceneUtility.ApplyFinalBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FHQ] Generated Level 03 Club Room: {ScenePath}");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Data");
            Directory.CreateDirectory("Assets/Materials");
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                ["floor"] = CreateMaterial("L3_Floor_Mat", new Color(0.22f, 0.24f, 0.22f)),
                ["wall"] = CreateMaterial("L3_Wall_Mat", new Color(0.42f, 0.44f, 0.43f)),
                ["wood"] = CreateMaterial("L3_OldWood_Mat", new Color(0.42f, 0.28f, 0.16f)),
                ["table"] = CreateMaterial("L3_TableFelt_Mat", new Color(0.05f, 0.45f, 0.25f)),
                ["rail"] = CreateMaterial("L3_TableRail_Mat", new Color(0.12f, 0.08f, 0.05f)),
                ["black"] = CreateMaterial("L3_PocketBlack_Mat", Color.black),
                ["white"] = CreateMaterial("L3_BallWhite_Mat", new Color(0.92f, 0.92f, 0.84f)),
                ["red"] = CreateMaterial("L3_StateRed_Mat", new Color(0.85f, 0.18f, 0.14f)),
                ["green"] = CreateMaterial("L3_StateGreen_Mat", new Color(0.16f, 0.9f, 0.38f)),
                ["amber"] = CreateMaterial("L3_PastAmber_Mat", new Color(1.0f, 0.64f, 0.18f)),
                ["cyan"] = CreateMaterial("L3_FutureCyan_Mat", new Color(0.18f, 0.78f, 1.0f)),
                ["door"] = CreateMaterial("L3_Door_Mat", new Color(0.12f, 0.18f, 0.32f)),
                ["exit"] = CreateMaterial("L3_Exit_Mat", new Color(0.45f, 1.0f, 0.55f))
            };
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

        private static void CreateLightingAndCamera()
        {
            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.075f);
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0f, 9.5f, -8.5f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            var light = new GameObject("Directional Light");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        }

        private static void CreateRoom(Transform parent, Dictionary<string, Material> materials)
        {
            CreateCube("Floor", parent, new Vector3(0f, -0.08f, 0f), new Vector3(12f, 0.16f, 9f), materials["floor"], true);
            CreateCube("NorthWall", parent, new Vector3(0f, 1.15f, 4.55f), new Vector3(12.2f, 2.3f, 0.28f), materials["wall"], true);
            CreateCube("SouthWall", parent, new Vector3(0f, 1.15f, -4.55f), new Vector3(12.2f, 2.3f, 0.28f), materials["wall"], true);
            CreateCube("WestWall", parent, new Vector3(-6.05f, 1.15f, 0f), new Vector3(0.28f, 2.3f, 9.2f), materials["wall"], true);
            CreateCube("EastWall_North", parent, new Vector3(6.05f, 1.15f, 2.9f), new Vector3(0.28f, 2.3f, 3.2f), materials["wall"], true);
            CreateCube("EastWall_South", parent, new Vector3(6.05f, 1.15f, -2.9f), new Vector3(0.28f, 2.3f, 3.2f), materials["wall"], true);
        }

        private static void CreateBilliardsPuzzle(Transform parent, Dictionary<string, Material> materials, GameObject futureLockRoot)
        {
            var tableRoot = new GameObject("BilliardsTable_L3_Billiards");
            tableRoot.transform.SetParent(parent);
            tableRoot.transform.position = new Vector3(-1.25f, 0f, 0f);

            CreateCube("Table_Felt", tableRoot.transform, new Vector3(0f, 0.52f, 0f), new Vector3(3.7f, 0.22f, 1.85f), materials["table"], true);
            CreateCube("Rail_North", tableRoot.transform, new Vector3(0f, 0.75f, 1.05f), new Vector3(3.95f, 0.26f, 0.25f), materials["rail"], true);
            CreateCube("Rail_South", tableRoot.transform, new Vector3(0f, 0.75f, -1.05f), new Vector3(3.95f, 0.26f, 0.25f), materials["rail"], true);
            CreateCube("Rail_West", tableRoot.transform, new Vector3(-2.05f, 0.75f, 0f), new Vector3(0.25f, 0.26f, 1.85f), materials["rail"], true);
            CreateCube("Rail_East", tableRoot.transform, new Vector3(2.05f, 0.75f, 0f), new Vector3(0.25f, 0.26f, 1.85f), materials["rail"], true);

            CreatePocket(tableRoot.transform, "Pocket_1", new Vector3(-1.55f, 0.88f, 0.68f), materials["black"]);
            CreatePocket(tableRoot.transform, "Pocket_2", new Vector3(0f, 0.88f, -0.72f), materials["black"]);
            CreatePocket(tableRoot.transform, "Pocket_3_Target", new Vector3(1.55f, 0.9f, 0.68f), materials["green"]);

            CreateSphere("CueBall", tableRoot.transform, new Vector3(-1.1f, 1.02f, -0.35f), new Vector3(0.28f, 0.28f, 0.28f), materials["white"], true);
            CreateSphere("TargetBall", tableRoot.transform, new Vector3(0.45f, 1.02f, 0.1f), new Vector3(0.28f, 0.28f, 0.28f), materials["red"], true);
            var pocketedBall = CreateSphere("Pocket3_ResultBall", tableRoot.transform, new Vector3(1.55f, 1.07f, 0.68f), new Vector3(0.28f, 0.28f, 0.28f), materials["green"], false);
            pocketedBall.SetActive(false);

            var readyLamp = CreateCube("BallResultLamp_Red", parent, new Vector3(0.7f, 1.15f, -1.9f), new Vector3(0.35f, 0.35f, 0.35f), materials["red"], false);
            var doneLamp = CreateCube("BallResultLamp_Green", parent, new Vector3(0.7f, 1.15f, -1.9f), new Vector3(0.35f, 0.35f, 0.35f), materials["green"], false);
            doneLamp.SetActive(false);

            var interact = CreateCube("Past_BilliardsShot_Interact", parent, new Vector3(-3.65f, 0.42f, -2.0f), new Vector3(0.65f, 0.84f, 0.65f), materials["amber"], false);
            var prompt = CreateWorldText("Prompt_PastShoot", "E: shoot P3", parent, new Vector3(-3.65f, 1.4f, -2.0f), materials["amber"].color);
            prompt.SetActive(false);

            var sender = interact.AddComponent<SemanticStateSender>();
            ConfigureSender(
                sender,
                EventKind.SetBallResult,
                EventDirection.Bidirectional,
                "BallResult",
                "Pocket_3",
                "L3_Billiards",
                GameRole.Past,
                2.0f,
                prompt,
                new[] { readyLamp });

            var applierGo = new GameObject("BallResult_Applier");
            applierGo.transform.SetParent(parent);
            var applier = applierGo.AddComponent<SemanticStateApplier>();
            ConfigureApplier(
                applier,
                "BallResult",
                "Pocket_3",
                "L3_Billiards",
                new[] { doneLamp, pocketedBall, futureLockRoot },
                new[] { readyLamp },
                null,
                null);
        }

        private static GameObject CreateFutureLockAndDoor(Transform parent, Dictionary<string, Material> materials)
        {
            var lockReadyLamp = CreateCube("LockReadyLamp_Green", parent, new Vector3(3.55f, 1.15f, 1.4f), new Vector3(0.35f, 0.35f, 0.35f), materials["green"], false);
            lockReadyLamp.SetActive(false);
            var lockWaitingLamp = CreateCube("LockWaitingLamp_Red", parent, new Vector3(3.55f, 1.15f, 1.4f), new Vector3(0.35f, 0.35f, 0.35f), materials["red"], false);

            var lockRoot = new GameObject("FutureLock_InteractRoot");
            lockRoot.transform.SetParent(parent);
            lockRoot.transform.position = new Vector3(3.55f, 0f, 1.4f);
            var console = CreateCube("Future_LockConsole", lockRoot.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.9f, 1.1f, 0.42f), materials["cyan"], true);
            var prompt = CreateWorldText("Prompt_FutureLock", "E: align lock", lockRoot.transform, new Vector3(0f, 1.35f, 0f), materials["cyan"].color);
            prompt.SetActive(false);

            var sender = console.AddComponent<SemanticStateSender>();
            ConfigureSender(
                sender,
                EventKind.SetLockState,
                EventDirection.Bidirectional,
                "LockState",
                "Aligned",
                "L3_Lock",
                GameRole.Future,
                2.0f,
                prompt,
                new[] { lockWaitingLamp });
            lockRoot.SetActive(false);

            var doorClosed = CreateCube("FinalDoor_Closed", parent, new Vector3(6.05f, 1.1f, 0f), new Vector3(0.34f, 2.2f, 1.55f), materials["door"], true);
            var doorOpen = CreateCube("FinalDoor_OpenPanel", parent, new Vector3(5.75f, 1.1f, 1.45f), new Vector3(0.28f, 2.05f, 1.15f), materials["green"], false);
            doorOpen.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            doorOpen.SetActive(false);

            var exitZone = CreateCube("ExitZone_L3_Exit", parent, new Vector3(6.85f, 0.08f, 0f), new Vector3(1.25f, 0.16f, 1.85f), materials["exit"], true);
            var exitCollider = exitZone.GetComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            var reachZone = exitZone.AddComponent<SemanticReachZone>();
            ConfigureReachZone(reachZone, "L3_Exit", GameRole.Future);
            exitZone.SetActive(false);

            var doorApplierGo = new GameObject("FinalDoor_LockState_Applier");
            doorApplierGo.transform.SetParent(parent);
            var doorApplier = doorApplierGo.AddComponent<SemanticStateApplier>();
            ConfigureApplier(
                doorApplier,
                "LockState",
                "Aligned",
                "L3_Lock",
                new[] { doorOpen, lockReadyLamp, exitZone },
                new[] { doorClosed, lockWaitingLamp },
                null,
                null);

            return lockRoot;
        }

        private static void CreateOptionalGlassBreak(Transform parent)
        {
            var relayGo = new GameObject("BallResult_To_GlassFracture_Relay");
            relayGo.transform.SetParent(parent);
            var relay = relayGo.AddComponent<SemanticStateRelay>();
            ConfigureRelay(relay);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefracturedWallPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[FHQ] Optional prefractured wall prefab not found: {PrefracturedWallPath}");
                return;
            }

            var wall = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (wall == null) return;

            wall.name = "Optional_GlassWall_L3_GlassWall";
            wall.transform.SetParent(parent);
            wall.transform.position = new Vector3(1.4f, 1.05f, 3.92f);
            wall.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            wall.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

            var temporal = wall.GetComponentInChildren<PrefracturedTemporalObject>(true);
            if (temporal != null)
            {
                var so = new SerializedObject(temporal);
                so.FindProperty("eventKind").enumValueIndex = (int)EventKind.SetSemanticState;
                so.FindProperty("direction").enumValueIndex = (int)EventDirection.Bidirectional;
                so.FindProperty("stateKey").stringValue = "FractureState";
                so.FindProperty("brokenValue").stringValue = "Broken";
                so.FindProperty("targetId").stringValue = "L3_GlassWall";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreateOldClubProps(Transform parent, Dictionary<string, Material> materials)
        {
            CreateCube("OldCabinet_Left", parent, new Vector3(-4.75f, 0.85f, 2.85f), new Vector3(0.7f, 1.7f, 1.1f), materials["wood"], true);
            CreateCube("OldCabinet_Right", parent, new Vector3(-3.9f, 0.85f, 2.85f), new Vector3(0.7f, 1.7f, 1.1f), materials["wood"], true);
            CreateCube("CueRack", parent, new Vector3(-4.6f, 0.75f, -3.2f), new Vector3(0.35f, 1.5f, 1.1f), materials["rail"], true);
            CreateCube("BrokenSofa", parent, new Vector3(2.35f, 0.32f, -3.35f), new Vector3(2.0f, 0.64f, 0.8f), materials["wood"], true);
            CreateWorldText("Label_P3", "P3", parent, new Vector3(0.4f, 1.55f, 0.9f), materials["green"].color);
            CreateWorldText("Label_FinalDoor", "Final Door", parent, new Vector3(5.0f, 1.75f, -0.95f), materials["exit"].color);
        }

        private static void CreateSceneManagers()
        {
            var levelData = CreateOrUpdateLevelData();

            var levelManagerGo = new GameObject("LevelManager");
            var levelPhotonView = levelManagerGo.AddComponent<PhotonView>();
            levelPhotonView.sceneViewId = 2;
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            var levelManagerSo = new SerializedObject(levelManager);
            levelManagerSo.FindProperty("levelData").objectReferenceValue = levelData;
            levelManagerSo.FindProperty("nextLevelScene").stringValue = string.Empty;
            levelManagerSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(levelPhotonView);

            var bus = new GameObject("TimelineEventBus");
            var busPhotonView = bus.AddComponent<PhotonView>();
            busPhotonView.sceneViewId = 1;
            bus.AddComponent<TimelineEventBus>();
            EditorUtility.SetDirty(busPhotonView);

            var store = new GameObject("SemanticStateStore");
            store.AddComponent<SemanticStateStore>();

            var spawner = new GameObject("PlayerSpawner");
            var pastSpawn = new GameObject("PastSpawnPoint");
            pastSpawn.transform.SetParent(spawner.transform);
            pastSpawn.transform.position = new Vector3(-4.35f, 1.1f, -2.25f);
            var futureSpawn = new GameObject("FutureSpawnPoint");
            futureSpawn.transform.SetParent(spawner.transform);
            futureSpawn.transform.position = new Vector3(-4.35f, 1.1f, 1.65f);

            var playerSpawner = spawner.AddComponent<PlayerSpawner>();
            var spawnerSo = new SerializedObject(playerSpawner);
            spawnerSo.FindProperty("pastSpawnPoint").objectReferenceValue = pastSpawn.transform;
            spawnerSo.FindProperty("futureSpawnPoint").objectReferenceValue = futureSpawn.transform;
            spawnerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static LevelData CreateOrUpdateLevelData()
        {
            var levelData = AssetDatabase.LoadAssetAtPath<LevelData>(LevelDataPath);
            if (levelData == null)
            {
                levelData = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(levelData, LevelDataPath);
            }

            levelData.levelIndex = 3;
            levelData.displayName = "Level 03 Club Room";
            levelData.sceneName = "Level03_ClubRoom";
            levelData.pastDateLabel = "1996 Club Room";
            levelData.futureDateLabel = "2026 Abandoned Club Room";
            levelData.randomSeed = 3003;
            levelData.passiveHintAfterSeconds = 45f;
            levelData.passiveHintForPast = "Use the billiards cue station near the table.";
            levelData.passiveHintForFuture = "Wait for P3, then align the lock console.";
            levelData.completeCondition = LevelData.LevelCompleteCondition.FuturePlayerReachZone;
            levelData.targetIdRequired = "L3_Exit";
            levelData.pastDialogue = new[] { "P3 is the pocket.", "I can take the shot." };
            levelData.futureDialogue = new[] { "The lock reacts to P3.", "Door is open." };
            levelData.SanitizeSerializedState();
            EditorUtility.SetDirty(levelData);
            return levelData;
        }

        private static void CreateHud()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var text = CreateText(
                canvasGo.transform,
                "HudText",
                "L3 Club Room | Past: shoot P3 | Future: align lock after P3 | E interact, R reset",
                16,
                TextAnchor.UpperLeft);
            SetRect(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, -26f), new Vector2(760f, 36f));
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            AssignMaterial(go, material);

            if (!keepCollider)
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }

            return go;
        }

        private static GameObject CreateSphere(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            AssignMaterial(go, material);

            if (!keepCollider)
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }

            return go;
        }

        private static void CreatePocket(Transform parent, string name, Vector3 position, Material material)
        {
            var pocket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pocket.name = name;
            pocket.transform.SetParent(parent);
            pocket.transform.localPosition = position;
            pocket.transform.localScale = new Vector3(0.38f, 0.035f, 0.38f);
            AssignMaterial(pocket, material);
            var collider = pocket.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }

        private static GameObject CreateWorldText(string name, string content, Transform parent, Vector3 position, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.transform.rotation = Quaternion.Euler(62f, 0f, 0f);

            var text = go.AddComponent<TextMesh>();
            text.text = content;
            text.fontSize = 42;
            text.characterSize = 0.075f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            return go;
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

        private static void AssignMaterial(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }

        private static void ConfigureSender(
            SemanticStateSender sender,
            EventKind kind,
            EventDirection direction,
            string stateKey,
            string stateValue,
            string targetId,
            GameRole role,
            float radius,
            GameObject prompt,
            GameObject[] deactivateAfterSend)
        {
            var so = new SerializedObject(sender);
            so.FindProperty("eventKind").enumValueIndex = (int)kind;
            so.FindProperty("direction").enumValueIndex = (int)direction;
            so.FindProperty("stateKey").stringValue = stateKey;
            so.FindProperty("stateValue").stringValue = stateValue;
            so.FindProperty("targetId").stringValue = targetId;
            so.FindProperty("restrictToRole").boolValue = true;
            so.FindProperty("requiredRole").enumValueIndex = (int)role;
            so.FindProperty("interactRadius").floatValue = radius;
            so.FindProperty("sendOnce").boolValue = true;
            so.FindProperty("promptUI").objectReferenceValue = prompt;
            SetObjectArray(so.FindProperty("deactivateAfterSend"), deactivateAfterSend);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureApplier(
            SemanticStateApplier applier,
            string stateKey,
            string expectedValue,
            string targetId,
            GameObject[] activateOnMatch,
            GameObject[] deactivateOnMatch,
            Collider[] enableCollidersOnMatch,
            Collider[] disableCollidersOnMatch)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("stateKey").stringValue = stateKey;
            so.FindProperty("expectedValue").stringValue = expectedValue;
            so.FindProperty("targetId").stringValue = targetId;
            so.FindProperty("applyExistingStateOnEnable").boolValue = true;
            SetObjectArray(so.FindProperty("activateOnMatch"), activateOnMatch);
            SetObjectArray(so.FindProperty("deactivateOnMatch"), deactivateOnMatch);
            SetObjectArray(so.FindProperty("enableCollidersOnMatch"), enableCollidersOnMatch);
            SetObjectArray(so.FindProperty("disableCollidersOnMatch"), disableCollidersOnMatch);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureReachZone(SemanticReachZone reachZone, string targetId, GameRole role)
        {
            var so = new SerializedObject(reachZone);
            so.FindProperty("targetId").stringValue = targetId;
            so.FindProperty("restrictToRole").boolValue = true;
            so.FindProperty("requiredRole").enumValueIndex = (int)role;
            so.FindProperty("sendOnce").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRelay(SemanticStateRelay relay)
        {
            var so = new SerializedObject(relay);
            so.FindProperty("sourceStateKey").stringValue = "BallResult";
            so.FindProperty("sourceExpectedValue").stringValue = "Pocket_3";
            so.FindProperty("sourceTargetId").stringValue = "L3_Billiards";
            so.FindProperty("outputEventKind").enumValueIndex = (int)EventKind.SetSemanticState;
            so.FindProperty("outputDirection").enumValueIndex = (int)EventDirection.Bidirectional;
            so.FindProperty("outputStateKey").stringValue = "FractureState";
            so.FindProperty("outputStateValue").stringValue = "Broken";
            so.FindProperty("outputTargetId").stringValue = "L3_GlassWall";
            so.FindProperty("sendOnce").boolValue = true;
            so.FindProperty("sendOnlyFromMasterClient").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedProperty property, Object[] values)
        {
            if (property == null) return;
            values ??= System.Array.Empty<Object>();
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

    }
}
