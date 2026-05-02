using UnityEngine;

namespace FutureHeroQuest.SceneFlow
{
    public class LevelTransitionAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = "Anchor_Default";
        [SerializeField] private string note;

        public string AnchorId => (anchorId ?? string.Empty).Trim();
        public string Note => note;
    }
}
