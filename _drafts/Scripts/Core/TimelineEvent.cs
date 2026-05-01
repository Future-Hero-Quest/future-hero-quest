using System;
using UnityEngine;

namespace FutureHeroQuest.Core
{
    /// <summary>
    /// 时间线事件类型枚举。所有"过去 -> 未来"的影响都通过这个枚举分类。
    /// 新增类型时，需要同时在未来世界的 PuzzleObject 子类中添加对应处理。
    /// </summary>
    public enum EventKind
    {
        PlantTree,
        ToggleSwitch,
        MoveBox,
        BreakWall,
        PickupItem,
        PlaceNote,
        Footprint,
    }

    /// <summary>
    /// 时间线事件结构。过去玩家(P1/Master)产生事件 -> 通过 PunRPC 广播 -> 未来玩家(P2/Client)消费。
    /// 故意设计为 struct + JsonUtility 可序列化，避免 Photon 自定义类型注册的麻烦。
    /// </summary>
    [Serializable]
    public struct TimelineEvent
    {
        public string EventId;
        public EventKind Kind;
        public string TargetId;
        public Vector3 Payload;
        public float Timestamp;

        public TimelineEvent(EventKind kind, string targetId, Vector3 payload)
        {
            EventId = Guid.NewGuid().ToString("N");
            Kind = kind;
            TargetId = targetId;
            Payload = payload;
            Timestamp = Time.time;
        }
    }
}
