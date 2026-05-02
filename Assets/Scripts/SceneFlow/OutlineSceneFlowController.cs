using System.Collections;
using FutureHeroQuest.Players;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace FutureHeroQuest.SceneFlow
{
    public class OutlineSceneFlowController : MonoBehaviour
    {
        public static OutlineSceneFlowController Instance { get; private set; }

        [SerializeField] private float fadeSeconds = 0.24f;
        [SerializeField] private float blackHoldSeconds = 0.12f;

        private bool _transitioning;
        private float _fadeAlpha;
        private string _pendingTargetAnchorId;
        private Vector3 _relativePosition;
        private Quaternion _relativeRotation = Quaternion.identity;
        private bool _hasPendingPlacement;

        public static OutlineSceneFlowController EnsureInstance()
        {
            if (Instance != null) return Instance;

            var existing = FindAnyObjectByType<OutlineSceneFlowController>();
            if (existing != null) return existing;

            var go = new GameObject(nameof(OutlineSceneFlowController));
            return go.AddComponent<OutlineSceneFlowController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        public void BeginTransition(LevelBoundaryTransition transition, Transform player, LevelTransitionAnchor sourceAnchor)
        {
            if (_transitioning || transition == null || player == null) return;

            if (sourceAnchor != null)
            {
                _relativePosition = Quaternion.Inverse(sourceAnchor.transform.rotation)
                    * (player.position - sourceAnchor.transform.position);
                _relativeRotation = Quaternion.Inverse(sourceAnchor.transform.rotation) * player.rotation;
            }
            else
            {
                _relativePosition = Vector3.zero;
                _relativeRotation = player.rotation;
            }

            _pendingTargetAnchorId = string.IsNullOrWhiteSpace(transition.TargetAnchorId)
                ? transition.SourceAnchorId
                : transition.TargetAnchorId;
            _hasPendingPlacement = true;

            StartCoroutine(TransitionRoutine(transition.NextSceneName, transition.NextScenePath));
        }

        private IEnumerator TransitionRoutine(string nextSceneName, string nextScenePath)
        {
            _transitioning = true;
            OutlineLocalPlayerController.SetInputLocked(true);

            yield return FadeTo(1f);
            yield return new WaitForSeconds(blackHoldSeconds);

            if (PhotonNetwork.InRoom)
            {
                PlayerSpawner.DestroyOwnedPlayerObjectsForSceneReload();
                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.LoadLevel(nextSceneName);
            }
            else
            {
                LoadOffline(nextSceneName, nextScenePath);
            }
        }

        private static void LoadOffline(string nextSceneName, string nextScenePath)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(nextScenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(nextScenePath, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(nextSceneName);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_hasPendingPlacement) return;
            StartCoroutine(ApplyPendingPlacement());
        }

        private IEnumerator ApplyPendingPlacement()
        {
            yield return null;
            yield return null;

            LevelTransitionAnchor targetAnchor = FindAnchor(_pendingTargetAnchorId);
            Transform player = FindLocalPlayer();

            if (targetAnchor != null && player != null)
            {
                Vector3 targetPosition = targetAnchor.transform.position
                    + targetAnchor.transform.rotation * _relativePosition;
                Quaternion targetRotation = targetAnchor.transform.rotation * _relativeRotation;
                TeleportPlayer(player, targetPosition, targetRotation);
            }

            _hasPendingPlacement = false;
            yield return FadeTo(0f);

            OutlineLocalPlayerController.SetInputLocked(false);
            _transitioning = false;
        }

        private IEnumerator FadeTo(float target)
        {
            float start = _fadeAlpha;
            float duration = Mathf.Max(0.01f, fadeSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeAlpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            _fadeAlpha = target;
        }

        private static LevelTransitionAnchor FindAnchor(string anchorId)
        {
            if (string.IsNullOrWhiteSpace(anchorId)) return null;

            var anchors = FindObjectsByType<LevelTransitionAnchor>(FindObjectsInactive.Include);
            foreach (LevelTransitionAnchor anchor in anchors)
            {
                if (anchor != null && string.Equals(anchor.AnchorId, anchorId.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return anchor;
            }

            return null;
        }

        private static Transform FindLocalPlayer()
        {
            if (OutlineLocalPlayerController.LocalPlayer != null)
                return OutlineLocalPlayerController.LocalPlayer.transform;

            var players = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude);
            foreach (PlayerController player in players)
            {
                if (player != null && player.photonView != null && player.photonView.IsMine)
                    return player.transform;
            }

            return null;
        }

        private static void TeleportPlayer(Transform player, Vector3 position, Quaternion rotation)
        {
            var outline = player.GetComponent<OutlineLocalPlayerController>();
            if (outline != null)
            {
                outline.TeleportTo(position, rotation);
                return;
            }

            var network = player.GetComponent<PlayerController>();
            if (network != null)
            {
                network.TeleportTo(position, rotation);
                return;
            }

            player.SetPositionAndRotation(position, rotation);
        }

        private void OnGUI()
        {
            if (_fadeAlpha <= 0.001f) return;

            Color prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(_fadeAlpha));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
