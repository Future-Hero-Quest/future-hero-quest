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

namespace FutureHeroQuest.EditorTools
{
    /// <summary>
    /// Generates the minimum playable Level 2 archive loop.
    /// Run from FHQ/Build Level 2 Archive Loop.
    /// </summary>
    public static class FhqLevel02ArchiveBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level02_Archive.unity";
        private const string LevelDataPath = "Assets/Data/LevelData_Level02_Archive.asset";

        [MenuItem("FHQ/Build Level 2 Archive Loop")]
        public static void BuildLevel02ArchiveLoop()
        {
            EnsureFolders();

            LevelData levelData = CreateOrUpdateLevelData();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level02_Archive";

            Material floorPast = CreateMaterial("Level02_PastWarm_Floor_Mat", new Color(0.56f, 0.43f, 0.27f));
            Material floorFuture = CreateMaterial("Level02_FutureCool_Floor_Mat", new Color(0.20f, 0.29f, 0.40f));
            Material wallPast = CreateMaterial("Level02_PastWarm_Wall_Mat", new Color(0.74f, 0.58f, 0.36f));
            Material wallFuture = CreateMaterial("Level02_FutureCool_Wall_Mat", new Color(0.28f, 0.41f, 0.55f));
            Material wood = CreateMaterial("Level02_Wood_Mat", new Color(0.37f, 0.23f, 0.13f));
            Material cabinet = CreateMaterial("Level02_Cabinet_Mat", new Color(0.42f, 0.43f, 0.40f));
            Material clue = CreateMaterial("Level02_ClueBlue_Mat", new Color(0.18f, 0.72f, 1.0f));
            Material key = CreateMaterial("Level02_KeyGold_Mat", new Color(1.0f, 0.76f, 0.16f));
            Material locked = CreateMaterial("Level02_LockedRed_Mat", new Color(0.75f, 0.18f, 0.16f));
            Material unlocked = CreateMaterial("Level02_UnlockedGreen_Mat", new Color(0.18f, 0.75f, 0.36f));
            Material dark = CreateMaterial("Level02_DarkShelf_Mat", new Color(0.11f, 0.12f, 0.13f));

            CreateCameraAndLight();

            GameObject root = new GameObject("Level02_ArchiveRoot");
            CreateZone(root.transform, "Past_1996_WarmArchive", new Vector3(-5f, 0f, 0f), floorPast, wallPast);
            CreateZone(root.transform, "Future_2026_ColdArchive", new Vector3(5f, 0f, 0f), floorFuture, wallFuture);
            CreateTimelineDivider(root.transform);

            CreatePastArchiveProps(root.transform, wood, cabinet, key, clue);
            CreateFutureArchiveProps(root.transform, wood, cabinet, dark, clue, locked, unlocked);
            CreateSemanticLoop(root.transform, clue, key, locked, unlocked);

            CreateLevelManager(levelData);
            CreatePlayerSpawner();
            CreateHud();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            FhqBuildSceneUtility.ApplyFinalBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log($"[FHQ] Built Level 2 archive loop: {ScenePath}");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Data");
            EnsureFolder("Assets/Materials");
        }

        private static LevelData CreateOrUpdateLevelData()
        {
            LevelData data = AssetDatabase.LoadAssetAtPath<LevelData>(LevelDataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, LevelDataPath);
            }

            data.levelIndex = 2;
            data.displayName = "Level 2 Archive";
            data.sceneName = "Level02_Archive";
            data.pastDateLabel = "1996";
            data.futureDateLabel = "2026";
            data.randomSeed = 314;
            data.passiveHintAfterSeconds = 120f;
            data.passiveHintForFuture = "Read the archive clue, then wait for the key.";
            data.passiveHintForPast = "Use the highlighted cabinet to place the key.";
            data.pastDialogue = new[] { "Cabinet 2 is reacting.", "The spare key is in place." };
            data.futureDialogue = new[] { "Archive 314 points to cabinet 2.", "The cabinet door is unlocked." };
            data.completeCondition = LevelData.LevelCompleteCondition.FuturePlayerReachZone;
            data.targetIdRequired = "L2_Exit";
            data.SanitizeSerializedState();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void CreateCameraAndLight()
        {
            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
            camera.transform.position = new Vector3(0f, 12f, -10f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            var light = new GameObject("Directional Light");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        }

        private static void CreateZone(Transform parent, string name, Vector3 center, Material floor, Material wall)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent, false);

            CreateCube(zone.transform, "Floor", center + new Vector3(0f, -0.1f, 0f), new Vector3(8f, 0.2f, 8f), floor);
            CreateCube(zone.transform, "FrontWall", center + new Vector3(0f, 1f, -4.15f), new Vector3(8.4f, 2f, 0.3f), wall);

            bool isPast = center.x < 0f;
            if (isPast)
            {
                CreateCube(zone.transform, "BackWall", center + new Vector3(0f, 1f, 4.15f), new Vector3(8.4f, 2f, 0.3f), wall);
                CreateCube(zone.transform, "LeftWall", center + new Vector3(-4.15f, 1f, 0f), new Vector3(0.3f, 2f, 8.4f), wall);
                CreateCube(zone.transform, "TimeDividerWall", center + new Vector3(4.15f, 1f, 0f), new Vector3(0.3f, 2f, 8.4f), wall);
            }
            else
            {
                CreateCube(zone.transform, "BackWall_Left", center + new Vector3(-1.45f, 1f, 4.15f), new Vector3(5.1f, 2f, 0.3f), wall);
                CreateCube(zone.transform, "BackWall_Right", center + new Vector3(3.55f, 1f, 4.15f), new Vector3(1.1f, 2f, 0.3f), wall);
                CreateCube(zone.transform, "TimeDividerWall", center + new Vector3(-4.15f, 1f, 0f), new Vector3(0.3f, 2f, 8.4f), wall);
                CreateCube(zone.transform, "RightWall", center + new Vector3(4.15f, 1f, 0f), new Vector3(0.3f, 2f, 8.4f), wall);
            }
        }

        private static void CreateTimelineDivider(Transform parent)
        {
            CreateWorldLabel(parent, "PastLabel", "PAST 1996 / K", new Vector3(-5f, 0.08f, -4.75f), Color.white, 0.28f);
            CreateWorldLabel(parent, "FutureLabel", "FUTURE 2026 / M", new Vector3(5f, 0.08f, -4.75f), Color.white, 0.28f);
            CreateWorldLabel(parent, "LoopLabel", "FUTURE CLUE -> PAST KEY -> FUTURE EXIT", new Vector3(0f, 0.08f, 4.75f), Color.white, 0.22f);
        }

        private static void CreatePastArchiveProps(Transform parent, Material wood, Material cabinet, Material key, Material clue)
        {
            Transform root = new GameObject("Past_ArchiveProps").transform;
            root.SetParent(parent, false);

            CreateBookshelf(root, new Vector3(-7.1f, 0.7f, 0.7f), wood);
            CreateBookshelf(root, new Vector3(-3.3f, 0.7f, 0.7f), wood);
            CreateDesk(root, new Vector3(-6.6f, 0f, -2.1f), wood);

            CreateCabinet(root, "Past_Cabinet01", new Vector3(-7.1f, 0.8f, 2.95f), cabinet, "1");
            GameObject cabinet2 = CreateCabinet(root, "Past_Cabinet02_Target", new Vector3(-5.1f, 0.8f, 2.95f), cabinet, "2");
            CreateCabinet(root, "Past_Cabinet03", new Vector3(-3.1f, 0.8f, 2.95f), cabinet, "3");

            GameObject highlight = CreateCube(root, "Past_Cabinet02_ClueHighlight", new Vector3(-5.1f, 1.75f, 2.72f), new Vector3(1.2f, 0.12f, 0.08f), clue);
            highlight.SetActive(false);
            cabinet2.AddComponent<BoxCollider>();

            GameObject keyRoot = new GameObject("Past_KeyPoint_AfterClue");
            keyRoot.transform.SetParent(root, false);
            keyRoot.transform.position = new Vector3(-5.1f, 0.05f, 1.35f);
            CreateCube(keyRoot.transform, "KeyPedestal", keyRoot.transform.position + Vector3.up * 0.12f, new Vector3(1.1f, 0.24f, 1.1f), key);
            CreateCube(keyRoot.transform, "KeyVisual", keyRoot.transform.position + new Vector3(0f, 0.55f, 0f), new Vector3(0.85f, 0.16f, 0.16f), key);
            CreateWorldLabel(keyRoot.transform, "VisibleLabel", "PLACE KEY", keyRoot.transform.position + new Vector3(0f, 1.0f, 0f), Color.yellow, 0.18f);
            GameObject keyPrompt = CreateWorldLabel(keyRoot.transform, "Prompt", "E: Place Key", keyRoot.transform.position + new Vector3(0f, 1.32f, 0f), Color.yellow, 0.18f);
            keyPrompt.SetActive(false);

            var sender = keyRoot.AddComponent<SemanticStateSender>();
            ConfigureSender(sender, EventKind.SetKeyState, EventDirection.PastToFuture, "KeyState", "Placed", "L2_Key", GameRole.Past, keyPrompt, new[] { keyRoot });
            keyRoot.SetActive(false);

            GameObject activator = new GameObject("Past_KeyPoint_ClueApplier");
            activator.transform.SetParent(root, false);
            var applier = activator.AddComponent<SemanticStateApplier>();
            ConfigureApplier(applier, "ClueState", "Archive314", "L2_ArchiveClue", new[] { keyRoot, highlight }, null);
        }

        private static void CreateFutureArchiveProps(Transform parent, Material wood, Material cabinet, Material dark, Material clue, Material locked, Material unlocked)
        {
            Transform root = new GameObject("Future_ArchiveProps").transform;
            root.SetParent(parent, false);

            CreateBookshelf(root, new Vector3(2.9f, 0.7f, 0.2f), dark);
            CreateBookshelf(root, new Vector3(4.5f, 0.7f, 0.2f), dark);
            CreateDesk(root, new Vector3(3.1f, 0f, -2.2f), wood);
            CreateCabinet(root, "Future_RustedCabinet", new Vector3(6.7f, 0.8f, 2.2f), cabinet, "314");

            GameObject cluePoint = new GameObject("Future_Archive314_CluePoint");
            cluePoint.transform.SetParent(root, false);
            cluePoint.transform.position = new Vector3(3.1f, 0.05f, -2.2f);
            CreateCube(cluePoint.transform, "ArchiveCard", cluePoint.transform.position + Vector3.up * 0.32f, new Vector3(1.15f, 0.08f, 0.75f), clue);
            CreateWorldLabel(cluePoint.transform, "VisibleLabel", "ARCHIVE 314", cluePoint.transform.position + new Vector3(0f, 0.9f, 0f), Color.cyan, 0.18f);
            GameObject cluePrompt = CreateWorldLabel(cluePoint.transform, "Prompt", "E: Read Clue", cluePoint.transform.position + new Vector3(0f, 1.22f, 0f), Color.cyan, 0.18f);
            cluePrompt.SetActive(false);

            var sender = cluePoint.AddComponent<SemanticStateSender>();
            ConfigureSender(sender, EventKind.SetClueState, EventDirection.FutureToPast, "ClueState", "Archive314", "L2_ArchiveClue", GameRole.Future, cluePrompt, null);

            GameObject doorFrame = CreateCube(root, "Future_LockedDoorFrame", new Vector3(7.6f, 1.1f, 4.25f), new Vector3(1.9f, 2.2f, 0.18f), locked);
            Object.DestroyImmediate(doorFrame.GetComponent<BoxCollider>());
            GameObject lockedDoor = CreateCube(root, "Future_LockedCabinetDoor_Blocker", new Vector3(7.6f, 1.05f, 4.05f), new Vector3(1.35f, 2.1f, 0.36f), locked);
            GameObject openDoor = CreateCube(root, "Future_UnlockedDoorMarker", new Vector3(7.6f, 1.05f, 4.08f), new Vector3(1.35f, 0.16f, 0.36f), unlocked);
            openDoor.SetActive(false);

            GameObject exitBeacon = CreateCube(root, "Future_ExitBeacon_AfterUnlock", new Vector3(7.6f, 0.15f, 5.45f), new Vector3(1.5f, 0.3f, 1.1f), unlocked);
            exitBeacon.SetActive(false);
            CreateWorldLabel(root, "ExitLabel", "EXIT", new Vector3(7.6f, 0.08f, 5.95f), Color.green, 0.24f);

            GameObject unlockApplier = new GameObject("Future_Door_KeyApplier");
            unlockApplier.transform.SetParent(root, false);
            var applier = unlockApplier.AddComponent<SemanticStateApplier>();
            ConfigureApplier(applier, "KeyState", "Placed", "L2_Key", new[] { openDoor, exitBeacon }, new[] { lockedDoor });

            GameObject exitZone = CreateCube(root, "Future_ExitReachZone", new Vector3(7.6f, 0.5f, 5.45f), new Vector3(1.6f, 1f, 1.2f), unlocked);
            var exitCollider = exitZone.GetComponent<BoxCollider>();
            if (exitCollider != null) exitCollider.isTrigger = true;
            var reachZone = exitZone.AddComponent<SemanticReachZone>();
            ConfigureReachZone(reachZone, "L2_Exit", GameRole.Future);
        }

        private static void CreateSemanticLoop(Transform parent, Material clue, Material key, Material locked, Material unlocked)
        {
            Transform root = new GameObject("Semantic_State_Legend").transform;
            root.SetParent(parent, false);

            CreateCube(root, "Step1_FutureToPast", new Vector3(0f, 0.2f, -1.4f), new Vector3(0.35f, 0.35f, 0.35f), clue);
            CreateWorldLabel(root, "Step1_Label", "1  ClueState=Archive314", new Vector3(0f, 0.2f, -0.95f), Color.cyan, 0.14f);
            CreateCube(root, "Step2_PastToFuture", new Vector3(0f, 0.2f, 0.25f), new Vector3(0.35f, 0.35f, 0.35f), key);
            CreateWorldLabel(root, "Step2_Label", "2  KeyState=Placed", new Vector3(0f, 0.2f, 0.7f), Color.yellow, 0.14f);
            CreateCube(root, "Step3_Unlock", new Vector3(0f, 0.2f, 1.9f), new Vector3(0.35f, 0.35f, 0.35f), unlocked);
            CreateWorldLabel(root, "Step3_Label", "3  Future door unlocks", new Vector3(0f, 0.2f, 2.35f), Color.green, 0.14f);

            CreateCube(root, "LockedStateSwatch", new Vector3(0f, 0.2f, 3.15f), new Vector3(0.28f, 0.28f, 0.28f), locked);
        }

        private static void CreateLevelManager(LevelData levelData)
        {
            var levelManager = new GameObject("LevelManager");
            var photonView = levelManager.AddComponent<PhotonView>();
            photonView.sceneViewId = 2;
            var manager = levelManager.AddComponent<LevelManager>();
            var serialized = new SerializedObject(manager);
            serialized.FindProperty("levelData").objectReferenceValue = levelData;
            serialized.FindProperty("nextLevelScene").stringValue = "Level03_ClubRoom";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(photonView);
        }

        private static void CreatePlayerSpawner()
        {
            var spawner = new GameObject("PlayerSpawner");
            var pastSpawn = CreateSpawnPoint(spawner.transform, "PastSpawnPoint", new Vector3(-6.8f, 1.1f, -3.1f), new Color(1.0f, 0.7f, 0.35f));
            var futureSpawn = CreateSpawnPoint(spawner.transform, "FutureSpawnPoint", new Vector3(2.6f, 1.1f, -3.1f), new Color(0.35f, 0.75f, 1.0f));

            var playerSpawner = spawner.AddComponent<PlayerSpawner>();
            var serialized = new SerializedObject(playerSpawner);
            serialized.FindProperty("pastSpawnPoint").objectReferenceValue = pastSpawn.transform;
            serialized.FindProperty("futureSpawnPoint").objectReferenceValue = futureSpawn.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateSpawnPoint(Transform parent, string name, Vector3 position, Color color)
        {
            Material material = CreateMaterial($"Level02_{name}_Mat", color);
            GameObject spawn = new GameObject(name);
            spawn.transform.SetParent(parent, false);
            spawn.transform.position = position;

            GameObject pad = CreateCube(parent, $"{name}_Pad", new Vector3(position.x, 0.03f, position.z), new Vector3(1.1f, 0.06f, 1.1f), material);
            Object.DestroyImmediate(pad.GetComponent<BoxCollider>());
            CreateWorldLabel(parent, $"{name}_Label", name.Replace("SpawnPoint", " Spawn"), position + new Vector3(0f, -0.85f, 0.9f), Color.white, 0.16f);
            return spawn;
        }

        private static void CreateHud()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            Text title = CreateText(canvasGo.transform, "Title", "Level 2 Archive Loop", 20, TextAnchor.UpperLeft);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(210f, -22f), new Vector2(390f, 34f));

            Text flow = CreateText(
                canvasGo.transform,
                "Flow",
                "Future: read Archive 314 -> Past: place key -> Future: door unlocks -> reach EXIT",
                15,
                TextAnchor.UpperLeft);
            SetRect(flow.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, -52f), new Vector2(740f, 34f));
        }

        private static GameObject CreateCabinet(Transform parent, string name, Vector3 position, Material material, string number)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            CreateCube(root.transform, "Body", position, new Vector3(1.25f, 1.55f, 0.55f), material);
            CreateCube(root.transform, "Handle", position + new Vector3(0.38f, 0f, -0.31f), new Vector3(0.09f, 0.55f, 0.08f), material);
            CreateWorldLabel(root.transform, "Number", number, position + new Vector3(0f, 0.88f, -0.36f), Color.white, 0.18f);
            return root;
        }

        private static void CreateBookshelf(Transform parent, Vector3 position, Material material)
        {
            CreateCube(parent, "Bookshelf", position, new Vector3(1.25f, 1.4f, 0.45f), material);
            CreateCube(parent, "Bookshelf_ShelfA", position + new Vector3(0f, 0.22f, -0.26f), new Vector3(1.35f, 0.08f, 0.08f), material);
            CreateCube(parent, "Bookshelf_ShelfB", position + new Vector3(0f, -0.22f, -0.26f), new Vector3(1.35f, 0.08f, 0.08f), material);
        }

        private static void CreateDesk(Transform parent, Vector3 position, Material material)
        {
            CreateCube(parent, "ArchiveDesk_Top", position + new Vector3(0f, 0.55f, 0f), new Vector3(1.9f, 0.18f, 1.05f), material);
            CreateCube(parent, "ArchiveDesk_Base", position + new Vector3(0f, 0.27f, 0f), new Vector3(1.55f, 0.5f, 0.72f), material);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return go;
        }

        private static GameObject CreateWorldLabel(Transform parent, string name, string text, Vector3 position, Color color, float size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = size;
            mesh.fontSize = 64;
            mesh.color = color;
            return go;
        }

        private static void ConfigureSender(
            SemanticStateSender sender,
            EventKind kind,
            EventDirection direction,
            string stateKey,
            string stateValue,
            string targetId,
            GameRole requiredRole,
            GameObject prompt,
            GameObject[] deactivateAfterSend)
        {
            var serialized = new SerializedObject(sender);
            serialized.FindProperty("eventKind").enumValueIndex = (int)kind;
            serialized.FindProperty("direction").enumValueIndex = (int)direction;
            serialized.FindProperty("stateKey").stringValue = stateKey;
            serialized.FindProperty("stateValue").stringValue = stateValue;
            serialized.FindProperty("targetId").stringValue = targetId;
            serialized.FindProperty("restrictToRole").boolValue = true;
            serialized.FindProperty("requiredRole").enumValueIndex = (int)requiredRole;
            serialized.FindProperty("interactRadius").floatValue = 1.7f;
            serialized.FindProperty("interactKey").intValue = (int)KeyCode.E;
            serialized.FindProperty("sendOnce").boolValue = true;
            serialized.FindProperty("promptUI").objectReferenceValue = prompt;
            SetObjectArray(serialized.FindProperty("deactivateAfterSend"), deactivateAfterSend);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureApplier(
            SemanticStateApplier applier,
            string stateKey,
            string expectedValue,
            string targetId,
            GameObject[] activateOnMatch,
            GameObject[] deactivateOnMatch)
        {
            var serialized = new SerializedObject(applier);
            serialized.FindProperty("stateKey").stringValue = stateKey;
            serialized.FindProperty("expectedValue").stringValue = expectedValue;
            serialized.FindProperty("targetId").stringValue = targetId;
            serialized.FindProperty("applyExistingStateOnEnable").boolValue = true;
            SetObjectArray(serialized.FindProperty("activateOnMatch"), activateOnMatch);
            SetObjectArray(serialized.FindProperty("deactivateOnMatch"), deactivateOnMatch);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureReachZone(SemanticReachZone zone, string targetId, GameRole requiredRole)
        {
            var serialized = new SerializedObject(zone);
            serialized.FindProperty("targetId").stringValue = targetId;
            serialized.FindProperty("restrictToRole").boolValue = true;
            serialized.FindProperty("requiredRole").enumValueIndex = (int)requiredRole;
            serialized.FindProperty("sendOnce").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray(SerializedProperty property, GameObject[] objects)
        {
            if (objects == null)
            {
                property.arraySize = 0;
                return;
            }

            property.arraySize = objects.Length;
            for (int i = 0; i < objects.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
            }
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

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

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
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureFolder(string folder)
        {
            string normalized = folder.Replace("\\", "/").Trim('/');
            if (AssetDatabase.IsValidFolder(normalized)) return;

            string[] parts = normalized.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
