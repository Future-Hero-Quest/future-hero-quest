using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the tunnel collapse mechanic for Scene02_MountainTunnel.
/// 
/// In the outline prototype, this was implemented as simple active-toggle placeholders.
/// This upgraded version integrates with Temporal Physics Toolkit:
/// - When a node is "wired" (interacted), it triggers a projection via PastFutureTimelineController
/// - The projection simulates the collapse in a temporal physics scene
/// - The result is applied to the future view
/// 
/// For the outline prototype phase, it still supports the original active-toggle fallback
/// so the scene remains playable without the full Temporal Physics Toolkit runtime.
/// </summary>
public class TunnelCollapseController : MonoBehaviour
{
    [System.Serializable]
    public struct TunnelNode
    {
        public string nodeId;           // e.g. "N1", "N2"
        public GameObject intactVisual; // The intact tunnel section (active when not wired)
        public GameObject collapsedVisual; // The collapsed rubble (active when wired)
        public Collider intactCollider; // Blocking collider for intact state
        public Collider[] rubbleColliders; // Colliders for the rubble (player can crouch through)
        public Transform playerCrouchTarget; // Where the player should be after crouching through
        public bool isWired;
    }

    [Header("Tunnel Nodes")]
    [SerializeField] private List<TunnelNode> nodes = new List<TunnelNode>();

    [Header("Temporal Integration")]
    [SerializeField] private bool useTemporalProjection = false;
    [SerializeField] private PastFutureTimelineController timelineController;
    [SerializeField] private string projectionReasonPrefix = "L2: cap wired @";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly HashSet<string> _wiredNodes = new HashSet<string>();

    /// <summary>
    /// Wire a tunnel node by ID. Called from OutlineInteractable or TemporalOutlineInteractable.
    /// </summary>
    public void WireNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return;
        if (_wiredNodes.Contains(nodeId))
        {
            Log($"Node {nodeId} already wired, skipping.");
            return;
        }

        TunnelNode? targetNode = FindNode(nodeId);
        if (targetNode == null)
        {
            LogWarning($"Node {nodeId} not found in tunnel node list.");
            return;
        }

        TunnelNode node = targetNode.Value;
        node.isWired = true;
        _wiredNodes.Add(nodeId);

        // Apply visual/collider changes
        ApplyNodeState(node);

        // Trigger temporal projection if available
        if (useTemporalProjection && timelineController != null)
        {
            string reason = projectionReasonPrefix + nodeId;
            timelineController.NotifyPastInfluenceEnded(reason);
            Log($"Triggered temporal projection for {reason}");
        }
        else
        {
            Log($"Node {nodeId} wired (active-toggle fallback). Temporal projection disabled.");
        }

        // Update the node in the list
        int index = nodes.FindIndex(n => n.nodeId == nodeId);
        if (index >= 0)
        {
            nodes[index] = node;
        }
    }

    /// <summary>
    /// Check if a specific node is wired.
    /// </summary>
    public bool IsNodeWired(string nodeId)
    {
        return _wiredNodes.Contains(nodeId);
    }

    /// <summary>
    /// Get the count of wired nodes.
    /// </summary>
    public int WiredNodeCount => _wiredNodes.Count;

    /// <summary>
    /// Get all wired node IDs.
    /// </summary>
    public HashSet<string> WiredNodes => _wiredNodes;

    /// <summary>
    /// Reset all nodes to unwired state (for retry/restart).
    /// </summary>
    public void ResetAllNodes()
    {
        foreach (TunnelNode node in nodes)
        {
            TunnelNode n = node;
            n.isWired = false;
            ApplyNodeState(n);

            int index = nodes.FindIndex(x => x.nodeId == n.nodeId);
            if (index >= 0) nodes[index] = n;
        }

        _wiredNodes.Clear();
        Log("All tunnel nodes reset.");
    }

    private void ApplyNodeState(TunnelNode node)
    {
        // Intact visual
        if (node.intactVisual != null)
            node.intactVisual.SetActive(!node.isWired);

        // Collapsed visual
        if (node.collapsedVisual != null)
            node.collapsedVisual.SetActive(node.isWired);

        // Intact collider
        if (node.intactCollider != null)
            node.intactCollider.enabled = !node.isWired;

        // Rubble colliders (enable when wired, so player can interact with them)
        if (node.rubbleColliders != null)
        {
            foreach (Collider col in node.rubbleColliders)
            {
                if (col != null) col.enabled = node.isWired;
            }
        }

        Log($"Node {node.nodeId} state: {(node.isWired ? "WIRED (collapsed)" : "INTACT")}");
    }

    private TunnelNode? FindNode(string nodeId)
    {
        int index = nodes.FindIndex(n => n.nodeId == nodeId);
        return index >= 0 ? nodes[index] : (TunnelNode?)null;
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[TunnelCollapseController] {message}", this);
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[TunnelCollapseController] {message}", this);
    }

    private void OnDrawGizmosSelected()
    {
        foreach (TunnelNode node in nodes)
        {
            if (node.intactVisual != null)
            {
                Gizmos.color = node.isWired ? Color.red : Color.green;
                Gizmos.DrawWireSphere(node.intactVisual.transform.position, 0.5f);

                // Draw label
                Vector3 labelPos = node.intactVisual.transform.position + Vector3.up * 2f;
                Gizmos.color = Color.white;
#if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPos, node.nodeId);
#endif
            }
        }
    }
}
