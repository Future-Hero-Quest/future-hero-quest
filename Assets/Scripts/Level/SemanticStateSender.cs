using System.Collections.Generic;
using FutureHeroQuest.Core;
using FutureHeroQuest.Players;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Generic proximity interaction that sends a semantic state through TimelineEventBus.
    /// Example: K presses E near support slot, sending BridgeState=Supported.
    /// </summary>
    public class SemanticStateSender : MonoBehaviour
    {
        private static readonly List<SemanticStateSender> ActiveSenders = new List<SemanticStateSender>();

        [Header("State")]
        [SerializeField] private EventKind eventKind = EventKind.SetSemanticState;
        [SerializeField] private EventDirection direction = EventDirection.Bidirectional;
        [SerializeField] private string stateKey = "BridgeState";
        [SerializeField] private string stateValue = "Supported";
        [SerializeField] private string targetId;

        [Header("Interaction")]
        [SerializeField] private bool restrictToRole = true;
        [SerializeField] private GameRole requiredRole = GameRole.Past;
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool sendOnce = true;
        [SerializeField] private GameObject promptUI;
        [SerializeField] private GameObject[] deactivateAfterSend;

        private bool _sent;
        private Transform _localPlayer;

        private void OnEnable()
        {
            if (!ActiveSenders.Contains(this))
                ActiveSenders.Add(this);
        }

        private void OnDisable()
        {
            ActiveSenders.Remove(this);
            SetPrompt(false);
        }

        private void Update()
        {
            if (_sent && sendOnce)
            {
                SetPrompt(false);
                return;
            }

            if (!IsRoleAllowed())
            {
                SetPrompt(false);
                return;
            }

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null)
            {
                SetPrompt(false);
                return;
            }

            bool isNearest = FindNearestAvailable(_localPlayer.position, interactKey) == this;
            SetPrompt(isNearest);

            if (isNearest && Input.GetKeyDown(interactKey))
            {
                Send();
            }
        }

        public void Send()
        {
            if (_sent && sendOnce) return;

            var store = SemanticStateStore.EnsureInstance();
            store.SendState(eventKind, direction, stateKey, stateValue, targetId, transform.position);
            _sent = true;

            SetPrompt(false);
            if (deactivateAfterSend != null)
            {
                foreach (GameObject obj in deactivateAfterSend)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            Debug.Log($"[SemanticStateSender] Sent {stateKey}={stateValue} ({eventKind})");
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

        private void SetPrompt(bool active)
        {
            if (promptUI != null) promptUI.SetActive(active);
        }

        private static SemanticStateSender FindNearestAvailable(Vector3 playerPosition, KeyCode key)
        {
            SemanticStateSender nearest = null;
            float nearestSqrDistance = float.PositiveInfinity;

            for (int i = ActiveSenders.Count - 1; i >= 0; i--)
            {
                SemanticStateSender sender = ActiveSenders[i];
                if (sender == null)
                {
                    ActiveSenders.RemoveAt(i);
                    continue;
                }

                if (!sender.isActiveAndEnabled) continue;
                if (sender.interactKey != key) continue;
                if (sender._sent && sender.sendOnce) continue;
                if (!sender.IsRoleAllowed()) continue;

                float sqrDistance = (sender.transform.position - playerPosition).sqrMagnitude;
                if (sqrDistance > sender.interactRadius * sender.interactRadius) continue;
                if (sqrDistance >= nearestSqrDistance) continue;

                nearest = sender;
                nearestSqrDistance = sqrDistance;
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
