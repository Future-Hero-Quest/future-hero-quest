using FutureHeroQuest.Core;
using FutureHeroQuest.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FutureHeroQuest.EditorTools
{
    /// <summary>
    /// Builds a standalone smoke-test scene for the pre-fractured wall prefab.
    /// Safe to rerun from FHQ/Generate Fracture Test Scene.
    /// </summary>
    public static class FhqFractureTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/FractureTest.unity";
        private const string WallPrefabPath = "Assets/Prefabs/Level/PrefracturedTestWall.prefab";

        [MenuItem("FHQ/Generate Fracture Test Scene")]
        public static void GenerateScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Materials");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FractureTest";

            CreateCameraAndLight();
            CreateGroundAndBounds();
            GameObject wall = CreateWall();
            CreateTrigger();
            CreateReferenceCapsule();
            CreateTestController(wall);
            CreateHud();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FHQ] Generated fracture test scene: {ScenePath}");
        }

        [MenuItem("FHQ/Validate Fracture Test Scene")]
        public static void ValidateScene()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogError($"[FHQ] Missing fracture test scene: {ScenePath}");
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var driver = Object.FindAnyObjectByType<PrefracturedTemporalObject>();
            if (driver == null)
            {
                Debug.LogError("[FHQ] Fracture test validation failed: missing PrefracturedTemporalObject.");
                return;
            }

            var intact = driver.transform.Find("Intact")?.gameObject;
            var fractured = driver.transform.Find("Fractured")?.gameObject;
            if (intact == null || fractured == null)
            {
                Debug.LogError("[FHQ] Fracture test validation failed: missing Intact or Fractured root.");
                return;
            }

            driver.ResetToIntact();
            bool startsIntact = intact.activeSelf && !fractured.activeSelf && !driver.IsBroken;
            driver.BreakLocal();
            bool breaksOpen = !intact.activeSelf && fractured.activeSelf && driver.IsBroken;
            bool fragmentsReleased = false;
            foreach (Rigidbody rb in fractured.GetComponentsInChildren<Rigidbody>(true))
            {
                if (rb != null && !rb.isKinematic && rb.constraints == RigidbodyConstraints.None && rb.useGravity)
                {
                    fragmentsReleased = true;
                    break;
                }
            }

            driver.ResetToIntact();
            EditorSceneManager.SaveOpenScenes();

            if (!startsIntact || !breaksOpen || !fragmentsReleased)
            {
                Debug.LogError(
                    $"[FHQ] Fracture test validation failed. startsIntact={startsIntact}, breaksOpen={breaksOpen}, fragmentsReleased={fragmentsReleased}");
                return;
            }

            Debug.Log("[FHQ] Fracture test validation passed: intact -> broken -> reset.");
        }

        private static void CreateCameraAndLight()
        {
            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.07f);
            camera.transform.position = new Vector3(0f, 7.5f, -8.5f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            var light = new GameObject("Directional Light");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
        }

        private static void CreateGroundAndBounds()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(9f, 0.1f, 9f);
            AssignMaterial(ground, "FractureTest_Ground_Mat", new Color(0.2f, 0.24f, 0.22f));

            CreateWallBlock("BackWall", new Vector3(0f, 1f, 4.55f), new Vector3(9f, 2f, 0.2f));
            CreateWallBlock("LeftWall", new Vector3(-4.55f, 1f, 0f), new Vector3(0.2f, 2f, 9f));
            CreateWallBlock("RightWall", new Vector3(4.55f, 1f, 0f), new Vector3(0.2f, 2f, 9f));
        }

        private static void CreateWallBlock(string name, Vector3 position, Vector3 scale)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            AssignMaterial(block, "FractureTest_Bounds_Mat", new Color(0.16f, 0.18f, 0.2f));
        }

        private static GameObject CreateWall()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[FHQ] Missing wall prefab: {WallPrefabPath}");
                return null;
            }

            var wall = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            wall.name = "PrefracturedTestWall";
            wall.transform.position = new Vector3(0f, 1.2f, 1.6f);
            wall.transform.rotation = Quaternion.identity;
            wall.transform.localScale = Vector3.one;
            return wall;
        }

        private static void CreateTrigger()
        {
            var trigger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trigger.name = "FractureSemanticTrigger";
            trigger.transform.position = new Vector3(0f, 0.08f, -1.45f);
            trigger.transform.localScale = new Vector3(0.65f, 0.08f, 0.65f);
            AssignMaterial(trigger, "FractureTest_Trigger_Mat", new Color(0.95f, 0.72f, 0.22f));

            var sender = trigger.AddComponent<SemanticStateSender>();
            var so = new SerializedObject(sender);
            so.FindProperty("eventKind").enumValueIndex = (int)EventKind.SetSemanticState;
            so.FindProperty("direction").enumValueIndex = (int)EventDirection.Bidirectional;
            so.FindProperty("stateKey").stringValue = "FractureState";
            so.FindProperty("stateValue").stringValue = "Broken";
            so.FindProperty("targetId").stringValue = "TestWall";
            so.FindProperty("restrictToRole").boolValue = false;
            so.FindProperty("interactRadius").floatValue = 2.2f;
            so.FindProperty("interactKey").intValue = (int)KeyCode.E;
            so.FindProperty("sendOnce").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateReferenceCapsule()
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "ReferencePlayerCapsule";
            capsule.transform.position = new Vector3(0f, 1f, -2.8f);
            capsule.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            AssignMaterial(capsule, "FractureTest_Player_Mat", new Color(0.3f, 0.62f, 1f));
        }

        private static void CreateTestController(GameObject wall)
        {
            var controllerGo = new GameObject("FractureTestController");
            var controller = controllerGo.AddComponent<FractureTestController>();
            var so = new SerializedObject(controller);
            so.FindProperty("target").objectReferenceValue = wall != null ? wall.GetComponent<PrefracturedTemporalObject>() : null;
            so.FindProperty("direction").enumValueIndex = (int)EventDirection.Bidirectional;
            so.FindProperty("stateKey").stringValue = "FractureState";
            so.FindProperty("brokenValue").stringValue = "Broken";
            so.FindProperty("targetId").stringValue = "TestWall";
            so.FindProperty("showDebugGui").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHud()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var text = new GameObject("HintText");
            text.transform.SetParent(canvasGo.transform, false);
            var label = text.AddComponent<Text>();
            label.text = "Fracture smoke test: B local break, R reset, N semantic break, E near yellow pad in network play.";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 16;
            label.alignment = TextAnchor.UpperLeft;
            label.color = Color.white;
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(430f, -24f);
            rect.sizeDelta = new Vector2(760f, 40f);
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void AssignMaterial(GameObject obj, string materialName, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateMaterial(materialName, color);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

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
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
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
