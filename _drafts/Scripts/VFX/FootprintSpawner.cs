using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.VFX
{
    /// <summary>
    /// 过去玩家走路时每 1m 在地面发出 Footprint 事件 -> 未来世界对应坐标生成蓝色脚印 decal。
    /// 挂在过去玩家身上(只有过去玩家会调用 SendEvent，未来玩家收到后只生成 decal)。
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class FootprintSpawner : MonoBehaviourPunCallbacks
    {
        [SerializeField] private float minDistance = 1.0f;
        [SerializeField] private GameObject footprintPrefab;
        [SerializeField] private float decalLifetime = 10.0f;
        [SerializeField] private float yOffset = 0.02f;

        private Vector3 _lastPos;

        private void Start()
        {
            _lastPos = transform.position;
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived += HandleEventReceived;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived -= HandleEventReceived;
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            if (NetworkManager.Instance == null || NetworkManager.Instance.MyRole != GameRole.Past) return;

            float d = Vector3.Distance(transform.position, _lastPos);
            if (d >= minDistance)
            {
                _lastPos = transform.position;
                if (TimelineEventBus.Instance != null)
                {
                    TimelineEventBus.Instance.SendPastEvent(EventKind.Footprint, "footprint", transform.position);
                }
            }
        }

        private void HandleEventReceived(TimelineEvent evt)
        {
            if (evt.Kind != EventKind.Footprint) return;
            if (NetworkManager.Instance == null || NetworkManager.Instance.MyRole != GameRole.Future) return;
            SpawnLocalFootprint(evt.Payload);
        }

        private void SpawnLocalFootprint(Vector3 worldPos)
        {
            if (footprintPrefab == null) return;
            Vector3 pos = worldPos + Vector3.up * yOffset;
            var decal = Instantiate(footprintPrefab, pos, Quaternion.Euler(90, 0, 0));
            Destroy(decal, decalLifetime);
        }
    }
}
