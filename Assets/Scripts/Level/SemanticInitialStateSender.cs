using System.Collections;
using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Seeds a scene semantic state once after TimelineEventBus is ready.
    /// Useful for explicit default states such as KeyState=Missing.
    /// </summary>
    public class SemanticInitialStateSender : MonoBehaviour
    {
        [SerializeField] private EventKind eventKind = EventKind.SetSemanticState;
        [SerializeField] private EventDirection direction = EventDirection.Bidirectional;
        [SerializeField] private string stateKey = "KeyState";
        [SerializeField] private string stateValue = "Missing";
        [SerializeField] private string targetId;
        [SerializeField] private float sendDelaySeconds = 0.25f;
        [SerializeField] private bool sendOnlyFromMasterClient = true;

        private IEnumerator Start()
        {
            if (sendDelaySeconds > 0f)
                yield return new WaitForSeconds(sendDelaySeconds);

            while (TimelineEventBus.Instance == null)
                yield return null;

            if (sendOnlyFromMasterClient && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                yield break;

            SemanticStateStore store = SemanticStateStore.EnsureInstance();
            if (store.TryGetState(stateKey, targetId, out _))
                yield break;

            store.SendState(eventKind, direction, stateKey, stateValue, targetId, transform.position);
        }
    }
}
