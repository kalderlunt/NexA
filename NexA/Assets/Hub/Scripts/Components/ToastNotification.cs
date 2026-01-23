using DG.Tweening;
using NexA.Hub.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Système de notifications toast (inspiré Material Design)
    /// Usage: ToastManager.Show("Message", ToastType.Success);
    /// </summary>
    public static class ToastManager
    {
        private static GameObject _toastPrefab;
        private static Transform _container;

        /// <summary>
        /// Afficher une notification toast
        /// </summary>
        public static void Show(string message, ToastType type = ToastType.Info, float duration = 3f)
        {
            EnsureInitialized();

            // Charger et instancier le prefab
            if (_toastPrefab == null)
            {
                Debug.LogError("[ToastManager] Toast prefab not found in Resources/UI/ToastNotification");
                return;
            }

            GameObject toastObj = GameObject.Instantiate(_toastPrefab, _container);
            ToastNotification toast = toastObj.GetComponent<ToastNotification>();

            if (toast != null)
            {
                toast.Setup(message, type, duration);
            }
            else
            {
                Debug.LogError("[ToastManager] ToastNotification component not found on prefab");
                GameObject.Destroy(toastObj);
            }
        }

        private static void EnsureInitialized()
        {
            if (_container == null)
            {
                _container = Core.UIManager.Instance?.ToastContainer;
                
                if (_container == null)
                {
                    Debug.LogError("[ToastManager] UIManager.Instance.ToastContainer not found!");
                }
            }

            if (_toastPrefab == null)
            {
                _toastPrefab = Resources.Load<GameObject>("UI/ToastNotification");
            }
        }
    }

    /// <summary>
    /// Component attaché au prefab de toast
    /// </summary>
    public class ToastNotification : MonoBehaviour
    {
        [Header("UI References")]
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Colors")]
        [SerializeField] private Color infoColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color successColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color warningColor = new Color(1f, 0.7f, 0.2f);
        [SerializeField] private Color errorColor = new Color(0.9f, 0.3f, 0.3f);

        [Header("Position")]
        [SerializeField] private float visibleY = 20f;
        [SerializeField] private float hiddenY = -100f;

        private Sequence _animationSequence;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            Assert.IsNotNull(rectTransform, "[TestNotification] rectTransform is null");
            Assert.IsNotNull(canvasGroup, "[TestNotification] canvasGroup is null");
            Assert.IsNotNull(messageText, "[TestNotification] messageText is null");
            Assert.IsNotNull(backgroundImage, "[TestNotification] backgroundImage is null");
            Assert.IsNotNull(iconImage, "[TestNotification] iconImage is null");

            //Setup("qwoenqwioeqw", ToastType.Error, 6f);
        }
        
        public void Setup(string message, ToastType type, float duration)
        {
            messageText.text = message;

            // Définir la couleur selon le type
            Color typeColor = type switch
            {
                ToastType.Success => successColor,
                ToastType.Warning => warningColor,
                ToastType.Error => errorColor,
                _ => infoColor
            };

            if (backgroundImage != null)
                backgroundImage.color = typeColor;

            if (iconImage != null)
            {
                iconImage.color = Color.white;
                // TODO: changer le sprite de l'icône selon le type
            }

            // Animer l'apparition
            Animate(duration);
        }

        private void Animate(float displayDuration)
        {
            // État initial
            rectTransform.anchoredPosition = new Vector2(0, hiddenY);
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);

            _animationSequence = DOTween.Sequence();

            // Slide in + fade in
            _animationSequence.Append(rectTransform.DOAnchorPosY(visibleY, AnimationHelper.MEDIUM).SetEase(AnimationHelper.IN_BACK));
            _animationSequence.Join(canvasGroup.DOFade(1f, AnimationHelper.MEDIUM));

            // Attendre
            _animationSequence.AppendInterval(displayDuration);

            // Slide out + fade out
            _animationSequence.Append(rectTransform.DOAnchorPosY(hiddenY, AnimationHelper.FAST).SetEase(AnimationHelper.OUT_FAST));
            _animationSequence.Join(canvasGroup.DOFade(0f, AnimationHelper.FAST));

            // Détruire
            _animationSequence.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            _animationSequence?.Kill();
        }
    }
}

