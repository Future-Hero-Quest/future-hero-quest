using System.IO;
using FutureHeroQuest.Core;
using FutureHeroQuest.Level;
using FutureHeroQuest.Players;
using Photon.Pun;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FutureHeroQuest.EditorTools
{
    /// <summary>
    /// Builds the minimum playable whitebox loop for Level 1: Past repairs bridge support,
    /// Future bridge appears, Future reaches the exit zone.
    /// </summary>
    public static class FhqLevel01BridgeBuilder
    {
        private const string BridgeScenePath = "Assets/Scenes/Level01_Bridge.unity";
        private const string LevelDataPath = "Assets/LevelData/LevelData_Level01Bridge.asset";

        [MenuItem("FHQ/Build Level 1 Bridge Loop")]
        public static void BuildLevel01BridgeLoop()
        {
            EnsureFolders();
            LevelData levelData = CreateOrUpdateLevelData();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level01_Bridge";

            RenderSettings.ambientLight = new Color(0.48f, 0.5f, 0.54f);

            CreateCameraAndLights();
            CreateLevelSystems(levelData);
            CreatePlayerSpawner();

            var root = new GameObject("Level01_Bridge_Whitebox");
            CreatePastZone(root.transform);
            CreateFutureZone(root.transform);
            CreateSharedLabels(root.transform);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, BridgeScenePath);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(BridgeScenePath);

            Debug.Log("[FHQ] Built Level01_Bridge semantic loop.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Materials");
            Directory.CreateDirectory("Assets/LevelData");
        }

        private static LevelData CreateOrUpdateLevelData()
        {
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(LevelDataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, LevelDataPath);
            }

            data.levelIndex = 1;
            data.displayName = "Level 1 - Bridge Corridor";
            data.sceneName = "Level01_Bridge";
            data.pastDateLabel = "1996";
            data.futureDateLabel = "2026";
            data.randomSeed = 10101;
            data.completeCondition = LevelData.LevelCompleteCondition.FuturePlayerReachZone;
            data.targetIdRequired = "L1_Exit";
            data.passiveHintAfterSeconds = 120f;
            data.passiveHintForPast = "Repair the bridge support marker.";
            data.passiveHintForFuture = "Wait for the bridge to recover, then cross.";
            data.pastDialogue = new[] { "Support point marked.", "Repair complete." };
            data.futureDialogue = new[] { "Bridge is broken.", "Bridge restored." };
            data.SanitizeSerializedState();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void CreateCameraAndLights()
        {
            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 11.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            camera.transform.position = new Vector3(1.5f, 18f, -15f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            CreatePointLight("Past Warm Fill", new Vector3(-10f, 4f, -1f), new Color(1f, 0.72f, 0.45f), 18f, 1.4f);
            CreatePointLight("Future Cool Fill", new Vector3(11.5f, 4f, -1f), new Color(0.38f, 0.72f, 1f), 18f, 1.35f);
        }

        private static void CreatePointLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        private static void CreateLevelSystems(LevelData levelData)
        {
            var managerGo = new GameObject("LevelManager");
            var photonView = managerGo.AddComponent<PhotonView>();
            photonView.sceneViewId = 2;
            var manager = managerGo.AddComponent<LevelManager>();
            var managerSo = new SerializedObject(manager);
            managerSo.FindProperty("levelData").objectReferenceValue = levelData;
            managerSo.FindProperty("nextLevelScene").stringValue = "Level02_Archive";
            managerSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(photonView);
        }

        private static void CreatePlayerSpawner()
        {
            var spawnerGo = new GameObject("PlayerSpawner");
            var pastSpawn = new GameObject("PastSpawnPoint");
            pastSpawn.transform.SetParent(spawnerGo.transform);
            pastSpawn.transform.position = new Vector3(-14f, 1.15f, -1.7f);
            pastSpawn.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            var futureSpawn = new GameObject("FutureSpawnPoint");
            futureSpawn.transform.SetParent(spawnerGo.transform);
            futureSpawn.transform.position = new Vector3(6.7f, 1.15f, -1.7f);
            futureSpawn.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            var spawner = spawnerGo.AddComponent<PlayerSpawner>();
            var so = new SerializedObject(spawner);
            so.FindProperty("pastSpawnPoint").objectReferenceValue = pastSpawn.transform;
            so.FindProperty("futureSpawnPoint").objectReferenceValue = futureSpawn.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePastZone(Transform parent)
        {
            var zone = new GameObject("PastZone_1996_Warm");
            zone.transform.SetParent(parent);

            Material floorMat = CreateMaterial("L1_Past_Floor_Mat", new Color(0.48f, 0.36f, 0.23f));
            Material wallMat = CreateMaterial("L1_Past_Wall_Mat", new Color(0.68f, 0.55f, 0.39f));
            Material repairMat = CreateMaterial("L1_Repair_Mat", new Color(1f, 0.83f, 0.25f));
            Material brokenMat = CreateMaterial("L1_BrokenSupport_Mat", new Color(0.55f, 0.18f, 0.13f));
            Material fixedMat = CreateMaterial("L1_FixedSupport_Mat", new Color(0.28f, 0.72f, 0.35f));

            CreateCube("PastFloor", zone.transform, new Vector3(-10f, -0.1f, 0f), new Vector3(10f, 0.2f, 6f), floorMat, true);
            CreateBounds("PastBounds", zone.transform, new Vector3(-10f, 0f, 0f), new Vector3(10f, 6f), wallMat);

            CreateCube("BrokenSupport_A", zone.transform, new Vector3(-10.9f, 0.45f, -1.15f), new Vector3(0.45f, 0.9f, 0.45f), brokenMat, true);
            CreateCube("BrokenSupport_C", zone.transform, new Vector3(-10.9f, 0.45f, 1.15f), new Vector3(0.45f, 0.9f, 0.45f), brokenMat, true);

            var fixedSupports = new GameObject("PastFixedSupports");
            fixedSupports.transform.SetParent(zone.transform);
            CreateCube("FixedSupport_A", fixedSupports.transform, new Vector3(-9.8f, 0.7f, -1.15f), new Vector3(0.45f, 1.4f, 0.45f), fixedMat, true);
            CreateCube("FixedSupport_C", fixedSupports.transform, new Vector3(-9.8f, 0.7f, 1.15f), new Vector3(0.45f, 1.4f, 0.45f), fixedMat, true);
            fixedSupports.SetActive(false);

            var repair = CreateCube("PastRepairPoint_SendBridgeState", zone.transform, new Vector3(-12.4f, 0.08f, 0f), new Vector3(1.2f, 0.16f, 1.2f), repairMat, true);
            var sender = repair.AddComponent<SemanticStateSender>();
            var prompt = CreateWorldText(
                "RepairPrompt",
                zone.transform,
                "Press E: repair bridge support",
                new Vector3(-12.4f, 1.55f, 0f),
                34,
                Color.white,
                0.08f);
            prompt.gameObject.SetActive(false);
            ConfigureSender(sender, prompt.gameObject);

            var feedback = new GameObject("PastRepairFeedbackApplier");
            feedback.transform.SetParent(zone.transform);
            var applier = feedback.AddComponent<SemanticStateApplier>();
            ConfigureApplier(applier, new[] { fixedSupports }, new GameObject[0]);

            CreateWorldText("PastAreaLabel", zone.transform, "1996 / Past: support repair", new Vector3(-10f, 2.25f, -2.75f), 34, new Color(1f, 0.84f, 0.5f), 0.08f);
            CreateWorldText("PastHintLabel", zone.transform, "A + C supports are marked", new Vector3(-10.4f, 1.75f, 2.45f), 26, Color.white, 0.07f);
        }

        private static void CreateFutureZone(Transform parent)
        {
            var zone = new GameObject("FutureZone_2026_Cool");
            zone.transform.SetParent(parent);

            Material floorMat = CreateMaterial("L1_Future_Floor_Mat", new Color(0.18f, 0.29f, 0.42f));
            Material wallMat = CreateMaterial("L1_Future_Wall_Mat", new Color(0.2f, 0.42f, 0.56f));
            Material bridgeMat = CreateMaterial("L1_RestoredBridge_Mat", new Color(0.28f, 0.74f, 0.9f));
            Material blockerMat = CreateMaterial("L1_CollapsedGap_Mat", new Color(0.75f, 0.2f, 0.18f));
            Material exitMat = CreateMaterial("L1_Exit_Mat", new Color(0.22f, 0.95f, 0.48f));

            CreateCube("FutureEntryFloor", zone.transform, new Vector3(8f, -0.1f, 0f), new Vector3(5f, 0.2f, 6f), floorMat, true);
            CreateCube("FutureExitFloor", zone.transform, new Vector3(15f, -0.1f, 0f), new Vector3(5f, 0.2f, 6f), floorMat, true);
            CreateBounds("FutureBounds", zone.transform, new Vector3(11.5f, 0f, 0f), new Vector3(12.5f, 6f), wallMat);

            var bridgeRoot = new GameObject("FutureRestoredBridge_ActivatesOnBridgeState");
            bridgeRoot.transform.SetParent(zone.transform);
            CreateCube("BridgeDeck", bridgeRoot.transform, new Vector3(11.5f, -0.02f, 0f), new Vector3(2.3f, 0.28f, 5.1f), bridgeMat, true);
            CreateCube("BridgeNorthRail", bridgeRoot.transform, new Vector3(11.5f, 0.65f, 2.45f), new Vector3(2.3f, 1.2f, 0.22f), wallMat, true);
            CreateCube("BridgeSouthRail", bridgeRoot.transform, new Vector3(11.5f, 0.65f, -2.45f), new Vector3(2.3f, 1.2f, 0.22f), wallMat, true);
            bridgeRoot.SetActive(false);

            var brokenVisuals = new GameObject("FutureBrokenBridgeVisuals_DeactivatesOnRepair");
            brokenVisuals.transform.SetParent(zone.transform);
            var slabA = CreateCube("BrokenSlab_A", brokenVisuals.transform, new Vector3(10.75f, 0.12f, -0.7f), new Vector3(1.2f, 0.2f, 1.3f), blockerMat, false);
            slabA.transform.rotation = Quaternion.Euler(0f, 0f, 11f);
            var slabB = CreateCube("BrokenSlab_B", brokenVisuals.transform, new Vector3(12.25f, 0.12f, 0.7f), new Vector3(1.2f, 0.2f, 1.3f), blockerMat, false);
            slabB.transform.rotation = Quaternion.Euler(0f, 0f, -13f);

            var gapBlocker = CreateCube("FutureCollapsedGapBlocker", zone.transform, new Vector3(11.5f, 0.95f, 0f), new Vector3(1.5f, 1.9f, 5.25f), blockerMat, true);

            var applierGo = new GameObject("FutureBridgeStateApplier");
            applierGo.transform.SetParent(zone.transform);
            var applier = applierGo.AddComponent<SemanticStateApplier>();
            ConfigureApplier(applier, new[] { bridgeRoot }, new[] { gapBlocker, brokenVisuals });

            var exitZone = CreateCube("FutureExitReachZone", zone.transform, new Vector3(16.9f, 0.75f, 0f), new Vector3(0.8f, 1.5f, 4.4f), exitMat, true);
            var exitCollider = exitZone.GetComponent<Collider>();
            if (exitCollider != null) exitCollider.isTrigger = true;
            var reachZone = exitZone.AddComponent<SemanticReachZone>();
            ConfigureReachZone(reachZone);

            CreateCube("ExitFrameTop", zone.transform, new Vector3(16.9f, 2f, 0f), new Vector3(0.35f, 0.35f, 4.8f), exitMat, true);
            CreateCube("ExitFrameNorth", zone.transform, new Vector3(16.9f, 1f, 2.55f), new Vector3(0.35f, 2f, 0.28f), exitMat, true);
            CreateCube("ExitFrameSouth", zone.transform, new Vector3(16.9f, 1f, -2.55f), new Vector3(0.35f, 2f, 0.28f), exitMat, true);

            CreateWorldText("FutureAreaLabel", zone.transform, "2026 / Future: broken bridge", new Vector3(11.5f, 2.25f, -2.75f), 34, new Color(0.55f, 0.86f, 1f), 0.08f);
            CreateWorldText("FutureHintLabel", zone.transform, "BridgeState = Supported restores this path", new Vector3(11.5f, 1.75f, 2.55f), 25, Color.white, 0.065f);
            CreateWorldText("ExitLabel", zone.transform, "EXIT", new Vector3(16.9f, 2.55f, 0f), 40, Color.white, 0.09f);
        }

        private static void CreateSharedLabels(Transform parent)
        {
            CreateWorldText("GoalLabel", parent, "Level 1: Past repairs -> Future crosses", new Vector3(1f, 2.7f, 3.65f), 38, Color.white, 0.09f);
        }

        private static void CreateBounds(string name, Transform parent, Vector3 center, Vector3 size, Material material)
        {
            var bounds = new GameObject(name);
            bounds.transform.SetParent(parent);

            float x = center.x;
            float z = center.z;
            float halfX = size.x * 0.5f;
            float halfZ = size.y * 0.5f;

            CreateCube("NorthWall", bounds.transform, new Vector3(x, 1f, z + halfZ + 0.15f), new Vector3(size.x + 0.3f, 2f, 0.3f), material, true);
            CreateCube("SouthWall", bounds.transform, new Vector3(x, 1f, z - halfZ - 0.15f), new Vector3(size.x + 0.3f, 2f, 0.3f), material, true);
            CreateCube("WestWall", bounds.transform, new Vector3(x - halfX - 0.15f, 1f, z), new Vector3(0.3f, 2f, size.y + 0.3f), material, true);
            CreateCube("EastWall", bounds.transform, new Vector3(x + halfX + 0.15f, 1f, z), new Vector3(0.3f, 2f, size.y + 0.3f), material, true);
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            if (!keepCollider)
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null) Object.DestroyImmediate(collider);
            }

            return go;
        }

        private static TextMesh CreateWorldText(
            string name,
            Transform parent,
            string text,
            Vector3 position,
            int fontSize,
            Color color,
            float characterSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(62f, 0f, 0f);

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = fontSize;
            mesh.characterSize = characterSize;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            return mesh;
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

        private static void ConfigureSender(SemanticStateSender sender, GameObject prompt)
        {
            var so = new SerializedObject(sender);
            so.FindProperty("eventKind").enumValueIndex = (int)EventKind.SetBridgeState;
            so.FindProperty("direction").enumValueIndex = (int)EventDirection.PastToFuture;
            so.FindProperty("stateKey").stringValue = "BridgeState";
            so.FindProperty("stateValue").stringValue = "Supported";
            so.FindProperty("targetId").stringValue = "L1_Bridge";
            so.FindProperty("restrictToRole").boolValue = true;
            so.FindProperty("requiredRole").enumValueIndex = (int)GameRole.Past;
            so.FindProperty("interactRadius").floatValue = 2.0f;
            so.FindProperty("interactKey").intValue = (int)KeyCode.E;
            so.FindProperty("sendOnce").boolValue = true;
            so.FindProperty("promptUI").objectReferenceValue = prompt;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureApplier(SemanticStateApplier applier, GameObject[] activateOnMatch, GameObject[] deactivateOnMatch)
        {
            var so = new SerializedObject(applier);
            so.FindProperty("stateKey").stringValue = "BridgeState";
            so.FindProperty("expectedValue").stringValue = "Supported";
            so.FindProperty("targetId").stringValue = "L1_Bridge";
            so.FindProperty("applyExistingStateOnEnable").boolValue = true;
            SetGameObjectArray(so.FindProperty("activateOnMatch"), activateOnMatch);
            SetGameObjectArray(so.FindProperty("deactivateOnMatch"), deactivateOnMatch);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureReachZone(SemanticReachZone reachZone)
        {
            var so = new SerializedObject(reachZone);
            so.FindProperty("targetId").stringValue = "L1_Exit";
            so.FindProperty("restrictToRole").boolValue = true;
            so.FindProperty("requiredRole").enumValueIndex = (int)GameRole.Future;
            so.FindProperty("sendOnce").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetGameObjectArray(SerializedProperty property, GameObject[] objects)
        {
            property.arraySize = objects.Length;
            for (int i = 0; i < objects.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
            }
        }

        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void UpdateBuildSettings()
        {
            FhqBuildSceneUtility.ApplyFinalBuildSettings();
        }
    }
}
