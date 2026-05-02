using System.Collections.Generic;
using FutureHeroQuest.Core;
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
        private readonly HashSet<string> _changedTargetIds = new HashSet<string>();

        public LevelData CurrentLevelData => levelData;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (levelData != null)
            {
                Random.InitState(levelData.randomSeed);
                Debug.Log($"[LevelManager] Loaded {levelData.displayName} with seed {levelData.randomSeed}");
            }

            if (TimelineEventBus.Instance != null)
            {
                TimelineEventBus.Instance.OnEventReceived += HandleEventForCompletion;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (TimelineEventBus.Instance != null)
            {
                TimelineEventBus.Instance.OnEventReceived -= HandleEventForCompletion;
            }
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_ResetLevel), RpcTarget.AllViaServer);
            }
        }

        private void HandleEventForCompletion(TimelineEvent evt)
        {
            if (_completed) return;
            _changedTargetIds.Add(evt.TargetId);

            if (!PhotonNetwork.IsMasterClient) return;
            if (levelData == null) return;

            bool conditionMet = false;
            switch (levelData.completeCondition)
            {
                case LevelData.LevelCompleteCondition.AllPuzzlesChanged:
                    var puzzles = FindObjectsOfType<PuzzleObject>();
                    int total = puzzles.Length;
                    int changed = 0;
                    foreach (var p in puzzles) if (p.HasChanged) changed++;
                    conditionMet = total > 0 && changed >= total;
                    break;
                case LevelData.LevelCompleteCondition.FuturePlayerReachZone:
                    break;
                case LevelData.LevelCompleteCondition.CustomScript:
                    break;
            }

            if (conditionMet) MarkLevelComplete();
        }

        public void MarkLevelComplete()
        {
            if (!PhotonNetwork.IsMasterClient || _completed) return;
            photonView.RPC(nameof(RPC_LevelComplete), RpcTarget.AllViaServer);
        }

        [PunRPC]
        private void RPC_LevelComplete()
        {
            if (_completed) return;
            _completed = true;
            Debug.Log($"[LevelManager] Level completed: {levelData?.displayName}");

            if (PhotonNetwork.IsMasterClient && !string.IsNullOrEmpty(nextLevelScene))
            {
                Invoke(nameof(LoadNext), 3.0f);
            }
        }

        private void LoadNext()
        {
            PhotonNetwork.LoadLevel(nextLevelScene);
        }

        [PunRPC]
        private void RPC_ResetLevel()
        {
            if (TimelineEventBus.Instance != null) TimelineEventBus.Instance.ClearHistory();
            _completed = false;
            _changedTargetIds.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
