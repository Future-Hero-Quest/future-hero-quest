using FutureHeroQuest.Players;
using UnityEngine;

namespace FutureHeroQuest.SceneFlow
{
    public class OutlineInteractable : MonoBehaviour
    {
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactRadius = 1.8f;
        [SerializeField] private bool oneShot = true;
        [SerializeField] private GameObject promptUI;
        [SerializeField] private GameObject[] activateAfterUse;
        [SerializeField] private GameObject[] deactivateAfterUse;
        [SerializeField] private Collider[] enableCollidersAfterUse;
        [SerializeField] private Collider[] disableCollidersAfterUse;
        [SerializeField] private Behaviour[] enableBehavioursAfterUse;
        [SerializeField] private Behaviour[] disableBehavioursAfterUse;
        [SerializeField] private string successMessage;

        private bool _used;

        private void OnDisable()
        {
            SetPrompt(false);
        }

        private void Update()
        {
            if (_used && oneShot)
            {
                SetPrompt(false);
                return;
            }

            Transform player = FindLocalPlayer();
            bool canUse = player != null
                && (player.position - transform.position).sqrMagnitude <= interactRadius * interactRadius;

            SetPrompt(canUse);

            if (canUse && Input.GetKeyDown(interactKey))
                Use();
        }

        public void Use()
        {
            if (_used && oneShot) return;

            SetActive(activateAfterUse, true);
            SetActive(deactivateAfterUse, false);
            SetEnabled(enableCollidersAfterUse, true);
            SetEnabled(disableCollidersAfterUse, false);
            SetEnabled(enableBehavioursAfterUse, true);
            SetEnabled(disableBehavioursAfterUse, false);

            _used = true;
            SetPrompt(false);

            if (!string.IsNullOrWhiteSpace(successMessage))
                Debug.Log($"[OutlineInteractable] {successMessage}", this);
        }

        private void SetPrompt(bool active)
        {
            if (promptUI != null) promptUI.SetActive(active);
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

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;
            foreach (GameObject obj in objects)
            {
                if (obj != null) obj.SetActive(active);
            }
        }

        private static void SetEnabled(Collider[] colliders, bool enabled)
        {
            if (colliders == null) return;
            foreach (Collider collider in colliders)
            {
                if (collider != null) collider.enabled = enabled;
            }
        }

        private static void SetEnabled(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null) return;
            foreach (Behaviour behaviour in behaviours)
            {
                if (behaviour != null) behaviour.enabled = enabled;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
