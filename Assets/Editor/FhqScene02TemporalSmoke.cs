using System;
using System.Collections.Generic;
using System.IO;
using FutureHeroQuest.SceneFlow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FutureHeroQuest.EditorTools
{
    public static class FhqScene02TemporalSmoke
    {
        private const string ScenePath = "Assets/Scenes/Outline/Scene02_MountainTunnel.unity";
        private const string SceneRootName = "Scene02_MountainTunnel_Root";
        private const string RigName = "Scene02TemporalFractureSmokeRig";
        private const string ProjectionTargetName = "TemporalFractureProjectionTarget";
        private const string TriggerName = "TemporalFractureTrigger";
        private const string FragmentRootName = "fracFragments";
        private const string SecondaryCameraName = "Scene02DualViewCamera";

        [MenuItem("FHQ/Scene02/Setup Temporal Fracture Smoke")]
        public static void SetupScene02TemporalFractureSmoke()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[FHQ][Scene02Smoke] Stop Play Mode before running setup.");
                return;
            }

            Scene scene = EnsureSceneOpen();
            SetupScene(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FHQ][Scene02Smoke] Setup saved.");
        }

        [MenuItem("FHQ/Scene02/Exit Play Mode")]
        public static void ExitPlayMode()
        {
            EditorApplication.isPlaying = false;
        }

        [MenuItem("FHQ/Scene02/Enter Play Mode")]
        public static void EnterPlayMode()
        {
            EditorApplication.isPlaying = true;
        }

        [MenuItem("FHQ/Scene02/Trigger Visible Fracture Playback")]
        public static void TriggerVisibleFracturePlayback()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[FHQ][Scene02Smoke] Enter Play Mode before triggering visible fracture playback.");
                return;
            }

            Scene02TemporalFracturePlayback playback =
                UnityEngine.Object.FindAnyObjectByType<Scene02TemporalFracturePlayback>();
            if (playback == null)
            {
                Debug.LogError("[FHQ][Scene02Smoke] Missing Scene02TemporalFracturePlayback in active play scene.");
                return;
            }

            playback.TriggerPlayback();
        }

        [MenuItem("FHQ/Scene02/Validate Temporal Fracture Smoke")]
        public static void ValidateScene02TemporalFractureSmoke()
        {
            Scene scene = EnsureSceneOpen();
            ValidateScene(scene);
            Debug.Log("[FHQ][Scene02Smoke] Validation passed.");
        }

        public static void SetupAndValidateScene02()
        {
            Scene scene = EnsureSceneOpen();
            SetupScene(scene);
            EditorSceneManager.SaveScene(scene);
            ValidateScene(scene);
            Debug.Log("[FHQ][Scene02Smoke] Setup and validation passed.");
        }

        private static Scene EnsureSceneOpen()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException(ScenePath);
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.IsValid() && string.Equals(active.path, ScenePath, StringComparison.OrdinalIgnoreCase))
            {
                return active;
            }

            return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static void SetupScene(Scene scene)
        {
            GameObject sceneRoot = Required(FindInScene(scene, SceneRootName), SceneRootName);
            Transform root = sceneRoot.transform;
            RemoveDuplicateRootManagers(scene, sceneRoot);

            GameObject temporalManager = FindDirectChild(root, "TemporalPhysicsManager")
                ?? CreateChild(root, "TemporalPhysicsManager");
            PastFutureTimelineController timeline = EnsureComponent<PastFutureTimelineController>(temporalManager);
            TemporalPhysicsProjector projector = EnsureComponent<TemporalPhysicsProjector>(temporalManager);

            SetObjectReference(timeline, "temporalPhysicsProjector", projector);
            SetBool(timeline, "projectOnceOnStart", false);
            SetProjectorDefaults(projector);

            GameObject collapseManager = FindDirectChild(root, "TunnelCollapseManager")
                ?? CreateChild(root, "TunnelCollapseManager");
            TunnelCollapseController collapseController = EnsureComponent<TunnelCollapseController>(collapseManager);
            SetBool(collapseController, "useTemporalProjection", true);
            SetObjectReference(collapseController, "timelineController", timeline);

            GameObject wireInteractor = Required(FindInScene(scene, "TunnelWiringInteractor"), "TunnelWiringInteractor");
            TemporalOutlineInteractable temporalInteractable = EnsureComponent<TemporalOutlineInteractable>(wireInteractor);
            SetBool(temporalInteractable, "triggerProjectionOnUse", false);
            SetObjectReference(temporalInteractable, "timelineController", timeline);
            SetString(temporalInteractable, "projectionReason", "L2: cap wired @N7");

            GameObject rig = FindDirectChild(root, RigName) ?? CreateChild(root, RigName);
            rig.transform.localPosition = Vector3.zero;
            rig.transform.localRotation = Quaternion.identity;

            GameObject projectionTarget = CreateOrUpdateCube(
                rig.transform,
                ProjectionTargetName,
                new Vector3(-1.15f, 2.35f, 0.8f),
                new Vector3(0.85f, 0.85f, 0.85f));
            Rigidbody targetBody = EnsureComponent<Rigidbody>(projectionTarget);
            targetBody.isKinematic = true;
            targetBody.useGravity = false;
            targetBody.detectCollisions = false;
            targetBody.mass = 8f;
            targetBody.linearDamping = 0f;
            targetBody.angularDamping = 0.05f;
            targetBody.constraints = RigidbodyConstraints.FreezeAll;
            targetBody.position = projectionTarget.transform.position;
            targetBody.rotation = projectionTarget.transform.rotation;
            EnsureComponent<TemporalPhysicsBody>(projectionTarget);
            EnsureComponent<TemporalProjectionFractureOnCollision>(projectionTarget);

            GameObject trigger = CreateOrUpdateCube(
                rig.transform,
                TriggerName,
                new Vector3(-1.15f, 0.72f, 0.8f),
                new Vector3(1.35f, 0.35f, 1.35f));
            BoxCollider triggerCollider = EnsureComponent<BoxCollider>(trigger);
            triggerCollider.isTrigger = true;

            GameObject fragmentRoot = FindDirectChild(rig.transform, FragmentRootName)
                ?? CreateChild(rig.transform, FragmentRootName);
            fragmentRoot.transform.position = projectionTarget.transform.position;
            fragmentRoot.transform.localRotation = Quaternion.identity;
            fragmentRoot.SetActive(true);
            Rigidbody rootBody = EnsureComponent<Rigidbody>(fragmentRoot);
            rootBody.isKinematic = true;
            rootBody.useGravity = false;
            rootBody.detectCollisions = false;

            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -0.34f : 0.34f);
                float y = (i < 2 ? 0.24f : -0.14f);
                float z = (i < 2 ? -0.14f : 0.2f);
                GameObject fragment = CreateOrUpdateCube(
                    fragmentRoot.transform,
                    "fracFragment_" + (i + 1),
                    new Vector3(-1.15f, 1.1f, 0.9f) + new Vector3(x, y, z),
                    new Vector3(0.44f, 0.44f, 0.44f));
                Rigidbody fragmentBody = EnsureComponent<Rigidbody>(fragment);
                fragmentBody.isKinematic = true;
                fragmentBody.useGravity = false;
                fragmentBody.detectCollisions = false;
                fragmentBody.constraints = RigidbodyConstraints.FreezeAll;
                fragmentBody.position = fragment.transform.position;
                fragmentBody.rotation = fragment.transform.rotation;
                EnsureComponent<TemporalPhysicsBody>(fragment);
                fragment.SetActive(false);
            }

            GameObject collapseVisual = FindInScene(scene, "FutureCollapse_AfterWiring");
            ConfigureVisibleRockfallLayout(scene, collapseVisual);
            TextMesh collapseLabel = FindInScene(scene, "CollapseLabel")?.GetComponent<TextMesh>();
            if (collapseLabel != null)
            {
                collapseLabel.text = "ACCELERATED ROCKFALL";
                collapseLabel.transform.position = new Vector3(-1.15f, 1.95f, 0.9f);
                collapseLabel.characterSize = 0.075f * 64f / 96f;
            }

            TextMesh shadowLabel = FindInScene(scene, "ShadowLabel")?.GetComponent<TextMesh>();
            if (shadowLabel != null)
            {
                shadowLabel.text = "future route fractures open";
                shadowLabel.transform.position = new Vector3(1.7f, 1.05f, 1.0f);
                shadowLabel.characterSize = 0.055f * 64f / 96f;
            }

            Camera sceneCamera = FindInScene(scene, "Main Camera")?.GetComponent<Camera>();
            if (sceneCamera != null)
            {
                sceneCamera.transform.position = new Vector3(-1.15f, 5.35f, -4.0f);
                sceneCamera.transform.rotation = Quaternion.Euler(53f, 0f, 0f);
                sceneCamera.orthographic = true;
                sceneCamera.orthographicSize = 4.15f;
                sceneCamera.rect = new Rect(0f, 0f, 1f, 1f);
                sceneCamera.depth = 0f;
            }

            Camera secondarySceneCamera = CreateOrUpdateSecondaryCamera(scene, root);

            Scene02TemporalFracturePlayback playback = EnsureComponent<Scene02TemporalFracturePlayback>(wireInteractor);
            SetObjectReference(playback, "interactable", wireInteractor.GetComponent<OutlineInteractable>());
            SetObjectReference(playback, "timelineController", timeline);
            SetObjectReference(playback, "projectionTarget", projectionTarget);
            SetObjectReference(playback, "fragmentRoot", fragmentRoot);
            SetObjectReference(playback, "collapsedVisual", collapseVisual);
            SetObjectReference(playback, "statusLabel", collapseLabel);
            SetObjectReference(playback, "targetCamera", sceneCamera);
            SetObjectReference(playback, "secondaryCamera", secondarySceneCamera);
            SetBool(playback, "followLocalPlayerBeforeTrigger", true);
            SetVector3(playback, "fallbackGameplayFocus", new Vector3(-1.3f, 0.75f, 0.85f));
            SetVector3(playback, "gameplayCameraOffset", new Vector3(0.15f, 5.35f, -4.85f));
            SetVector3(playback, "gameplayCameraEuler", new Vector3(53f, 0f, 0f));
            SetFloat(playback, "gameplayCameraOrthographicSize", 4.15f);
            SetFloat(playback, "gameplayCameraFollowSharpness", 9f);
            SetVector3(playback, "armedTargetPosition", new Vector3(-1.15f, 2.35f, 0.8f));
            SetVector3(playback, "targetDropVelocity", new Vector3(0.15f, -8.25f, 0.25f));
            SetFloat(playback, "fragmentReleaseDelay", 0.28f);
            SetVector3(playback, "fragmentRootPosition", new Vector3(-1.15f, 1.1f, 0.9f));
            SetVector3(playback, "cameraPosition", new Vector3(-1.28f, 5.45f, -2.95f));
            SetVector3(playback, "cameraEuler", new Vector3(59f, 0f, 0f));
            SetFloat(playback, "cameraOrthographicSize", 2.65f);
            SetFloat(playback, "cameraSettleDuration", 0.28f);
            SetFloat(playback, "cameraShakeDuration", 0.55f);
            SetFloat(playback, "cameraShakeAmount", 0.045f);
            SetBool(playback, "showSecondaryView", true);
            SetVector3(playback, "secondaryCameraPosition", new Vector3(0.05f, 7.4f, -4.9f));
            SetVector3(playback, "secondaryCameraEuler", new Vector3(58f, 0f, 0f));
            SetFloat(playback, "secondaryCameraOrthographicSize", 5.5f);
            SetRect(playback, "secondaryCameraViewport", new Rect(0.68f, 0.62f, 0.30f, 0.34f));

            ConfigureTunnelNode(collapseController);
            TemporalPhysicsBody.EnsureAllRigidbodiesHaveTemporalBodies(scene, true);
            EditorUtility.SetDirty(sceneRoot);
        }

        private static void ConfigureVisibleRockfallLayout(Scene scene, GameObject collapseVisual)
        {
            if (collapseVisual == null)
            {
                return;
            }

            GameObject fallenRoof1 = FindInScene(scene, "FallenRoof_1");
            if (fallenRoof1 != null)
            {
                fallenRoof1.transform.position = new Vector3(-1.45f, 0.46f, 0.78f);
                fallenRoof1.transform.localScale = new Vector3(1.05f, 0.45f, 0.75f);
                fallenRoof1.transform.rotation = Quaternion.Euler(0f, 0f, 12f);
            }

            GameObject fallenRoof2 = FindInScene(scene, "FallenRoof_2");
            if (fallenRoof2 != null)
            {
                fallenRoof2.transform.position = new Vector3(-0.75f, 0.58f, 1.1f);
                fallenRoof2.transform.localScale = new Vector3(1.0f, 0.52f, 0.78f);
                fallenRoof2.transform.rotation = Quaternion.Euler(0f, 0f, -11f);
            }

            GameObject crawlGap = FindInScene(scene, "CrawlGap");
            if (crawlGap != null)
            {
                crawlGap.transform.position = new Vector3(-1.1f, 0.11f, 1.0f);
                crawlGap.transform.localScale = new Vector3(1.55f, 0.16f, 1.55f);
            }
        }

        private static void ConfigureTunnelNode(TunnelCollapseController controller)
        {
            GameObject intactVisual = FindInScene(controller.gameObject.scene, "PastWiringNode_N7");
            GameObject collapsedVisual = FindInScene(controller.gameObject.scene, "FutureCollapse_AfterWiring");
            GameObject blocker = FindInScene(controller.gameObject.scene, "FutureBossRouteBlocker_DeactivatesOnCollapse");

            SerializedObject serialized = new SerializedObject(controller);
            SerializedProperty nodes = serialized.FindProperty("nodes");
            nodes.arraySize = 1;
            SerializedProperty node = nodes.GetArrayElementAtIndex(0);
            node.FindPropertyRelative("nodeId").stringValue = "N7";
            node.FindPropertyRelative("intactVisual").objectReferenceValue = intactVisual;
            node.FindPropertyRelative("collapsedVisual").objectReferenceValue = collapsedVisual;
            node.FindPropertyRelative("intactCollider").objectReferenceValue = blocker != null ? blocker.GetComponent<Collider>() : null;

            Collider[] rubbleColliders = collapsedVisual != null
                ? collapsedVisual.GetComponentsInChildren<Collider>(true)
                : Array.Empty<Collider>();
            SerializedProperty rubbleProperty = node.FindPropertyRelative("rubbleColliders");
            rubbleProperty.arraySize = rubbleColliders.Length;
            for (int i = 0; i < rubbleColliders.Length; i++)
            {
                rubbleProperty.GetArrayElementAtIndex(i).objectReferenceValue = rubbleColliders[i];
            }

            node.FindPropertyRelative("isWired").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ValidateScene(Scene scene)
        {
            var failures = new List<string>();
            var warnings = new List<string>();

            GameObject sceneRoot = FindInScene(scene, SceneRootName);
            if (sceneRoot == null)
            {
                failures.Add("missing " + SceneRootName);
                ReportAndThrow(failures, warnings);
                return;
            }

            PastFutureTimelineController timeline = FindDirectChild(sceneRoot.transform, "TemporalPhysicsManager")
                ?.GetComponent<PastFutureTimelineController>();
            TemporalPhysicsProjector projector = FindDirectChild(sceneRoot.transform, "TemporalPhysicsManager")
                ?.GetComponent<TemporalPhysicsProjector>();
            TunnelCollapseController collapse = FindDirectChild(sceneRoot.transform, "TunnelCollapseManager")
                ?.GetComponent<TunnelCollapseController>();

            if (timeline == null) failures.Add("TemporalPhysicsManager is missing PastFutureTimelineController");
            if (projector == null) failures.Add("TemporalPhysicsManager is missing TemporalPhysicsProjector");
            if (collapse == null) failures.Add("TunnelCollapseManager is missing TunnelCollapseController");
            if (timeline != null && projector != null && ReadObjectReference(timeline, "temporalPhysicsProjector") != projector)
                failures.Add("PastFutureTimelineController.temporalPhysicsProjector is not wired to Scene02 projector");
            if (collapse != null && !ReadBool(collapse, "useTemporalProjection"))
                failures.Add("TunnelCollapseController.useTemporalProjection is false");
            if (collapse != null && timeline != null && ReadObjectReference(collapse, "timelineController") != timeline)
                failures.Add("TunnelCollapseController.timelineController is not wired to Scene02 timeline");

            GameObject wireInteractor = FindInScene(scene, "TunnelWiringInteractor");
            TemporalOutlineInteractable temporalInteractable = wireInteractor != null
                ? wireInteractor.GetComponent<TemporalOutlineInteractable>()
                : null;
            Scene02TemporalFracturePlayback playback = wireInteractor != null
                ? wireInteractor.GetComponent<Scene02TemporalFracturePlayback>()
                : null;
            if (temporalInteractable == null) failures.Add("TunnelWiringInteractor is missing TemporalOutlineInteractable");
            if (temporalInteractable != null && timeline != null && ReadObjectReference(temporalInteractable, "timelineController") != timeline)
                failures.Add("TemporalOutlineInteractable.timelineController is not explicit");
            if (playback == null) failures.Add("TunnelWiringInteractor is missing Scene02TemporalFracturePlayback");
            if (playback != null && timeline != null && ReadObjectReference(playback, "timelineController") != timeline)
                failures.Add("Scene02TemporalFracturePlayback.timelineController is not wired to Scene02 timeline");

            GameObject target = FindInScene(scene, ProjectionTargetName);
            GameObject trigger = FindInScene(scene, TriggerName);
            GameObject fragments = FindInScene(scene, FragmentRootName);
            if (target == null) failures.Add("missing projection target " + ProjectionTargetName);
            if (trigger == null) failures.Add("missing fracture trigger " + TriggerName);
            if (fragments == null) failures.Add("missing fragment root " + FragmentRootName);

            if (target != null)
            {
                if (target.GetComponent<Rigidbody>() == null) failures.Add("projection target missing Rigidbody");
                if (target.GetComponent<Collider>() == null) failures.Add("projection target missing Collider");
                if (target.GetComponent<TemporalPhysicsBody>() == null) failures.Add("projection target missing TemporalPhysicsBody");
                if (target.GetComponent<TemporalProjectionFractureOnCollision>() == null)
                    failures.Add("projection target missing TemporalProjectionFractureOnCollision");
            }

            if (trigger != null)
            {
                Collider triggerCollider = trigger.GetComponent<Collider>();
                if (triggerCollider == null) failures.Add("fracture trigger missing Collider");
                else if (!triggerCollider.isTrigger) failures.Add("fracture trigger collider is not marked trigger");
            }

            if (fragments != null)
            {
                Rigidbody[] fragmentRigidbodies = fragments.GetComponentsInChildren<Rigidbody>(true);
                Collider[] fragmentColliders = fragments.GetComponentsInChildren<Collider>(true);
                if (fragmentRigidbodies.Length < 4) failures.Add("fracFragments has fewer than 4 child/root rigidbodies");
                if (fragmentColliders.Length < 4) failures.Add("fracFragments has fewer than 4 child colliders");
            }

            Camera sceneCamera = FindInScene(scene, "Main Camera")?.GetComponent<Camera>();
            if (sceneCamera == null) failures.Add("Scene02 missing Main Camera");
            else if (!sceneCamera.orthographic || sceneCamera.orthographicSize > 5.8f)
                failures.Add("Scene02 camera is still too wide for fracture playback");

            Camera secondarySceneCamera = FindInScene(scene, SecondaryCameraName)?.GetComponent<Camera>();
            if (secondarySceneCamera == null) failures.Add("Scene02 missing secondary dual-view camera");
            else if (!secondarySceneCamera.orthographic || secondarySceneCamera.rect.width < 0.2f)
                failures.Add("Scene02 secondary dual-view camera is not configured as picture-in-picture");

            if (FindRootNamed(scene, "TemporalPhysicsManager") != null)
                failures.Add("root-level TemporalPhysicsManager duplicate exists outside " + SceneRootName);
            if (FindRootNamed(scene, "TunnelCollapseManager") != null)
                failures.Add("root-level TunnelCollapseManager duplicate exists outside " + SceneRootName);

            ReportAndThrow(failures, warnings);
        }

        private static void ReportAndThrow(List<string> failures, List<string> warnings)
        {
            foreach (string warning in warnings)
            {
                Debug.LogWarning("[FHQ][Scene02Smoke] " + warning);
            }

            if (failures.Count == 0)
            {
                return;
            }

            foreach (string failure in failures)
            {
                Debug.LogError("[FHQ][Scene02Smoke] " + failure);
            }

            throw new InvalidOperationException("[FHQ][Scene02Smoke] Validation failed: " + string.Join("; ", failures));
        }

        private static void SetProjectorDefaults(TemporalPhysicsProjector projector)
        {
            SetFloat(projector, "simulationStep", 0.02f);
            SetInt(projector, "maxSimulationSteps", 2000);
            SetInt(projector, "stepsPerFrame", 2000);
            SetBool(projector, "fastForwardInSingleFrame", true);
            SetBool(projector, "cloneStaticColliders", true);
            SetBool(projector, "removeBodiesOutsideBounds", true);
            SetBool(projector, "autoFitBoundsFromStaticColliders", true);
            SetFloat(projector, "staticBoundsPadding", 4f);
            SetFloat(projector, "verticalBoundsPadding", 12f);
        }

        private static void RemoveDuplicateRootManagers(Scene scene, GameObject sceneRoot)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root == null || root == sceneRoot)
                {
                    continue;
                }

                if (root.name == "TemporalPhysicsManager" || root.name == "TunnelCollapseManager")
                {
                    Debug.LogWarning("[FHQ][Scene02Smoke] Removing duplicate root-level " + root.name + ".");
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static GameObject CreateOrUpdateCube(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject go = FindDirectChild(parent, name);
            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = name;
                go.transform.SetParent(parent, true);
            }

            go.transform.position = position;
            go.transform.localScale = scale;
            go.SetActive(true);
            return go;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static GameObject Required(GameObject target, string name)
        {
            if (target == null)
            {
                throw new InvalidOperationException("[FHQ][Scene02Smoke] Missing required object: " + name);
            }

            return target;
        }

        private static GameObject FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }

                Transform[] children = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child != null && child.name == name)
                    {
                        return child.gameObject;
                    }
                }
            }

            return null;
        }

        private static Camera CreateOrUpdateSecondaryCamera(Scene scene, Transform root)
        {
            GameObject cameraGo = FindInScene(scene, SecondaryCameraName) ?? CreateChild(root, SecondaryCameraName);
            cameraGo.SetActive(true);
            cameraGo.transform.position = new Vector3(0.05f, 7.4f, -4.9f);
            cameraGo.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

            Camera camera = EnsureComponent<Camera>(cameraGo);
            camera.enabled = true;
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.rect = new Rect(0.68f, 0.62f, 0.30f, 0.34f);
            camera.depth = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.03f, 0.035f, 1f);
            camera.cullingMask = -1;

            AudioListener listener = cameraGo.GetComponent<AudioListener>();
            if (listener != null)
            {
                UnityEngine.Object.DestroyImmediate(listener);
            }

            return camera;
        }

        private static GameObject FindRootNamed(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return null;
        }

        private static GameObject FindDirectChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.gameObject : null;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static UnityEngine.Object ReadObjectReference(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null ? property.objectReferenceValue : null;
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static bool ReadBool(UnityEngine.Object target, string propertyName)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null && property.boolValue;
        }

        private static void SetString(UnityEngine.Object target, string propertyName, string value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetRect(UnityEngine.Object target, string propertyName, Rect value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.rectValue = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
