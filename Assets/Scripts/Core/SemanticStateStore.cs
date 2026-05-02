using System;
using System.Collections.Generic;
using UnityEngine;

namespace FutureHeroQuest.Core
{
    /// <summary>
    /// Stores latest semantic timeline states such as BridgeState=Supported.
    /// Scene objects can subscribe to this instead of parsing raw TimelineEvent values.
    /// </summary>
    public class SemanticStateStore : MonoBehaviour
    {
        public static SemanticStateStore Instance { get; private set; }

        public event Action<string, string, TimelineEvent> OnStateChanged;

        private readonly Dictionary<string, string> _states = new Dictionary<string, string>();
        private bool _subscribed;

        public static SemanticStateStore EnsureInstance()
        {
            if (Instance != null) return Instance;

            var go = new GameObject(nameof(SemanticStateStore));
            Instance = go.AddComponent<SemanticStateStore>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Update()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived -= HandleTimelineEvent;
            _subscribed = false;
        }

        public bool TryGetState(string stateKey, out string stateValue)
        {
            return TryGetState(stateKey, string.Empty, out stateValue);
        }

        public bool TryGetState(string stateKey, string targetId, out string stateValue)
        {
            stateKey = NormalizeKey(stateKey);
            targetId = NormalizeKey(targetId);
            return _states.TryGetValue(ComposeStoreKey(stateKey, targetId), out stateValue);
        }

        public bool HasState(string stateKey, string expectedValue)
        {
            return HasState(stateKey, string.Empty, expectedValue);
        }

        public bool HasState(string stateKey, string targetId, string expectedValue)
        {
            return TryGetState(stateKey, targetId, out string current)
                && string.Equals(current, NormalizeValue(expectedValue), StringComparison.OrdinalIgnoreCase);
        }

        public void ClearStates()
        {
            _states.Clear();
            Debug.Log($"[{nameof(SemanticStateStore)}] Cleared semantic states.");
        }

        public void SetLocalState(string stateKey, string stateValue)
        {
            var evt = new TimelineEvent(
                EventKind.SetSemanticState,
                EventDirection.Bidirectional,
                string.Empty,
                stateKey,
                stateValue,
                Vector3.zero,
                0);
            ApplyState(evt);
        }

        public void SendState(
            EventKind kind,
            EventDirection direction,
            string stateKey,
            string stateValue,
            string targetId = "",
            Vector3 payload = default)
        {
            if (TimelineEventBus.Instance == null)
            {
                Debug.LogError($"[{nameof(SemanticStateStore)}] TimelineEventBus is not ready.");
                return;
            }

            TimelineEventBus.Instance.SendStateEvent(kind, direction, stateKey, stateValue, targetId, payload);
        }

        private void TrySubscribe()
        {
            if (_subscribed || TimelineEventBus.Instance == null) return;

            TimelineEventBus.Instance.OnEventReceived += HandleTimelineEvent;
            _subscribed = true;

            foreach (TimelineEvent evt in TimelineEventBus.Instance.History)
            {
                HandleTimelineEvent(evt);
            }
        }

        private void HandleTimelineEvent(TimelineEvent evt)
        {
            if (!evt.HasSemanticState) return;
            ApplyState(evt);
        }

        private void ApplyState(TimelineEvent evt)
        {
            string key = NormalizeKey(evt.StateKey);
            if (string.IsNullOrEmpty(key)) return;

            string targetId = NormalizeKey(evt.TargetId);
            string storeKey = ComposeStoreKey(key, targetId);
            string value = NormalizeValue(evt.StateValue);
            if (_states.TryGetValue(storeKey, out string current)
                && string.Equals(current, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _states[storeKey] = value;
            string targetSuffix = string.IsNullOrEmpty(targetId) ? string.Empty : $" target={targetId}";
            Debug.Log($"[SemanticStateStore] {key}={value}{targetSuffix}");
            OnStateChanged?.Invoke(key, value, evt);
        }

        private static string NormalizeKey(string key)
        {
            return key?.Trim() ?? string.Empty;
        }

        private static string NormalizeValue(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static string ComposeStoreKey(string stateKey, string targetId)
        {
            stateKey = NormalizeKey(stateKey);
            targetId = NormalizeKey(targetId);
            return string.IsNullOrEmpty(targetId) ? stateKey : $"{targetId}::{stateKey}";
        }
    }
}
