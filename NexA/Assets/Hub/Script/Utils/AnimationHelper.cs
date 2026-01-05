using DG.Tweening;
using UnityEngine;

namespace NexA.Hub.Utils
{
    /// <summary>
    /// Design system pour toutes les animations DOTween du client
    /// Durées, easings, et helpers réutilisables
    /// </summary>
    public static class AnimationHelper
    {
        #region Timings

        public const float INSTANT = 0.1f;
        public const float FAST = 0.2f;
        public const float NORMAL = 0.3f;
        public const float MEDIUM = 0.4f;
        public const float SLOW = 0.5f;
        public const float VERY_SLOW = 0.8f;

        #endregion

        #region Easings

        public static readonly Ease IN_BACK = Ease.OutBack;
        public static readonly Ease IN_SMOOTH = Ease.OutQuad;
        public static readonly Ease IN_ELASTIC = Ease.OutElastic;
        public static readonly Ease OUT_SMOOTH = Ease.InQuad;
        public static readonly Ease OUT_FAST = Ease.InCubic;
        public static readonly Ease LOOP = Ease.InOutSine;
        public static readonly Ease HOVER = Ease.OutQuad;

        #endregion

        #region Screen Transitions

        /// <summary>
        /// Faire apparaître un écran (fade + slide)
        /// </summary>
        public static Sequence FadeInScreen(CanvasGroup canvasGroup, RectTransform rectTransform, float slideOffset = 50f)
        {
            Sequence sequence = DOTween.Sequence();
            
            // État initial
            canvasGroup.alpha = 0;
            rectTransform.anchoredPosition = new Vector2(slideOffset, 0);
            canvasGroup.gameObject.SetActive(true);

            // Animation
            sequence.Append(canvasGroup.DOFade(1f, NORMAL).SetEase(IN_SMOOTH));
            sequence.Join(rectTransform.DOAnchorPosX(0, NORMAL).SetEase(IN_SMOOTH));
            sequence.OnComplete(() => canvasGroup.interactable = true);

            return sequence;
        }

        /// <summary>
        /// Faire disparaître un écran (fade + slide)
        /// </summary>
        public static Sequence FadeOutScreen(CanvasGroup canvasGroup, RectTransform rectTransform, float slideOffset = -50f)
        {
            Sequence sequence = DOTween.Sequence();
            
            canvasGroup.interactable = false;

            // Animation
            sequence.Append(canvasGroup.DOFade(0f, FAST).SetEase(OUT_SMOOTH));
            sequence.Join(rectTransform.DOAnchorPosX(slideOffset, FAST).SetEase(OUT_FAST));
            sequence.OnComplete(() => canvasGroup.gameObject.SetActive(false));

            return sequence;
        }

        #endregion

        #region Panel Animations

        /// <summary>
        /// Panel qui "pop" (apparition avec scale bounce)
        /// </summary>
        public static Sequence PopIn(Transform transform, float duration = MEDIUM)
        {
            Sequence sequence = DOTween.Sequence();
            
            // État initial
            transform.localScale = Vector3.zero;
            transform.gameObject.SetActive(true);

            // Animation
            sequence.Append(transform.DOScale(1f, duration).SetEase(IN_BACK));

            return sequence;
        }

        /// <summary>
        /// Panel qui disparaît (scale to zero)
        /// </summary>
        public static Sequence PopOut(Transform transform, float duration = FAST)
        {
            Sequence sequence = DOTween.Sequence();
            
            // Animation
            sequence.Append(transform.DOScale(0f, duration).SetEase(OUT_FAST));
            sequence.OnComplete(() => transform.gameObject.SetActive(false));

            return sequence;
        }

        #endregion

        #region Button Interactions

        /// <summary>
        /// Hover sur un bouton (scale + optional glow)
        /// </summary>
        public static void ButtonHoverEnter(Transform button, CanvasGroup glow = null, float scaleMultiplier = 1.05f)
        {
            button.DOScale(scaleMultiplier, FAST).SetEase(HOVER);
            
            if (glow != null)
                glow.DOFade(0.3f, FAST);
        }

        /// <summary>
        /// Exit hover sur un bouton
        /// </summary>
        public static void ButtonHoverExit(Transform button, CanvasGroup glow = null)
        {
            button.DOScale(1f, FAST).SetEase(HOVER);
            
            if (glow != null)
                glow.DOFade(0f, FAST);
        }

        /// <summary>
        /// Animation de clic (punch scale)
        /// </summary>
        public static void ButtonClick(Transform button)
        {
            button.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f);
        }

        #endregion

        #region Loading Animations

        /// <summary>
        /// Spinner de chargement (rotation infinie)
        /// </summary>
        public static Tweener StartLoadingSpinner(Transform spinner)
        {
            return spinner.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Pulse pour un élément de loading (scale loop)
        /// </summary>
        public static Tweener StartLoadingPulse(Transform element, float scaleMin = 0.9f, float scaleMax = 1.1f)
        {
            return element.DOScale(scaleMax, 0.6f)
                .SetEase(LOOP)
                .SetLoops(-1, LoopType.Yoyo)
                .From(scaleMin);
        }

        /// <summary>
        /// Skeleton shimmer effect (gradient qui se déplace)
        /// </summary>
        public static Tweener StartShimmerEffect(RectTransform shimmerMask, float width)
        {
            shimmerMask.anchoredPosition = new Vector2(-width, 0);
            
            return shimmerMask.DOAnchorPosX(width, 1.5f)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        #endregion

        #region Notifications / Toasts

        /// <summary>
        /// Toast qui slide depuis le bas
        /// </summary>
        public static Sequence ShowToast(RectTransform toast, float visibleY = 20f, float hiddenY = -100f, float displayDuration = 3f)
        {
            Sequence sequence = DOTween.Sequence();
            
            // État initial
            toast.anchoredPosition = new Vector2(0, hiddenY);
            toast.gameObject.SetActive(true);

            // Slide in
            sequence.Append(toast.DOAnchorPosY(visibleY, MEDIUM).SetEase(IN_BACK));
            
            // Attendre
            sequence.AppendInterval(displayDuration);
            
            // Slide out
            sequence.Append(toast.DOAnchorPosY(hiddenY, FAST).SetEase(OUT_FAST));
            sequence.OnComplete(() => Object.Destroy(toast.gameObject));

            return sequence;
        }

        /// <summary>
        /// Badge notification (pop + shake)
        /// </summary>
        public static Sequence ShowNotificationBadge(Transform badge)
        {
            Sequence sequence = DOTween.Sequence();
            
            badge.localScale = Vector3.zero;
            badge.gameObject.SetActive(true);

            // Pop
            sequence.Append(badge.DOScale(1f, MEDIUM).SetEase(IN_BACK));
            
            // Shake
            sequence.Append(badge.DOShakeRotation(0.3f, strength: 15f, vibrato: 10));

            return sequence;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Shake pour indiquer une erreur
        /// </summary>
        public static void ShakeError(Transform transform, float strength = 10f)
        {
            transform.DOShakePosition(0.3f, strength: strength, vibrato: 10, fadeOut: true);
        }

        /// <summary>
        /// Highlight temporaire (flash color)
        /// </summary>
        public static void FlashHighlight(UnityEngine.UI.Image image, Color highlightColor, float duration = 0.5f)
        {
            Color originalColor = image.color;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(image.DOColor(highlightColor, duration * 0.3f));
            sequence.Append(image.DOColor(originalColor, duration * 0.7f));
        }

        /// <summary>
        /// Kill toutes les animations d'un GameObject
        /// </summary>
        public static void KillAllAnimations(GameObject target, bool complete = false)
        {
            target.transform.DOKill(complete);
            
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.DOKill(complete);
            
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.DOKill(complete);
        }

        #endregion
    }
}

