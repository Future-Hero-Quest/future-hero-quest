using System.Collections;
using FutureHeroQuest.Players;
using UnityEngine;

namespace FutureHeroQuest.SceneFlow
{
    public sealed class Scene02TemporalFracturePlayback : MonoBehaviour
    {
        [Header("Trigger")]
        [SerializeField] private OutlineInteractable interactable;
        [SerializeField] private PastFutureTimelineController timelineController;
        [SerializeField] private string projectionReason = "L2: cap wired @N7";

        [Header("Fracture Rig")]
        [SerializeField] private GameObject projectionTarget;
        [SerializeField] private GameObject fragmentRoot;
        [SerializeField] private GameObject collapsedVisual;
        [SerializeField] private TextMesh statusLabel;

        [Header("Playback")]
        [SerializeField] private Vector3 armedTargetPosition = new Vector3(-1.15f, 2.35f, 0.8f);
        [SerializeField] private Vector3 targetDropVelocity = new Vector3(0.15f, -8.25f, 0.25f);
        [SerializeField] private float fragmentReleaseDelay = 0.28f;
        [SerializeField] private Vector3 fragmentRootPosition = new Vector3(-1.15f, 1.1f, 0.9f);

        [Header("Gameplay Camera")]
        [SerializeField] private bool followLocalPlayerBeforeTrigger = true;
        [SerializeField] private Vector3 fallbackGameplayFocus = new Vector3(-1.3f, 0.75f, 0.85f);
        [SerializeField] private Vector3 gameplayCameraOffset = new Vector3(0.15f, 5.35f, -4.85f);
        [SerializeField] private Vector3 gameplayCameraEuler = new Vector3(53f, 0f, 0f);
        [SerializeField] private float gameplayCameraOrthographicSize = 4.15f;
        [SerializeField] private float gameplayCameraFollowSharpness = 9f;

        [Header("Camera")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Vector3 cameraPosition = new Vector3(-1.28f, 5.45f, -2.95f);
        [SerializeField] private Vector3 cameraEuler = new Vector3(59f, 0f, 0f);
        [SerializeField] private float cameraOrthographicSize = 2.65f;
        [SerializeField] private float cameraSettleDuration = 0.28f;
        [SerializeField] private float cameraShakeDuration = 0.55f;
        [SerializeField] private float cameraShakeAmount = 0.045f;

        [Header("Secondary View")]
        [SerializeField] private Camera secondaryCamera;
        [SerializeField] private bool showSecondaryView = true;
        [SerializeField] private Vector3 secondaryCameraPosition = new Vector3(0.05f, 7.4f, -4.9f);
        [SerializeField] private Vector3 secondaryCameraEuler = new Vector3(58f, 0f, 0f);
        [SerializeField] private float secondaryCameraOrthographicSize = 5.5f;
        [SerializeField] private Rect secondaryCameraViewport = new Rect(0.68f, 0.62f, 0.30f, 0.34f);

        private bool triggered;
        private Coroutine releaseRoutine;
        private Coroutine cameraRoutine;
        private Transform localPlayer;

        private static readonly Vector3[] FragmentOffsets =
        {
            new Vector3(-0.48f, 0.34f, -0.22f),
            new Vector3(0.46f, 0.28f, -0.12f),
            new Vector3(-0.28f, -0.08f, 0.28f),
            new Vector3(0.34f, -0.14f, 0.34f)
        };

        private static readonly Vector3[] FragmentVelocities =
        {
            new Vector3(-3.4f, -1.4f, -1.4f),
            new Vector3(3.2f, -1.6f, -0.9f),
            new Vector3(-1.8f, -2.1f, 1.6f),
            new Vector3(2.1f, -1.9f, 1.4f)
        };

        private void Awake()
        {
            if (interactable == null)
            {
                interactable = GetComponent<OutlineInteractable>();
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Start()
        {
            ApplyGameplayCameraFrame(true);
            ResetPlaybackState();
        }

        private void Update()
        {
            if (!triggered && interactable != null && interactable.IsUsed)
            {
                TriggerPlayback();
            }
        }

        private void LateUpdate()
        {
            if (!triggered)
            {
                ApplyGameplayCameraFrame(false);
            }
        }

        public void TriggerPlayback()
        {
            if (triggered)
            {
                return;
            }

            triggered = true;
            PlayImpactCameraFrame();
            ArmProjectionTargetForDrop();

            if (timelineController != null)
            {
                timelineController.NotifyPastInfluenceEnded(projectionReason);
            }

            if (statusLabel != null)
            {
                statusLabel.text = "TEMPORAL FRACTURE: ROCKFALL RELEASED";
            }

            releaseRoutine = StartCoroutine(ReleaseFragmentsAfterDelay());
            Debug.Log("[Scene02TemporalFracturePlayback] Projection requested and visible fracture playback armed.", this);
        }

        public void ResetPlaybackState()
        {
            triggered = false;

            if (releaseRoutine != null)
            {
                StopCoroutine(releaseRoutine);
                releaseRoutine = null;
            }

            if (cameraRoutine != null)
            {
                StopCoroutine(cameraRoutine);
                cameraRoutine = null;
            }

            ApplyGameplayCameraFrame(true);

            if (projectionTarget != null)
            {
                projectionTarget.SetActive(true);
                projectionTarget.transform.position = armedTargetPosition;
                projectionTarget.transform.rotation = Quaternion.identity;

                Rigidbody targetBody = projectionTarget.GetComponent<Rigidbody>();
                if (targetBody != null)
                {
                    targetBody.isKinematic = false;
                    targetBody.position = projectionTarget.transform.position;
                    targetBody.rotation = projectionTarget.transform.rotation;
                    targetBody.linearVelocity = Vector3.zero;
                    targetBody.angularVelocity = Vector3.zero;
                    targetBody.isKinematic = true;
                    targetBody.useGravity = false;
                    targetBody.detectCollisions = false;
                    targetBody.constraints = RigidbodyConstraints.FreezeAll;
                }
            }

            ResetFragments();

            if (statusLabel != null)
            {
                statusLabel.text = "N7 WIRED NODE: PROJECTED ROCKFALL";
            }
        }

        private IEnumerator ReleaseFragmentsAfterDelay()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, fragmentReleaseDelay));
            ReleaseFragments();
            releaseRoutine = null;
        }

        private void ArmProjectionTargetForDrop()
        {
            if (projectionTarget == null)
            {
                return;
            }

            projectionTarget.SetActive(true);
            projectionTarget.transform.position = armedTargetPosition;
            projectionTarget.transform.rotation = Quaternion.identity;

            Rigidbody targetBody = projectionTarget.GetComponent<Rigidbody>();
            if (targetBody == null)
            {
                return;
            }

            targetBody.isKinematic = false;
            targetBody.position = projectionTarget.transform.position;
            targetBody.rotation = projectionTarget.transform.rotation;
            targetBody.useGravity = true;
            targetBody.detectCollisions = true;
            targetBody.constraints = RigidbodyConstraints.None;
            targetBody.linearVelocity = targetDropVelocity;
            targetBody.angularVelocity = new Vector3(0.8f, 0.4f, -0.6f);
            targetBody.WakeUp();
        }

        private void ReleaseFragments()
        {
            if (collapsedVisual != null)
            {
                collapsedVisual.SetActive(true);
            }

            if (projectionTarget != null)
            {
                projectionTarget.SetActive(false);
            }

            if (fragmentRoot == null)
            {
                Debug.LogWarning("[Scene02TemporalFracturePlayback] Missing fragment root.", this);
                return;
            }

            fragmentRoot.SetActive(true);
            fragmentRoot.transform.position = fragmentRootPosition;
            fragmentRoot.transform.rotation = Quaternion.identity;

            Rigidbody rootBody = fragmentRoot.GetComponent<Rigidbody>();
            if (rootBody != null)
            {
                rootBody.isKinematic = true;
                rootBody.useGravity = false;
                rootBody.detectCollisions = false;
                rootBody.constraints = RigidbodyConstraints.FreezeAll;
            }

            Rigidbody[] bodies = fragmentRoot.GetComponentsInChildren<Rigidbody>(true);
            int released = 0;
            foreach (Rigidbody body in bodies)
            {
                if (body == null || body == rootBody)
                {
                    continue;
                }

                int index = released % FragmentOffsets.Length;
                body.gameObject.SetActive(true);
                body.transform.position = fragmentRootPosition + FragmentOffsets[index];
                body.transform.rotation = Quaternion.Euler(0f, released * 17f, released * 9f);
                body.isKinematic = false;
                body.position = body.transform.position;
                body.rotation = body.transform.rotation;
                body.useGravity = true;
                body.detectCollisions = true;
                body.constraints = RigidbodyConstraints.None;
                body.linearVelocity = FragmentVelocities[index];
                body.angularVelocity = new Vector3(1.4f + released, 0.8f, -1.1f);
                body.WakeUp();
                released++;
            }

            Debug.Log("[Scene02TemporalFracturePlayback] Released " + released + " visible fragments.", this);
        }

        private void ResetFragments()
        {
            if (fragmentRoot == null)
            {
                return;
            }

            fragmentRoot.SetActive(true);
            fragmentRoot.transform.position = fragmentRootPosition;
            fragmentRoot.transform.rotation = Quaternion.identity;

            Rigidbody rootBody = fragmentRoot.GetComponent<Rigidbody>();
            if (rootBody != null)
            {
                rootBody.isKinematic = true;
                rootBody.useGravity = false;
                rootBody.detectCollisions = false;
                rootBody.constraints = RigidbodyConstraints.FreezeAll;
            }

            Rigidbody[] bodies = fragmentRoot.GetComponentsInChildren<Rigidbody>(true);
            int fragmentIndex = 0;
            foreach (Rigidbody body in bodies)
            {
                if (body == null || body == rootBody)
                {
                    continue;
                }

                int index = fragmentIndex % FragmentOffsets.Length;
                body.gameObject.SetActive(false);
                body.transform.position = fragmentRootPosition + FragmentOffsets[index];
                body.transform.rotation = Quaternion.identity;
                body.isKinematic = false;
                body.constraints = RigidbodyConstraints.None;
                body.position = body.transform.position;
                body.rotation = body.transform.rotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                body.useGravity = false;
                body.detectCollisions = false;
                body.constraints = RigidbodyConstraints.FreezeAll;
                fragmentIndex++;
            }
        }

        private void ApplyGameplayCameraFrame(bool snap)
        {
            if (LocalViewOwnsGameplayCamera())
            {
                ApplySecondaryCameraFrame();
                return;
            }

            if (targetCamera == null)
            {
                return;
            }

            Vector3 focus = ResolveGameplayFocus();
            Vector3 targetPosition = new Vector3(focus.x, 0f, focus.z) + gameplayCameraOffset;
            if (snap)
            {
                targetCamera.transform.position = targetPosition;
            }
            else
            {
                float blend = 1f - Mathf.Exp(-gameplayCameraFollowSharpness * Time.deltaTime);
                targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, targetPosition, blend);
            }

            targetCamera.transform.rotation = Quaternion.Euler(gameplayCameraEuler);
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = gameplayCameraOrthographicSize;
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
            targetCamera.depth = 0f;

            ApplySecondaryCameraFrame();
        }

        private Vector3 ResolveGameplayFocus()
        {
            if (!followLocalPlayerBeforeTrigger)
            {
                return fallbackGameplayFocus;
            }

            if (localPlayer == null)
            {
                PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude);
                foreach (PlayerController player in players)
                {
                    if (player != null && player.photonView != null && player.photonView.IsMine)
                    {
                        localPlayer = player.transform;
                        break;
                    }
                }
            }

            return localPlayer != null ? localPlayer.position : fallbackGameplayFocus;
        }

        private void PlayImpactCameraFrame()
        {
            if (LocalViewOwnsGameplayCamera())
            {
                if (PlayerController.LocalViewOwnsCamera)
                {
                    PlayerController.AddLocalCameraShake(cameraShakeDuration, cameraShakeAmount * 1.75f);
                }

                ApplySecondaryCameraFrame();
                return;
            }

            if (cameraRoutine != null)
            {
                StopCoroutine(cameraRoutine);
            }

            cameraRoutine = StartCoroutine(ImpactCameraRoutine());
        }

        private IEnumerator ImpactCameraRoutine()
        {
            if (targetCamera == null)
            {
                yield break;
            }

            Vector3 startPosition = targetCamera.transform.position;
            Quaternion startRotation = targetCamera.transform.rotation;
            float startSize = targetCamera.orthographicSize;
            Quaternion targetRotation = Quaternion.Euler(cameraEuler);
            float settleDuration = Mathf.Max(0.01f, cameraSettleDuration);

            for (float elapsed = 0f; elapsed < settleDuration; elapsed += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / settleDuration);
                targetCamera.transform.position = Vector3.Lerp(startPosition, cameraPosition, t);
                targetCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                targetCamera.orthographicSize = Mathf.Lerp(startSize, cameraOrthographicSize, t);
                ApplyPrimaryCameraOutput();
                yield return null;
            }

            float shakeDuration = Mathf.Max(0f, cameraShakeDuration);
            for (float elapsed = 0f; elapsed < shakeDuration; elapsed += Time.deltaTime)
            {
                float fade = 1f - Mathf.Clamp01(elapsed / Mathf.Max(0.01f, shakeDuration));
                float shakeX = Mathf.Sin(Time.time * 57.3f) * cameraShakeAmount * fade;
                float shakeY = Mathf.Cos(Time.time * 43.7f) * cameraShakeAmount * fade;
                targetCamera.transform.position = cameraPosition + new Vector3(shakeX, shakeY, 0f);
                targetCamera.transform.rotation = targetRotation;
                targetCamera.orthographicSize = cameraOrthographicSize;
                ApplyPrimaryCameraOutput();
                yield return null;
            }

            targetCamera.transform.position = cameraPosition;
            targetCamera.transform.rotation = targetRotation;
            targetCamera.orthographicSize = cameraOrthographicSize;
            ApplyPrimaryCameraOutput();
            cameraRoutine = null;
        }

        private void ApplyPrimaryCameraOutput()
        {
            if (targetCamera == null)
            {
                return;
            }
            targetCamera.orthographic = true;
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
            targetCamera.depth = 0f;

            ApplySecondaryCameraFrame();
        }

        private static bool LocalViewOwnsGameplayCamera()
        {
            return PlayerController.LocalViewOwnsCamera || OutlineLocalPlayerController.LocalViewOwnsCamera;
        }

        private void ApplySecondaryCameraFrame()
        {
            if (secondaryCamera == null)
            {
                return;
            }

            secondaryCamera.enabled = showSecondaryView;
            if (!showSecondaryView)
            {
                return;
            }

            secondaryCamera.transform.position = secondaryCameraPosition;
            secondaryCamera.transform.rotation = Quaternion.Euler(secondaryCameraEuler);
            secondaryCamera.orthographic = true;
            secondaryCamera.orthographicSize = secondaryCameraOrthographicSize;
            secondaryCamera.rect = secondaryCameraViewport;
            secondaryCamera.depth = targetCamera != null ? targetCamera.depth + 1f : 1f;
            secondaryCamera.clearFlags = CameraClearFlags.SolidColor;
            secondaryCamera.backgroundColor = new Color(0.025f, 0.03f, 0.035f, 1f);
        }
    }
}
