using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TemporalPhysicsProjector : MonoBehaviour
{
    private const float DefaultStaticBoundsPadding = 4f;
    private const float DefaultVerticalBoundsPadding = 12f;

    [Header("Simulation")]
    [SerializeField] private string projectionSceneName = "TemporalPhysicsProjection";
    [SerializeField] private float simulationStep = 0.02f;
    [SerializeField] private int maxSimulationSteps = 2000;
    [SerializeField] private int stepsPerFrame = 2000;
    [SerializeField] private bool fastForwardInSingleFrame = true;

    [Header("Convergence")]
    [SerializeField] private float linearVelocityThreshold = 0.12f;
    [SerializeField] private float angularVelocityThreshold = 0.25f;
    [SerializeField] private int requiredStaticFrames = 8;

    [Header("Discovery")]
    [SerializeField] private bool includeInactiveBodies;
    [SerializeField] private bool cloneStaticColliders = true;

    [Header("Projection Boundary")]
    [SerializeField] private bool removeBodiesOutsideBounds = true;
    [SerializeField] private bool autoFitBoundsFromStaticColliders = true;
    [SerializeField] private Vector3 manualBoundsCenter = Vector3.zero;
    [SerializeField] private Vector3 manualBoundsSize = new Vector3(80f, 40f, 80f);
    [SerializeField] private float staticBoundsPadding = DefaultStaticBoundsPadding;
    [SerializeField] private float verticalBoundsPadding = DefaultVerticalBoundsPadding;
    [SerializeField] private float boundaryContactTolerance = 0.02f;

    public event Action<TemporalWorldState> OnProjectionCompleted;

    public bool IsProjecting { get; private set; }
    public TemporalWorldState LastProjection { get; private set; }
    public int LastBoundaryRemovalCount { get; private set; }

    private readonly List<TemporalPhysicsBody> trackedMainBodies = new List<TemporalPhysicsBody>();
    private readonly List<TemporalPhysicsBody> trackedCloneBodies = new List<TemporalPhysicsBody>();
    private readonly List<ITemporalProjectionStepBehaviour> projectionStepBehaviours =
        new List<ITemporalProjectionStepBehaviour>();
    private readonly Dictionary<TemporalPhysicsBody, TemporalPhysicsBody> mainToClone =
        new Dictionary<TemporalPhysicsBody, TemporalPhysicsBody>();
    private readonly Dictionary<TemporalPhysicsBody, Collider[]> cloneColliders =
        new Dictionary<TemporalPhysicsBody, Collider[]>();

    private Scene projectionScene;
    private PhysicsScene projectionPhysicsScene;
    private Coroutine activeProjection;
    private int currentBoundaryRemovalCount;

    public Coroutine StartProjection(Action<TemporalWorldState> onCompleted = null)
    {
        if (IsProjecting)
        {
            Debug.LogWarning("[TemporalPhysicsProjector] Projection already running.");
            return activeProjection;
        }

        activeProjection = StartCoroutine(ProjectRoutine(onCompleted));
        return activeProjection;
    }

    public Coroutine RequestProjection(Action<TemporalWorldState> onCompleted = null)
    {
        return StartProjection(onCompleted);
    }

    public Coroutine ProjectCurrentScene(Action<TemporalWorldState> onCompleted = null)
    {
        return StartProjection(onCompleted);
    }

    public int ApplyFutureStateToLiveBodies(TemporalWorldState futureState, bool includeVelocities = true)
    {
        if (futureState == null || futureState.objects == null)
        {
            return 0;
        }

        Dictionary<string, TemporalObjectState> statesById = new Dictionary<string, TemporalObjectState>();
        foreach (TemporalObjectState state in futureState.objects)
        {
            if (state != null && !string.IsNullOrWhiteSpace(state.objectId))
            {
                statesById[state.objectId] = state;
            }
        }

        if (statesById.Count == 0)
        {
            return 0;
        }

        Scene sourceScene = GetSourceScene();
        TemporalPhysicsBody.EnsureAllRigidbodiesHaveTemporalBodies(sourceScene, true);
        TemporalPhysicsBody[] liveBodies = FindObjectsByType<TemporalPhysicsBody>(
            FindObjectsInactive.Include);

        int appliedCount = 0;
        foreach (TemporalPhysicsBody body in liveBodies)
        {
            if (body == null || body.gameObject.scene != sourceScene)
            {
                continue;
            }

            if (body.IsExcludedFromTemporalProjection())
            {
                continue;
            }

            if (statesById.TryGetValue(body.ObjectId, out TemporalObjectState state))
            {
                body.ApplyState(state, includeVelocities);
                appliedCount++;
            }
        }

        return appliedCount;
    }

    public void CancelProjection()
    {
        if (activeProjection != null)
        {
            StopCoroutine(activeProjection);
            activeProjection = null;
        }

        IsProjecting = false;
        CleanupProjectionScene();
    }

    private IEnumerator ProjectRoutine(Action<TemporalWorldState> onCompleted)
    {
        IsProjecting = true;
        TemporalWorldState result = null;

        try
        {
            currentBoundaryRemovalCount = 0;
            CreateProjectionScene();

            Scene sourceScene = GetSourceScene();
            TemporalPhysicsBody.EnsureAllRigidbodiesHaveTemporalBodies(sourceScene, includeInactiveBodies);
            CloneStaticEnvironment(sourceScene);
            CloneTemporalBodies(sourceScene);
            ApplyMainStatesToClones();
            RegisterProjectionStepBehaviours();
            Bounds projectionBounds = CalculateProjectionBounds(sourceScene);
            CreateProjectionBoundaryTriggers(projectionBounds);
            RemoveCloneBodiesTouchingBoundary(projectionBounds);

            int simulatedSteps = 0;
            int staticFrames = 0;
            bool converged = trackedCloneBodies.Count == 0;

            int maxSteps = Mathf.Max(1, maxSimulationSteps);
            int frameBudget = Mathf.Max(1, stepsPerFrame);
            int requiredFrames = Mathf.Max(1, requiredStaticFrames);
            float stepDuration = GetSimulationStep();

            while (!converged && simulatedSteps < maxSteps)
            {
                int stepsThisFrame = 0;
                while (stepsThisFrame < frameBudget && simulatedSteps < maxSteps)
                {
                    projectionPhysicsScene.Simulate(stepDuration);
                    RunProjectionStepBehaviours(stepDuration, simulatedSteps);
                    RegisterProjectionSceneTemporalBodies();
                    RemoveCloneBodiesTouchingBoundary(projectionBounds);
                    simulatedSteps++;
                    stepsThisFrame++;

                    if (AreCloneBodiesStatic())
                    {
                        staticFrames++;
                        if (staticFrames >= requiredFrames)
                        {
                            converged = true;
                            break;
                        }
                    }
                    else
                    {
                        staticFrames = 0;
                    }
                }

                if (!fastForwardInSingleFrame && !converged && simulatedSteps < maxSteps)
                {
                    yield return null;
                }
            }

            RegisterProjectionSceneTemporalBodies();
            result = CaptureProjectionWorldState(simulatedSteps, converged, stepDuration);
            LastBoundaryRemovalCount = currentBoundaryRemovalCount;
        }
        finally
        {
            CleanupProjectionScene();
            activeProjection = null;
            IsProjecting = false;
        }

        LastProjection = result;
        if (result != null)
        {
            onCompleted?.Invoke(result);
            OnProjectionCompleted?.Invoke(result);
        }
    }

    private void CreateProjectionScene()
    {
        CleanupProjectionScene();

        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        string sceneName = projectionSceneName + "_" + Guid.NewGuid().ToString("N");
        projectionScene = SceneManager.CreateScene(sceneName, parameters);
        projectionPhysicsScene = projectionScene.GetPhysicsScene();

        if (!projectionScene.IsValid() || !projectionPhysicsScene.IsValid())
        {
            throw new InvalidOperationException("Failed to create temporal projection physics scene.");
        }
    }

    private Scene GetSourceScene()
    {
        Scene sourceScene = gameObject.scene;
        if (sourceScene.IsValid() && sourceScene.isLoaded && sourceScene != projectionScene)
        {
            return sourceScene;
        }

        return SceneManager.GetActiveScene();
    }

    private void CloneTemporalBodies(Scene sourceScene)
    {
        trackedMainBodies.Clear();
        trackedCloneBodies.Clear();
        mainToClone.Clear();
        cloneColliders.Clear();

        List<TemporalPhysicsBody> mainBodies = FindMainSceneBodies(sourceScene);
        mainBodies.Sort(CompareHierarchyDepth);

        foreach (TemporalPhysicsBody mainBody in mainBodies)
        {
            if (mainBody == null || mainBody.IsExcludedFromTemporalProjection())
            {
                continue;
            }

            TemporalPhysicsBody cloneBody = TryFindCloneFromMappedAncestor(mainBody);
            if (cloneBody == null)
            {
                GameObject clone = Instantiate(mainBody.gameObject);
                clone.name = mainBody.gameObject.name + " (Temporal Projection)";
                SceneManager.MoveGameObjectToScene(clone, projectionScene);
                DisableProjectionSideEffects(clone);
                cloneBody = clone.GetComponent<TemporalPhysicsBody>();
            }

            if (cloneBody == null)
            {
                Debug.LogWarning("[TemporalPhysicsProjector] Clone is missing TemporalPhysicsBody: " + mainBody.name);
                continue;
            }

            trackedMainBodies.Add(mainBody);
            trackedCloneBodies.Add(cloneBody);
            mainToClone[mainBody] = cloneBody;
            cloneColliders[cloneBody] = cloneBody.GetComponentsInChildren<Collider>(includeInactiveBodies);
        }
    }

    private void CloneStaticEnvironment(Scene sourceScene)
    {
        if (!cloneStaticColliders)
        {
            return;
        }

        GameObject[] roots = sourceScene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root == null || root == gameObject)
            {
                continue;
            }

            if (root.GetComponentInChildren<TemporalPhysicsBody>(includeInactiveBodies) != null)
            {
                continue;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactiveBodies);
            if (colliders.Length == 0)
            {
                continue;
            }

            if (ContainsNonKinematicRigidbody(root))
            {
                continue;
            }

            GameObject clone = Instantiate(root);
            clone.name = root.name + " (Temporal Static)";
            SceneManager.MoveGameObjectToScene(clone, projectionScene);
            DisableProjectionSideEffects(clone);
        }
    }

    private bool ContainsNonKinematicRigidbody(GameObject root)
    {
        Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(includeInactiveBodies);
        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb != null && !rb.isKinematic)
            {
                return true;
            }
        }

        return false;
    }

    private List<TemporalPhysicsBody> FindMainSceneBodies(Scene sourceScene)
    {
        TemporalPhysicsBody.EnsureAllRigidbodiesHaveTemporalBodies(sourceScene, includeInactiveBodies);

        FindObjectsInactive inactiveMode = includeInactiveBodies
            ? FindObjectsInactive.Include
            : FindObjectsInactive.Exclude;

        TemporalPhysicsBody[] bodies = FindObjectsByType<TemporalPhysicsBody>(inactiveMode);

        List<TemporalPhysicsBody> result = new List<TemporalPhysicsBody>();
        foreach (TemporalPhysicsBody body in bodies)
        {
            if (body == null || body.gameObject.scene != sourceScene)
            {
                continue;
            }

            if (body.IsExcludedFromTemporalProjection())
            {
                continue;
            }

            result.Add(body);
        }

        return result;
    }

    private void ApplyMainStatesToClones()
    {
        for (int i = 0; i < trackedMainBodies.Count; i++)
        {
            TemporalPhysicsBody mainBody = trackedMainBodies[i];
            TemporalPhysicsBody cloneBody = trackedCloneBodies[i];

            if (mainBody == null || cloneBody == null)
            {
                continue;
            }

            TemporalObjectState state = mainBody.CaptureState();
            cloneBody.ApplyState(state, true);
        }
    }

    private TemporalWorldState CaptureProjectionWorldState(
        int simulatedSteps,
        bool converged,
        float stepDuration)
    {
        TemporalWorldState worldState = new TemporalWorldState
        {
            tick = Time.frameCount,
            simulatedSteps = simulatedSteps,
            simulatedSeconds = simulatedSteps * stepDuration,
            converged = converged,
            objects = new List<TemporalObjectState>()
        };

        foreach (TemporalPhysicsBody cloneBody in trackedCloneBodies)
        {
            if (cloneBody == null)
            {
                continue;
            }

            worldState.objects.Add(cloneBody.CaptureState());
        }

        return worldState;
    }

    private void RegisterProjectionSceneTemporalBodies()
    {
        if (!projectionScene.IsValid())
        {
            return;
        }

        TemporalPhysicsBody.EnsureAllRigidbodiesHaveTemporalBodies(projectionScene, true);
        TemporalPhysicsBody[] bodies = FindObjectsByType<TemporalPhysicsBody>(FindObjectsInactive.Include);
        foreach (TemporalPhysicsBody body in bodies)
        {
            if (body == null || body.gameObject.scene != projectionScene)
            {
                continue;
            }

            if (body.IsExcludedFromTemporalProjection() || trackedCloneBodies.Contains(body))
            {
                continue;
            }

            trackedCloneBodies.Add(body);
            cloneColliders[body] = body.GetComponentsInChildren<Collider>(includeInactiveBodies);
        }
    }

    private void RegisterProjectionStepBehaviours()
    {
        projectionStepBehaviours.Clear();

        if (!projectionScene.IsValid())
        {
            return;
        }

        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null ||
                behaviour.gameObject.scene != projectionScene ||
                behaviour is not ITemporalProjectionStepBehaviour stepBehaviour ||
                !stepBehaviour.RunInTemporalProjection ||
                projectionStepBehaviours.Contains(stepBehaviour))
            {
                continue;
            }

            projectionStepBehaviours.Add(stepBehaviour);
        }
    }

    private void RunProjectionStepBehaviours(float stepDuration, int simulatedStep)
    {
        for (int i = projectionStepBehaviours.Count - 1; i >= 0; i--)
        {
            ITemporalProjectionStepBehaviour stepBehaviour = projectionStepBehaviours[i];
            if (stepBehaviour is not MonoBehaviour behaviour || behaviour == null)
            {
                projectionStepBehaviours.RemoveAt(i);
                continue;
            }

            if (!behaviour.isActiveAndEnabled)
            {
                continue;
            }

            stepBehaviour.SimulateProjectionStep(
                projectionPhysicsScene,
                stepDuration,
                simulatedStep);
        }
    }

    private bool AreCloneBodiesStatic()
    {
        float linearThresholdSqr = linearVelocityThreshold * linearVelocityThreshold;
        float angularThresholdSqr = angularVelocityThreshold * angularVelocityThreshold;

        foreach (TemporalPhysicsBody cloneBody in trackedCloneBodies)
        {
            if (cloneBody == null)
            {
                continue;
            }

            if (!cloneBody.gameObject.activeSelf)
            {
                continue;
            }

            Rigidbody rb = cloneBody.Rigidbody;
            if (rb == null)
            {
                rb = cloneBody.GetComponent<Rigidbody>();
            }

            if (rb == null || rb.isKinematic || rb.IsSleeping())
            {
                continue;
            }

            if (rb.linearVelocity.sqrMagnitude > linearThresholdSqr ||
                rb.angularVelocity.sqrMagnitude > angularThresholdSqr)
            {
                return false;
            }
        }

        return true;
    }

    private Bounds CalculateProjectionBounds(Scene sourceScene)
    {
        Bounds bounds = new Bounds(manualBoundsCenter, GetSafeBoundsSize(manualBoundsSize));
        if (!autoFitBoundsFromStaticColliders)
        {
            return bounds;
        }

        bool hasCollider = false;
        foreach (GameObject root in sourceScene.GetRootGameObjects())
        {
            if (root == null || root.GetComponentInChildren<TemporalPhysicsBody>(includeInactiveBodies) != null)
            {
                continue;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(includeInactiveBodies);
            foreach (Collider collider in colliders)
            {
                if (collider == null)
                {
                    continue;
                }

                if (!hasCollider)
                {
                    bounds = collider.bounds;
                    hasCollider = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }
        }

        if (!hasCollider)
        {
            return bounds;
        }

        float horizontalPadding = Mathf.Max(DefaultStaticBoundsPadding, staticBoundsPadding);
        float verticalPadding = Mathf.Max(DefaultVerticalBoundsPadding, verticalBoundsPadding);

        Vector3 size = bounds.size;
        size.x += horizontalPadding * 2f;
        size.z += horizontalPadding * 2f;
        size.y += verticalPadding * 2f;
        bounds.size = GetSafeBoundsSize(size);
        return bounds;
    }

    private Vector3 GetSafeBoundsSize(Vector3 size)
    {
        return new Vector3(
            Mathf.Max(1f, Mathf.Abs(size.x)),
            Mathf.Max(1f, Mathf.Abs(size.y)),
            Mathf.Max(1f, Mathf.Abs(size.z)));
    }

    private void RemoveCloneBodiesTouchingBoundary(Bounds projectionBounds)
    {
        if (!removeBodiesOutsideBounds)
        {
            return;
        }

        foreach (TemporalPhysicsBody cloneBody in trackedCloneBodies)
        {
            if (cloneBody == null || !cloneBody.gameObject.activeSelf)
            {
                continue;
            }

            if (IsProjectionBodyInsideBounds(cloneBody, projectionBounds))
            {
                continue;
            }

            DeactivateProjectionBody(cloneBody);
            currentBoundaryRemovalCount++;
        }
    }

    private bool IsProjectionBodyInsideBounds(
        TemporalPhysicsBody cloneBody,
        Bounds projectionBounds)
    {
        if (!cloneColliders.TryGetValue(cloneBody, out Collider[] colliders))
        {
            colliders = cloneBody.GetComponentsInChildren<Collider>(includeInactiveBodies);
            cloneColliders[cloneBody] = colliders;
        }

        bool hasEnabledCollider = false;
        foreach (Collider bodyCollider in colliders)
        {
            if (bodyCollider == null || !bodyCollider.enabled)
            {
                continue;
            }

            hasEnabledCollider = true;
            if (BoundsTouchOrCrossBoundary(bodyCollider.bounds, projectionBounds))
            {
                return false;
            }
        }

        if (hasEnabledCollider)
        {
            return true;
        }

        Vector3 position = cloneBody.Rigidbody != null
            ? cloneBody.Rigidbody.worldCenterOfMass
            : cloneBody.transform.position;

        return !PointTouchesOrCrossesBoundary(position, projectionBounds);
    }

    private bool BoundsTouchOrCrossBoundary(Bounds bodyBounds, Bounds projectionBounds)
    {
        float tolerance = Mathf.Max(0f, boundaryContactTolerance);
        Vector3 min = projectionBounds.min;
        Vector3 max = projectionBounds.max;

        return bodyBounds.min.x <= min.x + tolerance ||
            bodyBounds.min.y <= min.y + tolerance ||
            bodyBounds.min.z <= min.z + tolerance ||
            bodyBounds.max.x >= max.x - tolerance ||
            bodyBounds.max.y >= max.y - tolerance ||
            bodyBounds.max.z >= max.z - tolerance;
    }

    private bool PointTouchesOrCrossesBoundary(Vector3 point, Bounds projectionBounds)
    {
        float tolerance = Mathf.Max(0f, boundaryContactTolerance);
        Vector3 min = projectionBounds.min;
        Vector3 max = projectionBounds.max;

        return point.x <= min.x + tolerance ||
            point.y <= min.y + tolerance ||
            point.z <= min.z + tolerance ||
            point.x >= max.x - tolerance ||
            point.y >= max.y - tolerance ||
            point.z >= max.z - tolerance;
    }

    private void CreateProjectionBoundaryTriggers(Bounds projectionBounds)
    {
        if (!removeBodiesOutsideBounds)
        {
            return;
        }

        float thickness = Mathf.Max(0.1f, boundaryContactTolerance * 4f);
        Vector3 center = projectionBounds.center;
        Vector3 size = projectionBounds.size;
        Vector3 min = projectionBounds.min;
        Vector3 max = projectionBounds.max;

        CreateProjectionBoundaryTrigger(
            "Temporal Projection Kill Boundary -X",
            new Vector3(min.x, center.y, center.z),
            new Vector3(thickness, size.y + thickness * 2f, size.z + thickness * 2f));

        CreateProjectionBoundaryTrigger(
            "Temporal Projection Kill Boundary +X",
            new Vector3(max.x, center.y, center.z),
            new Vector3(thickness, size.y + thickness * 2f, size.z + thickness * 2f));

        CreateProjectionBoundaryTrigger(
            "Temporal Projection Kill Boundary -Y",
            new Vector3(center.x, min.y, center.z),
            new Vector3(size.x + thickness * 2f, thickness, size.z + thickness * 2f));

        CreateProjectionBoundaryTrigger(
            "Temporal Projection Kill Boundary +Y",
            new Vector3(center.x, max.y, center.z),
            new Vector3(size.x + thickness * 2f, thickness, size.z + thickness * 2f));

        CreateProjectionBoundaryTrigger(
            "Temporal Projection Kill Boundary -Z",
            new Vector3(center.x, center.y, min.z),
            new Vector3(size.x + thickness * 2f, size.y + thickness * 2f, thickness));

        CreateProjectionBoundaryTrigger(
            "Temporal Projection Kill Boundary +Z",
            new Vector3(center.x, center.y, max.z),
            new Vector3(size.x + thickness * 2f, size.y + thickness * 2f, thickness));
    }

    private void CreateProjectionBoundaryTrigger(string name, Vector3 center, Vector3 size)
    {
        GameObject boundary = new GameObject(name);
        SceneManager.MoveGameObjectToScene(boundary, projectionScene);
        boundary.transform.position = center;

        BoxCollider boundaryCollider = boundary.AddComponent<BoxCollider>();
        boundaryCollider.isTrigger = true;
        boundaryCollider.size = GetSafeBoundsSize(size);

        boundary.AddComponent<TemporalProjectionBoundary>();
    }

    private void DeactivateProjectionBody(TemporalPhysicsBody cloneBody)
    {
        Rigidbody rb = cloneBody.Rigidbody != null
            ? cloneBody.Rigidbody
            : cloneBody.GetComponent<Rigidbody>();

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        cloneBody.gameObject.SetActive(false);
    }

    private TemporalPhysicsBody TryFindCloneFromMappedAncestor(TemporalPhysicsBody mainBody)
    {
        Transform mainTransform = mainBody.transform.parent;
        while (mainTransform != null)
        {
            TemporalPhysicsBody ancestorBody = mainTransform.GetComponent<TemporalPhysicsBody>();
            if (ancestorBody != null &&
                mainToClone.TryGetValue(ancestorBody, out TemporalPhysicsBody ancestorClone) &&
                ancestorClone != null)
            {
                Transform cloneTransform = FindRelativeCloneTransform(
                    ancestorBody.transform,
                    ancestorClone.transform,
                    mainBody.transform);

                return cloneTransform != null
                    ? cloneTransform.GetComponent<TemporalPhysicsBody>()
                    : null;
            }

            mainTransform = mainTransform.parent;
        }

        return null;
    }

    private Transform FindRelativeCloneTransform(
        Transform mainRoot,
        Transform cloneRoot,
        Transform mainTarget)
    {
        Stack<int> siblingPath = new Stack<int>();
        Transform current = mainTarget;
        while (current != null && current != mainRoot)
        {
            siblingPath.Push(current.GetSiblingIndex());
            current = current.parent;
        }

        if (current != mainRoot)
        {
            return null;
        }

        Transform cloneCurrent = cloneRoot;
        while (siblingPath.Count > 0)
        {
            int index = siblingPath.Pop();
            if (index < 0 || index >= cloneCurrent.childCount)
            {
                return null;
            }

            cloneCurrent = cloneCurrent.GetChild(index);
        }

        return cloneCurrent;
    }

    private void DisableProjectionSideEffects(GameObject cloneRoot)
    {
        MonoBehaviour[] behaviours = cloneRoot.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null ||
                behaviour is TemporalPhysicsBody ||
                behaviour is TemporalProjectionBoundary ||
                behaviour is TemporalProjectionRuntimeBehaviourAllowList)
            {
                continue;
            }

            if (ShouldRunBehaviourInProjection(behaviour, out bool forceEnable))
            {
                if (forceEnable)
                {
                    behaviour.enabled = true;
                }

                continue;
            }

            behaviour.enabled = false;
        }
    }

    private bool ShouldRunBehaviourInProjection(MonoBehaviour behaviour, out bool forceEnable)
    {
        forceEnable = false;
        if (behaviour == null)
        {
            return false;
        }

        if (behaviour is ITemporalProjectionRuntimeBehaviour runtimeBehaviour &&
            runtimeBehaviour.RunInTemporalProjection)
        {
            forceEnable = true;
            return true;
        }

        TemporalProjectionRuntimeBehaviourAllowList[] allowLists =
            behaviour.GetComponentsInParent<TemporalProjectionRuntimeBehaviourAllowList>(true);

        foreach (TemporalProjectionRuntimeBehaviourAllowList allowList in allowLists)
        {
            if (allowList == null || !allowList.Allows(behaviour))
            {
                continue;
            }

            forceEnable = allowList.ForceEnableAllowedBehaviours;
            return true;
        }

        return false;
    }

    private int CompareHierarchyDepth(TemporalPhysicsBody a, TemporalPhysicsBody b)
    {
        int aDepth = GetHierarchyDepth(a != null ? a.transform : null);
        int bDepth = GetHierarchyDepth(b != null ? b.transform : null);
        return aDepth.CompareTo(bDepth);
    }

    private int GetHierarchyDepth(Transform transform)
    {
        int depth = 0;
        while (transform != null)
        {
            depth++;
            transform = transform.parent;
        }

        return depth;
    }

    private float GetSimulationStep()
    {
        return simulationStep > 0f ? simulationStep : Time.fixedDeltaTime;
    }

    private void CleanupProjectionScene()
    {
        trackedMainBodies.Clear();
        trackedCloneBodies.Clear();
        projectionStepBehaviours.Clear();
        mainToClone.Clear();
        cloneColliders.Clear();

        if (projectionScene.IsValid())
        {
            SceneManager.UnloadSceneAsync(projectionScene);
        }

        projectionScene = default;
        projectionPhysicsScene = default;
    }
}
