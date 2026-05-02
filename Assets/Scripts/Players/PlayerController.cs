using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.Players
{
    /// <summary>
    /// 第三人称角色控制（CharacterController 版本，避免 Rigidbody 网络抖动）。
    /// 仅本地 owner 接收输入，其他玩家通过 PhotonTransformView 同步位置。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float rotateSpeed = 720.0f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float fallRespawnY = -8.0f;

        private CharacterController _cc;
        private Vector3 _velocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            int senderActor = info.Sender != null ? info.Sender.ActorNumber : -1;
            int ownerActor = photonView.Owner != null ? photonView.Owner.ActorNumber : -1;
            Debug.Log($"[PlayerController] Photon instantiated {gameObject.name}. IsMine={photonView.IsMine}, Owner=#{ownerActor}, Sender=#{senderActor}");
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            if (transform.position.y < fallRespawnY && PlayerSpawner.TryGetSpawnPoseForLocalRole(out Vector3 spawnPos, out Quaternion spawnRot))
            {
                TeleportTo(spawnPos, spawnRot);
                Debug.LogWarning($"[PlayerController] Local player fell below {fallRespawnY}; returned to spawn {spawnPos}.");
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, 0, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 horizontal = input * moveSpeed;

            if (_cc.isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;
            else
                _velocity.y += gravity * Time.deltaTime;

            Vector3 motion = horizontal + Vector3.up * _velocity.y;
            _cc.Move(motion * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            bool wasEnabled = _cc != null && _cc.enabled;
            if (_cc != null) _cc.enabled = false;

            transform.SetPositionAndRotation(position, rotation);
            _velocity = Vector3.zero;

            if (_cc != null) _cc.enabled = wasEnabled;
        }
    }
}
