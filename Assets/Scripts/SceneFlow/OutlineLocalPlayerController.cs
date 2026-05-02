using UnityEngine;

namespace FutureHeroQuest.SceneFlow
{
    [RequireComponent(typeof(CharacterController))]
    public class OutlineLocalPlayerController : MonoBehaviour
    {
        public static OutlineLocalPlayerController LocalPlayer { get; private set; }

        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float rotateSpeed = 720.0f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float fallRespawnY = -8.0f;
        [SerializeField] private Transform respawnPoint;

        private static bool _inputLocked;
        private CharacterController _controller;
        private Vector3 _verticalVelocity;

        public static bool InputLocked => _inputLocked;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            LocalPlayer = this;
        }

        private void OnDisable()
        {
            if (LocalPlayer == this) LocalPlayer = null;
        }

        private void Update()
        {
            if (transform.position.y < fallRespawnY)
            {
                if (respawnPoint != null)
                    TeleportTo(respawnPoint.position, respawnPoint.rotation);
                return;
            }

            if (_inputLocked)
            {
                ApplyGravityOnly();
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 horizontal = input * moveSpeed;

            if (_controller.isGrounded && _verticalVelocity.y < 0f)
                _verticalVelocity.y = -2f;
            else
                _verticalVelocity.y += gravity * Time.deltaTime;

            _controller.Move((horizontal + _verticalVelocity) * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * Time.deltaTime);
            }
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            bool wasEnabled = _controller != null && _controller.enabled;
            if (_controller != null) _controller.enabled = false;

            transform.SetPositionAndRotation(position, rotation);
            _verticalVelocity = Vector3.zero;

            if (_controller != null) _controller.enabled = wasEnabled;
        }

        public static void SetInputLocked(bool locked)
        {
            _inputLocked = locked;
        }

        private void ApplyGravityOnly()
        {
            if (_controller == null || _controller.isGrounded) return;
            _verticalVelocity.y += gravity * Time.deltaTime;
            _controller.Move(_verticalVelocity * Time.deltaTime);
        }
    }
}
