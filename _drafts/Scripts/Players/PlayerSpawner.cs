using FutureHeroQuest.Core;
using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.Players
{
    /// <summary>
    /// 关卡场景中挂一个空 GameObject，进入场景后根据本机角色 spawn 对应 Prefab。
    /// 注意：Prefab 必须放在 Resources/ 目录下，名字与 prefabName 一致。
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private string pastPrefabName = "PastPlayer";
        [SerializeField] private string futurePrefabName = "FuturePlayer";
        [SerializeField] private Transform pastSpawnPoint;
        [SerializeField] private Transform futureSpawnPoint;

        private void Start()
        {
            if (NetworkManager.Instance == null)
            {
                Debug.LogError("[PlayerSpawner] NetworkManager not found.");
                return;
            }

            var role = NetworkManager.Instance.MyRole;
            string prefabName = role == GameRole.Past ? pastPrefabName : futurePrefabName;
            Transform spawn = role == GameRole.Past ? pastSpawnPoint : futureSpawnPoint;

            Vector3 pos = spawn != null ? spawn.position : Vector3.zero;
            Quaternion rot = spawn != null ? spawn.rotation : Quaternion.identity;

            PhotonNetwork.Instantiate(prefabName, pos, rot);
            Debug.Log($"[PlayerSpawner] Spawned {prefabName} at {pos} for role {role}");
        }
    }
}
