using FutureHeroQuest.Core;
using FutureHeroQuest.Players;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Sends a one-shot reach-zone event when the local player enters.
    /// Useful for LevelManager targetIdRequired completion checks.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SemanticReachZone : MonoBehaviour
    {
        [SerializeField] private string targetId = "level_exit";
        [SerializeField] private bool restrictToRole = true;
        [SerializeField] private GameRole requiredRole = GameRole.Future;
        [SerializeField] private bool sendOnce = true;

        private bool _sent;

        private void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_sent && sendOnce) return;
            if (!IsRoleAllowed()) return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || !player.photonView.IsMine) return;

            if (TimelineEventBus.Instance == null)
            {
                Debug.LogError($"[{nameof(SemanticReachZone)}] TimelineEventBus is not ready.");
                return;
            }

            TimelineEventBus.Instance.SendBidirectional(EventKind.ReachZone, targetId, transform.position);
            _sent = true;
            Debug.Log($"[SemanticReachZone] Reached {targetId}");
        }

        private bool IsRoleAllowed()
        {
            if (!restrictToRole) return true;
            return NetworkManager.Instance == null || NetworkManager.Instance.MyRole == requiredRole;
        }
    }
}
