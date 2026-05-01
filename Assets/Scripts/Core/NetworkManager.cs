using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace FutureHeroQuest.Core
{
    /// <summary>
    /// Photon 连接 + 房间管理 + 角色分配。
    /// 流程：Start() 自动连 Master -> UI 调用 CreateRoom/JoinRoom -> OnJoinedRoom 分配角色 -> 加载第一关。
    ///
    /// 注意：必须先在 Inspector 中配置好 Photon AppID（Resources/PhotonServerSettings 资产里）。
    /// 否则 ConnectUsingSettings() 会失败。
    /// </summary>
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance { get; private set; }

        public const string ROOM_NAME = "FutureHeroQuestRoom";
        public const string FIRST_LEVEL = "Level01_Tree";

        [SerializeField] private string gameVersion = "0.1.0";

        public GameRole MyRole { get; private set; } = GameRole.Past;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = gameVersion;
            ConnectToMaster();
        }

        public void ConnectToMaster()
        {
            if (PhotonNetwork.IsConnected) return;
            PhotonNetwork.ConnectUsingSettings();
        }

        public void CreateRoom()
        {
            var options = new RoomOptions { MaxPlayers = 2 };
            PhotonNetwork.CreateRoom(ROOM_NAME, options);
        }

        public void JoinRoom()
        {
            PhotonNetwork.JoinRoom(ROOM_NAME);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("[NetworkManager] Connected to Photon Master Server.");
        }

        public override void OnJoinedRoom()
        {
            MyRole = PhotonNetwork.IsMasterClient ? GameRole.Past : GameRole.Future;
            Debug.Log($"[NetworkManager] Joined room as {MyRole}. PlayerCount={PhotonNetwork.CurrentRoom.PlayerCount}");

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(FIRST_LEVEL);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"[NetworkManager] Remote player joined: {newPlayer.NickName} (#{newPlayer.ActorNumber})");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogWarning($"[NetworkManager] Remote player left: {otherPlayer.NickName}");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogError($"[NetworkManager] Disconnected: {cause}");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[NetworkManager] CreateRoom failed: {message}");
            JoinRoom();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[NetworkManager] JoinRoom failed: {message}. Trying to create instead.");
            CreateRoom();
        }
    }
}
