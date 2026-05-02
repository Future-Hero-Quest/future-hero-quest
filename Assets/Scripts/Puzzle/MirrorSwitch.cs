using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 第 3 关《镜像》的开关。每个开关属于一个时空 (Past/Future)，只有对应玩家能拨。
    /// 拨动时发送 Bidirectional 事件，双方都收到 -> Host 仲裁 pattern 是否对齐。
    ///
    /// 在场景中放 8 个 (4 个 Past 时空 + 4 个 Future 时空)。
    /// targetId 命名建议: past_sw_1 / past_sw_2 / ... / future_sw_4
    /// </summary>
    public class MirrorSwitch : MonoBehaviour
    {
        [Header("身份")]
        [SerializeField] private string switchTargetId = "past_sw_1";
        [SerializeField] private GameRole belongsTo = GameRole.Past;
        [SerializeField] private int switchIndex = 0;

        [Header("交互")]
        [SerializeField] private float interactRadius = 1.0f;
        [SerializeField] private GameObject promptUI;

        [Header("视觉")]
        [SerializeField] private GameObject onMesh;
        [SerializeField] private GameObject offMesh;
        [SerializeField] private AudioClip toggleSfx;

        public bool IsOn { get; private set; }
        public GameRole BelongsTo => belongsTo;
        public int Index => switchIndex;
        public string TargetId => switchTargetId;

        private Transform _localPlayer;

        private void OnEnable()
        {
            ApplyVisual();
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived += HandleEvent;
            MirrorRoomController.Register(this);
        }

        private void OnDisable()
        {
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived -= HandleEvent;
            MirrorRoomController.Unregister(this);
        }

        private void Update()
        {
            if (NetworkManager.Instance == null) return;
            if (NetworkManager.Instance.MyRole != belongsTo) return;

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _localPlayer.position);
            bool inRange = dist <= interactRadius;
            if (promptUI != null) promptUI.SetActive(inRange);

            if (inRange && Input.GetKeyDown(KeyCode.E)) Toggle();
        }

        private void FindLocalPlayer()
        {
            var players = FindObjectsOfType<Players.PlayerController>();
            foreach (var p in players)
                if (p.photonView.IsMine) { _localPlayer = p.transform; break; }
        }

        private void Toggle()
        {
            bool newState = !IsOn;
            if (TimelineEventBus.Instance != null)
            {
                Vector3 payload = new Vector3(switchIndex, newState ? 1f : 0f, (int)belongsTo);
                TimelineEventBus.Instance.SendBidirectional(EventKind.ToggleSwitch, switchTargetId, payload);
            }
            if (toggleSfx != null) AudioSource.PlayClipAtPoint(toggleSfx, transform.position);
        }

        private void HandleEvent(TimelineEvent evt)
        {
            if (evt.Kind != EventKind.ToggleSwitch) return;
            if (evt.TargetId != switchTargetId) return;
            IsOn = evt.Payload.y > 0.5f;
            ApplyVisual();
            MirrorRoomController.Instance?.OnSwitchChanged(this);
        }

        private void ApplyVisual()
        {
            if (onMesh != null) onMesh.SetActive(IsOn);
            if (offMesh != null) offMesh.SetActive(!IsOn);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = belongsTo == GameRole.Past ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
