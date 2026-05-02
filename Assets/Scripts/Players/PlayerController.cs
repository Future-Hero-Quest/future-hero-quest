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
        public static bool LocalViewOwnsCamera => LocalCameraOwner != null;

        private static PlayerController LocalCameraOwner;
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
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private float sprintMultiplier = 1.55f;
        [SerializeField] private float crouchSpeedMultiplier = 0.55f;
        [SerializeField] private float crouchHeight = 1.15f;
        [SerializeField] private float crouchEyeHeight = 0.95f;
        [SerializeField] private float crouchTransitionSharpness = 12f;

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
        [SerializeField] private float walkBobFrequency = 7.5f;
        [SerializeField] private float sprintBobFrequency = 10.5f;
        [SerializeField] private float viewBobAmount = 0.035f;
        [SerializeField] private float handBobAmount = 0.028f;
        [SerializeField] private float interactReachDistance = 0.16f;
        [SerializeField] private float interactReachDuration = 0.18f;

        [Header("HUD")]
        [SerializeField] private bool showControlHints = true;
        [SerializeField] private bool showCrosshair = true;
        [SerializeField] private float interactProbeDistance = 2.6f;
        [SerializeField] private float interactProbeRadius = 2.0f;
        [SerializeField] private LayerMask interactLayerMask = ~0;

        private CharacterController _cc;
        private Vector3 _velocity;
        private Camera _camera;
        private Renderer[] _bodyRenderers;
        private Transform _viewModelRoot;
        private bool _firstPersonView;
        private float _yaw;
        private float _pitch;
        private float _cameraShakeTime;
        private float _cameraShakeDuration;
        private float _cameraShakeAmount;
        private float _standingHeight;
        private Vector3 _standingCenter;
        private float _currentEyeHeight;
        private float _crouchBlend;
        private float _moveAmount;
        private float _bobPhase;
        private float _interactReachTime;
        private bool _isSprinting;
        private bool _isCrouching;
        private bool _nearInteractable;
        private GUIStyle _hintStyle;
        private GUIStyle _promptStyle;
        private GUIStyle _crosshairStyle;

        private bool IsLocallyControlled => photonView == null || photonView.IsMine || !PhotonNetwork.InRoom;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (_cc != null)
            {
                _standingHeight = _cc.height;
                _standingCenter = _cc.center;
            }

            _currentEyeHeight = eyeHeight;
        }

        private void Start()
        {
            if (!IsLocallyControlled)
            {
                return;
            }

            LocalCameraOwner = this;
            _firstPersonView = defaultFirstPerson;
            _yaw = transform.eulerAngles.y;
            _pitch = 0f;
            _bodyRenderers = GetComponentsInChildren<Renderer>(true);
            EnsureLocalCamera();
            ApplyViewVisibility();
            ApplyCursorState();
        }

        private void OnDestroy()
        {
            if (LocalCameraOwner == this)
            {
                LocalCameraOwner = null;
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            int senderActor = info.Sender != null ? info.Sender.ActorNumber : -1;
            int ownerActor = photonView.Owner != null ? photonView.Owner.ActorNumber : -1;
            Debug.Log($"[PlayerController] Photon instantiated {gameObject.name}. IsMine={photonView.IsMine}, Owner=#{ownerActor}, Sender=#{senderActor}");
        }

        private void Update()
        {
            if (!IsLocallyControlled) return;

            if (LocalCameraOwner != this)
            {
                LocalCameraOwner = this;
            }

            EnsureLocalCamera();
            HandleViewToggle();
            HandleLookInput();
            HandleInteractAnimationInput();
            UpdateInteractPromptState();

            if (transform.position.y < fallRespawnY && PlayerSpawner.TryGetSpawnPoseForLocalRole(out Vector3 spawnPos, out Quaternion spawnRot))
            {
                TeleportTo(spawnPos, spawnRot);
                Debug.LogWarning($"[PlayerController] Local player fell below {fallRespawnY}; returned to spawn {spawnPos}.");
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            UpdateCrouchState();

            _isSprinting = !_isCrouching && Input.GetKey(sprintKey) && v > 0.1f;
            float speed = moveSpeed;
            if (_isCrouching)
            {
                speed *= crouchSpeedMultiplier;
            }
            else if (_isSprinting)
            {
                speed *= sprintMultiplier;
            }

            Vector3 horizontal = ResolveMoveDirection(h, v) * speed;
            _moveAmount = Mathf.Clamp01(horizontal.magnitude / Mathf.Max(0.01f, moveSpeed * sprintMultiplier));

            if (_cc.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            if (_cc.isGrounded && Input.GetButtonDown("Jump"))
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (!_cc.isGrounded)
            {
                _velocity.y += gravity * Time.deltaTime;
            }

            Vector3 motion = horizontal + Vector3.up * _velocity.y;
            _cc.Move(motion * Time.deltaTime);

            if (_firstPersonView)
            {
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            }
            else if (horizontal.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (!IsLocallyControlled)
            {
                return;
            }

            UpdateLocalCamera();
        }

        private void OnGUI()
        {
            if (!IsLocallyControlled)
            {
                return;
            }

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
            if (_cc == null) _cc = GetComponent<CharacterController>();
            bool wasEnabled = _cc != null && _cc.enabled;
            if (_cc != null) _cc.enabled = false;

            transform.SetPositionAndRotation(position, rotation);
            _velocity = Vector3.zero;
            _yaw = rotation.eulerAngles.y;

            if (_cc != null) _cc.enabled = wasEnabled;
        }

        public static void AddLocalCameraShake(float duration, float amount)
        {
            if (LocalCameraOwner != null)
            {
                LocalCameraOwner.AddCameraShake(duration, amount);
            }
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

        private void HandleInteractAnimationInput()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                _interactReachTime = interactReachDuration;
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

        private void UpdateCrouchState()
        {
            if (_cc == null)
            {
                return;
            }

            _isCrouching = Input.GetKey(crouchKey);
            float targetBlend = _isCrouching ? 1f : 0f;
            float blend = 1f - Mathf.Exp(-crouchTransitionSharpness * Time.deltaTime);
            _crouchBlend = Mathf.Lerp(_crouchBlend, targetBlend, blend);

            float targetHeight = Mathf.Lerp(_standingHeight, crouchHeight, _crouchBlend);
            _cc.height = targetHeight;
            _cc.center = _standingCenter + Vector3.down * ((_standingHeight - targetHeight) * 0.5f);
            _currentEyeHeight = Mathf.Lerp(eyeHeight, crouchEyeHeight, _crouchBlend);
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

            EnsureViewModel();
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
                targetPosition = transform.position + Vector3.up * _currentEyeHeight + viewRotation * new Vector3(0.08f, -0.02f, 0.12f);
                targetPosition += ResolveViewBobOffset(viewRotation);
                targetRotation = viewRotation;
                _camera.fieldOfView = firstPersonFov;
            }
            else
            {
                Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);
                Vector3 focus = transform.position + thirdPersonFocusOffset;
                targetPosition = transform.position + yawRotation * thirdPersonOffset;
                targetRotation = Quaternion.LookRotation(focus - targetPosition, Vector3.up);
                _camera.fieldOfView = thirdPersonFov;
            }

            Vector3 shakeOffset = ConsumeCameraShakeOffset();
            targetPosition += shakeOffset;

            if (_firstPersonView)
            {
                _camera.transform.SetPositionAndRotation(targetPosition, targetRotation);
            }
            else
            {
                float blend = 1f - Mathf.Exp(-cameraFollowSharpness * Time.deltaTime);
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, blend);
                _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, targetRotation, blend);
            }

            UpdateViewModelMotion();
        }

        private Vector3 ResolveViewBobOffset(Quaternion viewRotation)
        {
            if (!_cc.isGrounded || _moveAmount <= 0.01f)
            {
                return Vector3.zero;
            }

            float frequency = _isSprinting ? sprintBobFrequency : walkBobFrequency;
            _bobPhase += Time.deltaTime * frequency;
            float vertical = Mathf.Sin(_bobPhase) * viewBobAmount * _moveAmount;
            float lateral = Mathf.Cos(_bobPhase * 0.5f) * viewBobAmount * 0.45f * _moveAmount;
            return viewRotation * new Vector3(lateral, vertical, 0f);
        }

        private Vector3 ConsumeCameraShakeOffset()
        {
            if (_cameraShakeTime <= 0f || _cameraShakeDuration <= 0f)
            {
                return Vector3.zero;
            }

            _cameraShakeTime = Mathf.Max(0f, _cameraShakeTime - Time.deltaTime);
            float fade = _cameraShakeTime / _cameraShakeDuration;
            float x = Mathf.Sin(Time.time * 57.3f) * _cameraShakeAmount * fade;
            float y = Mathf.Cos(Time.time * 43.7f) * _cameraShakeAmount * fade;
            return new Vector3(x, y, 0f);
        }

        private void AddCameraShake(float duration, float amount)
        {
            _cameraShakeDuration = Mathf.Max(_cameraShakeDuration, duration);
            _cameraShakeTime = Mathf.Max(_cameraShakeTime, duration);
            _cameraShakeAmount = Mathf.Max(_cameraShakeAmount, amount);
        }

        private void ApplyViewVisibility()
        {
            if (_bodyRenderers != null)
            {
                foreach (Renderer bodyRenderer in _bodyRenderers)
                {
                    if (bodyRenderer != null)
                    {
                        bodyRenderer.enabled = !_firstPersonView;
                    }
                }
            }

            if (_viewModelRoot != null)
            {
                _viewModelRoot.gameObject.SetActive(_firstPersonView);
            }
        }

        private void UpdateViewModelMotion()
        {
            if (_viewModelRoot == null)
            {
                return;
            }

            _interactReachTime = Mathf.Max(0f, _interactReachTime - Time.deltaTime);
            float reachProgress = interactReachDuration > 0f
                ? 1f - (_interactReachTime / interactReachDuration)
                : 1f;
            float reach = _interactReachTime > 0f
                ? Mathf.Sin(Mathf.Clamp01(reachProgress) * Mathf.PI) * interactReachDistance
                : 0f;

            float bob = _cc != null && _cc.isGrounded ? _moveAmount : 0f;
            float handX = Mathf.Sin(_bobPhase * 0.75f) * handBobAmount * bob;
            float handY = Mathf.Cos(_bobPhase) * handBobAmount * bob;

            _viewModelRoot.localPosition = new Vector3(handX, handY, reach);
            _viewModelRoot.localRotation = Quaternion.Euler(handY * -18f, handX * 16f, handX * -10f);
        }

        private void EnsureViewModel()
        {
            if (_camera == null || _viewModelRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("LocalFirstPersonHands");
            root.hideFlags = HideFlags.DontSave;
            root.transform.SetParent(_camera.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            _viewModelRoot = root.transform;

            Material sleeve = CreateViewModelMaterial(new Color(0.03f, 0.18f, 0.24f, 1f));
            Material hand = CreateViewModelMaterial(new Color(0.95f, 0.72f, 0.48f, 1f));
            CreateViewModelBlock("RightSleeve", new Vector3(0.36f, -0.34f, 0.62f), new Vector3(0.18f, 0.16f, 0.42f), sleeve);
            CreateViewModelBlock("RightHand", new Vector3(0.34f, -0.30f, 0.86f), new Vector3(0.16f, 0.12f, 0.18f), hand);
            CreateViewModelBlock("LeftSleeve", new Vector3(-0.28f, -0.38f, 0.64f), new Vector3(0.14f, 0.13f, 0.34f), sleeve);
            CreateViewModelBlock("LeftHand", new Vector3(-0.28f, -0.34f, 0.84f), new Vector3(0.13f, 0.10f, 0.16f), hand);
            ApplyViewVisibility();
        }

        private void CreateViewModelBlock(string blockName, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.hideFlags = HideFlags.DontSave;
            block.transform.SetParent(_viewModelRoot, false);
            block.transform.localPosition = localPosition;
            block.transform.localRotation = Quaternion.identity;
            block.transform.localScale = localScale;

            Collider blockCollider = block.GetComponent<Collider>();
            if (blockCollider != null)
            {
                Destroy(blockCollider);
            }

            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material CreateViewModelMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = color;
            material.hideFlags = HideFlags.DontSave;
            return material;
        }
    }
}
