using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace FutureHeroQuest.Core
{
    /// <summary>
    /// 时间线事件总线（核心）。所有"过去 -> 未来"的影响都走这里。
    /// 仅 MasterClient 可发出事件，事件通过 PunRPC 广播到所有客户端。
    /// 历史事件被保留以支持重连补发（幂等）。
    ///
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

        public void SendEvent(EventKind kind, string targetId, Vector3 payload)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("[TimelineEventBus] Only MasterClient (Past P1) can send timeline events.");
                return;
            }

            var evt = new TimelineEvent(kind, targetId, payload);
            string json = JsonUtility.ToJson(evt);
            photonView.RPC(nameof(RPC_ReceiveEvent), RpcTarget.AllViaServer, json);
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
    }
}
