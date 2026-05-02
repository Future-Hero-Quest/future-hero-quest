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
        [SerializeField] private bool allowInteractKey = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactRadius = 2.2f;

        private bool _sent;
        private Collider _zoneCollider;
        private Transform _localPlayer;

        private void Reset()
        {
            var zoneCollider = GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;
        }

        private void Awake()
        {
            _zoneCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (!allowInteractKey || (_sent && sendOnce)) return;
            if (!IsRoleAllowed()) return;

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null) return;

            if (Input.GetKeyDown(interactKey) && IsLocalPlayerInReach())
            {
                SendReach();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_sent && sendOnce) return;
            if (!IsRoleAllowed()) return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || !player.photonView.IsMine) return;

            _localPlayer = player.transform;
            SendReach();
        }

        private void SendReach()
        {
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

        private void FindLocalPlayer()
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude);
            foreach (PlayerController player in players)
            {
                if (player.photonView.IsMine)
                {
                    _localPlayer = player.transform;
                    return;
                }
            }
        }

        private bool IsLocalPlayerInReach()
        {
            if (_localPlayer == null) return false;

            if (_zoneCollider != null)
            {
                Vector3 closest = _zoneCollider.ClosestPoint(_localPlayer.position);
                if ((closest - _localPlayer.position).sqrMagnitude <= interactRadius * interactRadius)
                    return true;
            }

            return (transform.position - _localPlayer.position).sqrMagnitude <= interactRadius * interactRadius;
        }
    }
}
