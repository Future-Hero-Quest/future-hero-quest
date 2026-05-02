using FutureHeroQuest.Players;
using UnityEngine;

namespace FutureHeroQuest.SceneFlow
{
    [RequireComponent(typeof(Collider))]
    public class LevelBoundaryTransition : MonoBehaviour
    {
        [SerializeField] private string nextSceneName;
        [SerializeField] private string nextScenePath;
        [SerializeField] private string sourceAnchorId;
        [SerializeField] private string targetAnchorId;
        [SerializeField] private bool requireInteract = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private GameObject promptUI;

        private Transform _candidatePlayer;
        private bool _transitionStarted;

        public string NextSceneName => nextSceneName;
        public string NextScenePath => nextScenePath;
        public string SourceAnchorId => sourceAnchorId;
        public string TargetAnchorId => targetAnchorId;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnDisable()
        {
            SetPrompt(false);
        }

        private void Update()
        {
            if (_transitionStarted || _candidatePlayer == null)
            {
                SetPrompt(false);
                return;
            }

            SetPrompt(requireInteract);

            if (!requireInteract || Input.GetKeyDown(interactKey))
                BeginTransition(_candidatePlayer);
        }

        private void OnTriggerEnter(Collider other)
        {
            Transform player = TryGetPlayer(other);
            if (player == null) return;

            _candidatePlayer = player;
            if (!requireInteract)
                BeginTransition(player);
        }

        private void OnTriggerExit(Collider other)
        {
            Transform player = TryGetPlayer(other);
            if (player != null && player == _candidatePlayer)
            {
                _candidatePlayer = null;
                SetPrompt(false);
            }
        }

        private void BeginTransition(Transform player)
        {
            if (_transitionStarted) return;
            _transitionStarted = true;
            SetPrompt(false);

            LevelTransitionAnchor sourceAnchor = FindAnchor(sourceAnchorId);
            OutlineSceneFlowController.EnsureInstance()
                .BeginTransition(this, player, sourceAnchor);
        }

        private static Transform TryGetPlayer(Collider collider)
        {
            if (collider == null) return null;

            var outlinePlayer = collider.GetComponentInParent<OutlineLocalPlayerController>();
            if (outlinePlayer != null) return outlinePlayer.transform;

            var networkPlayer = collider.GetComponentInParent<PlayerController>();
            if (networkPlayer != null && networkPlayer.photonView != null && networkPlayer.photonView.IsMine)
                return networkPlayer.transform;

            return null;
        }

        private static LevelTransitionAnchor FindAnchor(string anchorId)
        {
            if (string.IsNullOrWhiteSpace(anchorId)) return null;

            var anchors = FindObjectsByType<LevelTransitionAnchor>(FindObjectsInactive.Exclude);
            foreach (LevelTransitionAnchor anchor in anchors)
            {
                if (anchor != null && string.Equals(anchor.AnchorId, anchorId.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return anchor;
            }

            return null;
        }

        private void SetPrompt(bool active)
        {
            if (promptUI != null) promptUI.SetActive(active);
        }
    }
}
