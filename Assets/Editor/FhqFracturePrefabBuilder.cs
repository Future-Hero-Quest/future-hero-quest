using System.IO;
using FutureHeroQuest.Core;
using FutureHeroQuest.Level;
using UnityEditor;
using UnityEngine;

namespace FutureHeroQuest.EditorTools
{
    /// <summary>
    /// Generates a small OpenFracture prefab wired to the semantic timeline layer.
    /// Run from FHQ/Generate Prefractured Test Wall after OpenFracture is installed.
    /// </summary>
    public static class FhqFracturePrefabBuilder
    {
        private const string PrefabFolder = "Assets/Prefabs/Level";
        private const string MeshFolder = "Assets/Generated/FractureMeshes/TestWall";
        private const string PrefabPath = PrefabFolder + "/PrefracturedTestWall.prefab";

        [MenuItem("FHQ/Generate Prefractured Test Wall")]
        public static void GeneratePrefracturedTestWall()
        {
            EnsureFolder(PrefabFolder);
            ResetFolder(MeshFolder);

            GameObject root = new GameObject("PrefracturedTestWall");
            try
            {
                GameObject intact = CreateIntactWall(root.transform);
                var prefracture = ConfigurePrefracture(intact);
                prefracture.ComputeFracture();

                GameObject fractured = root.transform.Find("IntactFragments")?.gameObject;
                if (fractured == null)
                {
                    Debug.LogError("[FHQ] OpenFracture did not generate fragment root.");
                    return;
                }

                fractured.name = "Fractured";
                ConfigureFragments(fractured);

                intact.SetActive(true);
                fractured.SetActive(false);
                ConfigureTimelineDriver(root, intact, fractured);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[FHQ] Generated prefractured test wall prefab: {PrefabPath}");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateIntactWall(Transform parent)
        {
            GameObject intact = GameObject.CreatePrimitive(PrimitiveType.Cube);
            intact.name = "Intact";
            intact.transform.SetParent(parent, false);
            intact.transform.localScale = new Vector3(2.4f, 2.0f, 0.28f);

            var renderer = intact.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateMaterial("FractureWall_Intact_Mat", new Color(0.42f, 0.55f, 0.62f));

            var rigidbody = intact.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            return intact;
        }

        private static Prefracture ConfigurePrefracture(GameObject intact)
        {
            var prefracture = intact.AddComponent<Prefracture>();
            prefracture.triggerOptions = new TriggerOptions();
            prefracture.callbackOptions = new CallbackOptions();
            prefracture.fractureOptions = new FractureOptions
            {
                fragmentCount = 16,
                xAxis = true,
                yAxis = true,
                zAxis = false,
                detectFloatingFragments = false,
                asynchronous = false,
                insideMaterial = CreateMaterial("FractureWall_Inside_Mat", new Color(0.18f, 0.2f, 0.22f)),
                textureScale = Vector2.one,
                textureOffset = Vector2.zero
            };
            prefracture.prefractureOptions = new PrefractureOptions
            {
                unfreezeAll = true,
                saveFragmentsToDisk = true,
                saveLocation = MeshFolder
            };
            return prefracture;
        }

        private static void ConfigureFragments(GameObject fractured)
        {
            foreach (Rigidbody rb in fractured.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            foreach (Collider fragmentCollider in fractured.GetComponentsInChildren<Collider>(true))
            {
                fragmentCollider.enabled = false;
            }
        }

        private static void ConfigureTimelineDriver(GameObject root, GameObject intact, GameObject fractured)
        {
            var driver = root.AddComponent<PrefracturedTemporalObject>();
            var serialized = new SerializedObject(driver);
            serialized.FindProperty("eventKind").enumValueIndex = (int)EventKind.SetSemanticState;
            serialized.FindProperty("direction").enumValueIndex = (int)EventDirection.Bidirectional;
            serialized.FindProperty("stateKey").stringValue = "FractureState";
            serialized.FindProperty("brokenValue").stringValue = "Broken";
            serialized.FindProperty("targetId").stringValue = "TestWall";
            serialized.FindProperty("intactRoot").objectReferenceValue = intact;
            serialized.FindProperty("fracturedRoot").objectReferenceValue = fractured;
            serialized.FindProperty("startIntact").boolValue = true;
            serialized.FindProperty("autoCollectFragments").boolValue = true;
            serialized.FindProperty("makeFragmentsKinematicUntilBroken").boolValue = true;
            serialized.FindProperty("toggleFragmentColliders").boolValue = true;
            serialized.FindProperty("useGravityWhenBroken").boolValue = true;
            serialized.FindProperty("freezeConstraintsUntilBroken").boolValue = true;
            serialized.FindProperty("impulseStrength").floatValue = 1.4f;
            serialized.FindProperty("torqueStrength").floatValue = 0.45f;
            serialized.FindProperty("jitterStrength").floatValue = 0.2f;
            serialized.FindProperty("impulseSeed").intValue = 31415;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material CreateMaterial(string name, Color color)
        {
            EnsureFolder("Assets/Materials");
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

        private static void ResetFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                AssetDatabase.DeleteAsset(folder);
            EnsureFolder(folder);
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
