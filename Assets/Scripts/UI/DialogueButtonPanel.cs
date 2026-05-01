using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace FutureHeroQuest.UI
{
    /// <summary>
    /// 底部 8 个台词按钮面板。点击发送给对方显示气泡。
    /// 数字键 1-8 也能触发对应按钮。
    /// 当前角色(Past/Future)对应不同的台词列表(由 LevelData 配置)。
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class DialogueButtonPanel : MonoBehaviourPun
    {
        [SerializeField] private Button[] buttons = new Button[8];
        [SerializeField] private Text[] buttonLabels = new Text[8];
        [SerializeField] private DialogueBubble myBubbleForReceiveTest;

        private string[] _myDialogue;
        private static DialogueBubble _remoteBubble;

        public static void RegisterBubble(DialogueBubble bubble)
        {
            _remoteBubble = bubble;
        }

        private void Start()
        {
            ApplyRoleDialogue();
            for (int i = 0; i < buttons.Length; i++)
            {
                int idx = i;
                if (buttons[i] != null) buttons[i].onClick.AddListener(() => SendDialogue(idx));
            }
        }

        private void Update()
        {
            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SendDialogue(i);
            }
        }

        private void ApplyRoleDialogue()
        {
            var role = NetworkManager.Instance != null ? NetworkManager.Instance.MyRole : GameRole.Past;
            var data = Level.LevelManager.Instance != null ? Level.LevelManager.Instance.CurrentLevelData : null;
            if (data == null)
            {
                Debug.LogWarning("[DialogueButtonPanel] No LevelData found, using empty dialogue.");
                return;
            }
            _myDialogue = role == GameRole.Past ? data.pastDialogue : data.futureDialogue;
            for (int i = 0; i < buttonLabels.Length; i++)
            {
                if (buttonLabels[i] == null) continue;
                buttonLabels[i].text = (_myDialogue != null && i < _myDialogue.Length) ? _myDialogue[i] : "";
            }
        }

        private void SendDialogue(int idx)
        {
            if (_myDialogue == null || idx < 0 || idx >= _myDialogue.Length) return;
            string msg = _myDialogue[idx];
            photonView.RPC(nameof(RPC_ReceiveDialogue), RpcTarget.Others, msg);
        }

        [PunRPC]
        private void RPC_ReceiveDialogue(string msg)
        {
            if (_remoteBubble != null) _remoteBubble.Show(msg);
            else Debug.Log($"[Dialogue] {msg}");
        }
    }
}
