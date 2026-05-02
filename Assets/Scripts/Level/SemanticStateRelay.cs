using System;
using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Relays one semantic state into another semantic state.
    /// Useful for visual-only chained effects such as breaking glass after a puzzle result.
    /// </summary>
    public class SemanticStateRelay : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private string sourceStateKey = "BallResult";
        [SerializeField] private string sourceExpectedValue = "Pocket_3";
        [SerializeField] private string sourceTargetId = "L3_Billiards";
        [SerializeField] private bool applyExistingStateOnEnable = true;

        [Header("Output")]
        [SerializeField] private EventKind outputEventKind = EventKind.SetSemanticState;
        [SerializeField] private EventDirection outputDirection = EventDirection.Bidirectional;
        [SerializeField] private string outputStateKey = "FractureState";
        [SerializeField] private string outputStateValue = "Broken";
        [SerializeField] private string outputTargetId = "L3_GlassWall";
        [SerializeField] private bool sendOnce = true;
        [SerializeField] private bool sendOnlyFromMasterClient = true;

        private SemanticStateStore _store;
        private bool _sent;

        private void OnEnable()
        {
            _store = SemanticStateStore.EnsureInstance();
            _store.OnStateChanged += HandleStateChanged;

            if (applyExistingStateOnEnable
                && _store.TryGetState(sourceStateKey, sourceTargetId, out string current)
                && IsExpectedValue(current))
            {
                TrySend();
            }
        }

        private void OnDisable()
        {
            if (_store != null)
                _store.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(string key, string value, TimelineEvent evt)
        {
            if (!string.Equals(key, sourceStateKey, StringComparison.OrdinalIgnoreCase)) return;
            if (!string.IsNullOrWhiteSpace(sourceTargetId)
                && !string.Equals(evt.TargetId, sourceTargetId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (IsExpectedValue(value)) TrySend();
        }

        private void TrySend()
        {
            if (_sent && sendOnce) return;
            if (sendOnlyFromMasterClient && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;

            _store.SendState(
                outputEventKind,
                outputDirection,
                outputStateKey,
                outputStateValue,
                outputTargetId,
                transform.position);

            _sent = true;
            Debug.Log($"[SemanticStateRelay] Relayed {sourceStateKey} to {outputStateKey}={outputStateValue}");
        }

        private bool IsExpectedValue(string value)
        {
            return string.Equals(
                value?.Trim(),
                sourceExpectedValue?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
