using System;
using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Inspector-driven state receiver. Example: BridgeState=Supported activates a future bridge prefab.
    /// </summary>
    public class SemanticStateApplier : MonoBehaviour
    {
        [SerializeField] private string stateKey = "BridgeState";
        [SerializeField] private string expectedValue = "Supported";
        [SerializeField] private string targetId;
        [SerializeField] private bool applyExistingStateOnEnable = true;

        [Header("On match")]
        [SerializeField] private GameObject[] activateOnMatch;
        [SerializeField] private GameObject[] deactivateOnMatch;
        [SerializeField] private Collider[] enableCollidersOnMatch;
        [SerializeField] private Collider[] disableCollidersOnMatch;
        [SerializeField] private Behaviour[] enableBehavioursOnMatch;
        [SerializeField] private Behaviour[] disableBehavioursOnMatch;

        [Header("On mismatch")]
        [SerializeField] private bool applyMismatchState;
        [SerializeField] private GameObject[] activateOnMismatch;
        [SerializeField] private GameObject[] deactivateOnMismatch;

        private SemanticStateStore _store;

        private void OnEnable()
        {
            _store = SemanticStateStore.EnsureInstance();
            _store.OnStateChanged += HandleStateChanged;

            if (applyExistingStateOnEnable && _store.TryGetState(stateKey, targetId, out string current))
            {
                Apply(IsMatch(current));
            }
        }

        private void OnDisable()
        {
            if (_store != null)
                _store.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(string key, string value, TimelineEvent evt)
        {
            if (!string.Equals(key, stateKey, StringComparison.OrdinalIgnoreCase)) return;
            if (!string.IsNullOrWhiteSpace(targetId)
                && !string.Equals(evt.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Apply(IsMatch(value));
        }

        private bool IsMatch(string value)
        {
            return string.Equals(value?.Trim(), expectedValue?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private void Apply(bool matched)
        {
            if (matched)
            {
                SetActive(activateOnMatch, true);
                SetActive(deactivateOnMatch, false);
                SetEnabled(enableCollidersOnMatch, true);
                SetEnabled(disableCollidersOnMatch, false);
                SetEnabled(enableBehavioursOnMatch, true);
                SetEnabled(disableBehavioursOnMatch, false);
                return;
            }

            if (!applyMismatchState) return;
            SetActive(activateOnMismatch, true);
            SetActive(deactivateOnMismatch, false);
        }

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;
            foreach (GameObject obj in objects)
            {
                if (obj != null) obj.SetActive(active);
            }
        }

        private static void SetEnabled(Collider[] colliders, bool enabled)
        {
            if (colliders == null) return;
            foreach (Collider collider in colliders)
            {
                if (collider != null) collider.enabled = enabled;
            }
        }

        private static void SetEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null) return;
            foreach (Behaviour behaviour in behaviours)
            {
                if (behaviour != null) behaviour.enabled = enabled;
            }
        }
    }
}
