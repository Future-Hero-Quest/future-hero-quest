using UnityEngine;

namespace FutureHeroQuest.SceneFlow
{
    [RequireComponent(typeof(CharacterController))]
    public class OutlineLocalPlayerController : MonoBehaviour
    {
        public static OutlineLocalPlayerController LocalPlayer { get; private set; }

        private static readonly string[] InteractableTypeNames =
        {
            "OutlineInteractable",
            "TemporalOutlineInteractable",
            "SemanticStateSender",
            "SemanticReachZone",
            "LetterSender",
            "LetterReceiver",
            "MirrorSwitch",
            "SafeBox",
            "TreeSeedling",
            "LevelBoundaryTransition"
        };

        [SerializeField] private float moveSpeed = 4.0f;
        [SerializeField] private float rotateSpeed = 720.0f;
        [SerializeField] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float fallRespawnY = -8.0f;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private float sprintMultiplier = 1.55f;

        [Header("View")]
        [SerializeField] private bool defaultFirstPerson = true;
        [SerializeField] private KeyCode toggleViewKey = KeyCode.V;
        [SerializeField] private bool lockCursorInFirstPerson = true;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float minPitch = -55f;
        [SerializeField] private float maxPitch = 68f;
        [SerializeField] private float eyeHeight = 1.52f;
        [SerializeField] private float firstPersonFov = 72f;
        [SerializeField] private float thirdPersonFov = 64f;
        [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 2.25f, -4.15f);
        [SerializeField] private Vector3 thirdPersonFocusOffset = new Vector3(0f, 1.15f, 0f);
        [SerializeField] private float cameraFollowSharpness = 14f;

        [Header("HUD")]
        [SerializeField] private bool showControlHints = true;
        [SerializeField] private bool showCrosshair = true;
        [SerializeField] private float interactProbeDistance = 2.6f;
        [SerializeField] private float interactProbeRadius = 2.0f;
        [SerializeField] private LayerMask interactLayerMask = ~0;

        private static bool _inputLocked;
        private CharacterController _controller;
        private Vector3 _verticalVelocity;
        private Camera _camera;
        private Renderer[] _bodyRenderers;
        private bool _firstPersonView;
        private float _yaw;
        private float _pitch;
        private bool _nearInteractable;
        private GUIStyle _hintStyle;
        private GUIStyle _promptStyle;
        private GUIStyle _crosshairStyle;

        public static bool InputLocked => _inputLocked;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _bodyRenderers = GetComponentsInChildren<Renderer>(true);
            _yaw = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            LocalPlayer = this;
            _firstPersonView = defaultFirstPerson;
            EnsureLocalCamera();
            ApplyViewVisibility();
            ApplyCursorState();
        }

        private void OnDisable()
        {
            if (LocalPlayer == this) LocalPlayer = null;
        }

        private void Update()
        {
            EnsureLocalCamera();
            HandleViewToggle();

            if (!_inputLocked)
            {
                HandleLookInput();
            }

            UpdateInteractPromptState();

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
            float speed = moveSpeed * (Input.GetKey(sprintKey) && v > 0.1f ? sprintMultiplier : 1f);
            Vector3 horizontal = ResolveMoveDirection(h, v) * speed;

            if (_controller.isGrounded && _verticalVelocity.y < 0f)
                _verticalVelocity.y = -2f;

            if (_controller.isGrounded && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)))
            {
                _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else
            {
                _verticalVelocity.y += gravity * Time.deltaTime;
            }

            _controller.Move((horizontal + _verticalVelocity) * Time.deltaTime);

            if (_firstPersonView)
            {
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }
            else if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            UpdateLocalCamera();
        }

        private void OnGUI()
        {
            BuildGuiStyles();

            if (showCrosshair && _firstPersonView)
            {
                Rect crosshairRect = new Rect(Screen.width * 0.5f - 10f, Screen.height * 0.5f - 12f, 20f, 24f);
                GUI.Label(crosshairRect, "+", _crosshairStyle);
            }

            if (showControlHints)
            {
                Rect hintRect = new Rect(12f, Screen.height - 154f, 270f, 86f);
                GUI.Box(hintRect, GUIContent.none);
                GUI.Label(new Rect(hintRect.x + 12f, hintRect.y + 8f, 246f, 20f), "WASD / Arrows  Move", _hintStyle);
                GUI.Label(new Rect(hintRect.x + 12f, hintRect.y + 28f, 246f, 20f), "Mouse  Look", _hintStyle);
                GUI.Label(new Rect(hintRect.x + 12f, hintRect.y + 48f, 246f, 20f), "Space  Jump    Shift  Sprint", _hintStyle);
                GUI.Label(new Rect(hintRect.x + 12f, hintRect.y + 68f, 246f, 20f), "V  View    Esc  Cursor", _hintStyle);
            }

            if (_nearInteractable)
            {
                Rect promptRect = new Rect(Screen.width * 0.5f - 92f, Screen.height * 0.62f, 184f, 34f);
                GUI.Box(promptRect, GUIContent.none);
                GUI.Label(promptRect, "E  Interact", _promptStyle);
            }

            Rect buttonRect = new Rect(12f, Screen.height - 62f, 112f, 30f);
            if (GUI.Button(buttonRect, _firstPersonView ? "1st Person" : "3rd Person"))
            {
                ToggleViewMode();
            }
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            bool wasEnabled = _controller != null && _controller.enabled;
            if (_controller != null) _controller.enabled = false;

            transform.SetPositionAndRotation(position, rotation);
            _verticalVelocity = Vector3.zero;
            _yaw = rotation.eulerAngles.y;

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

        private void HandleViewToggle()
        {
            if (Input.GetKeyDown(toggleViewKey))
            {
                ToggleViewMode();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void ToggleViewMode()
        {
            _firstPersonView = !_firstPersonView;
            ApplyViewVisibility();
            ApplyCursorState();
            EnsureLocalCamera();
        }

        private void ApplyCursorState()
        {
            if (_firstPersonView && lockCursorInFirstPerson)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleLookInput()
        {
            if (!ShouldUseMouseLook())
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _yaw += mouseX;
            _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);
        }

        private bool ShouldUseMouseLook()
        {
            if (_firstPersonView)
            {
                return Cursor.lockState == CursorLockMode.Locked || !lockCursorInFirstPerson;
            }

            return Cursor.lockState == CursorLockMode.Locked || Input.GetMouseButton(1);
        }

        private Vector3 ResolveMoveDirection(float horizontalInput, float verticalInput)
        {
            Vector3 input = new Vector3(horizontalInput, 0f, verticalInput);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            Vector3 move = forward * input.z + right * input.x;
            return move.sqrMagnitude > 1f ? move.normalized : move;
        }

        private void EnsureLocalCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                _camera = cameraObject.AddComponent<Camera>();
            }

            _camera.enabled = true;
            _camera.rect = new Rect(0f, 0f, 1f, 1f);
            _camera.depth = 0f;
            _camera.orthographic = false;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.045f, 0.05f, 0.055f, 1f);
        }

        private void UpdateLocalCamera()
        {
            if (_camera == null)
            {
                return;
            }

            Quaternion viewRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 targetPosition;
            Quaternion targetRotation;

            if (_firstPersonView)
            {
                targetPosition = transform.position + Vector3.up * eyeHeight;
                targetRotation = viewRotation;
                _camera.fieldOfView = firstPersonFov;
                _camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
                return;
            }

            Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 focus = transform.position + thirdPersonFocusOffset;
            targetPosition = transform.position + yawRotation * thirdPersonOffset;
            targetRotation = Quaternion.LookRotation(focus - targetPosition, Vector3.up);
            _camera.fieldOfView = thirdPersonFov;

            float blend = 1f - Mathf.Exp(-cameraFollowSharpness * Time.deltaTime);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, blend);
            _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, targetRotation, blend);
        }

        private void ApplyViewVisibility()
        {
            if (_bodyRenderers == null)
            {
                return;
            }

            foreach (Renderer bodyRenderer in _bodyRenderers)
            {
                if (bodyRenderer != null)
                {
                    bodyRenderer.enabled = !_firstPersonView;
                }
            }
        }

        private void UpdateInteractPromptState()
        {
            _nearInteractable = false;

            if (_camera != null && Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit,
                    interactProbeDistance, interactLayerMask, QueryTriggerInteraction.Collide))
            {
                if (IsInteractableCandidate(hit.transform))
                {
                    _nearInteractable = true;
                    return;
                }
            }

            Collider[] nearby = Physics.OverlapSphere(transform.position, interactProbeRadius, interactLayerMask, QueryTriggerInteraction.Collide);
            foreach (Collider candidate in nearby)
            {
                if (candidate != null && IsInteractableCandidate(candidate.transform))
                {
                    _nearInteractable = true;
                    return;
                }
            }
        }

        private bool IsInteractableCandidate(Transform candidate)
        {
            if (candidate == null || candidate.root == transform.root)
            {
                return false;
            }

            if (HasInteractableMarker(candidate.gameObject))
            {
                return true;
            }

            Transform parent = candidate.parent;
            while (parent != null)
            {
                if (parent.root == transform.root)
                {
                    return false;
                }

                if (HasInteractableMarker(parent.gameObject))
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        private static bool HasInteractableMarker(GameObject candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            MonoBehaviour[] behaviours = candidate.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (IsRecognizedInteractable(behaviour))
                {
                    return true;
                }
            }

            behaviours = candidate.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (IsRecognizedInteractable(behaviour))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRecognizedInteractable(MonoBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return false;
            }

            string typeName = behaviour.GetType().Name;
            foreach (string interactableTypeName in InteractableTypeNames)
            {
                if (typeName == interactableTypeName)
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildGuiStyles()
        {
            if (_hintStyle != null)
            {
                return;
            }

            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = Color.white }
            };
            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _crosshairStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }
    }
}
