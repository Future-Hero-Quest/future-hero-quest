using System.Collections;
using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Puzzle
{
    /// <summary>
    /// 可被改写物体基类。所有"过去事件 -> 未来变化"的物体都继承自这里。
    /// 未来玩家的物体在 OnEnable 订阅 TimelineEventBus，收到 targetId 匹配的事件时切换状态。
    /// 过去玩家的物体则负责发出事件（通过 SendChangeEvent）。
    ///
    /// 状态切换通过半透明叠加（A 档·30 分钟实现）：
    /// initialState alpha 1->0，changedState alpha 0->1，0.5s 完成。
    /// </summary>
    public abstract class PuzzleObject : MonoBehaviour
    {
        [Header("时间线事件配置")]
        [Tooltip("跨时空唯一 ID，过去/未来世界用此 ID 关联。例: tree_garden_01")]
        [SerializeField] protected string targetId;

        [Tooltip("此物体响应的事件类型")]
        [SerializeField] protected EventKind respondsTo;

        [Header("视觉切换")]
        [SerializeField] protected GameObject initialStateRoot;
        [SerializeField] protected GameObject changedStateRoot;
        [SerializeField] protected float fadeDuration = 0.5f;
        [SerializeField] protected AudioClip transitionSfx;

        protected bool _hasChanged;

        public string TargetId => targetId;
        public bool HasChanged => _hasChanged;

        protected virtual void OnEnable()
        {
            if (TimelineEventBus.Instance != null)
            {
                TimelineEventBus.Instance.OnEventReceived += HandleEvent;
            }
            ApplyInitialVisual();
        }

        protected virtual void OnDisable()
        {
            if (TimelineEventBus.Instance != null)
            {
                TimelineEventBus.Instance.OnEventReceived -= HandleEvent;
            }
        }

        protected virtual void HandleEvent(TimelineEvent evt)
        {
            if (evt.Kind != respondsTo) return;
            if (evt.TargetId != targetId) return;
            if (_hasChanged) return;

            _hasChanged = true;
            StartCoroutine(SwitchToChangedState(evt));
        }

        protected virtual IEnumerator SwitchToChangedState(TimelineEvent evt)
        {
            if (transitionSfx != null)
            {
                AudioSource.PlayClipAtPoint(transitionSfx, transform.position);
            }

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                SetAlpha(initialStateRoot, 1f - p);
                if (changedStateRoot != null && p > 0f) changedStateRoot.SetActive(true);
                SetAlpha(changedStateRoot, p);
                yield return null;
            }

            ApplyChangedVisual();
            OnChangeComplete(evt);
        }

        protected virtual void ApplyInitialVisual()
        {
            if (initialStateRoot != null) initialStateRoot.SetActive(true);
            if (changedStateRoot != null) changedStateRoot.SetActive(false);
            SetAlpha(initialStateRoot, 1f);
        }

        protected virtual void ApplyChangedVisual()
        {
            if (initialStateRoot != null) initialStateRoot.SetActive(false);
            if (changedStateRoot != null) changedStateRoot.SetActive(true);
            SetAlpha(changedStateRoot, 1f);
        }

        protected virtual void OnChangeComplete(TimelineEvent evt) { }

        protected static void SetAlpha(GameObject root, float a)
        {
            if (root == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_BaseColor"))
                    {
                        var c = mat.GetColor("_BaseColor");
                        c.a = a;
                        mat.SetColor("_BaseColor", c);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        var c = mat.GetColor("_Color");
                        c.a = a;
                        mat.SetColor("_Color", c);
                    }
                }
            }
        }

        public virtual void SendChangeEvent(Vector3 payload)
        {
            if (TimelineEventBus.Instance == null)
            {
                Debug.LogError($"[{nameof(PuzzleObject)}] TimelineEventBus is not ready.");
                return;
            }
            TimelineEventBus.Instance.SendEvent(respondsTo, targetId, payload);
        }
    }
}
