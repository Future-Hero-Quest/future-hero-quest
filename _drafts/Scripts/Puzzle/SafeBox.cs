using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 第 2 关《时空信件》的保险柜（K 端）。
    /// K 走近按 E 弹出密码输入 UI，输对后发送 OpenSafe 事件 -> 通关。
    ///
    /// 简化处理：不真做密码输入，按 E 直接判定（由 LetterReceiver 已经把密码暴露给了玩家）。
    /// 也可以挂一个 KeypadInputUI 做完整密码输入。
    /// </summary>
    public class SafeBox : MonoBehaviour
    {
        [SerializeField] private string safeTargetId = "safe_library_01";
        [SerializeField] private string requiredPassword = "3-1-4";
        [SerializeField] private float interactRadius = 1.2f;
        [SerializeField] private GameObject promptUI;
        [SerializeField] private GameObject keypadInputUI;
        [SerializeField] private TMPro.TMP_InputField passwordInput;
        [SerializeField] private GameObject closedMesh;
        [SerializeField] private GameObject openMesh;
        [SerializeField] private AudioClip openSfx;

        private bool _opened;
        private bool _keypadOpen;
        private Transform _localPlayer;

        private void Update()
        {
            if (_opened) return;
            if (NetworkManager.Instance == null || NetworkManager.Instance.MyRole != GameRole.Past) return;

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _localPlayer.position);
            bool inRange = dist <= interactRadius;

            if (!_keypadOpen)
            {
                if (promptUI != null) promptUI.SetActive(inRange);
                if (inRange && Input.GetKeyDown(KeyCode.E)) ShowKeypad();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return)) TrySubmit();
                if (Input.GetKeyDown(KeyCode.Escape)) HideKeypad();
            }
        }

        private void FindLocalPlayer()
        {
            var players = FindObjectsOfType<Players.PlayerController>();
            foreach (var p in players)
                if (p.photonView.IsMine) { _localPlayer = p.transform; break; }
        }

        private void ShowKeypad()
        {
            _keypadOpen = true;
            if (keypadInputUI != null) keypadInputUI.SetActive(true);
            if (passwordInput != null) { passwordInput.text = ""; passwordInput.ActivateInputField(); }
            if (promptUI != null) promptUI.SetActive(false);
        }

        private void HideKeypad()
        {
            _keypadOpen = false;
            if (keypadInputUI != null) keypadInputUI.SetActive(false);
        }

        private void TrySubmit()
        {
            if (passwordInput == null) return;
            string entered = passwordInput.text.Trim();
            if (entered == requiredPassword)
            {
                Open();
            }
            else
            {
                Debug.Log($"[SafeBox] Wrong password: {entered}");
                passwordInput.text = "";
            }
        }

        private void Open()
        {
            _opened = true;
            HideKeypad();
            if (closedMesh != null) closedMesh.SetActive(false);
            if (openMesh != null) openMesh.SetActive(true);
            if (openSfx != null) AudioSource.PlayClipAtPoint(openSfx, transform.position);

            if (TimelineEventBus.Instance != null)
                TimelineEventBus.Instance.SendPastEvent(EventKind.OpenSafe, safeTargetId, transform.position);

            Debug.Log("[SafeBox] Opened! Level 2 should complete.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
