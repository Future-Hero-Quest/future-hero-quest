using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 第 1 关《种树》的过去侧触发器。
    /// 挂在过去世界的"树苗位置"上，玩家走近按 E 触发种树事件。
    /// 事件抵达未来世界后，对应的 PuzzleObject(初始=空地, 改变=大树)切换状态。
    /// </summary>
    public class TreeSeedling : MonoBehaviour
    {
        [SerializeField] private string treeTargetId = "tree_garden_01";
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private GameObject promptUI;
        [SerializeField] private GameObject seedlingMesh;

        private bool _planted;
        private Transform _localPlayer;

        private void Update()
        {
            if (_planted) return;

            if (_localPlayer == null) FindLocalPlayer();
            if (_localPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _localPlayer.position);
            bool inRange = dist <= interactRadius;

            if (promptUI != null) promptUI.SetActive(inRange);

            if (inRange && Input.GetKeyDown(KeyCode.E))
            {
                PlantTree();
            }
        }

        private void FindLocalPlayer()
        {
            var role = NetworkManager.Instance != null ? NetworkManager.Instance.MyRole : GameRole.Past;
            if (role != GameRole.Past) return;

            var players = FindObjectsOfType<Players.PlayerController>();
            foreach (var p in players)
            {
                if (p.photonView.IsMine)
                {
                    _localPlayer = p.transform;
                    break;
                }
            }
        }

        private void PlantTree()
        {
            _planted = true;
            if (TimelineEventBus.Instance == null)
            {
                Debug.LogError("[TreeSeedling] TimelineEventBus not ready.");
                return;
            }
            TimelineEventBus.Instance.SendEvent(EventKind.PlantTree, treeTargetId, transform.position);
            if (seedlingMesh != null) seedlingMesh.SetActive(false);
            if (promptUI != null) promptUI.SetActive(false);
            Debug.Log($"[TreeSeedling] Planted! Sent PlantTree event for {treeTargetId}.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
