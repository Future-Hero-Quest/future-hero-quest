using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace FutureHeroQuest.Core
{
    /// <summary>
    /// 时间线事件总线（v2 · 全双工）。
    ///
    /// v1 设计：只 MasterClient 能发，单向(过去→未来)
    /// v2 设计：双方都能发，事件携带 Direction 字段，接收方按方向决定是否响应
    ///
    /// 历史事件被保留以支持重连补发（幂等）。
    /// 通过 Photon RaiseEvent 广播，不依赖场景 PhotonView ID，避免跨场景持久对象和关卡对象 ID 冲突。
    /// </summary>
    public class TimelineEventBus : MonoBehaviourPunCallbacks
    {
        private const byte TimelineEventCode = 42;

        public static TimelineEventBus Instance { get; private set; }

        public event Action<TimelineEvent> OnEventReceived;

        private readonly List<TimelineEvent> _eventHistory = new List<TimelineEvent>();

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.NetworkingClient.EventReceived += HandlePhotonEvent;
        }

        public override void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= HandlePhotonEvent;
            base.OnDisable();
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

        public void SendEvent(EventKind kind, EventDirection direction, string targetId, Vector3 payload)
        {
            if (!CanSend(direction, kind)) return;
            int actor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : 0;
            var evt = new TimelineEvent(kind, direction, targetId, payload, actor);
            Publish(evt);
        }

        public void SendStateEvent(
            EventKind kind,
            EventDirection direction,
            string stateKey,
            string stateValue,
            string targetId = "",
            Vector3 payload = default)
        {
            if (string.IsNullOrWhiteSpace(stateKey))
            {
                Debug.LogWarning($"[TimelineEventBus] Ignored semantic state event {kind}: stateKey is empty.");
                return;
            }

            if (!CanSend(direction, kind)) return;
            int actor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : 0;
            var evt = new TimelineEvent(kind, direction, targetId, stateKey, stateValue, payload, actor);
            Publish(evt);
        }

        public void SendPastStateEvent(EventKind kind, string stateKey, string stateValue, string targetId = "", Vector3 payload = default)
        {
            SendStateEvent(kind, EventDirection.PastToFuture, stateKey, stateValue, targetId, payload);
        }

        public void SendFutureStateEvent(EventKind kind, string stateKey, string stateValue, string targetId = "", Vector3 payload = default)
        {
            SendStateEvent(kind, EventDirection.FutureToPast, stateKey, stateValue, targetId, payload);
        }

        public void SendBidirectionalStateEvent(EventKind kind, string stateKey, string stateValue, string targetId = "", Vector3 payload = default)
        {
            SendStateEvent(kind, EventDirection.Bidirectional, stateKey, stateValue, targetId, payload);
        }

        private void Publish(TimelineEvent evt)
        {
            string json = JsonUtility.ToJson(evt);
            if (!PhotonNetwork.InRoom)
            {
                ReceiveEventJson(json);
                return;
            }

            var options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            if (!PhotonNetwork.RaiseEvent(TimelineEventCode, json, options, SendOptions.SendReliable))
            {
                Debug.LogWarning($"[TimelineEventBus] Failed to raise event {evt.Kind}.");
            }
        }

        private bool CanSend(EventDirection direction, EventKind kind)
        {
            var role = NetworkManager.Instance != null ? NetworkManager.Instance.MyRole : GameRole.Past;

            if (direction == EventDirection.PastToFuture && role != GameRole.Past)
            {
                Debug.LogWarning($"[TimelineEventBus] PastToFuture event {kind} can only be sent by Past player.");
                return false;
            }
            if (direction == EventDirection.FutureToPast && role != GameRole.Future)
            {
                Debug.LogWarning($"[TimelineEventBus] FutureToPast event {kind} can only be sent by Future player.");
                return false;
            }

            return true;
        }

        public void SendPastEvent(EventKind kind, string targetId, Vector3 payload)
        {
            SendEvent(kind, EventDirection.PastToFuture, targetId, payload);
        }

        public void SendFutureEvent(EventKind kind, string targetId, Vector3 payload)
        {
            SendEvent(kind, EventDirection.FutureToPast, targetId, payload);
        }

        public void SendBidirectional(EventKind kind, string targetId, Vector3 payload)
        {
            SendEvent(kind, EventDirection.Bidirectional, targetId, payload);
        }

        private void HandlePhotonEvent(EventData photonEvent)
        {
            if (photonEvent.Code != TimelineEventCode) return;
            if (photonEvent.CustomData is string json)
            {
                ReceiveEventJson(json);
            }
        }

        private void ReceiveEventJson(string json)
        {
            var evt = JsonUtility.FromJson<TimelineEvent>(json);
            _eventHistory.Add(evt);
            OnEventReceived?.Invoke(evt);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            foreach (var evt in _eventHistory)
            {
                string json = JsonUtility.ToJson(evt);
                var options = new RaiseEventOptions { TargetActors = new[] { newPlayer.ActorNumber } };
                PhotonNetwork.RaiseEvent(TimelineEventCode, json, options, SendOptions.SendReliable);
            }
        }

        public void ClearHistory()
        {
            _eventHistory.Clear();
            if (SemanticStateStore.Instance != null)
                SemanticStateStore.Instance.ClearStates();
        }

        public IReadOnlyList<TimelineEvent> History => _eventHistory;

        /// <summary>
        /// 判定本机角色是否应当响应这个事件方向。
        /// PastToFuture: 只有未来玩家应当响应
        /// FutureToPast: 只有过去玩家应当响应
        /// Bidirectional: 双方都响应
        /// </summary>
        public static bool ShouldRespondTo(EventDirection direction)
        {
            if (NetworkManager.Instance == null) return true;
            var role = NetworkManager.Instance.MyRole;
            switch (direction)
            {
                case EventDirection.PastToFuture: return role == GameRole.Future;
                case EventDirection.FutureToPast: return role == GameRole.Past;
                case EventDirection.Bidirectional: return true;
                default: return false;
            }
        }
    }
}
