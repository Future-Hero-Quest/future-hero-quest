using System.Collections;
using FutureHeroQuest.Core;
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

        private IEnumerator Start()
        {
            if (sendDelaySeconds > 0f)
                yield return new WaitForSeconds(sendDelaySeconds);

            while (TimelineEventBus.Instance == null)
                yield return null;

            SemanticStateStore.EnsureInstance()
                .SendState(eventKind, direction, stateKey, stateValue, targetId, transform.position);
        }
    }
}
