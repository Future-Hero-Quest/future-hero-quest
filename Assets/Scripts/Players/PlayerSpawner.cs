using FutureHeroQuest.Core;
using System.Collections;
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

        private bool _hasSpawned;

        private void Start()
        {
            StartCoroutine(SpawnWhenPhotonIsReady());
        }

        private IEnumerator SpawnWhenPhotonIsReady()
        {
            if (NetworkManager.Instance == null)
            {
                Debug.LogError("[PlayerSpawner] NetworkManager not found.");
                yield break;
            }

            while (!PhotonNetwork.InRoom || !PhotonNetwork.IsMessageQueueRunning)
                yield return null;

            yield return null;
            yield return new WaitForSeconds(0.25f);

            if (_hasSpawned)
                yield break;

            var role = NetworkManager.Instance.MyRole;
            string prefabName = role == GameRole.Past ? pastPrefabName : futurePrefabName;
            Transform spawn = role == GameRole.Past ? pastSpawnPoint : futureSpawnPoint;

            Vector3 pos = spawn != null ? spawn.position : Vector3.zero;
            Quaternion rot = spawn != null ? spawn.rotation : Quaternion.identity;

            PhotonNetwork.Instantiate(prefabName, pos, rot);
            _hasSpawned = true;
            Debug.Log($"[PlayerSpawner] Spawned {prefabName} at {pos} for role {role}. QueueRunning={PhotonNetwork.IsMessageQueueRunning}");
        }

        private void OnGUI()
        {
            int roomPlayers = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
            int spawnedCapsules = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude).Length;
            GUI.Label(new Rect(12, 40, 360, 24), $"Room Players: {roomPlayers} / Spawned Capsules: {spawnedCapsules}");
        }
    }
}
