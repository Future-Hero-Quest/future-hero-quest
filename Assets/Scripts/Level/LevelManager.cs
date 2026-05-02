using System.Collections;
using System.Collections.Generic;
using FutureHeroQuest.Core;
using FutureHeroQuest.Players;
using FutureHeroQuest.Puzzle;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// 关卡入口管理器。每个关卡场景挂一个，负责：
    /// 1. 应用 LevelData(主题/UI/种子)
    /// 2. 监听过关条件
    /// 3. 仅 MasterClient 仲裁过关 -> RPC 通知所有人
    /// 4. 处理重置(R 键)
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class LevelManager : MonoBehaviourPunCallbacks
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private LevelData levelData;
        [SerializeField] private string nextLevelScene;

        private bool _completed;
        private bool _completionRequested;
        private bool _resetRequested;
        private readonly HashSet<string> _changedTargetIds = new HashSet<string>();

        public LevelData CurrentLevelData => levelData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (levelData != null)
            {
                levelData.SanitizeSerializedState();
                if (!levelData.TryValidateForRuntime(out string validationMessage))
                {
                    Debug.LogWarning($"[LevelManager] LevelData validation warning: {validationMessage}", levelData);
                }

                Random.InitState(levelData.randomSeed);
                Debug.Log($"[LevelManager] Loaded {levelData.displayName} with seed {levelData.randomSeed}");
            }
            else
            {
                Debug.LogWarning("[LevelManager] No LevelData assigned; completion conditions are disabled.", this);
            }

            if (TimelineEventBus.Instance != null)
            {
                TimelineEventBus.Instance.OnEventReceived += HandleEventForCompletion;
            }
        }

        private void OnDestroy()
        {
            if (TimelineEventBus.Instance != null)
            {
                TimelineEventBus.Instance.OnEventReceived -= HandleEventForCompletion;
            }
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("[LevelManager] Reset ignored on non-master client. Use the host/Editor side to reset the level.");
                    return;
                }

                RequestResetLevel();
            }
        }

        private void HandleEventForCompletion(TimelineEvent evt)
        {
            if (_completed || _completionRequested) return;
            if (!string.IsNullOrEmpty(evt.TargetId)) _changedTargetIds.Add(evt.TargetId);

            if (!PhotonNetwork.IsMasterClient) return;
            if (levelData == null) return;

            bool conditionMet = false;
            switch (levelData.completeCondition)
            {
                case LevelData.LevelCompleteCondition.AllPuzzlesChanged:
                    conditionMet = AreRequiredPuzzleTargetsChanged();
                    break;
                case LevelData.LevelCompleteCondition.FuturePlayerReachZone:
                    conditionMet = !string.IsNullOrEmpty(levelData.targetIdRequired)
                        && _changedTargetIds.Contains(levelData.targetIdRequired);
                    break;
                case LevelData.LevelCompleteCondition.CustomScript:
                    break;
            }

            if (conditionMet) MarkLevelComplete();
        }

        public void MarkLevelComplete()
        {
            if (_completed || _completionRequested) return;

            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_RequestLevelComplete), RpcTarget.MasterClient);
                return;
            }

            CompleteFromMaster();
        }

        [PunRPC]
        private void RPC_RequestLevelComplete(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Debug.Log($"[LevelManager] Level complete requested by actor #{info.Sender?.ActorNumber}");
            CompleteFromMaster();
        }

        private void CompleteFromMaster()
        {
            if (_completed || _completionRequested) return;
            _completionRequested = true;

            if (!PhotonNetwork.InRoom)
            {
                RPC_LevelComplete();
                return;
            }

            photonView.RPC(nameof(RPC_LevelComplete), RpcTarget.AllViaServer);
        }

        [PunRPC]
        private void RPC_LevelComplete()
        {
            if (_completed) return;
            _completed = true;
            _completionRequested = true;
            Debug.Log($"[LevelManager] Level completed: {levelData?.displayName}");

            if ((!PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient) && !string.IsNullOrEmpty(nextLevelScene))
            {
                Invoke(nameof(LoadNext), 3.0f);
            }
        }

        private void LoadNext()
        {
            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) return;
            if (string.IsNullOrEmpty(nextLevelScene)) return;

            if (PhotonNetwork.InRoom)
                PhotonNetwork.LoadLevel(nextLevelScene);
            else
                SceneManager.LoadScene(nextLevelScene);
        }

        public void RequestResetLevel()
        {
            if (_resetRequested) return;

            if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_RequestResetLevel), RpcTarget.MasterClient);
                return;
            }

            ResetFromMaster();
        }

        [PunRPC]
        private void RPC_RequestResetLevel(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Debug.LogWarning($"[LevelManager] Ignored remote reset request by actor #{info.Sender?.ActorNumber}; reset is host-only.");
        }

        private void ResetFromMaster()
        {
            if (_resetRequested) return;
            _resetRequested = true;
            string activeScene = SceneManager.GetActiveScene().name;

            if (!PhotonNetwork.InRoom)
            {
                StartCoroutine(ResetCurrentSceneLocally(activeScene));
                return;
            }

            photonView.RPC(nameof(RPC_ResetCurrentLevel), RpcTarget.AllViaServer, activeScene);
        }

        [PunRPC]
        private void RPC_ResetCurrentLevel(string sceneName, PhotonMessageInfo info)
        {
            int masterActor = PhotonNetwork.MasterClient != null ? PhotonNetwork.MasterClient.ActorNumber : -1;
            int senderActor = info.Sender != null ? info.Sender.ActorNumber : -1;
            if (PhotonNetwork.InRoom && senderActor != masterActor)
            {
                Debug.LogWarning($"[LevelManager] Ignored reset broadcast from non-master actor #{senderActor}.");
                return;
            }

            _resetRequested = true;
            StartCoroutine(ResetCurrentSceneLocally(sceneName));
        }

        private void PrepareResetLevelLocal()
        {
            if (TimelineEventBus.Instance != null) TimelineEventBus.Instance.ClearHistory();
            PlayerSpawner.DestroyOwnedPlayerObjectsForSceneReload();
            _completed = false;
            _completionRequested = false;
            _changedTargetIds.Clear();
            CancelInvoke(nameof(LoadNext));
        }

        private IEnumerator ResetCurrentSceneLocally(string sceneName)
        {
            PrepareResetLevelLocal();
            yield return new WaitForSeconds(0.25f);
            SceneManager.LoadScene(string.IsNullOrEmpty(sceneName) ? SceneManager.GetActiveScene().name : sceneName);
        }

        private bool AreRequiredPuzzleTargetsChanged()
        {
            if (!string.IsNullOrEmpty(levelData.targetIdRequired))
                return _changedTargetIds.Contains(levelData.targetIdRequired);

            var requiredTargetIds = new HashSet<string>();
            var puzzles = FindObjectsByType<PuzzleObject>(FindObjectsInactive.Exclude);
            foreach (var puzzle in puzzles)
            {
                if (!string.IsNullOrEmpty(puzzle.TargetId))
                    requiredTargetIds.Add(puzzle.TargetId);
            }

            if (requiredTargetIds.Count == 0) return false;

            foreach (string targetId in requiredTargetIds)
            {
                if (!_changedTargetIds.Contains(targetId)) return false;
            }

            return true;
        }
    }
}
