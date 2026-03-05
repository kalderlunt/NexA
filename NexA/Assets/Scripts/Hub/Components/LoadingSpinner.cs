using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using UnityEngine.UI;

namespace Utils
{
    /// <summary>
    /// Loading spinner with infinite rotation and automatic fade in
    /// </summary>
    [RequireComponent(typeof(CanvasGroup), typeof(Image))]
    public class LoadingSpinner : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float fadeDuration = AnimationHelper.NORMAL;
        [SerializeField] private RotationDirection rotationDirection = RotationDirection.CounterClockwise;
        
        [Header("References")]
        private Transform spinnerTransform;
        private CanvasGroup canvasGroup;
        private Tweener rotationTween;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Assert.IsNotNull(canvasGroup, "CanvasGroup component is missing!");
            
            spinnerTransform = transform;
            Assert.IsNotNull(spinnerTransform, "Spinner Transform cannot be null!");
        }

        private void OnEnable()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration).SetEase(AnimationHelper.IN_SMOOTH);
            
            // Start infinite rotation with configured direction
            rotationTween = AnimationHelper.StartLoadingSpinner(spinnerTransform, rotationDirection);
        }

        private void OnDisable()
        {
            // Stop the rotation
            if (rotationTween != null && rotationTween.IsActive())
            {
                rotationTween.Kill();
                rotationTween = null;
            }
            
            // Reset
            canvasGroup.alpha = 0f;
            spinnerTransform.rotation = Quaternion.identity;
        }
    }
}
