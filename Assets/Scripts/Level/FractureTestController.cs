using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Small smoke-test controller for pre-fractured objects.
    /// B breaks locally, R resets locally, N sends the semantic break event when TimelineEventBus exists.
    /// </summary>
    public class FractureTestController : MonoBehaviour
    {
        [SerializeField] private PrefracturedTemporalObject target;
        [SerializeField] private EventKind eventKind = EventKind.SetSemanticState;
        [SerializeField] private EventDirection direction = EventDirection.Bidirectional;
        [SerializeField] private string stateKey = "FractureState";
        [SerializeField] private string brokenValue = "Broken";
        [SerializeField] private string targetId = "TestWall";
        [SerializeField] private KeyCode localBreakKey = KeyCode.B;
        [SerializeField] private KeyCode semanticBreakKey = KeyCode.N;
        [SerializeField] private KeyCode resetKey = KeyCode.R;
        [SerializeField] private bool showDebugGui = true;

        private string _lastAction = "Ready";

        private void Update()
        {
            if (Input.GetKeyDown(localBreakKey))
            {
                BreakLocal();
            }

            if (Input.GetKeyDown(semanticBreakKey))
            {
                SendSemanticBreak();
            }

            if (Input.GetKeyDown(resetKey))
            {
                ResetLocal();
            }
        }

        public void BreakLocal()
        {
            if (target == null)
            {
                Debug.LogWarning("[FractureTestController] No target assigned.");
                return;
            }

            target.BreakLocal();
            _lastAction = "Local break sent";
            Debug.Log("[FractureTestController] Local break.");
        }

        public void ResetLocal()
        {
            if (target == null)
            {
                Debug.LogWarning("[FractureTestController] No target assigned.");
                return;
            }

            target.ResetToIntact();
            _lastAction = "Local reset";
            Debug.Log("[FractureTestController] Local reset.");
        }

        public void SendSemanticBreak()
        {
            if (TimelineEventBus.Instance == null)
            {
                _lastAction = "No TimelineEventBus; fell back to local break";
                Debug.LogWarning("[FractureTestController] TimelineEventBus is not available. Breaking locally.");
                BreakLocal();
                return;
            }

            SemanticStateStore.EnsureInstance().SendState(
                eventKind,
                direction,
                stateKey,
                brokenValue,
                targetId,
                target != null ? target.transform.position : transform.position);

            _lastAction = "Semantic break sent";
            Debug.Log($"[FractureTestController] Sent {stateKey}={brokenValue} target={targetId}.");
        }

        private void OnGUI()
        {
            if (!showDebugGui) return;

            bool broken = target != null && target.IsBroken;
            string bus = TimelineEventBus.Instance != null ? "yes" : "no";
            GUI.Box(new Rect(12, 72, 360, 112), "Fracture Test");
            GUI.Label(new Rect(24, 100, 330, 22), $"B local break | N semantic break | R reset");
            GUI.Label(new Rect(24, 124, 330, 22), $"Target broken: {broken}");
            GUI.Label(new Rect(24, 148, 330, 22), $"TimelineEventBus: {bus}");
            GUI.Label(new Rect(24, 172, 330, 22), _lastAction);
        }
    }
}
