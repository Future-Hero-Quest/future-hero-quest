using FutureHeroQuest.SceneFlow;
using UnityEngine;

/// <summary>
/// Temporal-aware version of OutlineInteractable.
/// When the player interacts, instead of just toggling GameObjects/Colliders,
/// it triggers a Temporal Physics Projection via PastFutureTimelineController.
/// The projection result is then applied to the Future Sub Scene.
/// 
/// Falls back to the original OutlineInteractable behaviour if no
/// PastFutureTimelineController is found in the scene.
/// </summary>
[RequireComponent(typeof(OutlineInteractable))]
public class TemporalOutlineInteractable : MonoBehaviour
{
    [Header("Temporal Projection")]
    [SerializeField] private bool triggerProjectionOnUse = true;
    [SerializeField] private string projectionReason = "Temporal interaction";
    [SerializeField] private bool autoFindTimelineController = true;
    [SerializeField] private PastFutureTimelineController timelineController;

    [Header("Fallback")]
    [SerializeField] private bool fallbackToOriginalBehaviour = true;

    private OutlineInteractable _outlineInteractable;
    private bool _initialized;

    private void Awake()
    {
        _outlineInteractable = GetComponent<OutlineInteractable>();
        if (autoFindTimelineController && timelineController == null)
        {
            timelineController = FindAnyObjectByType<PastFutureTimelineController>();
        }
    }

    private void OnEnable()
    {
        if (_outlineInteractable != null)
        {
            // We don't replace Use(), we hook into it via a secondary approach:
            // The OutlineInteractable already handles its own Update loop.
            // We'll use a separate Update to detect when it's been used.
        }
    }

    private void Update()
    {
        if (_outlineInteractable == null) return;

        // Detect if OutlineInteractable was just used by checking a frame-delayed flag.
        // Since we can't easily hook into it without modifying the original,
        // we use a simple approach: check if the interactable's key was pressed
        // while the player is in range.
        DetectAndTriggerProjection();
    }

    private void DetectAndTriggerProjection()
    {
        if (!triggerProjectionOnUse) return;
        if (timelineController == null && fallbackToOriginalBehaviour) return;

        // We use reflection to check the private _used field of OutlineInteractable
        // as a lightweight hook. Alternatively, the scene designer can call
        // RequestProjection() directly from a UnityEvent.
        bool used = GetOutlineInteractableUsed();
        if (used && !_initialized)
        {
            _initialized = true;
            RequestProjection();
        }
    }

    /// <summary>
    /// Public method that can be called from a UnityEvent or other scripts.
    /// </summary>
    public void RequestProjection()
    {
        if (timelineController == null)
        {
            if (fallbackToOriginalBehaviour)
            {
                Debug.Log("[TemporalOutlineInteractable] No PastFutureTimelineController, using original behaviour.", this);
            }
            return;
        }

        timelineController.NotifyPastInfluenceEnded(projectionReason);
        Debug.Log($"[TemporalOutlineInteractable] Triggered projection: {projectionReason}", this);
    }

    private bool GetOutlineInteractableUsed()
    {
        if (_outlineInteractable == null) return false;

        var field = typeof(OutlineInteractable).GetField("_used",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);

        if (field != null && field.GetValue(_outlineInteractable) is bool used)
        {
            return used;
        }

        return false;
    }

    /// <summary>
    /// Reset the interaction state (for retry scenarios like dropping the ladder).
    /// </summary>
    public void ResetInteraction()
    {
        _initialized = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (timelineController != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, timelineController.transform.position);
        }
    }
}
