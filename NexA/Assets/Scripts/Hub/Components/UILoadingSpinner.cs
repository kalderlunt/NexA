using UnityEngine;
using DG.Tweening;
using UnityEngine.Assertions;
using Utils;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Loading spinner that rotates infinitely with fade in/out
    /// Automatically starts spinning when enabled
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
    public class UILoadingSpinner : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Rotation direction (clockwise or counter-clockwise)")]
        [SerializeField] private RotationDirection direction = RotationDirection.CounterClockwise;
        
        [Tooltip("Fade in the canvas group when enabled")]
        [SerializeField] private bool fadeOnEnable = true;
        
        [Tooltip("Duration of the fade in animation")]
        [SerializeField] private float fadeDuration = AnimationHelper.VERY_SLOW;

        private CanvasGroup canvasGroup;
        private Transform spinnerTransform;
        private Tweener rotationTweener;

        private void Awake()
        {
            spinnerTransform = transform;
            canvasGroup = GetComponent<CanvasGroup>();
            Assert.IsNotNull(canvasGroup, "[UILoadingSpinner] CanvasGroup component is missing!");
        }

        private void OnEnable()
        {
            // Start rotation
            StartSpinning();
            
            // Fade in canvas group if enabled
            if (fadeOnEnable)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeDuration).SetEase(AnimationHelper.IN_SMOOTH);
            }
        }

        private void OnDisable()
        {
            StopSpinning();
        }

        /// <summary>
        /// Start the infinite rotation
        /// </summary>
        public void StartSpinning()
        {
            // Kill any existing tweens on this transform
            spinnerTransform.DOKill();
            
            // Reset rotation to ensure clean state
            spinnerTransform.localEulerAngles = Vector3.zero;
            
            // Use LocalAxisAdd mode to avoid accumulation issues
            float rotationAngle = direction == RotationDirection.Clockwise ? -360f : 360f;
            rotationTweener = spinnerTransform.DOLocalRotate(new Vector3(0, 0, rotationAngle), 1f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        /// <summary>
        /// Stop the rotation
        /// </summary>
        public void StopSpinning()
        {
            if (rotationTweener != null && rotationTweener.IsActive())
            {
                rotationTweener.Kill();
                rotationTweener = null;
            }
        }

        /// <summary>
        /// Change the rotation direction at runtime
        /// </summary>
        public void SetDirection(RotationDirection newDirection)
        {
            direction = newDirection;
        }

        /// <summary>
        /// Fade out the canvas group and disable the spinner
        /// </summary>
        public void FadeOutAndDisable(float duration = AnimationHelper.FAST)
        {
            canvasGroup.DOFade(0f, duration)
                .SetEase(AnimationHelper.OUT_SMOOTH)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
