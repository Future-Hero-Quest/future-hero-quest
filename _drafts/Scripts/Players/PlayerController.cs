using Photon.Pun;
using UnityEngine;

namespace FutureHeroQuest.Players
{
    /// <summary>
    /// 第三人称角色控制（CharacterController 版本，避免 Rigidbody 网络抖动）。
    /// 仅本地 owner 接收输入，其他玩家通过 PhotonTransformView 同步位置。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviourPun
    {
        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float rotateSpeed = 720.0f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController _cc;
        private Vector3 _velocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

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
    }
}
