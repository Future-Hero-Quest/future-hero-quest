using System;
using System.Collections.Generic;
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
    /// 必须挂在场景中带 PhotonView 的常驻 GameObject 上（DontDestroyOnLoad）。
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class TimelineEventBus : MonoBehaviourPunCallbacks
    {
        public static TimelineEventBus Instance { get; private set; }

        public event Action<TimelineEvent> OnEventReceived;

        private readonly List<TimelineEvent> _eventHistory = new List<TimelineEvent>();

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
            var role = NetworkManager.Instance != null ? NetworkManager.Instance.MyRole : GameRole.Past;

            if (direction == EventDirection.PastToFuture && role != GameRole.Past)
            {
                Debug.LogWarning($"[TimelineEventBus] PastToFuture event {kind} can only be sent by Past player.");
                return;
            }
            if (direction == EventDirection.FutureToPast && role != GameRole.Future)
            {
                Debug.LogWarning($"[TimelineEventBus] FutureToPast event {kind} can only be sent by Future player.");
                return;
            }

            int actor = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : 0;
            var evt = new TimelineEvent(kind, direction, targetId, payload, actor);
            string json = JsonUtility.ToJson(evt);
            photonView.RPC(nameof(RPC_ReceiveEvent), RpcTarget.AllViaServer, json);
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

        [PunRPC]
        private void RPC_ReceiveEvent(string json)
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
                photonView.RPC(nameof(RPC_ReceiveEvent), newPlayer, json);
            }
        }

        public void ClearHistory()
        {
            _eventHistory.Clear();
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
