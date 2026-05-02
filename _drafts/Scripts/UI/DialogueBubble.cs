using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FutureHeroQuest.UI
{
    /// <summary>
    /// 对话气泡：显示对方刚发来的台词，3 秒后自动淡出。
    /// 推荐挂在 Canvas (Screen Space - Overlay) 顶部居中。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueBubble : MonoBehaviour
    {
        [SerializeField] private Text bubbleText;
        [SerializeField] private float showDuration = 3.0f;
        [SerializeField] private float fadeDuration = 0.3f;

        private CanvasGroup _cg;
        private Coroutine _activeRoutine;

        private void Awake()
        {
            _cg = GetComponent<CanvasGroup>();
            _cg.alpha = 0f;
        }

        public void Show(string message)
        {
            if (bubbleText != null) bubbleText.text = message;
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            yield return Fade(0f, 1f, fadeDuration);
            yield return new WaitForSeconds(showDuration);
            yield return Fade(1f, 0f, fadeDuration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            _cg.alpha = to;
        }
    }
}
