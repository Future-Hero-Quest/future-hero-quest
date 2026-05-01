using System;
using UnityEngine;

namespace FutureHeroQuest.Core
{
    /// <summary>
    /// 时间线事件类型枚举。所有跨时空影响都通过这个枚举分类。
    /// 新增类型时，需要同时在 PuzzleObject 子类中添加对应处理。
    /// </summary>
    public enum EventKind
    {
        // 第 1 关 (过去 -> 未来)
        PlantTree,

        // 第 2 关 (未来 -> 过去) v2 新增
        SendLetter,           // M 把信送回过去 -> K 端坐标生成光柱+信件
        OpenSafe,             // K 输入密码开保险柜 -> 通关

        // 第 3 关 (双向) v2 新增
        ToggleSwitch,         // 双方各自拨开关 (past_sw_X / future_sw_X)
        DoorUnlocked,         // pattern 对齐 -> Host 仲裁 -> 门开

        // 通用
        MoveBox,
        BreakWall,
        PickupItem,
        PlaceNote,
        Footprint,
    }

    /// <summary>
    /// 事件方向。决定事件由谁发出、谁应当响应。
    /// v2 新增：原来只有过去->未来，现在支持双向。
    /// </summary>
    public enum EventDirection
    {
        PastToFuture,
        FutureToPast,
        Bidirectional,
    }

    /// <summary>
    /// 时间线事件结构。通过 PunRPC 在双方间广播。
    /// 故意设计为 struct + JsonUtility 可序列化，避免 Photon 自定义类型注册的麻烦。
    /// </summary>
    [Serializable]
    public struct TimelineEvent
    {
        public string EventId;
        public EventKind Kind;
        public EventDirection Direction;
        public string TargetId;
        public Vector3 Payload;
        public float Timestamp;
        public int SenderActorNumber;

        public TimelineEvent(EventKind kind, EventDirection dir, string targetId, Vector3 payload, int senderActor)
        {
            EventId = Guid.NewGuid().ToString("N");
            Kind = kind;
            Direction = dir;
            TargetId = targetId;
            Payload = payload;
            Timestamp = Time.time;
            SenderActorNumber = senderActor;
        }
    }
}
