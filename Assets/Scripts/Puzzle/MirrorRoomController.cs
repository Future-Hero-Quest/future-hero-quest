using System.Collections.Generic;
using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 第 3 关《镜像》房间控制器。仲裁 8 个开关的 pattern 是否对齐 -> 开门 -> 通关。
    /// 仅 MasterClient 仲裁过关 (避免双方判定不一致)。
    ///
    /// pattern 配置: targetPastPattern[i] / targetFuturePattern[i]
    /// 例如: past = [true, false, true, false], future = [false, true, false, true]
    ///   表示过去开关 1010, 未来开关 0101 时开门
    /// </summary>
    public class MirrorRoomController : MonoBehaviour
    {
        public static MirrorRoomController Instance { get; private set; }

        [Header("过关条件 - 目标 pattern (4 位)")]
        [SerializeField] private bool[] targetPastPattern = new bool[] { true, false, true, false };
        [SerializeField] private bool[] targetFuturePattern = new bool[] { false, true, false, true };

        [Header("视觉")]
        [SerializeField] private GameObject doorClosed;
        [SerializeField] private GameObject doorOpened;
        [SerializeField] private AudioClip doorOpenSfx;

        private static readonly List<MirrorSwitch> _registered = new List<MirrorSwitch>();
        private bool _doorOpened;

        public static void Register(MirrorSwitch sw)
        {
            if (!_registered.Contains(sw)) _registered.Add(sw);
        }
        public static void Unregister(MirrorSwitch sw) => _registered.Remove(sw);

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void OnSwitchChanged(MirrorSwitch changed)
        {
            if (_doorOpened) return;
            if (!PhotonNetwork.IsMasterClient) return;

            if (CheckPatternMatched()) OpenDoor();
        }

        private bool CheckPatternMatched()
        {
            int matched = 0, totalChecked = 0;
            foreach (var sw in _registered)
            {
                bool[] target = sw.BelongsTo == GameRole.Past ? targetPastPattern : targetFuturePattern;
                if (sw.Index < 0 || sw.Index >= target.Length) continue;
                totalChecked++;
                if (sw.IsOn == target[sw.Index]) matched++;
            }
            return totalChecked >= 8 && matched == totalChecked;
        }

        private void OpenDoor()
        {
            _doorOpened = true;
            if (doorClosed != null) doorClosed.SetActive(false);
            if (doorOpened != null) doorOpened.SetActive(true);
            if (doorOpenSfx != null && doorOpened != null)
                AudioSource.PlayClipAtPoint(doorOpenSfx, doorOpened.transform.position);

            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.SendBidirectional(EventKind.DoorUnlocked, "mirror_room_door", Vector3.zero);

            if (Level.LevelManager.Instance != null)
                Level.LevelManager.Instance.MarkLevelComplete();

            Debug.Log("[MirrorRoomController] Pattern matched! Door opened, Level 3 complete.");
        }
    }
}
