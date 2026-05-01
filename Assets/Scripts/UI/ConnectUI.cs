using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace FutureHeroQuest.UI
{
    /// <summary>
    /// 主菜单 UI：创建房间 / 加入房间 / 状态显示。
    /// 挂在主菜单 Canvas 上，把 Inspector 字段拖好即可。
    /// </summary>
    public class ConnectUI : MonoBehaviour
    {
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Text statusText;

        private void Start()
        {
            if (createButton != null) createButton.onClick.AddListener(OnCreateClicked);
            if (joinButton != null) joinButton.onClick.AddListener(OnJoinClicked);
            SetButtonsInteractable(false);
        }

        private void Update()
        {
            if (statusText != null)
            {
                statusText.text = $"State: {PhotonNetwork.NetworkClientState}";
            }

            bool ready = PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom;
            SetButtonsInteractable(ready);
        }

        private void SetButtonsInteractable(bool v)
        {
            if (createButton != null) createButton.interactable = v;
            if (joinButton != null) joinButton.interactable = v;
        }

        private void OnCreateClicked()
        {
            NetworkManager.Instance.CreateRoom();
        }

        private void OnJoinClicked()
        {
            NetworkManager.Instance.JoinRoom();
        }
    }
}
