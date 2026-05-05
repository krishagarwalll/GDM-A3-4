using System.Collections;
using UnityEngine;

namespace Game.Core.Scenes
{
    /// <summary>
    /// Persistent two-bar curtain. Owns nothing about scene loading — exposes
    /// Cover/Reveal/RunBetween coroutines so any system (loader, death screen,
    /// cutscene trigger) can drive transitions through the same overlay.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        [Header("Bars")]
        [SerializeField] private RectTransform leftBar;
        [SerializeField] private RectTransform rightBar;

        [Header("Timing")]
        [SerializeField] private float coverDuration = 0.35f;
        [SerializeField] private float revealDuration = 0.35f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // 0 = bars meeting in middle (covered); 1 = bars off-screen
        private float _offset = 1f;
        private float _halfWidth;
        private Coroutine _running;

        public bool IsBusy => _running != null;
        public bool IsCovered => Mathf.Approximately(_offset, 0f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CacheHalfWidth();
            _offset = 1f;
            ApplyOffset();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Slide bars in to cover the screen. Idempotent.</summary>
        public IEnumerator Cover()
        {
            yield return PlayTo(0f, coverDuration);
        }

        /// <summary>Slide bars out to reveal the screen. Idempotent.</summary>
        public IEnumerator Reveal()
        {
            yield return PlayTo(1f, revealDuration);
            if (Mathf.Approximately(_offset, 1f))
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Cover → run a middle action (e.g. swap scenes) → reveal.
        /// Yields one frame between middle and reveal so the new scene's
        /// Awake/Start/camera setup completes before the curtain opens.
        /// </summary>
        public IEnumerator RunBetween(IEnumerator middle)
        {
            yield return Cover();
            if (middle != null) yield return middle;
            yield return null;
            yield return Reveal();
        }

        private IEnumerator PlayTo(float target, float duration)
        {
            gameObject.SetActive(true);
            yield return null;             // let layout settle if just activated
            CacheHalfWidth();

            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Animate(target, duration));
            yield return _running;
            _running = null;
        }

        private IEnumerator Animate(float toT, float duration)
        {
            float fromT = _offset;
            if (duration <= 0f || Mathf.Approximately(fromT, toT))
            {
                _offset = toT;
                ApplyOffset();
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                _offset = Mathf.Lerp(fromT, toT, ease.Evaluate(k));
                ApplyOffset();
                yield return null;
            }
            _offset = toT;
            ApplyOffset();
        }

        private void CacheHalfWidth()
        {
            var canvas = GetComponentInParent<Canvas>(true);
            if (canvas == null) return;
            var canvasRect = canvas.GetComponent<RectTransform>();
            float w = canvasRect.rect.width;
            if (w > 0f) _halfWidth = w * 0.5f;
        }

        private void ApplyOffset()
        {
            if (leftBar != null)
            {
                var p = leftBar.anchoredPosition;
                p.x = -_halfWidth * _offset;
                leftBar.anchoredPosition = p;
            }
            if (rightBar != null)
            {
                var p = rightBar.anchoredPosition;
                p.x = _halfWidth * _offset;
                rightBar.anchoredPosition = p;
            }
        }
    }
}
