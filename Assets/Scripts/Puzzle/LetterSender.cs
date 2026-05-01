using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 第 2 关《时空信件》未来侧：M 在废墟图书馆拾取信件后,按 E "送回"过去。
    /// 触发后会发送 FutureToPast 事件,K 端的 LetterReceiver 收到后生成光柱+信件。
    ///
    /// 挂在未来世界的"原始信件"GameObject 上(例如废墟书架上的发黄信件)。
    /// </summary>
    public class LetterSender : MonoBehaviour
    {
        [SerializeField] private string letterTargetId = "letter_library_01";
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private GameObject promptPickupUI;
        [SerializeField] private GameObject promptSendUI;
        [SerializeField] private GameObject letterMesh;
        [SerializeField] private Vector3 spawnOffsetForPast = Vector3.zero;
        [SerializeField] private AudioClip pickupSfx;
        [SerializeField] private AudioClip sendSfx;

        private enum State { Initial, PickedUp, Sent }
        private State _state = State.Initial;

        private Transform _localPlayer;

        private void Update()
        {
            if (_state == State.Sent) return;
            if (NetworkManager.Instance == null || NetworkManager.Instance.MyRole != GameRole.Future) return;

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _localPlayer.position);
            bool inRange = dist <= interactRadius;

            switch (_state)
            {
                case State.Initial:
                    if (promptPickupUI != null) promptPickupUI.SetActive(inRange);
                    if (promptSendUI != null) promptSendUI.SetActive(false);
                    if (inRange && Input.GetKeyDown(KeyCode.E)) PickupLetter();
                    break;
                case State.PickedUp:
                    if (promptPickupUI != null) promptPickupUI.SetActive(false);
                    if (promptSendUI != null) promptSendUI.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.E)) SendLetterToPast();
                    break;
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
            _state = State.PickedUp;
            if (pickupSfx != null) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
            if (letterMesh != null) letterMesh.SetActive(false);
            Debug.Log("[LetterSender] M picked up letter, ready to send back.");
        }

        private void SendLetterToPast()
        {
            _state = State.Sent;
            if (sendSfx != null) AudioSource.PlayClipAtPoint(sendSfx, transform.position);
            if (promptSendUI != null) promptSendUI.SetActive(false);

            Vector3 spawnPos = transform.position + spawnOffsetForPast;
            if (TimelineEventBus.Instance == null)
            {
                Debug.LogError("[LetterSender] TimelineEventBus not ready.");
                return;
            }
            TimelineEventBus.Instance.SendFutureEvent(EventKind.SendLetter, letterTargetId, spawnPos);
            Debug.Log($"[LetterSender] Letter {letterTargetId} sent back to past at {spawnPos}");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
            if (spawnOffsetForPast != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + spawnOffsetForPast, 0.3f);
            }
        }
    }
}
