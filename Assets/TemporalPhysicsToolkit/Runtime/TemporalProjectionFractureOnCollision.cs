using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class TemporalProjectionFractureOnCollision : MonoBehaviour, ITemporalProjectionStepBehaviour
{
    [SerializeField] private string triggerTag = "TemporalFractureTrigger";
    [SerializeField] private string fragmentRootName = "fracFragments";
    [SerializeField] private string projectionSceneNamePrefix = "TemporalPhysicsProjection";
    [SerializeField] private bool runInTemporalProjection = true;
    [SerializeField] private bool fractureOnlyOnce = true;
    [SerializeField] private float projectionContactPadding = 0.03f;

    private bool fractured;
    private readonly Collider[] overlapResults = new Collider[32];

    public bool RunInTemporalProjection => runInTemporalProjection;

    public void SimulateProjectionStep(
        PhysicsScene physicsScene,
        float stepDuration,
        int simulatedStep)
    {
        if (!runInTemporalProjection ||
            fractured ||
            !isActiveAndEnabled ||
            !IsRunningInTemporalProjectionScene())
        {
            return;
        }

        if (TouchesTriggerInProjection(physicsScene))
        {
            FractureInProjection();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsRunningInTemporalProjectionScene())
        {
            return;
        }

        if (fractureOnlyOnce && fractured)
        {
            return;
        }

        if (collision == null ||
            collision.collider == null ||
            !IsTriggerCollider(collision.collider))
        {
            return;
        }

        FractureInProjection();
    }

    public void FractureInProjection()
    {
        if (!IsRunningInTemporalProjectionScene())
        {
            return;
        }

        if (fractureOnlyOnce && fractured)
        {
            return;
        }

        GameObject fragmentRoot = FindFragmentRootInScene();
        if (fragmentRoot == null)
        {
            Debug.LogWarning(
                "[TemporalProjectionFractureOnCollision] Could not find fragment root '" +
                fragmentRootName +
                "' in projection scene.",
                this);
            return;
        }

        Rigidbody sourceRigidbody = GetComponent<Rigidbody>();
        Vector3 inheritedVelocity = sourceRigidbody != null
            ? sourceRigidbody.linearVelocity
            : Vector3.zero;
        Vector3 inheritedAngularVelocity = sourceRigidbody != null
            ? sourceRigidbody.angularVelocity
            : Vector3.zero;

        fragmentRoot.transform.SetPositionAndRotation(transform.position, transform.rotation);
        fragmentRoot.SetActive(true);

        Rigidbody rootRigidbody = fragmentRoot.GetComponent<Rigidbody>();
        if (rootRigidbody != null)
        {
            rootRigidbody.isKinematic = true;
            rootRigidbody.useGravity = false;
            rootRigidbody.detectCollisions = false;
        }

        Rigidbody[] fragmentRigidbodies = fragmentRoot.GetComponentsInChildren<Rigidbody>(true);
        int activatedCount = 0;
        foreach (Rigidbody fragmentRigidbody in fragmentRigidbodies)
        {
            if (fragmentRigidbody == null || fragmentRigidbody == rootRigidbody)
            {
                continue;
            }

            fragmentRigidbody.gameObject.SetActive(true);
            fragmentRigidbody.isKinematic = false;
            fragmentRigidbody.useGravity = true;
            fragmentRigidbody.detectCollisions = true;
            fragmentRigidbody.constraints = RigidbodyConstraints.None;
            fragmentRigidbody.linearVelocity = inheritedVelocity;
            fragmentRigidbody.angularVelocity = inheritedAngularVelocity;
            fragmentRigidbody.WakeUp();
            activatedCount++;
        }

        fractured = true;
        gameObject.SetActive(false);
        Debug.Log(
            "[TemporalProjectionFractureOnCollision] Activated " +
            activatedCount +
            " projected fragments from '" +
            fragmentRoot.name +
            "'.",
            fragmentRoot);
    }

    private bool TouchesTriggerInProjection(PhysicsScene physicsScene)
    {
        if (!physicsScene.IsValid())
        {
            return false;
        }

        Collider[] sourceColliders = GetComponentsInChildren<Collider>(false);
        foreach (Collider sourceCollider in sourceColliders)
        {
            if (sourceCollider == null || !sourceCollider.enabled)
            {
                continue;
            }

            Bounds bounds = sourceCollider.bounds;
            if (bounds.size.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            Vector3 halfExtents = bounds.extents +
                Vector3.one * Mathf.Max(0f, projectionContactPadding);

            int hitCount = physicsScene.OverlapBox(
                bounds.center,
                halfExtents,
                overlapResults,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = overlapResults[i];
                if (hit == null ||
                    hit == sourceCollider ||
                    hit.transform.IsChildOf(transform) ||
                    hit.gameObject.scene != gameObject.scene ||
                    !IsTriggerCollider(hit))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    private bool IsTriggerCollider(Collider candidate)
    {
        if (candidate == null || string.IsNullOrWhiteSpace(triggerTag))
        {
            return false;
        }

        GameObject candidateObject = candidate.gameObject;
        string objectName = candidateObject.name;
        return string.Equals(candidateObject.tag, triggerTag, StringComparison.Ordinal) ||
            string.Equals(objectName, triggerTag, StringComparison.Ordinal) ||
            objectName.StartsWith(triggerTag + " ", StringComparison.Ordinal) ||
            objectName.StartsWith(triggerTag + " (", StringComparison.Ordinal);
    }

    private GameObject FindFragmentRootInScene()
    {
        Scene scene = gameObject.scene;
        if (!scene.IsValid())
        {
            return null;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root == null)
            {
                continue;
            }

            if (IsFragmentRootName(root.name))
            {
                return root;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != null && IsFragmentRootName(child.name))
                {
                    return child.gameObject;
                }
            }
        }

        return null;
    }

    private bool IsFragmentRootName(string objectName)
    {
        return string.Equals(objectName, fragmentRootName, StringComparison.Ordinal) ||
            objectName.StartsWith(fragmentRootName + " (", StringComparison.Ordinal);
    }

    private bool IsRunningInTemporalProjectionScene()
    {
        Scene scene = gameObject.scene;
        return scene.IsValid() &&
            scene.name.StartsWith(projectionSceneNamePrefix, StringComparison.Ordinal);
    }
}
