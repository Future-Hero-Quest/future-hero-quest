using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 第 2 关《时空信件》过去侧：监听 SendLetter 事件,在事件 payload 位置生成光柱+信件。
    /// 玩家走近按 E 拾取信件 -> 弹出 UI 显示密码。
    ///
    /// 这个组件挂在场景里一个常驻的 GameObject 上(LevelManager 的子物体即可),
    /// 不需要预先放置在某个位置 - 它根据 RPC 事件 payload 动态生成光柱。
    /// </summary>
    public class LetterReceiver : MonoBehaviour
    {
        [SerializeField] private string letterTargetId = "letter_library_01";
        [SerializeField] private GameObject lightPillarPrefab;
        [SerializeField] private GameObject letterPickupPrefab;
        [SerializeField] private string passwordToReveal = "3-1-4";
        [SerializeField] private GameObject passwordRevealUI;
        [SerializeField] private TMPro.TMP_Text passwordText;

        private GameObject _spawnedPillar;
        private GameObject _spawnedLetter;
        private bool _letterPickedUp;
        private Transform _localPlayer;

        private void OnEnable()
        {
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived += HandleEvent;
        }

        private void OnDisable()
        {
            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.OnEventReceived -= HandleEvent;
        }

        private void HandleEvent(TimelineEvent evt)
        {
            if (evt.Kind != EventKind.SendLetter) return;
            if (evt.TargetId != letterTargetId) return;
            if (evt.Direction != EventDirection.FutureToPast) return;
            if (NetworkManager.Instance == null || NetworkManager.Instance.MyRole != GameRole.Past) return;
            if (_spawnedLetter != null) return;

            SpawnLightPillarAndLetter(evt.Payload);
        }

        private void SpawnLightPillarAndLetter(Vector3 pos)
        {
            if (lightPillarPrefab != null)
                _spawnedPillar = Instantiate(lightPillarPrefab, pos, Quaternion.identity);
            if (letterPickupPrefab != null)
                _spawnedLetter = Instantiate(letterPickupPrefab, pos, Quaternion.identity);
            Debug.Log($"[LetterReceiver] Letter spawned at {pos} (received from future)");
        }

        private void Update()
        {
            if (_letterPickedUp) return;
            if (_spawnedLetter == null) return;
            if (NetworkManager.Instance == null || NetworkManager.Instance.MyRole != GameRole.Past) return;

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null) return;

            float dist = Vector3.Distance(_spawnedLetter.transform.position, _localPlayer.position);
            if (dist <= 1.5f && Input.GetKeyDown(KeyCode.E))
            {
                PickupLetter();
            }
        }

        private void FindLocalPlayer()
        {
            var players = FindObjectsOfType<Players.PlayerController>();
            foreach (var p in players)
                if (p.photonView.IsMine) { _localPlayer = p.transform; break; }
        }

        private void PickupLetter()
        {
            _letterPickedUp = true;
            if (_spawnedLetter != null) Destroy(_spawnedLetter);
            if (_spawnedPillar != null) Destroy(_spawnedPillar, 1f);
            if (passwordRevealUI != null) passwordRevealUI.SetActive(true);
            if (passwordText != null) passwordText.text = passwordToReveal;
            Debug.Log($"[LetterReceiver] Letter picked up by past. Password revealed: {passwordToReveal}");
        }
    }
}
